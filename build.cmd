@ECHO OFF
SET PATH=%PATH%;%SystemRoot%\Microsoft.NET\Framework\v4.0.30319

PUSHD %~dp0

WHERE msbuild >nul 2>nul
IF %ERRORLEVEL% NEQ 0 (
	ECHO MSBuild not found. Aborting.
	GOTO ERROR
)

WHERE nuget >nul 2>nul
IF %ERRORLEVEL% NEQ 0 (
  ECHO NuGet not found. Aborting.
  GOTO ERROR
)

CALL nuget restore .\Mastersign.Expressions.sln
IF %ERRORLEVEL% NEQ 0 (
	ECHO Installing NuGet dependencies failed. Aborting.
	GOTO ERROR
)

CALL msbuild .\Mastersign.Expressions.sln /v:minimal /p:Configuration=Release
IF %ERRORLEVEL% NEQ 0 (
	ECHO Building solution failed. Aborting.
	GOTO ERROR
)

PUSHD .\Mastersign.Expressions
CALL nuget pack Mastersign.Expressions.csproj -Prop Configuration=Release -OutputDirectory ..\releases -Verbosity quiet
POPD

GOTO END

:ERROR
POPD
EXIT /B 1

:END
POPD