import xml.etree.ElementTree as ET
import sys


zh_cn_exe="Assets\\CustomUIExe\\zh-cn.wxl"
zh_cn_msi="Assets\\Msi\\zh-cn.wxl"


def update_localization_element(zh_cn:str, id:str, value:str):
    tree = ET.parse(zh_cn)
    root = tree.getroot()
    string_elements = root.findall("{http://schemas.microsoft.com/wix/2006/localization}String")
    for string_element in string_elements:
        if string_element.get('Id') == id:
            string_element.text = value

    tree = ET.ElementTree(root)
    ET.register_namespace('',"http://schemas.microsoft.com/wix/2006/localization")
    tree.write(zh_cn, encoding="utf-8", xml_declaration=True)



update_localization_element(zh_cn_msi, 'ProductName', sys.argv[1])
update_localization_element(zh_cn_msi, 'Description', sys.argv[2])
update_localization_element(zh_cn_msi, 'AppIcon', sys.argv[3])
update_localization_element(zh_cn_exe, 'IconSourceFile', sys.argv[3])
update_localization_element(zh_cn_exe, 'ExeProcessName', sys.argv[4])
print('update CustomUIExe/zh-cn.wxl.xml && Msi/zh-cn.wxl success')