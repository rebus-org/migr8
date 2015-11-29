@echo off

set msbuild=%ProgramFiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe

echo Migr8 build script

if "%1%"=="" (
  echo.
  echo Please specify a version as an argument!
  echo.
  goto exit
)

echo Building version %1% 

git commit -am"Committing last changes version %1%"
git push

"%msbuild%" "%~dp0\build.proj" /t:createNugetPackage /p:Version=%1%

git tag %1%
git push --tags

:exit