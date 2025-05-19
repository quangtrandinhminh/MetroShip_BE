@REM pushd Repository
dotnet ef database update --project WebAPI --startup-project WebAPI -v --context AppDbContext

pause
popd