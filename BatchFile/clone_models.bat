@echo off

set /p FOLDER=Enter folder to checkout (e.g. TempleTwin/Models):
set REPO=https://github.com/PrayaLabs/DigitalTwinSupport.git
set BRANCH=main

git clone --filter=blob:none --no-checkout %REPO%

cd DigitalTwinSupport

git sparse-checkout init --cone
git sparse-checkout set %FOLDER%
git checkout %BRANCH%

echo.
echo Successfully checked out %FOLDER%.
pause