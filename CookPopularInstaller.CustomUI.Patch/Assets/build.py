import subprocess


patch_build_script='build.sh'
product_name='CookPopularInstaller.Generate'
patch_version='1.0.0.1P01'
old_dir=r'D:\Users\chance.zheng\Desktop\Company\CookPopularInstaller\Output\Package'
new_dir=r'D:\Users\chance.zheng\Desktop\Company\CookPopularInstaller\Output\Publish'


def build(productName:str, oldDir:str, newDir:str):
    subprocess.run([patch_build_script, productName, patch_version, oldDir, newDir], shell=True)


build(product_name, old_dir, new_dir)