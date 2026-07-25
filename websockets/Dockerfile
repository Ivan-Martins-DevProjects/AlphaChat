FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY websockets/*.csproj ./websockets/
RUN dotnet restore websockets/websockets.csproj
COPY websockets/ ./websockets/
RUN dotnet publish websockets/websockets.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 5272
ENTRYPOINT ["dotnet", "websockets.dll"]
