HeatExePath="C:\Program Files (x86)\WiX Toolset v3.14\bin\heat.exe"

# 需要打包的文件夹
PackageFolderIn=

# 生成安装包的路径
PackageOutPath=

# 1. 批量生成所有目录文件结构
"$HeatExePath" dir $DirectoryPath -out $DirectoryWxiOutPath -gg -gl -sreg -scom -srd -platform x64 -template fragment -dr INSTALLFOLDER -cg DependencyLibrariesGroup -var var.DependencyLibrariesDir