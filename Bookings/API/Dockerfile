FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["BookingsService.Api/BookingsService.API.csproj", "Bookings.API/"]
COPY ["BookingsService.BLL/BookingsService.BLL.csproj", "Bookings.BLL/"]
COPY ["BookingsService.DAL/BookingsService.DAL.csproj", "Bookings.DAL/"]

RUN dotnet restore "Bookings.API/BookingsService.API.csproj"

COPY . .
WORKDIR /src/BookingsService.API
RUN dotnet publish "BookingsService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Bookings.API.dll"]