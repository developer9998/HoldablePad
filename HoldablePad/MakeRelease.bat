@echo off
for %%f in (.) do set eName=%%~nxf

mkdir BepInEx\plugins\%eName%
xcopy /s %~dp0bin\Debug\netstandard2.1\%eName%.dll %~dp0BepInEx\plugins\%eName%

powershell -command "Compress-Archive '%~dp0BepInEx' '%eName%'
powershell -command "Remove-Item '%~dp0BepInEx' -Recurse