# Resilient Form Automation

A C# Selenium console app that fills and verifies the CloudQA practice form.

## Requirements

- .NET 10 SDK
- Chrome
- ChromeDriver compatible with the installed Chrome version

The app looks for `chromedriver.exe` on `PATH` or in `CHROMEDRIVER_PATH`.
You can also provide its path with `--driver-path`. Do not commit browser
driver binaries to the repository.

## Run

From this directory in PowerShell:

```powershell
dotnet run
```

By default, the app uses the CloudQA Automation Practice Form and sample
values. Override the URL and form values as needed:

```powershell
dotnet run -- --url "https://app.cloudqa.io/home/AutomationPracticeForm" `
  --first-name "Automation" --last-name "Tester" --date-of-birth "1990-01-01" `
  --email "tester@example.com" --gender "Male" --state "India"
```

`AUTOMATION_FORM_URL` sets the default URL. `CHROMEDRIVER_PATH` sets the
ChromeDriver executable path. Command-line options override environment
variables and defaults.

Use test data only. Do not enter real personal information into a public demo
form.

Screenshots for the project can be added to `screenshots/`.
