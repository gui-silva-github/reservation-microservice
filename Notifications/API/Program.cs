using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Reservation.NotificationsService.API.DTOs;
using Reservation.NotificationsService.API.Middleware;
using Reservation.NotificationsService.BLL;
using Reservation.NotificationsService.BLL.Configuration;
using Reservation.NotificationsService.DAL;
using Reservation.NotificationsService.DAL.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection(RedisSettings.SectionName));

builder.Services.AddDAL();
builder.Services.AddBLL(builder.Configuration);

JwtSettings jwtSettings = builder.Configuration
    .GetSection(jwtSettings.SectionName)
    .Get<JwtSettings>() ?? throw new InvalidOperationException("As configurações de JWT não estão configuradas.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
       options.TokenValidationParameters = new TokenValidationParameters
       {
         ValidateIssuer = true,
         ValidateAudience = true,
         ValidateLifetime = true,
         ValidateIssuerSigningKey = true,
         ValidIssuer = jwtSettings.Issuer,
         ValidAudience = jwtSettings.Audience,
         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))  
       }; 
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOption(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value!.Errors.Select(error => error.ErrorMessage).ToArray()
                );

            var response = new ApiErrorResponse("Dados de Requisição inválidos.", errors, "ModelBinding");
            return new BadRequestObjectResult(response);
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    DatabaseInitializer databaseInitializer = scope.ServiceProvider.GetRequiredService<databaseInitializer>();
    await databaseInitializer.InitializeAsync();
}

app.UseExceptionHandlingMiddleware();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}

app.Run();
