Option Explicit

Dim installer, productCode, version
productCode = WScript.Argument(0)

Set installer = CreateObject("WindowsInstaller.Installer")
On Error Resume Next
version = installer.GetProductInfo(productCode, "ProductVersion")

If Err.Number <> 0 Then
	WScript.Echo "Error: " & Err.Description
Else
	WScript.Echo version
End If