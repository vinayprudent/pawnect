# Architecture Documentation

## Clean Architecture Overview

PawNect follows the Clean Architecture pattern as described by Robert C. Martin (Uncle Bob). This architecture ensures:

1. **Testability**: Business logic is testable without external dependencies
2. **Maintainability**: Code is organized and easy to navigate
3. **Flexibility**: Easy to add new features or change implementations
4. **Independence**: External frameworks are not tightly coupled

## Layer Responsibilities

### Domain Layer (`PawNect.Domain`)

**Responsibility**: Define core business entities and rules

**Contains**:
- `Entities/`: Core domain objects
  - `BaseEntity.cs`: Base class with common properties
  - `User.cs`: User entity
  - `Pet.cs`: Pet entity
  - `MedicalRecord.cs`: Medical records
  - `Appointment.cs`: Appointments

- `Enums/`: Business enumerations
  - `PetSpecies.cs`: Pet types
  - `PetStatus.cs`: Pet status values
  - `UserRole.cs`: User roles

- `Rules/`: Business logic validation
  - `PetRules.cs`: Pet validation rules
  - `UserRules.cs`: User validation rules

**Key Principle**: Domain layer must be independent. No references to external libraries except what's needed for the types themselves.

### Application Layer (`PawNect.Application`)

**Responsibility**: Orchestrate business logic and manage use cases

**Contains**:
- `DTOs/`: Data Transfer Objects
  - `Pet/`: Pet DTOs (CreatePetDto, UpdatePetDto, PetDto)
  - `User/`: User DTOs (RegisterUserDto, LoginUserDto, UserDto)
  - `ApiResponse.cs`: Standard response wrapper

- `Interfaces/`: Contracts
  - `IRepository<T>`: Generic repository interface
  - `IPetRepository.cs`: Pet-specific repository
  - `IUserRepository.cs`: User-specific repository
  - `IPetService.cs`: Pet service interface
  - `IUserService.cs`: User service interface

- `Services/`: Use case implementations
  - `PetService.cs`: Pet business logic
  - `UserService.cs`: User business logic

**Key Principle**: Services orchestrate business logic and use repositories to access data. DTOs provide a boundary between external world and domain entities.

### Infrastructure Layer (`PawNect.Infrastructure`)

**Responsibility**: Implement data persistence and external integrations

**Contains**:
- `DbContext/`:
  - `PawNectDbContext.cs`: Entity Framework Core configuration

- `Repositories/`:
  - `Repository<T>.cs`: Generic CRUD repository
  - `PetRepository.cs`: Pet-specific queries
  - `UserRepository.cs`: User-specific queries
  - `MedicalRecordRepository.cs`: Medical records queries
  - `AppointmentRepository.cs`: Appointments queries

- `Migrations/`: Database schema versions

**Key Principle**: Infrastructure implements the interfaces defined in Application layer. All data access goes through repositories.

### API Layer (`PawNect.API`)

**Responsibility**: Handle HTTP requests and responses

**Contains**:
- `Controllers/`:
  - `PetsController.cs`: Pet endpoints
  - `UsersController.cs`: User endpoints

- `Auth/`: Authentication logic

- `Middleware/`:
  - `ExceptionHandlingMiddleware.cs`: Global exception handling

- `Program.cs`: Dependency injection configuration
- `appsettings.json`: Configuration

**Key Principle**: Controllers only handle HTTP concerns. Business logic remains in services.

### Frontend Layers

**Pet Parent Web** (`PawNect.PetParent.Web`):
- Views for pet owners
- Manages their pets
- Views medical records and appointments

**Admin Portal** (`PawNect.AdminPortal.Web`):
- Administrative views
- System management
- User management

## Dependency Injection

Configure in `Program.cs`:

```csharp
// Register repositories
services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
services.AddScoped<IPetRepository, PetRepository>();
services.AddScoped<IUserRepository, UserRepository>();

// Register services
services.AddScoped<IPetService, PetService>();
services.AddScoped<IUserService, UserService>();

// Register DbContext
services.AddDbContext<PawNectDbContext>(options =>
    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
```

## Data Flow Example: Create Pet

```
Frontend (MVC)
    ↓
    POST /api/pets with CreatePetDto
    ↓
PetsController
    ├─ Validates ModelState
    ├─ Calls IPetService.CreatePetAsync()
    ↓
PetService
    ├─ Validates business rules using PetRules
    ├─ Creates Pet entity
    ├─ Calls IPetRepository.AddAsync()
    ↓
PetRepository
    ├─ Adds entity to DbSet
    ├─ Calls SaveChangesAsync()
    ↓
PawNectDbContext
    ├─ Maps Pet to database
    ├─ Executes SQL INSERT
    ↓
MS SQL Database
    ├─ Stores pet record
    ├─ Returns ID
    ↓
PetRepository returns Pet
    ↓
PetService maps to PetDto
    ↓
PetsController returns ApiResponse<PetDto>
    ↓
Frontend receives JSON response
    ↓
User sees confirmation
```

## Adding New Feature: Vaccination Records

### 1. Domain Layer
```csharp
// Domain/Entities/Vaccination.cs
public class Vaccination : BaseEntity
{
    public int PetId { get; set; }
    public string VaccineName { get; set; }
    public DateTime VaccinationDate { get; set; }
    public DateTime? NextDueDate { get; set; }
    public Pet? Pet { get; set; }
}
```

### 2. Application Layer
```csharp
// Application/DTOs/Vaccination/CreateVaccinationDto.cs
public class CreateVaccinationDto
{
    public int PetId { get; set; }
    public string VaccineName { get; set; }
    public DateTime VaccinationDate { get; set; }
    public DateTime? NextDueDate { get; set; }
}

// Application/Interfaces/IVaccinationService.cs
public interface IVaccinationService
{
    Task<VaccinationDto> CreateVaccinationAsync(CreateVaccinationDto dto);
    // ... other methods
}

// Application/Services/VaccinationService.cs
public class VaccinationService : IVaccinationService
{
    private readonly IVaccinationRepository _repo;
    // Implementation
}
```

### 3. Infrastructure Layer
```csharp
// Infrastructure/Repositories/VaccinationRepository.cs
public class VaccinationRepository : Repository<Vaccination>, IVaccinationRepository
{
    // Implementation
}
```

### 4. API Layer
```csharp
// API/Controllers/VaccinationsController.cs
[ApiController]
[Route("api/[controller]")]
public class VaccinationsController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> CreateVaccination(CreateVaccinationDto dto)
    {
        // Implementation
    }
}
```

## Testing Strategy

### Unit Tests
Test services and business logic:
```csharp
[TestClass]
public class PetServiceTests
{
    [TestMethod]
    public async Task CreatePet_WithValidData_ShouldSucceed()
    {
        // Mock repository
        // Call service
        // Assert result
    }
}
```

### Integration Tests
Test repositories and database interactions

### Acceptance Tests
Test API endpoints end-to-end

## Error Handling

### Validation Errors
```csharp
// Domain rules throw ArgumentException
var validation = PetRules.Validations.ValidatePetName(name);
if (!validation.IsValid)
    throw new ArgumentException(validation.Message);
```

### Business Logic Errors
```csharp
// Services throw InvalidOperationException
if (await _repo.EmailExistsAsync(email))
    throw new InvalidOperationException("Email already registered");
```

### HTTP Errors
```csharp
// Controllers return appropriate HTTP status codes
if (pet == null)
    return NotFound(ApiResponse<PetDto>.ErrorResponse("Pet not found"));
```

## Configuration Files

### appsettings.json
- Database connection strings
- Logging configuration
- API settings

### launchSettings.json
- Development vs Production settings
- Port configurations
- Environment variables

## Best Practices

1. **Always use interfaces**: Depend on abstractions, not concrete types
2. **Keep domain pure**: No dependencies on external libraries
3. **Validate early**: Check business rules in services
4. **Use DTOs**: Never expose entities directly in API
5. **Handle exceptions**: Always return appropriate HTTP status codes
6. **Log important events**: Track create, update, delete operations
7. **Use async/await**: Take advantage of async operations
8. **Follow naming conventions**: Clear, consistent naming throughout

## Troubleshooting

### Database Connection Issues
- Check connection string in appsettings.json
- Ensure SQL Server is running
- Verify user permissions

### Migration Errors
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### DependencyInjection Issues
- Ensure all interfaces are registered in Program.cs
- Check that implementations match interfaces
- Verify constructor parameters match registered services
