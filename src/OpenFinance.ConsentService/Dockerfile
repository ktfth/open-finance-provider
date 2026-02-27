FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/OpenFinance.ConsentService/OpenFinance.ConsentService.csproj", "src/OpenFinance.ConsentService/"]
COPY ["src/OpenFinance.Shared/OpenFinance.Shared.csproj", "src/OpenFinance.Shared/"]
RUN dotnet restore "src/OpenFinance.ConsentService/OpenFinance.ConsentService.csproj"
COPY . .
RUN dotnet build "src/OpenFinance.ConsentService/OpenFinance.ConsentService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/OpenFinance.ConsentService/OpenFinance.ConsentService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenFinance.ConsentService.dll"]
