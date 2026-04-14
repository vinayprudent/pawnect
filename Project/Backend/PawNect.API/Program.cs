using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PawNect.Application.Interfaces;
using PawNect.Application.Settings;
using PawNect.Domain.Entities;
using PawNect.Domain.Enums;
using PawNect.Application.Services;
using PawNect.API.Services;
using PawNect.Infrastructure.DbContext;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PawNect API", Version = "v1" });
});

// Database + application services
builder.Services.AddDbContext<PawNect.Infrastructure.DbContext.PawNectDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped(typeof(PawNect.Application.Interfaces.IRepository<>), typeof(PawNect.Infrastructure.Repositories.Repository<>));
builder.Services.AddScoped<PawNect.Application.Interfaces.IPetRepository, PawNect.Infrastructure.Repositories.PetRepository>();
builder.Services.AddScoped<PawNect.Application.Interfaces.IUserRepository, PawNect.Infrastructure.Repositories.UserRepository>();
builder.Services.AddScoped<PawNect.Application.Interfaces.IMedicalRecordRepository, PawNect.Infrastructure.Repositories.MedicalRecordRepository>();
builder.Services.AddScoped<PawNect.Application.Interfaces.IAppointmentRepository, PawNect.Infrastructure.Repositories.AppointmentRepository>();
builder.Services.AddScoped<PawNect.Application.Interfaces.IConsultationRepository, PawNect.Infrastructure.Repositories.ConsultationRepository>();
builder.Services.AddScoped<PawNect.Application.Interfaces.IDiagnosticOrderRepository, PawNect.Infrastructure.Repositories.DiagnosticOrderRepository>();
builder.Services.AddScoped<PawNect.Application.Interfaces.IDiagnosticReportRepository, PawNect.Infrastructure.Repositories.DiagnosticReportRepository>();
builder.Services.AddScoped<PawNect.Application.Interfaces.IRatingRepository, PawNect.Infrastructure.Repositories.RatingRepository>();

// Application services
builder.Services.AddScoped<PawNect.Application.Interfaces.IPetService, PawNect.Application.Services.PetService>();
builder.Services.AddScoped<PawNect.Application.Interfaces.IUserService, PawNect.Application.Services.UserService>();
builder.Services.AddScoped<PawNect.Application.Interfaces.IAppointmentService, PawNect.Application.Services.AppointmentService>();
builder.Services.AddScoped<PawNect.Application.Interfaces.IConsultationService, PawNect.Application.Services.ConsultationService>();
builder.Services.AddScoped<PawNect.Application.Interfaces.IDiagnosticOrderService, PawNect.Application.Services.DiagnosticOrderService>();
builder.Services.AddScoped<PawNect.Application.Interfaces.IDiagnosticReportService, PawNect.Application.Services.DiagnosticReportService>();
builder.Services.AddScoped<PawNect.Application.Interfaces.IRatingService, PawNect.Application.Services.RatingService>();
builder.Services.Configure<OtpSettings>(builder.Configuration.GetSection("Otp"));
builder.Services.AddScoped<INotificationSender, EmailNotificationSender>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();
var webRootExists = !string.IsNullOrWhiteSpace(app.Environment.WebRootPath) && Directory.Exists(app.Environment.WebRootPath);
var isDev = app.Environment.IsDevelopment();

// Apply pending migrations and seed default admin user
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PawNect.Infrastructure.DbContext.PawNectDbContext>();
    await context.Database.MigrateAsync();
    if (!await context.Users.AnyAsync(u => u.Role == UserRole.Admin && !u.IsDeleted))
    {
        context.Users.Add(new User
        {
            FirstName = "Admin",
            LastName = "PawNect",
            Email = "admin@pawnect.com",
            PhoneNumber = "",
            PasswordHash = UserService.HashPasswordForSeed("Admin@123"),
            Role = UserRole.Admin
        });
        await context.SaveChangesAsync();
    }
    if (!await context.LabTestCatalogItems.AnyAsync())
    {
        context.LabTestCatalogItems.AddRange(
            new LabTestCatalogItem { Name = "CBC", TestType = "Individual", Price = 450, SampleType = "Blood", Description = "Complete Blood Count" },
            new LabTestCatalogItem { Name = "LFT", TestType = "Individual", Price = 550, SampleType = "Blood", Description = "Liver Function Test" },
            new LabTestCatalogItem { Name = "KFT", TestType = "Individual", Price = 550, SampleType = "Blood", Description = "Kidney Function Test" },
            new LabTestCatalogItem { Name = "Thyroid", TestType = "Individual", Price = 650, SampleType = "Blood", Description = "Thyroid Profile" },
            new LabTestCatalogItem { Name = "Urinalysis", TestType = "Individual", Price = 350, SampleType = "Urine", Description = "Urinalysis" },
            new LabTestCatalogItem { Name = "Basic Health Check", TestType = "Package", Price = 1200, SampleType = "Blood", Description = "CBC, Basic biochemistry", TestsIncludedJson = "[\"CBC\",\"Basic biochemistry\"]" },
            new LabTestCatalogItem { Name = "Annual Wellness", TestType = "Package", Price = 2500, SampleType = "Blood", Description = "CBC, LFT, KFT, Thyroid, Urinalysis", TestsIncludedJson = "[\"CBC\",\"LFT\",\"KFT\",\"Thyroid\",\"Urinalysis\"]" },
            new LabTestCatalogItem { Name = "Senior Panel", TestType = "Package", Price = 3200, SampleType = "Blood", Description = "Extended panel for senior pets", TestsIncludedJson = "[\"CBC\",\"LFT\",\"KFT\",\"Thyroid\",\"Urinalysis\",\"Cardiac markers\"]" }
        );
        await context.SaveChangesAsync();
    }
}

// Configure the HTTP request pipeline
if (isDev)
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PawNect API v1"));
}

if (!isDev)
{
    app.UseHttpsRedirection();
}

if (webRootExists)
{
    app.UseStaticFiles();
}

// Keep local URLs simple; only apply /api base path in non-development hosts.
if (!isDev)
{
    app.UsePathBase("/api");
}
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
