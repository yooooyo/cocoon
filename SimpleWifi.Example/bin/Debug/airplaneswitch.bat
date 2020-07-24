0<!-- : 
@cls & @echo off && mode 50,03 && title <nul && title .\%~nx0 && explorer.exe ms-settings:network-airplanemode
%__APPDIR__%wScript.exe "%~dpnx0?.wsf" && 2>nul >nul %__APPDIR__%taskkill.exe /FI "WindowTitle eq settings*"
goto :EOF & rem :: --> <job> <script language = "vbscript">
Set objShell = WScript.CreateObject("WScript.Shell")
objShell.AppActivate "settings"
WScript.Sleep 333
objShell.SendKeys " "
WScript.Sleep 333 </script></job>