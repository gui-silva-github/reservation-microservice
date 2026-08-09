FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["API/SpacesService.API.csproj", "API/"]
COPY ["BLL/SpacesService.BLL.csproj", "BLL/"]
COPY ["DAL/SpacesService.DAL.csproj", "DAL/"]

RUN dotnet restore "API/SpacesService.API.csproj"

COPY . .
WORKDIR /src/API
RUN dotnet publish "SpacesService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

ENV ASPNETCORE_URLS=http://+:8080
ENV MYSQL_HOST=mysql
ENV MYSQL_PASSWORD=admin

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SpacesService.API.dll"]
