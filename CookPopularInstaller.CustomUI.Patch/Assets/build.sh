Name=$1
Version=$2
OldDir=$3
NewDir=$4
MSBuildExePath="C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
CurrentDir=$(dirname ${BASH_SOURCE[0]})

cd "$CurrentDir"
ExePath="..\\..\\Output\\bin\\Release\\AnyCPU\\CookPopularInstaller.CustomUI.Patch\\net48\\CookPopularInstaller.CustomUI.Patch.exe"
"$MSBuildExePath" "..\\CookPopularInstaller.CustomUI.Patch.csproj" -t:Restore,Build -p:Configuration=Release -p:Platform="AnyCPU"
# 获取差异文件并压缩和要删除的文件
start //wait "$ExePath" "$Name" "$Version" "$OldDir" "$NewDir"
# 将差异文件压缩包和删除文件嵌入到Exe
"$MSBuildExePath" "..\\CookPopularInstaller.CustomUI.Patch.csproj" -t:Restore,Build -p:Configuration=Release -p:Platform="AnyCPU"

# read -n 1