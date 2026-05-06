# Auth-JWT — Full Stack App

**.NET 10 Minimal API** + **React 19** + **TypeScript** + **Tailwind CSS v4** + **ShadCN UI**

## Stack

| Layer | Tech |
|---|---|
| Backend | .NET 10 Minimal API, EF Core, SQLite |
| Auth | JWT Bearer (BCrypt password hashing) |
| Architecture | Repository + Service pattern |
| Frontend | React 19, TypeScript, Vite |
| UI | Tailwind CSS v4, ShadCN UI (Radix) |
| HTTP Client | Axios |

## Running the project

### Backend

```bash
cd backend/AuthJWT.Api
dotnet run
```

API available at `http://localhost:5000`
Scalar API docs at `http://localhost:5000/scalar/v1`

### Frontend

```bash
cd frontend
npm run dev
```

App available at `http://localhost:5173`

## API Endpoints

### Auth
| Method | Endpoint | Auth |
|---|---|---|
| POST | /api/auth/register | No |
| POST | /api/auth/login | No |

### Todos (all require JWT)
| Method | Endpoint | Description |
|---|---|---|
| GET | /api/todos | List user todos |
| GET | /api/todos/{id} | Get todo by id |
| POST | /api/todos | Create todo |
| PUT | /api/todos/{id} | Update todo |
| DELETE | /api/todos/{id} | Delete todo |

## Architecture

```
backend/AuthJWT.Api/
├── Data/          # DbContext (EF Core + SQLite)
├── Models/        # User, TodoItem
├── DTOs/          # Request/Response records
├── Repositories/  # Data access layer (interfaces + implementations)
├── Services/      # Business logic (interfaces + implementations)
└── Endpoints/     # Minimal API endpoint mapping

frontend/src/
├── components/    # ProtectedRoute, ShadCN UI components
├── contexts/      # AuthContext (JWT storage, login/logout)
├── pages/         # LoginPage, RegisterPage, DashboardPage
├── services/      # api.ts (axios), auth.service.ts, todo.service.ts
└── types/         # Shared TypeScript interfaces
```
