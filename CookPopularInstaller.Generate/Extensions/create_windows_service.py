import sys
import os
# import ctypes
# from win32comext.shell.shell import ShellExecuteEx
# from functools import wraps


# def is_admin():
#     try:
#         return ctypes.windll.shell32.IsUserAnAdmin()
#     except:
#         return False

# def run_cmd_as_admin(func):
#     @wraps(func)
#     def inner(*args, **kwargs):
#         if not is_admin():
#             run_as_admin()
#         else:
#             func(*args, **kwargs)
#     return inner

# ctypes.windll.shell32.ShellExecuteW(None, u"runas", unicode(sys.executable), unicode(__file__), None, 1)
# create = f'sc.exe create {service_name} binPath={service_location} start=auto DisplayName={service_name}'
# description = f'sc.exe description {service_name} {service_description}'
# ShellExecuteEx(lpVerb='runas', lpFile=sys.executable, lpParameters=create, nShow=1)
# ShellExecuteEx(lpVerb='runas', lpFile=sys.executable, lpParameters=description, nShow=1)


def create_start_windows_service():
    os.system(f'sc.exe create {service_name} binPath="{service_location}" start=auto DisplayName={service_name}')
    os.system(f'sc.exe description {service_name} {service_description}')
    os.system(f'sc.exe start {service_name}')
    print(f"start {service_name} success!")


if __name__ == '__main__':
    service_name = sys.argv[1]
    service_location = sys.argv[2]
    service_description = sys.argv[3]
    create_start_windows_service()