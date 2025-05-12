import xml.etree.ElementTree as ET
from xml.etree.ElementTree import ProcessingInstruction,Element
import sys
import json
import uuid
import re


wxs = "Assets\Msi\Product.wxs"
wxi = "Assets\Msi\Directory.wxi"


'''
读取package.json文件
'''
def read_package_file():
    with open(f'CommandLine/{package_json}','r',encoding='utf-8') as f:
        content = json.load(f)
        project = content['Project']
        package_folder = project['PackageFolder']
        app_file_name = project['AppFileName']
        package_version = project['PackageVersion']

    return {'package_folder': package_folder, 'app_file_name': app_file_name, 'package_version': package_version}


def get_environment_element(name:str, value:str):
    component = Element('Component', {'Id': 'cmp'+str(uuid.uuid4()).replace('-',''),'Guid': str(uuid.uuid4())})
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
    component = Element('Component', {'Id': 'cmp'+str(uuid.uuid4()).replace('-',''),'Guid': str(uuid.uuid4())})     
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
    registry_component_group = component_group[0]
    registry_component_group.clear()
    registry_component_group.attrib = {'Id': 'RegistryComponents', 'Directory': 'INSTALLFOLDER'}
    with open(f'CommandLine/{package_json}','r',encoding='utf-8') as f:
        content = json.load(f)
        content = str(content).replace('[InstallFolder]', '[INSTALLFOLDER]').replace('[CompanyName]', '!(loc.CompanyName)').replace('[ProductName]', '!(loc.ProductName)').replace('[AppPath]', '[#App.exe]').replace('[ProductVersion]', '$(var.Version)')
        content = eval(content)      
        environment_variables = content['Extension']['EnvironmentVariables']
        registry_variables = content['Extension']['RegistryVariables']
        for environment_variable in environment_variables:
            environment_element = get_environment_element(environment_variable['Name'], environment_variable['Value'])
            registry_component_group.append(environment_element)
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
    package_version = package_config['package_version']
    version = re.sub('([^\u0030-\u0039+.])', '', package_version)

    #读取文件
    tree = ET.parse(wxs)
    #获取根节点
    root = tree.getroot()
    pathElement = ProcessingInstruction(f'define DependencyLibrariesDir={package_folder}')
    appElement = ProcessingInstruction(f'define ExeProcessName={app_file_name}')
    versionElement = ProcessingInstruction(f'define Version={version}')
    packagefolderElement = ProcessingInstruction('include $(sys.CURRENTDIR)Assets\Msi\Directory.wxi')
    root.insert(0,pathElement)
    root.insert(1,appElement)
    root.insert(2,versionElement)
    root.append(packagefolderElement)

    write_extensions(root)

    root.set('xmlns:bal',"http://schemas.microsoft.com/wix/BalExtension")
    root.set('xmlns:loc',"http://schemas.microsoft.com/wix/2006/localization")
    root.set('xmlns:util',"http://schemas.microsoft.com/wix/UtilExtension")
    ET.register_namespace('',"http://schemas.microsoft.com/wix/2006/wi")
    tree.write(wxs, encoding="utf-8", xml_declaration=True)


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
        source = files[0].get("Source")
        if source == f'$(var.DependencyLibrariesDir)\\{app_file_name}':
            print(files[0].tag)
            print(files[0].attrib)
            files[0].set("Id", "App.exe")
            break

    with open(f'CommandLine/{package_json}','r',encoding='utf-8') as f:
        content = json.load(f)
        services = content['Extension']['WindowsServices']
        for service in services:
            for component in components:
                files = component.findall("{http://schemas.microsoft.com/wix/2006/wi}File")
                source = files[0].get("Source")
                if source == service['Location'].replace('[InstallFolder]', '$(var.DependencyLibrariesDir)\\'):
                    service_install_element = Element('ServiceInstall', {'Id': service['Name'], 'Name': service['Name'], 'DisplayName': service['Name'], 'Description': service['Description'], 'Start': 'demand', 'Type': 'ownProcess', 'ErrorControl': 'normal', 'Account': 'LocalSystem', 'Vital': 'yes', 'Interactive': 'no'})
                    service_control_element = Element('ServiceControl', {'Id': service['Name'], 'Name': service['Name'], 'Start': 'install', 'Stop': 'both', 'Remove': 'uninstall', 'Wait': 'no'})
                    service_config_element = Element('ServiceConfig', {'ServiceName': service['Name'], 'OnInstall': 'yes', 'DelayedAutoStart': 'no'})
                    component.append(service_install_element)
                    component.append(service_control_element)
                    component.append(service_config_element)
                    break

    tree = ET.ElementTree(root)
    #注册命名空间前缀
    ET.register_namespace('',"http://schemas.microsoft.com/wix/2006/wi")
    tree.write(wxi, encoding="utf-8", xml_declaration=True)


if __name__=="__main__":
    package_json = sys.argv[1]
    package_config = read_package_file()
    update_directory(package_config['app_file_name'])
    update_product_variable_value()