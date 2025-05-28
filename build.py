import os
import sys
import shutil


generate_build_cmd = 'dotnet publish CookPopularInstaller.Generate\\CookPopularInstaller.Generate.csproj -c Release -f net48 -t:Rebuild -p:Platform=AnyCPU -p:MaxCpuCount=4 -o .\Output\Publish --self-contained true'
csharpcustomaction_build_cmd = 'MSBuild CookPopularInstaller.CSharpCustomAction\\CookPopularInstaller.CSharpCustomAction.csproj -p:Configuration=Release -t:Restore,Rebuild -p:Platform=AnyCPU -p:MaxCpuCount=4 -p:OutputPath=..\Output\Publish'
uninstall_build_cmd = 'dotnet build CookPopularInstaller.Uninstall\\CookPopularInstaller.Uninstall.csproj -c Release -t:Rebuild -p:Platform=AnyCPU -p:MaxCpuCount=4'
admin_build_cmd = 'dotnet build CookPopularInstaller.Admin\\CookPopularInstaller.Admin.csproj -c Release -t:Rebuild -p:Platform=AnyCPU -p:MaxCpuCount=4'
generate_commandLine_build_cmd = 'dotnet build CookPopularInstaller.Generate.CommandLine\\CookPopularInstaller.Generate.CommandLine.csproj -c Release -t:Restore,Rebuild -p:Platform=AnyCPU -p:MaxCpuCount=4'


def build_installer():
    os.system(generate_build_cmd)
    os.system(csharpcustomaction_build_cmd)
    os.system(uninstall_build_cmd)
    os.system(admin_build_cmd)
    os.system(generate_commandLine_build_cmd)


if __name__ == '__main__':
    if len(sys.argv) == 1:
        print('Debug Mode ...')
        build_installer()
    # elif len(sys.argv) == 2 and sys.argv[1] == "Release":
    #     print('Release Mode ...')
    #     if os.path.exists('C:\Build\\cookpopularinstaller\\'):
    #         shutil.rmtree('C:\Build\\cookpopularinstaller\\')
    #     os.system('xcopy /E/Y . "C:\Build\\cookpopularinstaller\\"')
    #     os.chdir('C:\\Build\\cookpopularinstaller\\')

    #     build_installer()
    else:
        print('Debug: "python build", Release: "python build Release"')
    