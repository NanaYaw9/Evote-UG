# E-Vote UG — Student Election Management Platform

E-Vote UG is a web-based platform for running secure, transparent student representative elections at the University of Ghana. Students can register, log in, browse elections, view candidates, and cast votes online. Administrators can create and manage elections, positions, and candidates, and monitor results in real time.

**Live demo:** https://evote-ug.netlify.app
**API:** https://evote-ug.onrender.com/swagger

---

## Features

- Student registration and login with JWT authentication
- Admin registration and login, separate from student accounts
- Create, edit, activate, conclude, and delete elections
- Positions and candidates tied to each election
- One-vote-per-position enforcement, including server-side protection against double voting
- Election status lifecycle (Draft → Active → Concluded) that controls when voting is allowed
- Live results with visual vote-count bars
- Admin dashboard for managing all elections in one place

---

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Blazor WebAssembly (.NET 8) |
| Backend API | ASP.NET Core Web API (.NET 8) |
| Database | PostgreSQL, hosted on Supabase |
| ORM | Entity Framework Core |
| Authentication | JWT (JSON Web Tokens), BCrypt password hashing |
| API Documentation | Swagger / OpenAPI |
| Frontend Hosting | Netlify |
| Backend Hosting | Render (Docker) |

---

## Project Structure

```
Evote-UG/
├── EVoteUG.Client/          # Blazor WebAssembly frontend
│   ├── Pages/                 # Razor pages (Login, Elections, Admin Dashboard, etc.)
│   ├── Layout/                 # Shared layout and navigation
│   ├── Services/                # HTTP service classes that call the API
│   ├── Shared/                  # Reusable components (e.g. AdminGuard)
│   └── wwwroot/                 # Static assets, including css/app.css
│
├── EVoteUG.Api/              # ASP.NET Core Web API
│   ├── Controllers/            # HTTP endpoints (Elections, Students, Admins, Votes, Auth...)
│   ├── Middleware/              # Global exception handling
│   ├── Extensions/              # Service/auth/Swagger configuration helpers
│   └── Program.cs                # App startup, CORS, JWT, database config
│
├── EVoteUG.Core/              # Shared contracts used by the API
│   ├── DTOs/                     # Request/response data shapes
│   ├── Interfaces/                 # Service interfaces
│   ├── Validators/                  # Input validation rules
│   └── Exceptions/                   # Custom exception types
│
├── EVoteUG.Infrastructure/   # Business logic and data access
│   ├── Services/                 # Service implementations (ElectionService, VotingService, etc.)
│   ├── Data/                      # EVoteUGDbContext, DbInitializer, Migrations
│   └── Security/                   # JWT token generation, password hashing
│
├── EVoteUG.Shared/            # Models and response wrappers shared across projects
│   ├── Models/                    # Election, Position, Candidate, Student, Admin, Vote, etc.
│   ├── Enums/                      # ElectionStatus, ElectionScope, UserRole, etc.
│   └── Responses/                    # ApiResponse<T> wrapper
│
├── EVoteUG.Tests/              # Unit tests
├── Dockerfile                    # Used for deploying the API to Render
└── EVoteUG.sln                     # Solution file
```

---

## Architecture

The application is split into a separate frontend and backend that communicate only over HTTP:

```
Browser → Blazor Client → ASP.NET Core API → Infrastructure (Services) → PostgreSQL Database
```

The Client never accesses the database directly. All data access goes through the API, which validates requests, enforces business rules (like blocking a second vote on the same position), and talks to the database through Entity Framework Core.

---

## Running Locally

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A PostgreSQL database (e.g. a free [Supabase](https://supabase.com) project)

### 1. Clone the repository

```bash
git clone https://github.com/NanaYaw9/Evote-UG.git
cd Evote-UG
```

### 2. Configure the database connection

In `EVoteUG.Api/appsettings.json`, set your PostgreSQL connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=YOUR_HOST;Port=5432;Database=postgres;Username=YOUR_USERNAME;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
}
```

Also set your JWT secret and initial admin credentials in the same file under `JwtSettings` and `InitialAdmin`.

### 3. Run the API

```bash
dotnet run --project EVoteUG.Api
```

This automatically applies database migrations and seeds an initial admin account on first run. The API will be available at `http://localhost:5059`, with Swagger docs at `http://localhost:5059/swagger`.

### 4. Run the Client

In a separate terminal:

```bash
dotnet run --project EVoteUG.Client
```

The app will be available at `http://localhost:5043`.

---

## Team

| Name | Student ID | Role |
|---|---|---|
| Amponsah Yaw Nana | 22181957 | Group Leader, Authentication & Security |
| Abena Twumwaa Ankobea | 22032277 | Database Design / Administration |
| Apeani Ann Timah | 22121034 | UI/UX Design |
| Arhin Ellen Yaayaa | 22040658 | Testing & Quality Assurance |
| Davis Benjamin Ato | 22046566 | System Analyst |
| Ejeh Joy Ave | 11358724 | Frontend Developer |
| Walid Wyacliff Mahama | 22233231 | Database Manager |
| Firdaus Maltiti Musah | 22021182 | Documentation Coordinator |
| Juliet Adoma Minta | 22058472 | Presentation & Demo Coordinator |
| Mabel Awuku Addae | 22014655 | Frontend Developer |
| Stephen Edem Kwame Doelawson | 22045257 | Backend Developer |


---

## License

This project was built as a semester project for the University of Ghana and is not licensed for external commercial use.
