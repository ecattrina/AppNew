# Используем официальный образ .NET 8.0 для базового этапа
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Этап сборки проекта
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AppNew.csproj", "./"]
RUN dotnet restore "AppNew.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "AppNew.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этап публикации проекта
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AppNew.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Финальный этап - создание образа для запуска
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "AppNew.dll"]
