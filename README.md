# Task Management App (.NET + Angular)

This project contains:
- `taskapp-service/`: ASP.NET Core Web API (.NET 9) with SQL Server, cookie authentication, and task CRUD endpoints.
- `taskapp-client/`: Angular app with login, side-by-side task list/editor, and sorting/filtering.

## Prerequisites

- .NET SDK 9.x
- Node.js 22+ and npm
- SQL Server or SQL Server Express

## Backend Setup

1. Update the SQL Server connection string in:
   - `taskapp-service/appsettings.json`
   - `taskapp-service/appsettings.Development.json`
2. From `taskapp-service/`, apply migrations:
   - `dotnet dotnet-ef database update`
3. Run the API:
   - `dotnet run`

Default development URL from launch settings:
- `https://localhost:7051`

Seeded user:
- Username: `admin`
- Password: `password123`

## Frontend Setup

1. In `taskapp-client/src/environments/environment.ts`, keep or adjust API URL:
   - `https://localhost:7051/api`
2. From `taskapp-client/`, install dependencies:
   - `npm install`
3. Run Angular app:
   - `npm start`

Default frontend URL:
- `http://localhost:4200`

## API Endpoints

Authentication:
- `POST /api/auth/login`
- `POST /api/auth/logout`
- `GET /api/auth/me`

Tasks (authenticated):
- `GET /api/tasks` (supports query parameters: `search`, `status`, `priority`, `sortBy`, `sortOrder`, `page`, `pageSize`)
- `GET /api/tasks/{id}`
- `POST /api/tasks`
- `PUT /api/tasks/{id}`
- `PATCH /api/tasks/{id}/complete`
- `DELETE /api/tasks/{id}`

## Verification Checklist

- Login succeeds with seeded user credentials.
- Task list loads after login.
- Add/Edit/Delete task works.
- Mark Complete updates task status.
- Sorting and filtering in the list panel update results.
