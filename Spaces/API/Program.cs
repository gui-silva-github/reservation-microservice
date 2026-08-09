using Microsoft.AspNetCore.Mvc;
using Reservation.SpacesService.API.Middleware;
using Reservation.SpacesService.BLL;
using Reservation.SpacesService.BLL.DTO;
using Reservation.SpacesService.DAL;
using Reservation.SpacesService.DAL.Context;

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
      policy.WithOrigins("http://localhost:4200")
        .AllowAnyMethod()
        .AllowAnyHeader(); 
   });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SpacesDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseExceptionHandlingMiddleware();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}

app.Run();
