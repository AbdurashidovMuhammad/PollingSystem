
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Polling.Api;
using Polling.Api.Middleware;
using Polling.Application;
using Polling.DataAccess;
using Polling.DataAccess.Persistence;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Token validatsiyasi sozlamalari
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, // Issuer (tokenni beruvchi) ni tekshirish
        ValidateAudience = true, // Audience (token qaysi resurs uchun berilganini) tekshirish
        ValidateLifetime = true, // Tokenning amal qilish muddatini tekshirish
        ValidateIssuerSigningKey = true, // Issuerning imzolovchi kalitini tekshirish

        ValidIssuer = builder.Configuration["Jwt:Issuer"], // appsettings.json dan olinadi
        ValidAudience = builder.Configuration["Jwt:Audience"], // appsettings.json dan olinadi
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"])) // appsettings.json dan olinadi
    };
});

builder.Services.AddAuthorization();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwagger();

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

app.UseMiddleware<JwtMiddleware>();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();

