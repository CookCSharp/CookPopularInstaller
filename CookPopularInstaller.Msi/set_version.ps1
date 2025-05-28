param(
	[string] $path
)
# Write-Host "press Enter to continue"

# $path = "D:\Users\chance.zheng\Desktop\Product.wxs"
$defaultVersion = "65535.65535.65535.65535"

# 加载XML内容
# [xml]$xml = Get-Content -Path $path
$xmlDoc = New-Object System.Xml.XmlDocument
$xmlDoc.Load($path)

# 创建命名空间管理器
$namespaceManager = New-Object System.Xml.XmlNamespaceManager($xmlDoc.NameTable)
$namespaceManager.AddNamespace("ns", "http://schemas.microsoft.com/wix/2006/wi")

# 使用命名空间查询XML节点
foreach	($file in $xmlDoc.SelectNodes("//ns:File", $namespaceManager))
{
	# if (-not $file.DefaultVersion)
	$file.SetAttribute("DefaultVersion", $defaultVersion)
}

$xmlDoc.Save($path)

# Read-Host -Prompt "press Enter to continue"
# [System.Console]::ReadKey($true)