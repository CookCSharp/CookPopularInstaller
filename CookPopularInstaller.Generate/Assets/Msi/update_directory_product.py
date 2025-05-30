import os
import sys
import json
import uuid
import re
import xml.etree.ElementTree as ET
from xml.etree.ElementTree import ProcessingInstruction,Element


wxi = 'Assets\Msi\Directory.wxi'
product_wxs = 'Assets\Msi\Product.wxs'
bundle_wxs = 'Assets\Exe\Bundle.wxs'
custom_bundle_wxs = 'Assets\CustomUIExe\Bundle.wxs'


'''
读取app.config文件
'''
def read_app_config():
    tree = ET.parse('CookPopularInstaller.Generate.exe.config')
    root = tree.getroot()
    elements = root.findall('appSettings/*')
    for element in elements:
        if element.get('key') == 'PackageFolder':
            package_folder = element.get('value')
        elif element.get('key') == 'AppFileName':
            app_file_name = element.get('value')
        elif element.get('key') == 'PackageVersion':
            package_version = element.get('value')
    
    return {'package_folder': package_folder, 'app_file_name': app_file_name, 'package_version': package_version}


'''
读取package.json文件
'''
def read_package_file():
    with open(package_json,'r',encoding='utf-8') as f1:
        content = json.load(f1)
        project = content['Project']
        package_folder = project['PackageFolder']
        package_name = project['PackageName']
        package_version = project['PackageVersion']
        app_file_name = project['AppFileName']

    with open(upgrade_code_json,'r',encoding='utf-8') as f2:
        content = json.load(f2)
        if package_name in content:
            upgrade_code = content[package_name]

    return {'package_folder': package_folder, 'package_version': package_version, 'app_file_name': app_file_name, 'upgrade_code': upgrade_code }


def get_environment_element(name:str, value:str):
    component = Element('Component', {'Id': 'cmp'+str(uuid.uuid4()).replace('-',''), 'Guid': str(uuid.uuid4()), 'KeyPath': 'yes'})
    environment = Element('Environment', {'Id': 'cmp'+str(uuid.uuid4()).replace('-',''), 'Name': name, 'Action': 'set', 'Permanent': 'no', 'System': 'yes', 'Part': 'last', 'Value': value})
    component.append(environment)
    return component


def get_root(root:int):
    if (root == 0):
        return "HKCR"
    elif (root == 1):
        return "HKCU"
    elif (root == 2):
        return "HKLM"
    elif (root == 3):
        return "HKU"
    else:
        return "HKMU"


def get_data_type(type:int):
    if (type == 0):
        return "string"
    elif (type == 1):
        return "integer"
    elif (type == 2):
        return "binary"
    elif (type == 3):
        return "expandable"
    else:
        return "multiString"


def get_registry_element(root:int, key:str, name, type:int, value:str):
    croot = get_root(root)
    ctype = get_data_type(type)
    component = Element('Component', {'Id': 'cmp'+str(uuid.uuid4()).replace('-',''), 'Guid': str(uuid.uuid4()), 'Win64': '$(var.Win64)'})     
    registry_key = Element('RegistryKey', {'Root': croot, 'Key': key, 'Action': 'createAndRemoveOnUninstall', 'ForceCreateOnInstall': 'no', 'ForceDeleteOnUninstall': 'no'}) 
    registry_value = Element('RegistryValue', {'Root': croot, 'Key': key, 'Type': ctype, 'Value': value, 'Action': 'write', 'KeyPath': 'yes'})
    if name != None:
        registry_value.attrib.update({'Name': name})
    component.append(registry_key)
    component.append(registry_value)
    return component


'''
写入扩展项，通过更新Product.wxs文件
包括环境变量、注册表，实际修改注册表即可
'''
def write_extensions(root:Element):
    fragments = root.findall("{http://schemas.microsoft.com/wix/2006/wi}Fragment")   
    component_group = fragments[1].findall("{http://schemas.microsoft.com/wix/2006/wi}ComponentGroup")
    environment_component_group = component_group[0]
    environment_component_group.clear()
    environment_component_group.attrib = {'Id': 'EnvironmentComponents', 'Directory': 'INSTALLFOLDER'}
    registry_component_group = component_group[1]
    registry_component_group.clear()
    registry_component_group.attrib = {'Id': 'RegistryComponents', 'Directory': 'INSTALLFOLDER'}
    with open(package_json, 'r', encoding='utf-8') as f:
        content = json.load(f)
        content = str(content).replace('[InstallFolder]', '[INSTALLFOLDER]').replace('[CompanyName]', '!(loc.CompanyName)').replace('[ProductName]', '!(loc.ProductName)').replace('[AppPath]', '[#App.exe]').replace('[ProductVersion]', package_config['package_version'])
        content = eval(content)      
        environment_variables = content['Extension']['EnvironmentVariables']
        registry_variables = content['Extension']['RegistryVariables']
        for environment_variable in environment_variables:
            environment_element = get_environment_element(environment_variable['Name'], environment_variable['Value'])
            environment_component_group.append(environment_element)
        for registry_variable in registry_variables:
            name = None
            value = ''
            if 'Name' in registry_variable:
                name = registry_variable['Name']
            if 'Value' in registry_variable:
                value = registry_variable['Value']
            registry_element = get_registry_element(registry_variable['RegistryHive'], registry_variable['Path'], name, registry_variable['RegistryValueKind'], value)
            registry_component_group.append(registry_element)
            

'''
更新Product.wxs文件内容，包括定义变量、写入环境变量、注册表信息
'''
def update_product_variable_value():
    package_folder = package_config['package_folder']
    app_file_name = package_config['app_file_name']   
    version = re.sub('([^\u0030-\u0039+.])', '', package_version)

    #读取文件
    tree = ET.parse(product_wxs)
    #获取根节点
    root = tree.getroot()
    pathElement = ProcessingInstruction(f'define DependencyLibrariesDir={package_folder}')
    appElement = ProcessingInstruction(f'define ExeProcessName={app_file_name}')  
    upgradeCodeElement = ProcessingInstruction(f'define UpgradeCode={upgrade_code}')
    versionElement = ProcessingInstruction(f'define Version={version}')
    packagefolderElement = ProcessingInstruction('include $(sys.CURRENTDIR)Assets\Msi\Directory.wxi')
    # <?define Platform=x86 ?>
    # <?if $(var.Platform)=x64 ?>
    #     <?define Win64="yes" ?>
    #     <?define PlatformProgramFilesFolder="ProgramFiles64Folder" ?>
    # <?else ?>
    #     <?define Win64="no" ?>
    #     <?define PlatformProgramFilesFolder="ProgramFilesFolder" ?>
    # <?endif ?>
    platformElement = ProcessingInstruction(f'define Platform={platform}')
    ifElement1 = ProcessingInstruction(f'if $(var.Platform)=x64')
    ifElement2 = ProcessingInstruction(f'define Win64="yes"')
    ifElement3 = ProcessingInstruction(f'define PlatformProgramFilesFolder="ProgramFiles64Folder"')
    ifElement4 = ProcessingInstruction(f'else')
    ifElement5 = ProcessingInstruction(f'define Win64="no"')
    ifElement6 = ProcessingInstruction(f'define PlatformProgramFilesFolder="ProgramFilesFolder"')
    ifElement7 = ProcessingInstruction(f'endif')
    
    root.insert(0, pathElement)
    root.insert(1, appElement)   
    root.insert(2, upgradeCodeElement)
    root.insert(3, versionElement)
    root.insert(4, platformElement)
    root.insert(5, ifElement1)
    root.insert(6, ifElement2)
    root.insert(7, ifElement3)
    root.insert(8, ifElement4)
    root.insert(9, ifElement5)
    root.insert(10, ifElement6)
    root.insert(11, ifElement7)
    root.append(packagefolderElement)

    # product = root.findall("{http://schemas.microsoft.com/wix/2006/wi}Product")
    # product[0].set('UpgradeCode', '{'+str(uuid.uuid4())+'}')

    write_extensions(root)

    # 更改ApplicationDesktopComponent、ApplicationStartMenuComponent的Guid值
    fragments = root.findall("{http://schemas.microsoft.com/wix/2006/wi}Fragment")   
    directory_refs = fragments[1].findall("{http://schemas.microsoft.com/wix/2006/wi}DirectoryRef")
    directory_refs[0].findall("{http://schemas.microsoft.com/wix/2006/wi}Component")[0].set('Guid', '{'+str(uuid.uuid4())+'}')
    directory_refs[1].findall("{http://schemas.microsoft.com/wix/2006/wi}Component")[0].set('Guid', '{'+str(uuid.uuid4())+'}')
   
    root.set('xmlns:bal',"http://schemas.microsoft.com/wix/BalExtension")
    root.set('xmlns:loc',"http://schemas.microsoft.com/wix/2006/localization")
    root.set('xmlns:util',"http://schemas.microsoft.com/wix/UtilExtension")
    ET.register_namespace('',"http://schemas.microsoft.com/wix/2006/wi")
    tree.write(product_wxs, encoding="utf-8", xml_declaration=True)


def update_bundle_variable_value(source):
    #读取文件
    tree = ET.parse(source)
    #获取根节点
    root = tree.getroot()
    upgradeCodeElement = ProcessingInstruction(f'define UpgradeCode={upgrade_code}')
    packageVersionElement = ProcessingInstruction(f'define PackageVersion={package_version}')
    root.insert(0, upgradeCodeElement)
    root.insert(1, packageVersionElement)

    # bundle = root.findall("{http://schemas.microsoft.com/wix/2006/wi}Bundle")
    # bundle[0].set('UpgradeCode', upgrade_code)
    
    root.set('xmlns:bal',"http://schemas.microsoft.com/wix/BalExtension")
    root.set('xmlns:loc',"http://schemas.microsoft.com/wix/2006/localization")
    root.set('xmlns:netfx',"http://schemas.microsoft.com/wix/NetFxExtension")
    root.set('xmlns:util',"http://schemas.microsoft.com/wix/UtilExtension")
    ET.register_namespace('',"http://schemas.microsoft.com/wix/2006/wi")
    tree.write(source, encoding="utf-8", xml_declaration=True)


'''
更新Directory.wxi中主运行程序的ID及增加Windows服务
'''
def update_directory(app_file_name:str):
    #读取文件
    tree = ET.parse(wxi)
    #获取根节点
    root = tree.getroot()
    root.tag = "Include"
    #使用XPath语法寻址
    fragments = root.findall("{http://schemas.microsoft.com/wix/2006/wi}Fragment")
    componentGroup = fragments[1].findall("{http://schemas.microsoft.com/wix/2006/wi}ComponentGroup")
    components = componentGroup[0].findall("{http://schemas.microsoft.com/wix/2006/wi}Component")
    for component in components:
        files = component.findall("{http://schemas.microsoft.com/wix/2006/wi}File")
        if len(files) == 0:
            continue
        source = files[0].get("Source")
        if source == f'$(var.DependencyLibrariesDir)\\{app_file_name}':
            print(files[0].tag)
            print(files[0].attrib)
            files[0].set("Id", "App.exe")
            break

    with open(f'{package_json}', 'r', encoding='utf-8') as f:
        content = json.load(f)
        services = content['Extension']['WindowsServices']
        for service in services:
            for component in components:
                files = component.findall("{http://schemas.microsoft.com/wix/2006/wi}File")
                if len(files) == 0:
                    continue
                source = files[0].get("Source")
                if source == service['Location'].replace('[InstallFolder]', '$(var.DependencyLibrariesDir)\\'):                  
                    service_name = service['Name']
                    service_id = ''.join([i.strip(' ') for i in service_name])
                    service_install_element = Element('ServiceInstall', {'Id': service_id, 'Name': service_name, 'DisplayName': service_name, 'Description': service['Description'], 'Start': 'auto', 'Type': 'ownProcess', 'ErrorControl': 'normal', 'Account': 'LocalSystem', 'Vital': 'yes', 'Interactive': 'no'})
                    service_control_element = Element('ServiceControl', {'Id': service_id, 'Name': service_name, 'Start': 'install', 'Stop': 'both', 'Remove': 'uninstall', 'Wait': 'no'})
                    service_config_element = Element('ServiceConfig', {'Id': service_id, 'ServiceName': service_name, 'OnInstall': 'yes', 'OnReinstall': 'yes', 'OnUninstall': 'no', 'DelayedAutoStart': 'no'})
                    component.append(service_install_element)
                    component.append(service_control_element)
                    component.append(service_config_element)
                    break

    tree = ET.ElementTree(root)
    #注册命名空间前缀
    ET.register_namespace('',"http://schemas.microsoft.com/wix/2006/wi")
    tree.write(wxi, encoding="utf-8", xml_declaration=True)

    version_ps1 = os.path.abspath(__file__) + "/../set_version.ps1"
    os.system(f"powershell -ExecutionPolicy Bypass -File \"{version_ps1}\" \"{wxi}\"")


if __name__=="__main__":
    if len(sys.argv) == 4:
        package_json = sys.argv[1]
        upgrade_code_json = sys.argv[2]
        platform = sys.argv[3]
    else:
        sys.exit(0)
    package_config = read_package_file()
    upgrade_code = package_config['upgrade_code']
    package_version = package_config['package_version']

    update_directory(package_config['app_file_name'])
    update_product_variable_value()
    update_bundle_variable_value(bundle_wxs)
    update_bundle_variable_value(custom_bundle_wxs)