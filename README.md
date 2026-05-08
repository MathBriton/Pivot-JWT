# NexusDesk

Plataforma de CRM e gerenciamento de workflow orientada a empresas. Projeto de portfolio que demonstra arquitetura enterprise real com .NET 10 Minimal API e React.

## Stack

| Camada | Tecnologia |
|---|---|
| Backend | .NET 10 Minimal API, EF Core, PostgreSQL |
| Autenticação | JWT Bearer, BCrypt |
| Arquitetura | Monólito modular — Repository + Service pattern |
| Validação | FluentValidation |
| Testes | xUnit, FluentAssertions, Moq |
| Frontend | React 19, TypeScript, Vite |
| UI | Tailwind CSS v4, ShadCN UI (Radix) |
| Cliente HTTP | Axios, TanStack Query |

## Módulos

| Módulo | Status |
|---|---|
| Auth & Autorização | ✅ Implementado |
| Gerenciamento de Clientes | 🔜 Em breve |
| Tarefas & Workflow | 🔜 Em breve |
| Logs de Auditoria | 🔜 Em breve |
| Dashboard | 🔜 Em breve |

## Como executar

### Backend

```bash
cd backend/AuthJWT.Api
dotnet run
```

API disponível em `http://localhost:5000`  
Documentação Scalar em `http://localhost:5000/scalar/v1`

### Frontend

```bash
cd frontend
npm install
npm run dev
```

App disponível em `http://localhost:5173`

## Endpoints

### Auth

| Método | Endpoint | Autenticação |
|---|---|---|
| POST | /api/auth/register | Não |
| POST | /api/auth/login | Não |

### Padrão de resposta

```json
{
  "success": true,
  "message": "Operação realizada com sucesso.",
  "data": {}
}
```

## Arquitetura

```
backend/AuthJWT.Api/
├── Data/          # DbContext (EF Core)
├── Domain/        # Entidades e regras de negócio
├── DTOs/          # Records de Request/Response
├── Endpoints/     # Mapeamento de endpoints Minimal API
├── Repositories/  # Acesso a dados (interfaces + implementações)
├── Services/      # Lógica de negócio (interfaces + implementações)
├── Shared/        # ApiResponse<T> — wrapper de resposta padrão
└── Validators/    # FluentValidation (RegisterRequest, LoginRequest)

frontend/src/
├── components/    # ProtectedRoute, componentes ShadCN UI
├── contexts/      # AuthContext (JWT, login/logout)
├── pages/         # LoginPage, RegisterPage, DashboardPage
├── services/      # api.ts (axios), auth.service.ts
└── types/         # Interfaces TypeScript compartilhadas
```

## Princípios de engenharia

- **TDD** — testes escritos antes da implementação (Red → Green → Refactor)
- **XP** — iterações pequenas, design simples, feedback rápido
- **SOLID** — cada classe tem responsabilidade única e bem definida
- **Clean Code** — nomenclatura explícita, funções de 4–20 linhas
