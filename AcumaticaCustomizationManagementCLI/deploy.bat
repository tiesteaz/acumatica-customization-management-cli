@echo off
set SourceDir=%1
set TargetDir="C:\Deploy\AcumaticaCustomizationAPI"

if not exist "%SourceDir%" (
    echo Source directory does not exist: %SourceDir%
    exit /b 1
)

if not exist "%TargetDir%" (
    echo Target directory does not exist. Creating: %TargetDir%
    mkdir "%TargetDir%"
)

echo Copying files from "%SourceDir%" to "%TargetDir%"...
xcopy "%SourceDir%*" "%TargetDir%" /Y
echo Files copied successfully.