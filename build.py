import os
import shutil


generate_build_cmd = 'dotnet publish CookPopularInstaller.Generate\\CookPopularInstaller.Generate.csproj -c Release -f net48 -r win-x86 -t:Rebuild -p:Platform=AnyCPU -p:maxcpucount=2 -o .\Output\Publish --self-contained true'
csharpcustomaction_build_cmd = 'MSBuild CookPopularInstaller.CSharpCustomAction\\CookPopularInstaller.CSharpCustomAction.csproj -p:Configuration=Release -t:Restore,Rebuild -p:Platform=AnyCPU -p:maxcpucount=2'
generate_commandLine_build_cmd = 'dotnet build CookPopularInstaller.Generate.CommandLine\\CookPopularInstaller.Generate.CommandLine.csproj -c Release -t:Rebuild -p:Platform=AnyCPU -p:maxcpucount=2'


def build_cookpopularinstaller():
    os.system(generate_build_cmd)
    os.system(csharpcustomaction_build_cmd)
    os.system(generate_commandLine_build_cmd)


if __name__ == '__main__':
    if os.path.exists('C:\Build\\cookpopularinstaller\\'):
        shutil.rmtree('C:\Build\\cookpopularinstaller\\')
    os.system('xcopy /E/Y . "C:\Build\\cookpopularinstaller\\"')
    os.chdir('C:\\Build\\cookpopularinstaller\\')
    build_cookpopularinstaller()