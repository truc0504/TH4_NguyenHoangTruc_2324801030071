FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
    WORKDIR /src
    COPY . .
    RUN dotnet publish PolicyService/PolicyService.csproj -c Release -o /app/publish
    FROM mcr.microsoft.com/dotnet/aspnet:8.0
    WORKDIR /app
    COPY --from=build /app/publish .
    ENV ASPNETCORE_URLS=http://+:5050
    EXPOSE 5050
    ENTRYPOINT ["dotnet", "PolicyService.dll"]
