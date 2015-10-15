@echo off

SETLOCAL

SET NUGET=%LocalAppData%\NuGet\NuGet.exe
SET FAKE=%LocalAppData%\FAKE\tools\Fake.exe
SET NYX=%LocalAppData%\Nyx\tools\build.fsx
SET MSBUILD14_TOOLS_PATH="%ProgramFiles(x86)%\MSBuild\14.0\bin\MSBuild.exe"
SET MSBUILD12_TOOLS_PATH="%ProgramFiles(x86)%\MSBuild\12.0\bin\MSBuild.exe"
SET BUILD_TOOLS_PATH=%MSBUILD14_TOOLS_PATH%

IF NOT EXIST %MSBUILD14_TOOLS_PATH% (
  echo In order to run this tool you need either Visual Studio 2015 or
  echo Microsoft Build Tools 2015 tools installed.
  echo.
  echo Visit this page to download either:
  echo.
  echo http://www.visualstudio.com/en-us/downloads/visual-studio-2015-downloads-vs
  echo.
  echo Attempting to fall back to MSBuild 12 for building only
  echo.
  IF NOT EXIST %MSBUILD12_TOOLS_PATH% (
    echo Could not find MSBuild 12.  Please install build tools ^(See above^)
    exit /b 1
  ) else (
    set BUILD_TOOLS_PATH=%MSBUILD12_TOOLS_PATH%
  )
)

echo Downloading latest version of NuGet.exe...
IF NOT EXIST %LocalAppData%\NuGet md %LocalAppData%\NuGet
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest 'https://www.nuget.org/nuget.exe' -OutFile '%NUGET%'"

echo Downloading latest version of Fake.exe...
%NUGET% "install" "FAKE" "-OutputDirectory" "%LocalAppData%" "-ExcludeVersion"

echo Downloading latest version of Nuget.Core...
%NUGET% "install" "Nuget.Core" "-OutputDirectory" "%LocalAppData%" "-ExcludeVersion"

echo Downloading latest version of Nyx...
%NUGET% "install" "Nyx" "-OutputDirectory" "%LocalAppData%" "-ExcludeVersion"

SET TARGET="Build"

IF NOT [%1]==[] (set TARGET="%1")

SET SUMMARY="Hystrix.NET"
SET DESCRIPTION="Hystrix.NET"

%FAKE% %NYX% "target=%TARGET%" appName=Hystrix appSummary=%SUMMARY% appDescription=%DESCRIPTION% nugetPackageName=Hystrix
