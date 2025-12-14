# Multi-arch support - works on Windows x64 and Raspberry Pi ARM64
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy ALL project files (core, math, web)
COPY NutrientOptimizer.Core/NutrientOptimizer.Core.csproj NutrientOptimizer.Core/
COPY NutrientOptimizer.Math/NutrientOptimizer.Math.csproj NutrientOptimizer.Math/
COPY NutrientOptimizer.Web/NutrientOptimizer.Web.csproj NutrientOptimizer.Web/

# Restore (will resolve project references)
RUN dotnet restore NutrientOptimizer.Web/NutrientOptimizer.Web.csproj

# Copy all source code
COPY . .

# Publish the web app
WORKDIR /src/NutrientOptimizer.Web
RUN dotnet publish NutrientOptimizer.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NutrientOptimizer.Web.dll"]