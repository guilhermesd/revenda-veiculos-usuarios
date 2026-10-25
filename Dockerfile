FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore Fiap.RevendaVeiculos.Clientes.slnx
RUN dotnet publish src/Fiap.RevendaVeiculos.Clientes.Api/Fiap.RevendaVeiculos.Clientes.Api.csproj -c Release -o /app/publish --no-restore
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet","Fiap.RevendaVeiculos.Clientes.Api.dll"]
