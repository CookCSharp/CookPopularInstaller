using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CookPopularInstaller.Admin
{
    public static class PELocator
    {
        // PE文件结构常量
        private const ushort IMAGE_DOS_SIGNATURE = 0x5A4D; // "MZ"
        private const uint IMAGE_NT_SIGNATURE = 0x00004550; // "PE\0\0"
        private const int PE_HEADER_OFFSET_LOCATION = 0x3C; // DOS头中指向NT头的偏移的地址
        private const ushort IMAGE_NT_OPTIONAL_HDR32_MAGIC = 0x10B; // PE32 魔数
        private const ushort IMAGE_NT_OPTIONAL_HDR64_MAGIC = 0x20B; // PE32+ (64位) 魔数
        private const int RESOURCE_DIRECTORY_INDEX = 2; // 数据目录中资源表的索引
        private const uint RT_MANIFEST = 24; // 清单资源类型ID

        // 用于存储节区信息
        private struct SectionInfo
        {
            public uint VirtualAddress;    // 节区的相对虚拟地址 (RVA)
            public uint SizeOfRawData;     // 节区在磁盘上的大小
            public uint PointerToRawData;  // 节区在文件中的偏移
        }

        /// <summary>
        /// 将相对虚拟地址 (RVA) 转换为文件偏移。
        /// </summary>
        private static long RvaToFileOffset(uint rva, SectionInfo[] sections)
        {
            foreach (var section in sections)
            {
                if (rva >= section.VirtualAddress && rva < section.VirtualAddress + section.SizeOfRawData)
                {
                    return section.PointerToRawData + (rva - section.VirtualAddress);
                }
            }
            return 0; // 未找到
        }

        /// <summary>
        /// 解析资源目录中的条目。
        /// </summary>
        /// <param name="br">BinaryReader实例。</param>
        /// <param name="directoryFileOffset">当前要解析的资源目录的文件偏移。</param>
        /// <param name="targetId">如果 findFirstIdEntry 为 false，则为要查找的ID。</param>
        /// <param name="findFirstIdEntry">如果为 true，则忽略 targetId 并返回第一个基于ID的条目的偏移字段。</param>
        /// <returns>找到的条目的Offset字段值；如果未找到，则返回 -1。</returns>
        private static long FindResourceEntryOffset(BinaryReader br, long directoryFileOffset, uint targetId, bool findFirstIdEntry)
        {
            br.BaseStream.Seek(directoryFileOffset, SeekOrigin.Begin);
            // IMAGE_RESOURCE_DIRECTORY 结构 (共16字节)
            // Characteristics (4), TimeDateStamp (4), MajorVersion (2), MinorVersion (2)
            // NumberOfNamedEntries (2), NumberOfIdEntries (2)
            br.BaseStream.Seek(12, SeekOrigin.Current); // 跳到 NumberOfNamedEntries
            ushort numberOfNamedEntries = br.ReadUInt16();
            ushort numberOfIdEntries = br.ReadUInt16();
            long entriesStartFileOffset = br.BaseStream.Position; // 条目数组的起始位置

            // 首先搜索ID条目
            for (int i = 0; i < numberOfIdEntries; i++)
            {
                br.BaseStream.Seek(entriesStartFileOffset + (i * 8L), SeekOrigin.Begin); // 每个条目8字节
                uint id = br.ReadUInt32();
                uint offset = br.ReadUInt32(); // 条目的Offset字段

                if (findFirstIdEntry || id == targetId)
                {
                    return offset;
                }
            }

            // 对于RT_MANIFEST，通常是通过ID查找。如果需要，可以添加对命名条目的搜索，
            // 但这会增加复杂性且不常见于清单资源类型本身。
            // if (!findFirstIdEntry && targetId == RT_MANIFEST) { ... }

            return -1; // 未找到
        }

        /// <summary>
        /// 查找PE文件中清单（Manifest）资源的文件偏移地址。
        /// </summary>
        /// <param name="filePath">PE文件的路径。</param>
        /// <returns>清单资源的文件偏移地址；如果未找到或发生错误，则返回 -1。</returns>
        public static (long, long) GetManifestFileOffset(string filePath)
        {
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var br = new BinaryReader(fs))
                {
                    // 1. 读取DOS头，找到NT头的偏移 (e_lfanew)
                    if (br.ReadUInt16() != IMAGE_DOS_SIGNATURE) throw new BadImageFormatException("无效的DOS签名。");
                    fs.Seek(PE_HEADER_OFFSET_LOCATION, SeekOrigin.Begin);
                    uint ntHeaderOffset = br.ReadUInt32();
                    fs.Seek(ntHeaderOffset, SeekOrigin.Begin);

                    // 2. 验证NT头签名 ("PE\0\0")
                    if (br.ReadUInt32() != IMAGE_NT_SIGNATURE) throw new BadImageFormatException("无效的NT签名。");

                    // 3. 读取文件头 (IMAGE_FILE_HEADER - 20字节)
                    //    我们需要 numberOfSections 和 sizeOfOptionalHeader
                    fs.Seek(2, SeekOrigin.Current); // 跳过 Machine
                    ushort numberOfSections = br.ReadUInt16();
                    fs.Seek(12, SeekOrigin.Current); // 跳过 TimeDateStamp, PointerToSymbolTable, NumberOfSymbols
                    ushort sizeOfOptionalHeader = br.ReadUInt16();
                    fs.Seek(2, SeekOrigin.Current); // 跳过 Characteristics
                    long optionalHeaderStartOffset = fs.Position;

                    // 4. 读取可选头 (IMAGE_OPTIONAL_HEADER) 的魔数，判断PE32/PE32+
                    ushort optHeaderMagic = br.ReadUInt16();
                    uint resourceTableRva, resourceTableSize;
                    long dataDirectoryBaseOffset; // 数据目录开始的偏移（相对于可选头起始）

                    if (optHeaderMagic == IMAGE_NT_OPTIONAL_HDR32_MAGIC) // PE32
                    {
                        // NumberOfRvaAndSizes 在可选头偏移92处, 数据目录在96处。资源表是第2个条目。
                        // 96 + (2 * 8) = 112 (相对于可选头起始)
                        dataDirectoryBaseOffset = optionalHeaderStartOffset + 96;
                        fs.Seek(optionalHeaderStartOffset + 92, SeekOrigin.Begin); // 定位到 NumberOfRvaAndSizes
                    }
                    else if (optHeaderMagic == IMAGE_NT_OPTIONAL_HDR64_MAGIC) // PE32+
                    {
                        // NumberOfRvaAndSizes 在可选头偏移108处, 数据目录在112处。资源表是第2个条目。
                        // 112 + (2 * 8) = 128 (相对于可选头起始)
                        dataDirectoryBaseOffset = optionalHeaderStartOffset + 112;
                        fs.Seek(optionalHeaderStartOffset + 108, SeekOrigin.Begin); // 定位到 NumberOfRvaAndSizes
                    }
                    else throw new BadImageFormatException("无效的可选头魔数。");

                    uint numberOfRvaAndSizes = br.ReadUInt32();
                    if (RESOURCE_DIRECTORY_INDEX >= numberOfRvaAndSizes) throw new BadImageFormatException("资源目录索引超出范围。");

                    // 读取资源表的RVA和大小
                    fs.Seek(dataDirectoryBaseOffset + (RESOURCE_DIRECTORY_INDEX * 8), SeekOrigin.Begin);
                    resourceTableRva = br.ReadUInt32();
                    resourceTableSize = br.ReadUInt32();

                    if (resourceTableRva == 0) throw new BadImageFormatException("资源表RVA为0，文件可能没有资源。");

                    // 5. 读取节表 (Section Headers)，用于RVA到文件偏移的转换
                    var sections = new SectionInfo[numberOfSections];
                    long sectionHeadersFileOffset = optionalHeaderStartOffset + sizeOfOptionalHeader;
                    fs.Seek(sectionHeadersFileOffset, SeekOrigin.Begin);
                    for (int i = 0; i < numberOfSections; i++)
                    {
                        fs.Seek(8, SeekOrigin.Current); // Name (8 bytes)
                        fs.Seek(4, SeekOrigin.Current); // VirtualSize / Misc_PhysicalAddress
                        sections[i].VirtualAddress = br.ReadUInt32();
                        sections[i].SizeOfRawData = br.ReadUInt32();
                        sections[i].PointerToRawData = br.ReadUInt32();
                        fs.Seek(16, SeekOrigin.Current); // Skip PointerToRelocations, PointerToLinenumbers, NumberOfRelocations, NumberOfLinenumbers, Characteristics
                    }

                    long rootResourceDirFileOffset = RvaToFileOffset(resourceTableRva, sections);
                    if (rootResourceDirFileOffset == 0) throw new BadImageFormatException("无法将资源表RVA映射到文件偏移。");

                    // 6. 解析资源目录结构以找到清单
                    // 第1层: 查找类型为 RT_MANIFEST (24) 的条目
                    long typeEntryOffset = FindResourceEntryOffset(br, rootResourceDirFileOffset, RT_MANIFEST, false);
                    if (typeEntryOffset == -1 || (typeEntryOffset & 0x80000000) == 0) // 高位为1表示是子目录
                        throw new BadImageFormatException("未找到RT_MANIFEST资源类型目录或它不是一个目录。");
                    long idDirFileOffset = rootResourceDirFileOffset + (typeEntryOffset & 0x7FFFFFFF);

                    // 第2层: 查找清单名称/ID (通常是第一个ID条目)
                    long idEntryOffset = FindResourceEntryOffset(br, idDirFileOffset, 0, true);
                    if (idEntryOffset == -1 || (idEntryOffset & 0x80000000) == 0)
                        throw new BadImageFormatException("未找到清单ID目录或它不是一个目录。");
                    long langDirFileOffset = rootResourceDirFileOffset + (idEntryOffset & 0x7FFFFFFF);

                    // 第3层: 查找语言条目 (通常是第一个ID条目，指向数据)
                    long langEntryOffset = FindResourceEntryOffset(br, langDirFileOffset, 0, true);
                    if (langEntryOffset == -1 || (langEntryOffset & 0x80000000) != 0) // 高位为0表示是指向数据条目的偏移
                        throw new BadImageFormatException("未找到清单语言/数据条目或者是目录。");
                    long manifestDataEntryFileOffset = rootResourceDirFileOffset + langEntryOffset; //OffsetToData
                    long manifestDataEntrySizeOffset = manifestDataEntryFileOffset + 4; //Size

                    // 读取 IMAGE_RESOURCE_DATA_ENTRY 结构
                    fs.Seek(manifestDataEntryFileOffset, SeekOrigin.Begin);
                    uint manifestDataRva = br.ReadUInt32(); // 清单内容的RVA
                    uint manifestDataSize = br.ReadUInt32(); // 清单内容的大小 (可选)

                    long manifestActualFileOffset = RvaToFileOffset(manifestDataRva, sections);
                    if (manifestActualFileOffset == 0 && manifestDataRva != 0) throw new BadImageFormatException("无法将清单数据RVA映射到文件偏移。");
                    if (manifestDataRva == 0) throw new BadImageFormatException("清单数据RVA为0。");

                    return (manifestActualFileOffset + 3, manifestDataEntrySizeOffset); //EFBBBF
                }
            }
            catch (BadImageFormatException ex)
            {
                Console.WriteLine($"PE Error: {ex.Message}");
                return (-1, -1);
            }
            catch (EndOfStreamException ex)
            {
                Console.WriteLine($"File Error: {ex.Message}");
                return (-1, -1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                return (-1, -1);
            }
        }

        /// <summary>
        /// 将指定的清单内容写入PE文件，并将执行级别设置为需要管理员权限。
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="mainifest"></param>
        public static void ElevateAdministrator(string filePath, string mainifest)
        {
            var (manifestOffset, manifestSizeOffset) = GetManifestFileOffset(filePath);

            if (manifestOffset != -1 && manifestSizeOffset != -1)
            {
                //Console.WriteLine($"清单（Manifest）文件起始地址: 0x{manifestOffset:X}");

                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                var mainifestBuffer = Encoding.UTF8.GetBytes(mainifest);
                fs.Seek(manifestOffset, SeekOrigin.Begin);
                fs.Write(mainifestBuffer, 0, mainifestBuffer.Length);

                fs.Seek(manifestSizeOffset, SeekOrigin.Begin);               
                fs.WriteByte(0xDD); //length(requireAdministrator)-length(asInvoker)=11，故0xD2->0xDD
                fs.Flush();
            }
            else
            {
                Console.WriteLine("未能找到清单文件或解析PE时发生错误。");
            }
        }
    }
}
