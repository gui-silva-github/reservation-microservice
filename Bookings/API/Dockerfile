FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["API/BookingsService.API.csproj", "API/"]
COPY ["BLL/BookingsService.BLL.csproj", "BLL/"]
COPY ["DAL/BookingsService.DAL.csproj", "DAL/"]

RUN dotnet restore "API/BookingsService.API.csproj"

COPY . .
WORKDIR /src/API
RUN dotnet publish "BookingsService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BookingsService.API.dll"]
