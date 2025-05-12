Name=$1
Version=$2
OldDir=$3
NewDir=$4
MSBuildExePath="C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
CurrentDir=$(dirname ${BASH_SOURCE[0]})

cd "$CurrentDir"
ExePath="..\\..\\Output\\bin\\Release\\AnyCPU\\NCATestInstaller.CustomUI.Patch\\net48\\NCATestInstaller.CustomUI.Patch.exe"
"$MSBuildExePath" "..\\NCATestInstaller.CustomUI.Patch.csproj" -t:Restore,Build -p:Configuration=Release -p:Platform="AnyCPU"
# ��ȡ�����ļ���ѹ����Ҫɾ�����ļ�
start //wait "$ExePath" "$Name" "$Version" "$OldDir" "$NewDir"
# �������ļ�ѹ������ɾ���ļ�Ƕ�뵽Exe
"$MSBuildExePath" "..\\NCATestInstaller.CustomUI.Patch.csproj" -t:Restore,Build -p:Configuration=Release -p:Platform="AnyCPU"

# read -n 1