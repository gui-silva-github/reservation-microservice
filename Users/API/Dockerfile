FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["API/UsersService.API.csproj", "API/"]
COPY ["BLL/UsersService.BLL.csproj", "BLL/"]
COPY ["DAL/UsersService.DAL.csproj", "DAL/"]

RUN dotnet restore "API/UsersService.API.csproj"

COPY . .
WORKDIR /src/API
RUN dotnet publish "UsersService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UsersService.API.dll"]
