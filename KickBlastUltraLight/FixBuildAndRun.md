# FixBuildAndRun Notes

This project is configured to avoid debug profile and IConfiguration issues:

1. Uses only one profile in `Properties/launchSettings.json` with `"commandName": "Project"`.
2. No `executablePath` is used.
3. No `IConfiguration` or binder package usage.
4. WPF app startup is handled in `App.xaml.cs` via `OnStartup`.
5. SQLite DB is initialized in `Db.Init()` with safe folder creation.

If build issues happen:
- Ensure Visual Studio has `.NET desktop development` workload.
- Ensure target framework `.NET 8.0` is installed.
