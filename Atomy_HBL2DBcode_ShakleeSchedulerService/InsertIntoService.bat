set mypath=%~dp0
set projectname=TEMPLATE
cmd /k "sc.exe create "%projectname%" binpath="%mypath%%projectname%.exe --contentRoot %mypath%""
pause