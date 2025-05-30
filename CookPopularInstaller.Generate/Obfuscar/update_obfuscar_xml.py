import sys
import os
import json
import xml.etree.ElementTree as ET


obfuscar_xml="Obfuscar\\Obfuscar.xml"


def update_obfuscar_xml_element(name:str, value:str):
    tree = ET.parse(obfuscar_xml)
    root = tree.getroot()
    var_elements = root.findall("*")
    for var_element in var_elements:
        if var_element.get('name') == name:
            var_element.set('value', value)

    tree.write(obfuscar_xml, encoding="utf-8", xml_declaration=True)


def add_module_element(file_name:str):
    with open(file_name, 'r', encoding='utf-8') as f:
        content = json.load(f)
        confuse_dll_names = content['Confuse']['ConfuseDllNames']

    tree = ET.parse(obfuscar_xml)
    root = tree.getroot()
    module_elements = root.findall("Module")
    for module_element in module_elements:
        root.remove(module_element)

    for dll_name in confuse_dll_names:   
        module = ET.Element(f'Module file="$(InPath)\{dll_name}"')  
        root.insert(-2, module)

    tree.write(obfuscar_xml, encoding="utf-8", xml_declaration=True, short_empty_elements=True)
    print('update Obfuscar.xml success')


update_obfuscar_xml_element('InPath', sys.argv[1])
add_module_element(sys.argv[2])