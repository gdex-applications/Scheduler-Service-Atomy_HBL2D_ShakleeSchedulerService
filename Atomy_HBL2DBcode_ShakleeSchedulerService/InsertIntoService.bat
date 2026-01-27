set mypath=%~dp0
set projectname=Atomy_HBL2DBcode_ShakleeSchedulerService
cmd /k "sc.exe create "%projectname%" binpath="%mypath%%projectname%.exe --contentRoot %mypath%""
pause