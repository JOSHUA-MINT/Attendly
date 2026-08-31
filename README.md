# Attendly

**"Know your attendance. Plan your semester. Connect with your campus."**

A full-stack student attendance management and campus social platform built with **ASP.NET Core MVC**, **C#**, **Razor Views**, **Supabase PostgreSQL**, and **Razorpay**.

---

## Features

### Core Features
- **Attendance Tracking** - Mark present, absent, cancelled, late, or excused for every lecture
- **Subject Management** - Create, edit, archive subjects with target attendance percentages
- **Smart Attendance Calculator** - Real-time attendance percentage calculation with status indicators (SAFE/WARNING/CRITICAL)
- **Attendance Criteria Toggle** - Set custom attendance requirements (50%, 60%, 75%, 80%, 85%, or custom)
- **"Can I Skip?" Simulator** - Simulate missing classes and see the real impact on attendance
- **Attendance Analytics** - Charts, trends, and insights about attendance patterns
- **Weekly Timetable** - Visual weekly timetable grid integrated with subjects
- **Academic Calendar** - Monthly calendar with events, holidays, and exams
- **Smart Holiday Planner** - Analyze extended weekends with attendance impact calculations
- **Holiday Trip Simulator** - Calculate attendance impact before planning trips

### Social Features
- **Student Connect** - Find students traveling the same railway line (Western/Central Line)
- **Connection System** - Send, accept, reject connection requests
- **Real-time Chat** - Message accepted connections
- **Student Profiles** - Viewable profiles with privacy controls
- **Report/Block System** - Safety features for the community

### Premium Features (Razorpay)
- Advanced attendance predictions
- Unlimited skip simulations
- Advanced holiday planning
- Detailed monthly reports
- Export attendance reports
- AI-style attendance insights
- Custom attendance rules

### Admin Dashboard
- Platform statistics
- Manage holidays
- Review reports
- Manage user accounts

---

## Tech Stack

| Component | Technology |
|-----------|-----------|
| **Backend Framework** | ASP.NET Core MVC (.NET 8) |
| **Language** | C# |
| **Frontend** | Razor Views / Razor Syntax |
| **Styling** | Custom CSS with Bootstrap 5 |
| **Database** | Supabase PostgreSQL |
| **Authentication** | ASP.NET Authentication with Supabase |
| **Payments** | Razorpay |
| **Real-time** | Supabase Realtime (optional) |

---

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Supabase](https://supabase.com/) account
- [Razorpay](https://razorpay.com/) account (for payments)

---

## Setup Instructions

### 1. Clone the Repository

```bash
git clone <your-repo-url>
cd Attendly
```

### 2. Configure Database (Supabase)

1. Create a new project at [supabase.com](https://supabase.com)
2. Go to **SQL Editor** in your Supabase dashboard
3. Run the SQL from `Data/migrations/001_initial_schema.sql`
4. Go to **Settings > API** to get your credentials:
 - Project URL
 - Anon/Public Key
 - Service Role Key (for server-side operations)

### 3. Configure Razorpay (Optional, for payments)

1. Create an account at [razorpay.com](https://razorpay.com/)
2. Go to **Settings > API Keys**
3. Generate Test API Keys (for development)
4. Note down Key ID and Key Secret

### 4. Configure Application Settings

Edit `appsettings.Development.json`:

```json
{
 "Supabase": {
 "Url": "https://your-project.supabase.co",
 "AnonKey": "eyJhbGciOiJIUzI1NiIs...",
 "ServiceRoleKey": "eyJhbGciOiJIUzI1NiIs..."
 },
 "Razorpay": {
 "KeyId": "rzp_test_xxxxxxxxxxxx",
 "KeySecret": "xxxxxxxxxxxxxxxxxxxxxxxx"
 }
}
```

Or use environment variables (recommended for production):

```bash
export SUPABASE_URL="https://your-project.supabase.co"
export SUPABASE_ANON_KEY="your-anon-key"
export SUPABASE_SERVICE_ROLE_KEY="your-service-role-key"
export RAZORPAY_KEY_ID="rzp_test_xxxxxxxxxxxx"
export RAZORPAY_KEY_SECRET="xxxxxxxxxxxxxxxxxxxxxxxx"
```

### 5. Restore Dependencies

```bash
dotnet restore
```

### 6. Run the Application

```bash
dotnet run
```

The application will be available at `https://localhost:5001` (or the port shown in the console).

---

## Database Schema

The application uses the following tables in Supabase PostgreSQL:

| Table | Description |
|-------|-------------|
| `student_profiles` | User accounts and profiles |
| `subjects` | Student subjects |
| `attendance_records` | Individual attendance entries |
| `timetable_entries` | Weekly timetable entries |
| `calendar_events` | Personal calendar events |
| `holidays` | Public/college holidays (managed by admin) |
| `connections` | Friend/connection requests |
| `conversations` | Chat conversations |
| `messages` | Chat messages |
| `reports` | User reports |
| `subscriptions` | Premium subscriptions |

---

## Project Structure

```
Attendly/
├── Configuration/
│ └── AppConfiguration.cs # Configuration helpers
├── Controllers/
│ ├── AccountController.cs # Auth (register, login, logout)
│ ├── DashboardController.cs # Student dashboard
│ ├── SubjectsController.cs # CRUD for subjects
│ ├── AttendanceController.cs # Attendance tracking & simulator
│ ├── AnalyticsController.cs # Analytics dashboard
│ ├── TimetableController.cs # Weekly timetable
│ ├── CalendarController.cs # Calendar view
│ ├── PlannerController.cs # Holiday/trip planner
│ ├── ConnectController.cs # Student directory & connections
│ ├── MessagesController.cs # Chat/messaging
│ ├── PremiumController.cs # Premium & Razorpay
│ ├── ProfileController.cs # User profile
│ ├── AdminController.cs # Admin dashboard
│ └── HomeController.cs # Landing pages
├── Models/
│ ├── StudentProfile.cs
│ ├── Subject.cs
│ ├── AttendanceRecord.cs
│ ├── TimetableEntry.cs
│ ├── CalendarEvent.cs
│ ├── Holiday.cs
│ ├── Connection.cs
│ ├── Conversation.cs
│ ├── Message.cs
│ ├── Report.cs
│ └── Subscription.cs
├── ViewModels/
│ └── ViewModels.cs # All view models
├── Services/
│ ├── AttendanceCalculationService.cs # Core attendance math
│ ├── AnalyticsService.cs # Analytics engine
│ ├── PlannerService.cs # Holiday/weekend planning
│ └── RazorpayService.cs # Payment integration
├── Data/
│ ├── ISupabaseService.cs # Data access interface
│ ├── SupabaseService.cs # Supabase client implementation
│ └── migrations/
│ └── 001_initial_schema.sql
├── Middleware/
│ └── SessionMiddleware.cs
├── Helpers/ # Utility classes
├── Views/
│ ├── _ViewImports.cshtml
│ ├── _ViewStart.cshtml
│ ├── Shared/
│ │ ├── _Layout.cshtml
│ │ └── Partials/
│ │ ├── _Navbar.cshtml
│ │ └── _Footer.cshtml
│ ├── Home/
│ │ ├── Index.cshtml # Landing page
│ │ ├── Features.cshtml
│ │ └── Privacy.cshtml / Terms.cshtml
│ ├── Account/
│ │ ├── Login.cshtml
│ │ ├── Register.cshtml
│ │ └── ForgotPassword.cshtml
│ ├── Dashboard/
│ │ └── Index.cshtml
│ ├── Subjects/
│ │ ├── Index.cshtml
│ │ └── Create.cshtml
│ ├── Attendance/
│ │ ├── Index.cshtml
│ │ ├── Mark.cshtml
│ │ ├── SkipSimulator.cshtml
│ │ └── Criteria.cshtml
│ ├── Analytics/
│ │ └── Index.cshtml
│ ├── Timetable/
│ │ └── Index.cshtml
│ ├── Calendar/
│ │ └── Index.cshtml
│ ├── Planner/
│ │ ├── Index.cshtml
│ │ └── Simulate.cshtml
│ ├── Connect/
│ │ ├── Index.cshtml
│ │ └── Connections.cshtml
│ ├── Messages/
│ │ ├── Index.cshtml
│ │ └── Chat.cshtml
│ ├── Premium/
│ │ └── Index.cshtml
│ ├── Profile/
│ │ ├── Index.cshtml
│ │ └── Edit.cshtml
│ └── Admin/
│ ├── Index.cshtml
│ ├── ManageHolidays.cshtml
│ └── ManageReports.cshtml
├── wwwroot/
│ ├── css/
│ │ └── site.css
│ ├── js/
│ │ └── site.js
│ └── images/
│ └── avatars/
├── Program.cs
├── Attendly.csproj
├── appsettings.json
├── appsettings.Development.json
└── .env.example
```

---

## Architecture

### Clean Architecture Principles

The application follows clean architecture with clear separation of concerns:

```
┌─────────────────────────────────────────────┐
│ Presentation Layer (Views / Controllers) │
├─────────────────────────────────────────────┤
│ Application Layer (Services / ViewModels) │
├─────────────────────────────────────────────┤
│ Data Layer (SupabaseService / Models) │
├─────────────────────────────────────────────┤
│ External (Supabase / Razorpay API) │
└─────────────────────────────────────────────┘
```

### Data Flow

1. **Controllers** handle HTTP requests and return views
2. **Services** contain business logic (attendance calculations, analytics, planning)
3. **SupabaseService** handles all database operations via Supabase client
4. **ViewModels** shape data for views
5. **Models** represent database entities

---

## Key Business Logic

### Attendance Calculation Engine

```csharp
Attendance% = Present Lectures / Total Conducted Lectures × 100

Classes Needed for Target = ceil((Present × 100 - Target × Total) / Target)
Safe Absences = floor(Present × 100 / Target - Total)
```

### Smart Holiday Planning

The planner analyzes:
- Upcoming holidays
- Student timetable
- Current attendance per subject
- Target attendance percentage
- Weekend opportunities

Then calculates:
- Lectures missed if holiday is taken
- Projected attendance after the holiday
- Whether the student stays above their target

### Attendance Simulator

Simulates N upcoming lectures with X absences:
```
Projected = (Present + (N - X)) / (Total + N) × 100
```

---

## Razorpay Integration

### Payment Flow

1. Frontend calls `POST /Premium/CreateOrder` with plan type
2. Backend creates Razorpay order server-side
3. Razorpay Checkout opens for payment
4. Razorpay returns payment signature
5. Frontend calls `POST /Premium/VerifyPayment` with order details
6. Backend verifies signature using HMAC SHA256
7. On success, subscription is created and user upgraded

### Security

- Razorpay keys are **never** exposed in frontend code
- Order creation happens server-side
- Payment signatures verified server-side using HMAC SHA256
- Subscription state stored in database

---

## Security Features

- ASP.NET Authentication with secure password handling (BCrypt)
- Anti-forgery tokens on all forms
- Server-side input validation
- Row-level security via Supabase
- Authorization checks on all protected routes
- No secrets in frontend code
- Environment variables for all sensitive configuration
- Secure HTTP headers (HSTS in production)

---

## Environment Variables

| Variable | Description |
|----------|-------------|
| `SUPABASE_URL` | Your Supabase project URL |
| `SUPABASE_ANON_KEY` | Supabase anonymous/public key |
| `SUPABASE_SERVICE_ROLE_KEY` | Supabase service role key (server-side only) |
| `RAZORPAY_KEY_ID` | Razorpay key ID |
| `RAZORPAY_KEY_SECRET` | Razorpay key secret (server-side only) |
| `ASPNETCORE_ENVIRONMENT` | Environment (Development/Production) |

---

## Development

### Running with Hot Reload

```bash
dotnet watch run
```

### Adding a New Feature

1. Add Model in `Models/`
2. Add ViewModel in `ViewModels/`
3. Add service interface and implementation in `Services/`
4. Add Supabase methods in `Data/SupabaseService.cs`
5. Add controller action in appropriate `Controllers/` file
6. Create Razor view in `Views/[Controller]/`

---

## Production Deployment

1. Set `ASPNETCORE_ENVIRONMENT=Production`
2. Configure production Supabase keys
3. Configure production Razorpay keys
4. Enable HTTPS
5. Set up proper CORS policies
6. Use environment variables for all secrets
7. Enable Razorpay webhook for subscription status updates

---

## Screenshots

### Landing Page
Hero section with attendance preview and feature cards.

### Dashboard
Personalized student command center with:
- Overall attendance circle
- Today's schedule
- Subject cards with status
- Quick actions
- Upcoming holidays
- Weekend opportunities

### Attendance Tracking
- Mark attendance for today's classes
- View attendance history
- "Can I Skip?" simulator with visual results

### Holiday Planner
- Extended weekend opportunities
- Attendance impact analysis
- Trip simulator with projected attendance

### Connect
- Western Line / Central Line student directories
- Search and filter students
- Connection requests

### Premium
- Free vs Premium comparison
- Razorpay payment integration

---

## License

This project is built for educational purposes as a college project demonstrating ASP.NET Core MVC with Supabase integration.

---

## Author

Built with Claude Code by Anthropic.
# Attendly
