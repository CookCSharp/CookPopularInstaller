@echo off
REM Run As Admin,0��ʾ����ʾCMD���壬1��ʾ��ʾCMD��
%1 %2
ver|find "5.">nul&&goto:Admin
mshta vbscript:createobject("shell.application").shellexecute("%~s0","goto :Admin","","runas",0)(window.close)&goto :eof
:Admin

md "C:\Program Files (x86)\Chance"

REM pause