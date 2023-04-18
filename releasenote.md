v1.0.0.1-pre3:
- 修复安装包升级时UpgradeCode重复bug导致卸载出问题
- 更新安装其它组件时的安装信息，同时设定只有安装时才触发CustomAction
- 更新注册表'Path'键值为'AppPath'
- 修复版本号不能含有字母的问题
- 增加空文件夹的打包

v1.0.0.1-pre2:
- 新增3种主题风格
- 新增非管理员安装(安装系统盘亦可)
- 更新Generate与CommandLine的配置文件读取与存储方式为package.json文件


v1.0.0.1-pre：
- 支持msi与exe两种安装包格式
- 支持自定UI界面操作
- 支持命令行打包(集成到CI/CD)
- 支持混淆obfucar
- 支持日志
- 支持Patch、依赖预置、扩展功能(Windows服务自启动/注册表写入/环境变量等)
- 支持卸载时卸载依赖
- 支持win7 win10 winserver2012