# Tasks Management App

A full-stack task management application built with a .NET 10 microservices backend and an Angular 18 frontend.

## Architecture

```
┌─────────────┐     HTTP      ┌──────────────────────────────────────────┐
│  Angular 18 │ ──────────── ▶│         Ocelot API Gateway :5000         │
│   :4200     │               └───────────────┬──────────────────────────┘
└─────────────┘                               │  routes + JWT auth + rate limiting
                                              │
                         ┌────────────────────┼────────────────────┐
                         ▼                                         ▼
              ┌──────────────────┐                    ┌──────────────────┐
              │   AuthService    │  gRPC user check   │   TaskService    │
              │  REST  :5100     │ ◀─────────────────▶│  REST  :5200     │
              │  gRPC  :5101     │                    │  gRPC  :5201     │
              └────────┬─────────┘                    └────────┬─────────┘
                       │                                       │
                  MySQL auth_db                          MySQL tasks_db
```

### Services

| Service | Port | Responsibilities |
|---|---|---|
| **Gateway** | 5000 | Request routing, JWT validation, `X-User-Id` header injection, IP rate limiting |
| **AuthService** | 5100 / 5101 | Registration, login, JWT issuance, gRPC user validation |
| **TaskService** | 5200 / 5201 | Tasks and categories CRUD, user isolation |
| **Frontend** | 4200 | Angular SPA with NG-Zorro UI |

### Key design decisions

- **No JWT validation in TaskService** — the Gateway validates the token and forwards the authenticated user's ID via the `X-User-Id` header, removing duplicated auth logic downstream.
- **gRPC for inter-service calls** — TaskService validates users against AuthService over a dedicated HTTP/2 gRPC port.
- **CQRS via MediatR** — commands and queries are handled separately in both backend services.
- **Clean Architecture** — `Domain / Application / Infrastructure / Endpoints` layering in each service.

## Tech Stack

**Backend**
- .NET 10, C#
- FastEndpoints (minimal API style)
- MediatR (CQRS)
- Entity Framework Core 9 + Pomelo MySQL driver
- Grpc.AspNetCore (protobuf)
- Ocelot API Gateway
- BCrypt.Net-Next (password hashing)
- DotNetEnv (`.env` loading)

**Frontend**
- Angular 18 (standalone components)
- NG-Zorro Ant Design 18
- RxJS

**Database**
- MySQL 8 (two separate databases: `auth_db`, `tasks_db`)

## Features

- **Authentication** — register, login, JWT-based sessions, `/me` endpoint
- **Tasks** — create, read, update, delete with server-side pagination and filtering
  - Status: `Todo` · `In Progress` · `Done` · `Cancelled`
  - Priority: `Low` · `Medium` · `High` · `Critical`
  - Due date with overdue highlighting
  - Free-form tags
  - Optional category assignment
- **Categories** — create, rename, delete; scoped per user
- **Rate limiting** — 10 requests/minute per IP on `/api/auth/login` and `/api/auth/register`
- **Swagger UI** — available on each service (`/swagger`)

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) and Angular CLI (`npm install -g @angular/cli`)
- MySQL 8 running locally

## Getting Started

### 1. Environment

```bash
cp .env.example .env
```

Edit `.env` and fill in your secrets:

```env
# JWT (shared by all services)
Jwt__Secret=your-super-secret-key-at-least-32-chars
Jwt__Issuer=TasksApp.AuthService
Jwt__Audience=TasksApp
Jwt__ExpiryHours=1

# AuthService database
ConnectionStrings__AuthDb=Server=localhost;Port=3306;Database=auth_db;Uid=root;Pwd=your-password;

# TaskService database
ConnectionStrings__TasksDb=Server=localhost;Port=3306;Database=tasks_db;Uid=root;Pwd=your-password;
AuthService__GrpcAddress=http://localhost:5101
```

### 2. Database migrations

```bash
dotnet ef database update --project Backend/src/AuthService
dotnet ef database update --project Backend/src/TaskService
```

### 3. Frontend dependencies

```bash
cd Frontend && npm install
```

### 4. Run everything

```bash
./run-all.sh
```

This starts all four processes concurrently and kills them all on `Ctrl+C`.

| URL | What |
|---|---|
| `http://localhost:4200` | Angular frontend |
| `http://localhost:5000` | API Gateway |
| `http://localhost:5100/swagger` | AuthService Swagger |
| `http://localhost:5200/swagger` | TaskService Swagger |

## API Reference

All routes go through the Gateway at `:5000`. Authenticated routes require `Authorization: Bearer <token>`.

### Auth

| Method | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/api/auth/register` | — | Create account |
| `POST` | `/api/auth/login` | — | Get JWT token |
| `GET` | `/api/auth/me` | Required | Current user profile |

### Tasks

| Method | Path | Description |
|---|---|---|
| `GET` | `/api/tasks` | List tasks (paginated, filterable) |
| `POST` | `/api/tasks` | Create task |
| `GET` | `/api/tasks/{id}` | Get task by ID |
| `PUT` | `/api/tasks/{id}` | Update task |
| `DELETE` | `/api/tasks/{id}` | Delete task |

Query params for `GET /api/tasks`: `page`, `pageSize`, `status`, `priority`, `categoryId`, `search`.

### Categories

| Method | Path | Description |
|---|---|---|
| `GET` | `/api/categories` | List categories |
| `POST` | `/api/categories` | Create category |
| `PUT` | `/api/categories/{id}` | Rename category |
| `DELETE` | `/api/categories/{id}` | Delete category |

## Project Structure

```
see-sharp/
├── Backend/
│   └── src/
│       ├── AuthService/
│       │   ├── Application/        # MediatR commands, queries, DTOs
│       │   ├── Domain/             # User entity
│       │   ├── Endpoints/          # FastEndpoints
│       │   └── Infrastructure/     # EF Core, JWT, gRPC server
│       ├── Gateway/                # Ocelot config (ocelot.json)
│       └── TaskService/
│           ├── Application/        # MediatR commands, queries, DTOs
│           ├── Domain/             # TaskItem, Category, enums
│           ├── Endpoints/          # FastEndpoints
│           └── Infrastructure/     # EF Core, gRPC client
│   └── tests/
│       ├── AuthService.UnitTests
│       ├── AuthService.IntegrationTests
│       ├── TaskService.UnitTests
│       └── TaskService.IntegrationTests
├── Frontend/
│   └── src/app/
│       ├── auth/                   # Login, register, guard, interceptor
│       ├── tasks/                  # Task list, task service, category service
│       └── shared/models/          # Shared TypeScript interfaces
├── .env.example
├── run-all.sh
└── run-all-debug.sh
```

## Running Tests

```bash
dotnet test Backend/tests/AuthService.UnitTests
dotnet test Backend/tests/AuthService.IntegrationTests
dotnet test Backend/tests/TaskService.UnitTests
dotnet test Backend/tests/TaskService.IntegrationTests
```
