using CommandsService.AsyncDataServices;
using CommandsService.Data;
using CommandsService.EventProcessing;
using CommandsService.SyncDataServices.Grpc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Use In-Memory Database
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("InMem"));

// Register the repository
builder.Services.AddScoped<ICommandRepo, CommandRepo>();

builder.Services.AddControllers();

// Register the MessageBusSubscriber as a Hosted Service
builder.Services.AddHostedService<MessageBusSubscriber>();

// Register the EventProcessor
builder.Services.AddSingleton<IEventProcessor, EventProcessor>();

// AutoMapper Configuration
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Register the gRPC Client
builder.Services.AddScoped<IPlatformDataClient, PlatformDataClient>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "api/c/swagger/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/api/c/swagger/v1/swagger.json", "Commands API V1");
        c.RoutePrefix = "api/c/swagger";
    });
//}

//if (!app.Environment.IsDevelopment())
//{
//    app.UseHttpsRedirection();
//}

app.UseAuthorization();

app.MapControllers();

// Seed the database using gRPC client
PrepDb.PrepPopulation(app);

app.Run();
