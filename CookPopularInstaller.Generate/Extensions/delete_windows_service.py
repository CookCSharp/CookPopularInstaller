import sys
import os

def delete_windows_service():
    os.system(f"sc.exe stop {service_name}")
    os.system(f"sc.exe delete {service_name}")
    print(f"delete {service_name} success!")


if __name__ == '__main__':
    service_name = sys[1]
    delete_windows_service()