# Ledgerly — local expense tracker

Ledgerly is a focused, single-user expense tracker built with ASP.NET Core Razor Pages on .NET 10. It has no external NuGet packages, JavaScript frameworks, CDNs, or external service dependencies.

## Features

- Add, edit, and delete expenses (deletion has a confirmation page)
- Fixed category list, title, amount, date, and optional notes
- Amount validation: positive, capped at 1,000,000,000, and no more than two decimal places
- Locale-compatible amount entry (both the current culture's separator and `.` are accepted)
- Filter by category and month
- Spending total clearly scoped to the records currently shown
- Responsive, keyboard-friendly layout with an empty state
- JSON persistence in `App_Data/expenses.json`, outside `wwwroot`
- Configurable display currency code in `appsettings.json` (`USD` by default)
- Built-in Razor Pages anti-forgery protection on write operations

The currency code changes display labels only. Ledgerly does not perform currency conversion.

## Requirements and run instructions

Install the **.NET 10 SDK**. The SDK is required; this project has not been designed for an older target framework.

From this directory, run:

```text
dotnet run --project ExpenseTracker.csproj
```

Then open <http://localhost:5080>. The included launch profile uses loopback HTTP at that URL.

The first run creates `App_Data` safely. The app starts with no seed entries; every row is created by the user.

## Data and safety notes

This is a local, single-user app with no authentication or authorization. **Do not publish it publicly** without adding authentication/authorization, HTTPS and deployment hardening, and a production-grade database. The JSON file is intended for a small local workload, not concurrent multi-user hosting.

Writes use an in-process semaphore and a temporary file followed by an atomic replacement. A malformed existing JSON file causes an error and is never silently replaced. Back up `App_Data/expenses.json` if the records matter to you.

## Project shape

- `Pages/Index` — filtered list and totals
- `Pages/Create` and `Pages/Edit` — shared expense form
- `Pages/Delete` — confirmation before deletion
- `Data/JsonExpenseStore` — locked JSON persistence
- `Models` — expense data, categories, and validation
- `wwwroot` — local CSS and small vanilla JavaScript only
