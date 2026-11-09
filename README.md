# NeoRoute / RaceDay – Final Submission

ASP.NET Core 8 solution consisting of a RESTful Web API and an MVC front-end for managing running/walking/cycling events (RaceDay theme).

## Architecture

```
Browser → NeoRoute.Web (MVC) → NeoRoute.API → SQL Server / Azure Blob Storage
```

**Important rule observed:** The MVC application never talks directly to Azure Blob Storage. All file uploads go through `POST /api/files/upload` on the API.

### Projects
| Project | Description |
|---------|-------------|
| **NeoRoute.API** | REST API – Auth, Events, Enrolments, Results, Profile, Categories, Files |
| **NeoRoute.Web** | MVC front-end with role-based navigation and RaceDay theme |
| **NeoRoute.API.Tests** | Unit / integration tests for auth |

## Technologies
- ASP.NET Core 8 (MVC + Web API)
- Entity Framework Core 8 + SQL Server
- Cookie authentication + role-based authorization (Organiser / Participant)
- Azure.Storage.Blobs (via API only)
- BCrypt for password hashing
- Bootstrap 5 + Bootstrap Icons
- Docker (multi-stage build for the MVC app)

## Features Completed
- Registration & Login (cookie session)
- Role-based navigation and authorization
- Organiser: create / list events (with optional banner image)
- Participant: browse events, enrol, view own enrolments & results
- Profile view & update (including profile picture upload)
- Enrolment status management (Pending / Confirmed / Cancelled / Waitlisted) with colour badges
- Azure Blob Storage integration **through the API**
- Secure file validation (type + 5 MB size limit)
- Docker support for the MVC application

## Prerequisites
- .NET 8 SDK
- SQL Server / LocalDB (or SQL Server in Docker)
- Optional: Azure Storage Account for blob images

## Configuration

### API (`NeoRoute.API/appsettings.json`)
```json
{
  "ConnectionStrings": {
    "RaceDayDB": "Server=...;Database=RaceDayDB;..."
  },
  "AzureBlobStorage": {
    "ConnectionString": "<your Azure Storage connection string>",
    "ContainerName": "event-images"
  }
}
```

### Web (`NeoRoute.Web/appsettings.json`)
```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:5000"
  }
}
```

Never commit real secrets. Use User Secrets, environment variables or Azure Key Vault in production.

## Run Locally

### 1. Start the API
```bash
cd NeoRoute.API
dotnet run
```
API listens on **http://localhost:5000** (Swagger: http://localhost:5000/swagger)

The database is auto-created on first run (`EnsureCreated`).

### 2. Start the MVC app
```bash
cd NeoRoute.Web
dotnet run
```
Open the URL shown (typically http://localhost:5298 or similar).

## Docker (MVC)

From the solution root (`NeoRoute-Part3`):

```bash
docker build -t neo-route .
docker run -d -p 8080:8080 \
  -e ApiSettings__BaseUrl=http://host.docker.internal:5000 \
  --name neo-route neo-route
```

Open **http://localhost:8080**

> The API must be running on the host (or another container reachable from the Web container). Adjust `ApiSettings__BaseUrl` accordingly.

## Environment Variables
| Variable | Used by | Purpose |
|----------|---------|---------|
| `ConnectionStrings__RaceDayDB` | API | SQL Server connection |
| `AzureBlobStorage__ConnectionString` | API | Azure Storage |
| `AzureBlobStorage__ContainerName` | API | Blob container name |
| `ApiSettings__BaseUrl` | Web | Base URL of the API |
| `ASPNETCORE_URLS` | both | Listening URLs |

## Testing Checklist
1. Application starts successfully
2. Registration works (Organiser & Participant)
3. Login works and sets role in session
4. Role-based navigation is correct
5. Profile can be updated
6. Profile picture upload works (via API → Blob)
7. Organiser can create events with banner
8. Event list shows banner images
9. Participants can enrol
10. Enrolment statuses show with colour (green=Confirmed, amber=Pending, etc.)
11. Docker image builds and runs
12. Invalid file types / oversized files are rejected

## Project Structure
```
NeoRoute-Part3/
├── Dockerfile
├── NeoRoute.sln
├── README.md
├── NeoRoute.API/
│   ├── Controllers/
│   ├── DTOs/
│   ├── Data/
│   ├── Models/
│   ├── Services/AzureBlobService.cs
│   └── Program.cs
└── NeoRoute.Web/
    ├── Controllers/
    ├── Services/ApiService.cs
    ├── Views/
    └── wwwroot/css/site.css
```

## Notes
- Enrolment statuses: **Confirmed** (green), **Pending** (amber/orange), **Cancelled** (red), **Waitlisted** (grey).
- All image operations follow: MVC → API → Azure Blob Storage.
- Cookie name used by the API is `NeoRoute.Auth`.
