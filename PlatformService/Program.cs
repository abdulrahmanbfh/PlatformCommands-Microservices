using Microsoft.EntityFrameworkCore;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.SyncDataServices.Grpc;
using PlatformService.SyncDataServices.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure DbContext
if (builder.Environment.IsProduction())
{
    Console.WriteLine("--> Using MSSQL Db");
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseSqlServer(builder.Configuration.GetConnectionString("PlatformsConn")));
}
else
{
    Console.WriteLine("--> Using InMem Db");
    builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("InMem"));
}


// Register the repository
builder.Services.AddScoped<IPlatformRepo, PlatformRepo>();


// Sync HTTP Client
builder.Services.AddHttpClient<ICommandDataClient, HttpCommandDataClient>();

//builder.Services.AddSingleton<IMessageBusClient, MessageBusClient>();
builder.Services.AddSingleton<IMessageBusClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    return MessageBusClient
        .CreateAsync(configuration)
        .GetAwaiter()
        .GetResult();
});

// Register the gRPC Service
builder.Services.AddGrpc();

builder.Services.AddControllers();

// AutoMapper Configuration
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

Console.WriteLine($"--> CommandsService Endpoint: {builder.Configuration["CommandsService"]}");



var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger(c =>
{
    c.RouteTemplate = "api/swagger/{documentName}/swagger.json";
});
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/api/swagger/v1/swagger.json", "Platform API V1");
    c.RoutePrefix = "api/swagger";
});
//}

//if (!app.Environment.IsDevelopment())
//{
//    app.UseHttpsRedirection();
//}

app.UseAuthorization();

app.MapControllers();

// Map gRPC Service
app.MapGrpcService<GrpcPlatformService>();


// Map proto file
app.MapGet("/protos/platforms.proto", async context =>
    {
        await context.Response.WriteAsync(File.ReadAllText("Protos/platforms.proto"));
    }
);

// Seed the database based on environment, selecting between In-Memory and MSSQL
PrepDb.PrepPopulation(app, builder.Environment.IsProduction());

app.Run();
