
using Polling.Application;
using Polling.DataAccess;
using Polling.DataAccess.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication(builder.Environment, builder.Configuration)
                .AddDataAccess(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()//WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

using var scope = app.Services.CreateScope();

await AutomatedMigration.MigrateAsync(scope.ServiceProvider);

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseAuthorization();


app.MapControllers();

app.Run();

