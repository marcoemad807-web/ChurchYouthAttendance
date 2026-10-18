@echo off
echo Terminating locked ChurchYouthAttendance.API processes...
taskkill /F /IM ChurchYouthAttendance.API.exe /T
echo Done!
pause
