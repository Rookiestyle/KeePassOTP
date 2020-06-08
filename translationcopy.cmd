@echo off
cd %~dp0

if "%1" == "Debug" (
	xcopy /y /c /i Translations\*.xml "..\_KeePass_Debug\Plugins\Translations" /EXCLUDE:..\translationsnocopy.txt
)
if "%1" == "ReleasePlgx" (
	xcopy /y /c /i Translations\*.xml "..\_KeePass_Release\Plugins\Translations" /EXCLUDE:..\translationsnocopy.txt
	xcopy /y /c /i Translations\*.xml "..\_Releases\Translations" /EXCLUDE:..\translationsnocopy.txt
)