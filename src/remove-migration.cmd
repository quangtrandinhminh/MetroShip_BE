@REM pushd Repository
dotnet ef migrations remove --project Repository --startup-project WebAPI -v --context AppDbContext

pause
popd
