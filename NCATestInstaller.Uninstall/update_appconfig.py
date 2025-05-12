import sys
import xml.etree.ElementTree as ET

def update_app_config(app_config:str, company_name:str, product_name:str):
    tree = ET.parse(app_config)
    root = tree.getroot()
    elements = root.findall('appSettings/*')
    for element in elements:
        if element.get('key') == 'CompanyName':
            element.set('value', company_name)
        elif element.get('key') == 'ProductName':
            element.set('value', product_name)
    tree.write(app_config, encoding="utf-8", xml_declaration=True)

update_app_config(sys.argv[1], sys.argv[2], sys.argv[3])