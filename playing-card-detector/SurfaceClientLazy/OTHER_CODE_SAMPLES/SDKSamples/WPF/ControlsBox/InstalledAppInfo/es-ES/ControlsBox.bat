@ECHO Off
pushd "%~dp0"

ECHO.
ECHO Adding International Language registry key...
PAUSE
ECHO.

::-------------------------------------------------------------------------------
:: Try to copy a file to a protected directory to see if the script is being run 
:: as admin. If not, elevate or warn the user and quit. 
:: - Make sure we dont leave the file on the hard drive!

copy ControlsBox.reg "%ProgramFiles%" > NUL
IF NOT EXIST "%ProgramFiles%\ControlsBox.reg" goto NO_RIGHTS
del "%ProgramFiles%\ControlsBox.reg"

IF ERRORLEVEL 1 goto IMPORTERROR

IF "%PROCESSOR_ARCHITECTURE%"=="x86" (
  REG IMPORT ControlsBox.reg  
) ELSE (
  %WINDIR%\SysWOW64\REG IMPORT ControlsBox.reg
)
GOTO COMPLETE

:NO_RIGHTS
ECHO.
ECHO Please rerun this batch file as administrator. 
PAUSE
ECHO.  
GOTO EOF

:IMPORTERROR
ECHO.
ECHO Error adding International Language registry key...
PAUSE
ECHO.
GOTO EOF

:COMPLETE
ECHO.
ECHO Successfully added International Language registry key.
PAUSE
ECHO. 

:EOF
popd
