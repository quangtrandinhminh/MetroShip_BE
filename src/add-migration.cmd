@REM pushd Repository
For /f "tokens=2-4 delims=/ " %%a in ('date /t') do (set mydate=%%c%%a%%b)
For /f "tokens=1-2 delims=/:" %%a in ("%TIME%") do (set mytime=%%a%%b)
dotnet ef migrations add "init_%mydate%_%mytime%" --project Repository --startup-project WebAPI --output-dir Migrations -v --context AppDbContext

pause
popd