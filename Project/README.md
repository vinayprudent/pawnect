# PawNect - Clean Architecture Prototype

## Project Overview

PawNect is a comprehensive pet management system built using **Clean Architecture** principles with .NET 10. The application consists of multiple layers that ensure separation of concerns, maintainability, and scalability.

## Technology Stack

- **.NET 10** - Latest .NET framework
- **ASP.NET Web API** - For RESTful API services
- **ASP.NET MVC** - For frontend user interfaces
- **Entity Framework Core 10** - ORM for database operations
- **MS SQL Server** - Relational database
- **Swagger** - API documentation

## Project Structure

```
PawNect.sln
│
├── Backend
│   ├── PawNect.Domain
│   │   ├── Entities (Pet, User, MedicalRecord, Appointment)
│   │   ├── Enums (PetSpecies, PetStatus, UserRole)
│   │   └── Rules (Business validation rules)
│   │
│   ├── PawNect.Application
│   │   ├── Services (PetService, UserService)
│   │   ├── Interfaces (IRepository, IPetService, IUserService, etc.)
│   │   └── DTOs (CreatePetDto, UpdatePetDto, UserDto, etc.)
│   │
│   ├── PawNect.Infrastructure
│   │   ├── DbContext (PawNectDbContext)
│   │   ├── Repositories (Repository, PetRepository, UserRepository, etc.)
│   │   └── Migrations (Database migrations)
│   │
│   └── PawNect.API
│       ├── Controllers (PetsController, UsersController)
│       ├── Auth (Authentication & Authorization)
│       ├── Middleware (Exception Handling, etc.)
│       └── Program.cs (Configuration)
│
├── Frontend
│   ├── PawNect.PetParent.Web (Pet Parent MVC)
│   │   ├── Controllers (HomeController, PetsController)
│   │   ├── Views
│   │   ├── Models
│   │   └── wwwroot
│   │
│   └── PawNect.AdminPortal.Web (Admin MVC)
│       ├── Controllers (HomeController, AdminController)
│       ├── Views
│       ├── Models
│       └── wwwroot
│
└── Database
    └── MS SQL Server
```

## Architecture Layers

### 1. **Domain Layer** (`PawNect.Domain`)
The core of the application containing:
- **Entities**: BaseEntity, User, Pet, MedicalRecord, Appointment
- **Enums**: PetSpecies, PetStatus, UserRole
- **Rules**: Business logic validation (PetRules, UserRules)

**Dependencies**: None (No external dependencies)

### 2. **Application Layer** (`PawNect.Application`)
Bridges domain and infrastructure:
- **DTOs**: Data Transfer Objects for API communication
- **Services**: Business logic implementation
- **Interfaces**: Repository and Service contracts

**Dependencies**: Domain layer

### 3. **Infrastructure Layer** (`PawNect.Infrastructure`)
Data access and external services:
- **DbContext**: Entity Framework Core configuration
- **Repositories**: Data access implementations
- **Migrations**: Database schema management

**Dependencies**: Domain, Application layers

### 4. **API Layer** (`PawNect.API`)
REST API endpoints:
- **Controllers**: REST endpoints (PetsController, UsersController)
- **Middleware**: Request/response processing
- **Auth**: Authentication & authorization logic
- **Configuration**: Dependency injection setup

**Dependencies**: All layers

### 5. **Frontend Layers** (MVC Applications)
User interfaces:
- **PawNect.PetParent.Web**: Pet owner interface
- **PawNect.AdminPortal.Web**: Administrator interface

**Dependencies**: API layer (via HTTP calls)

## Data Flow - Pet Creation Example

```
1. Pet Parent Frontend (MVC)
   ↓ POST /api/pets
2. PetsController (API)
   ↓ IPetService.CreatePetAsync()
3. PetService (Application)
   ↓ Validate with PetRules
   ↓ IPetRepository.AddAsync()
4. PetRepository (Infrastructure)
   ↓ DbContext.Pets.AddAsync()
5. Pet Entity (Domain)
   ↓ EF Core Maps to Database
6. MS SQL Database
   ↓ Response back through layers
7. JSON Response to Frontend
```

## Key Features Implemented

### Domain Entities
- **User**: Pet owners, veterinarians, trainers, groomers, admins
- **Pet**: Pet information with species, status, and health tracking
- **MedicalRecord**: Vaccination records, surgeries, checkups
- **Appointment**: Vet visits, training sessions, grooming appointments

### Business Rules
- Pet name validation (max 100 characters)
- Pet weight validation (0.1 - 200 kg)
- Date of birth validation
- User password minimum length (8 characters)
- Email uniqueness validation

### API Endpoints

#### Users
- `POST /api/users/register` - Register new user
- `POST /api/users/login` - User login
- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

#### Pets
- `GET /api/pets` - Get all pets
- `GET /api/pets/{id}` - Get pet by ID
- `GET /api/pets/owner/{ownerId}` - Get user's pets
- `POST /api/pets` - Create new pet
- `PUT /api/pets/{id}` - Update pet
- `DELETE /api/pets/{id}` - Delete pet

## Database Configuration

### Connection String
Located in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=PawNect;Trusted_Connection=True;Encrypt=false;"
  }
}
```

### Model Configurations
- User-Pet: One-to-Many (One user can own multiple pets)
- Pet-MedicalRecord: One-to-Many
- Pet-Appointment: One-to-Many
- Soft deletes implemented (IsDeleted flag)

### Database migrations
Use the migration script from the **solution root** (same folder as `Run-PawNect.ps1`):

| Action | Command |
|--------|--------|
| Apply pending migrations | `.\Run-Migrations.ps1` or `Run-Migrations.cmd` |
| List migrations | `.\Run-Migrations.ps1 -List` |
| Add a new migration | `.\Run-Migrations.ps1 -Add "MigrationName"` |
| Generate SQL script (no apply) | `.\Run-Migrations.ps1 -Script` |

The script uses `Backend\PawNect.API` for the connection string and `Backend\PawNect.Infrastructure` for migrations.

## Setup Instructions

### Prerequisites
- .NET 10 SDK
- MS SQL Server
- Visual Studio 2022 or VS Code

### Steps

1. **Clone Repository**
```bash
cd d:\Projects\PawNect\Project
```

2. **Apply database migrations**
From the solution root:
```powershell
.\Run-Migrations.ps1
```
Or from cmd: `Run-Migrations.cmd`
See **Database migrations** below for more options (add migration, generate SQL script).

3. **Build Solution**
```bash
dotnet build
```

4. **Run API**
```bash
cd Backend/PawNect.API
dotnet run
```
API runs at: `https://localhost:5001` or `http://localhost:5000`

5. **Run Pet Parent Web**
```bash
cd Frontend/PawNect.PetParent.Web
dotnet run
```

6. **Run Admin Portal**
```bash
cd Frontend/PawNect.AdminPortal.Web
dotnet run
```

## Clean Architecture Principles

### Dependency Direction
- Dependencies flow inward (from outer layers to inner)
- Domain layer has no dependencies
- Application layer depends on Domain
- Infrastructure layer depends on Domain and Application
- API layer depends on all layers

### Separation of Concerns
- **Domain**: Business entities and rules
- **Application**: Business logic and orchestration
- **Infrastructure**: Data access and external integrations
- **API**: HTTP endpoints and request handling
- **Frontend**: User interfaces

### Testability
- Interfaces enable dependency injection
- Business logic separated from data access
- DTOs decouple API from domain models
- Easy to mock dependencies for testing

## Future Enhancements

- [ ] JWT authentication
- [ ] Role-based authorization
- [ ] Appointment reminders
- [ ] Medical record document storage
- [ ] Pet health tracking dashboard
- [ ] Notification system
- [ ] Payment integration
- [ ] Mobile application
- [ ] Real-time updates with SignalR
- [ ] Comprehensive logging and monitoring

## API Documentation

Swagger UI is available at: `http://localhost:5000/swagger/index.html`

## Contributing

When adding new features:
1. Add entities to Domain layer
2. Create DTOs in Application layer
3. Implement repository in Infrastructure layer
4. Create service in Application layer
5. Add controller endpoint in API layer
6. Update database migrations

## License

Proprietary - PawNect 2026
