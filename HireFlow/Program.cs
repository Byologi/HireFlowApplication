using HireFlow.Infrastructure.Data;
using HireFlow.Services;
using HireFlow.Services.Applications;
using HireFlow.Services.BackgroundQueue;
using HireFlow.Services.Jobs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<QueuedHostedService>();

// Add DbContext
builder.Services.AddDbContext<HireFlowDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IJobService, JobService>();

builder.Services.AddScoped<IApplicationService, ApplicationService>();

var app = builder.Build();

// Automatic Migration
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HireFlowDbContext>();
    db.Database.Migrate();
}

// Middleware
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

// Map controllers
app.MapControllers();

app.Run();