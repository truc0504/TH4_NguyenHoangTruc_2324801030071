FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
    WORKDIR /src
    COPY . .
    RUN dotnet publish PolicySearchService/PolicySearchService.csproj -c Release -o /app/publish
    FROM mcr.microsoft.com/dotnet/aspnet:8.0
    WORKDIR /app
    COPY --from=build /app/publish .
    ENV ASPNETCORE_URLS=http://+:5065
    EXPOSE 5065
    ENTRYPOINT ["dotnet", "PolicySearchService.dll"]
