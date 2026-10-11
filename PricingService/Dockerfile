FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
    WORKDIR /src
    COPY . .
    RUN dotnet publish PricingService/PricingService.csproj -c Release -o /app/publish
    FROM mcr.microsoft.com/dotnet/aspnet:8.0
    WORKDIR /app
    COPY --from=build /app/publish .
    ENV ASPNETCORE_URLS=http://+:5040
    EXPOSE 5040
    ENTRYPOINT ["dotnet", "PricingService.dll"]
