import xml.etree.ElementTree as ET
import os
import sys


def update_directory_wix():
    directory_wxi = os.path.abspath(__file__) + "/../Directory.wxi"
    #读取文件
    tree = ET.parse(directory_wxi)
    #获取根节点
    root = tree.getroot()
    root.tag = "Include"
    #使用XPath语法寻址
    fragments = root.findall("{http://schemas.microsoft.com/wix/2006/wi}Fragment")
    componentGroup = fragments[1].findall("{http://schemas.microsoft.com/wix/2006/wi}ComponentGroup")
    components = componentGroup[0].findall("{http://schemas.microsoft.com/wix/2006/wi}Component")
    for component in components:
        files = component.findall("{http://schemas.microsoft.com/wix/2006/wi}File")
        if len(files) > 0:
            source = files[0].get("Source")
            if source == f'$(var.DependencyLibrariesDir)\\{exe_name}':
                # print(files[0].tag)
                # print(files[0].attrib)
                files[0].set("Id", "App.exe")
                break

    tree = ET.ElementTree(root)
    #注册命名空间前缀
    ET.register_namespace('',"http://schemas.microsoft.com/wix/2006/wi")
    tree.write(directory_wxi, encoding="utf-8", xml_declaration=True)

    version_ps1 = os.path.abspath(__file__) + "/../set_version.ps1"
    os.system(f"powershell -ExecutionPolicy Bypass -File \"{version_ps1}\" Directory.wxi")
    # os.system(f"powershell -ExecutionPolicy Bypass -File \"{version_ps1}\" \"{directory_wxi}\"")


if __name__=="__main__":
    exe_name = sys.argv[1]

    update_directory_wix()