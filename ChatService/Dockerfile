FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
    WORKDIR /src
    COPY . .
    RUN dotnet publish ChatService/ChatService.csproj -c Release -o /app/publish
    FROM mcr.microsoft.com/dotnet/aspnet:8.0
    WORKDIR /app
    COPY --from=build /app/publish .
    ENV ASPNETCORE_URLS=http://+:4099
    EXPOSE 4099
    ENTRYPOINT ["dotnet", "ChatService.dll"]
