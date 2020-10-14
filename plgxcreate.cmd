@echo off
cd %~dp0

for %%* in (.) do set CurrDirName=%%~nx*
echo Processing %CurrDirName%

echo Copying PlgX to KeePass plugin folder
copy "src\bin\Release\%CurrDirName%.plgx" "..\_KeePass_Release\Plugins\%CurrDirName%.plgx"

echo Releasing PlgX
move /y "src\bin\Release\%CurrDirName%.plgx" "..\_Releases\%CurrDirName%.plgx"
