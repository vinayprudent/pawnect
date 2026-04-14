# PawNect Setup & Development Guide

## Quick Start

### Prerequisites Installation

1. **Install .NET 10 SDK**
   - Download from: https://dotnet.microsoft.com/en-us/download
   - Verify: `dotnet --version` (should show 10.0.x)

2. **Install SQL Server**
   - Download SQL Server Express: https://www.microsoft.com/sql-server/sql-server-express
   - Or use SQL Server LocalDB (included with Visual Studio)
   - Connection string in code uses: `.` (LocalDB)

3. **Install Visual Studio 2022**
   - Download from: https://visualstudio.microsoft.com/
   - Include: ASP.NET and web development workload

### Initial Setup

#### Step 1: Open Solution
```bash
cd d:\Projects\PawNect\Project
dotnet sln list
```

#### Step 2: Restore Dependencies
```bash
dotnet restore
```

#### Step 3: Create Database
```bash
cd Backend/PawNect.Infrastructure
dotnet ef migrations add InitialCreate
dotnet ef database update
```

#### Step 4: Build Solution
```bash
cd ..
dotnet build
```

### Running the Application

#### Terminal 1: Start API
```bash
cd Backend/PawNect.API
dotnet run

# Output should show:
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: https://localhost:5001
#       Now listening on: http://localhost:5000
```

#### Terminal 2: Start Pet Parent Web
```bash
cd Frontend/PawNect.PetParent.Web
dotnet run

# Output should show:
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: https://localhost:5003
#       Now listening on: http://localhost:5002
```

#### Terminal 3: Start Admin Portal
```bash
cd Frontend/PawNect.AdminPortal.Web
dotnet run

# Output should show:
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: https://localhost:5005
#       Now listening on: http://localhost:5004
```

### Access Applications

- **API Swagger UI**: http://localhost:5000/swagger/index.html
- **Pet Parent Web**: http://localhost:5002
- **Admin Portal**: http://localhost:5004

## Project Structure Navigation

```
Backend/
├── PawNect.Domain/          ← Core business entities
├── PawNect.Application/     ← Services and DTOs
├── PawNect.Infrastructure/  ← Data access (DbContext, Repositories)
└── PawNect.API/             ← REST API endpoints

Frontend/
├── PawNect.PetParent.Web/   ← Pet owner interface
└── PawNect.AdminPortal.Web/ ← Admin interface
```

## Common Development Tasks

### Add a New Feature

**Example: Add Vaccination Management**

1. **Add Domain Entity**
```csharp
// Backend/PawNect.Domain/Entities/Vaccination.cs
public class Vaccination : BaseEntity
{
    public string Name { get; set; }
    public DateTime Date { get; set; }
    public int PetId { get; set; }
    public Pet? Pet { get; set; }
}
```

2. **Add DTO**
```csharp
// Backend/PawNect.Application/DTOs/Vaccination/CreateVaccinationDto.cs
public class CreateVaccinationDto
{
    public string Name { get; set; }
    public DateTime Date { get; set; }
    public int PetId { get; set; }
}
```

3. **Add Repository Interface**
```csharp
// Backend/PawNect.Application/Interfaces/IVaccinationRepository.cs
public interface IVaccinationRepository : IRepository<Vaccination>
{
    Task<IEnumerable<Vaccination>> GetByPetIdAsync(int petId);
}
```

4. **Implement Repository**
```csharp
// Backend/PawNect.Infrastructure/Repositories/VaccinationRepository.cs
public class VaccinationRepository : Repository<Vaccination>, IVaccinationRepository
{
    // Implementation
}
```

5. **Add Service Interface**
```csharp
// Backend/PawNect.Application/Interfaces/IVaccinationService.cs
public interface IVaccinationService
{
    Task<VaccinationDto> CreateAsync(CreateVaccinationDto dto);
}
```

6. **Implement Service**
```csharp
// Backend/PawNect.Application/Services/VaccinationService.cs
public class VaccinationService : IVaccinationService
{
    // Implementation
}
```

7. **Add Controller**
```csharp
// Backend/PawNect.API/Controllers/VaccinationsController.cs
[ApiController]
[Route("api/[controller]")]
public class VaccinationsController : ControllerBase
{
    // Implementation
}
```

8. **Register in Dependency Injection**
```csharp
// Backend/PawNect.API/Program.cs
builder.Services.AddScoped<IVaccinationRepository, VaccinationRepository>();
builder.Services.AddScoped<IVaccinationService, VaccinationService>();
```

9. **Update Database**
```bash
cd Backend/PawNect.API
dotnet ef migrations add AddVaccination
dotnet ef database update
```

### Run Database Migrations

```bash
# View pending migrations
cd Backend/PawNect.API
dotnet ef migrations list

# Add new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

### Debug Mode

1. **Set breakpoints** by clicking line numbers
2. **Start debugging**:
   - VS: F5 or Debug → Start Debugging
   - VS Code: F5 with launch.json configured

### Run Tests (When Added)

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test ./Backend/PawNect.Application.Tests

# Run with verbose output
dotnet test --verbosity=detailed
```

## API Testing with Curl

### Create Pet
```bash
curl -X POST http://localhost:5000/api/pets \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Buddy",
    "breed": "Golden Retriever",
    "species": 1,
    "dateOfBirth": "2020-01-15",
    "weightKg": 30,
    "ownerId": 1
  }'
```

### Get All Pets
```bash
curl http://localhost:5000/api/pets
```

### Get Pet by ID
```bash
curl http://localhost:5000/api/pets/1
```

### Update Pet
```bash
curl -X PUT http://localhost:5000/api/pets/1 \
  -H "Content-Type: application/json" \
  -d '{
    "id": 1,
    "name": "Buddy Updated",
    "breed": "Golden Retriever",
    "species": 1,
    "dateOfBirth": "2020-01-15",
    "weightKg": 31,
    "status": 1
  }'
```

### Delete Pet
```bash
curl -X DELETE http://localhost:5000/api/pets/1
```

### Register User
```bash
curl -X POST http://localhost:5000/api/users/register \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "phoneNumber": "1234567890",
    "password": "Password123",
    "role": 1,
    "city": "New York"
  }'
```

### Login
```bash
curl -X POST http://localhost:5000/api/users/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "Password123"
  }'
```

## Postman Collection

Create file: `PawNect.postman_collection.json`

Available endpoints:
- Users: Register, Login, Get All, Get by ID, Update, Delete
- Pets: Create, Get All, Get by ID, Get by Owner, Update, Delete

Import into Postman for easy API testing.

## Troubleshooting

### "Database connection failed"
- Check connection string in `appsettings.json`
- Verify SQL Server is running
- Test connection: `sqlcmd -S .`

### "Port already in use"
- Change port in `launchSettings.json`
- Kill process: `netstat -ano | findstr :5000`

### "Migration failed"
```bash
# Remove problematic migration
dotnet ef migrations remove

# View migrations
dotnet ef migrations list

# Reapply from scratch
dotnet ef database drop
dotnet ef database update
```

### "Dependency Injection Error"
- Ensure all interfaces are registered in `Program.cs`
- Check constructor parameters match services
- Run `dotnet build` to verify compilation

### "CORS Issues"
- Check CORS policy in API `Program.cs`
- Currently set to allow all origins (development)
- Update for production security

## IDE Setup

### Visual Studio 2022
1. Open `PawNect.sln`
2. Set `PawNect.API` as startup project
3. Press F5 to run

### VS Code
1. Install extensions:
   - C# (by Microsoft)
   - Postman (for API testing)
2. Open folder: `d:\Projects\PawNect\Project`
3. Terminal: Run commands above

## Code Quality

### Format Code
```bash
dotnet format
```

### Run Code Analysis
```bash
dotnet build /p:EnforceCodeStyleInBuild=true
```

## Production Deployment

1. **Publish Build**
```bash
dotnet publish -c Release -o ./publish
```

2. **Set Connection String**
```bash
set ASPNETCORE_ENVIRONMENT=Production
set ConnectionStrings__DefaultConnection=YourServerConnectionString
```

3. **Migrate Database**
```bash
dotnet ef database update --environment Production
```

4. **Run Application**
```bash
dotnet PawNect.API.dll
```

## Documentation

- **Architecture**: See [ARCHITECTURE.md](./ARCHITECTURE.md)
- **API Docs**: Swagger UI at http://localhost:5000/swagger
- **Code Comments**: XML documentation in source files

## Support & Questions

For issues or questions:
1. Check existing documentation
2. Review code comments
3. Check logs in Terminal
4. Verify all prerequisites are installed
