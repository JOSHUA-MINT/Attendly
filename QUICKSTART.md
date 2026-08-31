# Attendly - Quick Start Guide

## Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Supabase project (already configured with your keys)

## Setup Steps

### 1. Database Setup (Supabase SQL Editor)

Run this SQL in your Supabase project's SQL Editor to create all tables:

```sql
-- Copy the contents of Data/migrations/001_initial_schema.sql
-- and paste into the Supabase SQL Editor, then click "Run"
```

### 2. Configure Application

Your Supabase credentials are already configured in:
- `appsettings.Development.json`
- `appsettings.json`

### 3. Run the Application

```bash
cd Attendly
dotnet restore
dotnet run
```

The app will start at `http://localhost:5000` or `https://localhost:5001`.

### 4. First Steps

1. Open the browser to the URL shown in the console
2. Click **Sign Up** to create your account
3. After registration, add your first subject
4. Add timetable entries
5. Start marking attendance!

## Project Structure

```
Attendly/
├── Controllers/ # 14 controllers (MVC pattern)
├── Models/ # 11 entity models
├── ViewModels/ # Data transfer objects for views
├── Services/ # Business logic (attendance calc, analytics, planner, Razorpay)
├── Data/ # Supabase client + migration scripts
├── Views/ # 34 Razor views
├── wwwroot/ # CSS, JS, static assets
├── Configuration/ # App configuration helpers
└── Middleware/ # Custom middleware
```

## Key Files to Know

| File | Purpose |
|------|---------|
| `Program.cs` | App entry point, DI, middleware pipeline |
| `appsettings.json` | Configuration (already has Supabase keys) |
| `Data/migrations/001_initial_schema.sql` | Run this in Supabase SQL Editor |
| `Services/AttendanceCalculationService.cs` | Core attendance math engine |
| `Services/PlannerService.cs` | Holiday/weekend planner logic |
| `Controllers/DashboardController.cs` | Main dashboard page |

## Troubleshooting

### Supabase Connection Issues
- Verify your Supabase project is active
- Check that the URL and keys in appsettings.json are correct
- Ensure you've run the SQL migration to create tables

### Razorpay Not Working
- Razorpay keys are optional — the rest of the app works without them
- Add keys to appsettings.json when ready to accept payments

### Build Errors
- Ensure .NET 8.0 SDK is installed: `dotnet --version`
- Try `dotnet clean && dotnet restore`

## Features Overview

- Dashboard with attendance overview
- Subject management (add/edit/archive)
- Attendance tracking with status (present/absent/cancelled/late/excused)
- "Can I Skip?" simulator
- Attendance criteria toggle
- Analytics with charts
- Weekly timetable
- Monthly calendar
- Holiday planner with extended weekend finder
- Trip simulator
- Student connect (Western/Central Line directories)
- Connection requests and chat
- Premium subscription with Razorpay
- Admin dashboard

## Database Tables

All tables are in Supabase PostgreSQL:
- `student_profiles` - User accounts
- `subjects` - Student subjects
- `attendance_records` - Attendance entries
- `timetable_entries` - Weekly schedule
- `calendar_events` - Personal events
- `holidays` - Public holidays (admin managed)
- `connections` - Friend requests
- `conversations` - Chat threads
- `messages` - Chat messages
- `reports` - User reports
- `subscriptions` - Premium plans

## Development

```bash
# Hot reload during development
dotnet watch run

# Build only
dotnet build
```
