using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ReportGeneratorBackend.BackgroundService;
using ReportGeneratorBackend.Data;
using ReportGeneratorBackend.DTOs;
using ReportGeneratorBackend.Models;
using ReportGeneratorBackend;
using System;

var builder = WebApplication.CreateBuilder(args);

#region Service Registration

// -- Database Context --
// Registers Entity Framework Core with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// -- Real-time Communication --
// Registers SignalR for pushing updates to clients
builder.Services.AddSignalR();

// -- Background Services --
// Registers the hosted service to process reports asynchronously
builder.Services.AddHostedService<ReportProcessorService>();

// -- API Documentation --
// Registers Swagger/OpenAPI generators
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -- CORS Policy --
// Allows the frontend (different origin) to access this API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

#endregion

var app = builder.Build();

#region HTTP Request Pipeline

// -- Documentation UI --
// Enable Swagger UI in development mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// -- SignalR Endpoint --
// Maps the hub to /reportHub for client connections
app.MapHub<ReportHub>("/reportHub");

#endregion

#region API Endpoints

// -- Create Report Endpoint --
// POST /reports: Creates a new report job and notifies clients
app.MapPost("/reports", async (ApplicationDbContext db, IHubContext<ReportHub> hubContext, CreateReportDto dto) =>
{
    try
    {
        // 1. Validate Input
        if (string.IsNullOrEmpty(dto.ReportType))
        {
            return Results.BadRequest("ReportType is required.");
        }

        // 2. Create Job Entry
        var job = new ReportJob
        {
            Id = Guid.NewGuid(),
            ReportType = dto.ReportType,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = ReportStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // 3. Save to Database
        db.ReportJobs.Add(job);
        await db.SaveChangesAsync();

        // 4. Broadcast Update via SignalR
        await hubContext.Clients.All.SendAsync("ReceiveUpdate", job);

        return Results.Created($"/reports/{job.Id}", job);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message, statusCode: 500);
    }
})
.WithName("CreateReport")
.WithOpenApi();

// -- Get Reports Endpoint --
// GET /reports: Retrieves all report jobs sorted by date
app.MapGet("/reports", async (ApplicationDbContext db) =>
{
    try
    {
        var reports = await db.ReportJobs
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Results.Ok(reports);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message, statusCode: 500);
    }
})
.WithName("GetReports")
.WithOpenApi();

#endregion

app.Run();
