FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
    WORKDIR /src
    COPY . .
    RUN dotnet publish AuthService/AuthService.csproj -c Release -o /app/publish
    FROM mcr.microsoft.com/dotnet/aspnet:8.0
    WORKDIR /app
    COPY --from=build /app/publish .
    ENV ASPNETCORE_URLS=http://+:6060
    EXPOSE 6060
    ENTRYPOINT ["dotnet", "AuthService.dll"]
