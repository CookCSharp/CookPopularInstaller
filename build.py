import os
import sys
import shutil


generate_build_cmd = 'dotnet publish NCATestInstaller.Generate\\NCATestInstaller.Generate.csproj -c Release -f net48 -t:Rebuild -p:Platform=AnyCPU -p:MaxCpuCount=4 -o .\Output\Publish --self-contained true'
csharpcustomaction_build_cmd = 'MSBuild NCATestInstaller.CSharpCustomAction\\NCATestInstaller.CSharpCustomAction.csproj -p:Configuration=Release -t:Restore,Rebuild -p:Platform=AnyCPU -p:MaxCpuCount=4 -p:OutputPath=..\Output\Publish'
uninstall_build_cmd = 'dotnet build NCATestInstaller.Uninstall\\NCATestInstaller.Uninstall.csproj -c Release -t:Rebuild -p:Platform=AnyCPU -p:MaxCpuCount=4'
admin_build_cmd = 'dotnet build NCATestInstaller.Admin\\NCATestInstaller.Admin.csproj -c Release -t:Rebuild -p:Platform=AnyCPU -p:MaxCpuCount=4'
generate_commandLine_build_cmd = 'dotnet build NCATestInstaller.Generate.CommandLine\\NCATestInstaller.Generate.CommandLine.csproj -c Release -t:Restore,Rebuild -p:Platform=AnyCPU -p:MaxCpuCount=4'


def build_ncatestinstaller():
    os.system(generate_build_cmd)
    os.system(csharpcustomaction_build_cmd)
    os.system(uninstall_build_cmd)
    os.system(admin_build_cmd)
    os.system(generate_commandLine_build_cmd)


if __name__ == '__main__':
    if len(sys.argv) == 1:
        print('Debug Mode ...')
        build_ncatestinstaller()
    elif len(sys.argv) == 2 and sys.argv[1] == "Release":
        print('Release Mode ...')
        if os.path.exists('C:\Build\\ncatestinstaller\\'):
            shutil.rmtree('C:\Build\\ncatestinstaller\\')
        os.system('xcopy /E/Y . "C:\Build\\ncatestinstaller\\"')
        os.chdir('C:\\Build\\ncatestinstaller\\')

        build_ncatestinstaller()
    else:
        print('Debug: "python build", Release: "python build Release"')
    