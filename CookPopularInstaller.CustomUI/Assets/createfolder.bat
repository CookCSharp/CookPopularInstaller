@echo off
REM Run As Admin,0表示不显示CMD窗体，1表示显示CMD窗
%1 %2
ver|find "5.">nul&&goto:Admin
mshta vbscript:createobject("shell.application").shellexecute("%~s0","goto :Admin","","runas",0)(window.close)&goto :eof
:Admin

md "C:\Program Files (x86)\Chance"

REM pause