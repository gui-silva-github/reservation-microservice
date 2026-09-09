using Microsoft.AspNetCore.Mvc;
using Reservation.SpacesService.API.Middleware;
using Reservation.SpacesService.BLL;
using Reservation.SpacesService.BLL.DTO;
using Reservation.SpacesService.DAL;
using Reservation.SpacesService.DAL.Context;
using Reservation.SpacesService.DAL.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLogicLayer(builder.Configuration);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
       options.InvalidModelStateResponseFactory = context =>
       {
           var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value!.Errors.Select(error => error.ErrorMessage).ToArray()
            );

            var response = new ApiErrorResponse(
                "model_binding_error",
                "Dados da requisição inválidos.",
                errors
            );

            return new BadRequestObjectResult(response);
       };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
   options.AddDefaultPolicy(policy =>
   {
      policy.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
        .AllowAnyMethod()
        .AllowAnyHeader();
   });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SpacesDbContext>();
    dbContext.Database.EnsureCreated();
    SeedSpaces(dbContext);
}

app.UseCors();
app.UseExceptionHandlingMiddleware();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}

app.Run();

static void SeedSpaces(SpacesDbContext dbContext)
{
    if (dbContext.Spaces.Any())
    {
        return;
    }

    dbContext.Spaces.AddRange(
        new Space
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Sala Focus",
            Location = "São Paulo - Centro",
            Description = "Sala privativa para reuniões e trabalho concentrado.",
            Capacity = 6,
            PricePerHour = 80,
            IsActive = true
        },
        new Space
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "Coworking Open",
            Location = "São Paulo - Vila Madalena",
            Description = "Espaço compartilhado com mesas flexíveis e café.",
            Capacity = 20,
            PricePerHour = 35,
            IsActive = true
        },
        new Space
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Name = "Studio Criativo",
            Location = "Campinas",
            Description = "Estúdio com lousa, TV e iluminação para workshops.",
            Capacity = 12,
            PricePerHour = 95,
            IsActive = true
        }
    );

    dbContext.SaveChanges();
}
