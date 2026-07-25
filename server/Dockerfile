FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY server/*.csproj ./server/
RUN dotnet restore server/server.csproj
COPY server/ ./server/
RUN dotnet publish server/server.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 5126
ENTRYPOINT ["dotnet", "server.dll"]
