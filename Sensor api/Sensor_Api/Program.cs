using Microsoft.EntityFrameworkCore;
using Sensor_Api.Data;
using Sensor_Api.HostedServices;
using Sensor_Api.Hubs;
using Sensor_Api.Repositories;
using Sensor_Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddDbContext<SensorDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ISensorRepository, SensorRepository>();
builder.Services.AddScoped<ISensorService, SensorService>();

builder.Services.AddHostedService<SimulatorHostedService>();
builder.Services.AddHostedService<BatchProcessorHostedService>();
builder.Services.AddHostedService<PurgeHostedService>();
builder.Services.AddHostedService<DataCleanupHostedService>();

var reactAppUrl = "http://localhost:3000";
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins(reactAppUrl)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("CorsPolicy");
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<SensorHub>("/sensorHub");
});

app.Run();
