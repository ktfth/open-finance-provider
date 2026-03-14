FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/OpenFinance.InvestmentService/OpenFinance.InvestmentService.csproj", "src/OpenFinance.InvestmentService/"]
COPY ["src/OpenFinance.Shared/OpenFinance.Shared.csproj", "src/OpenFinance.Shared/"]
RUN dotnet restore "src/OpenFinance.InvestmentService/OpenFinance.InvestmentService.csproj"
COPY . .
RUN dotnet build "src/OpenFinance.InvestmentService/OpenFinance.InvestmentService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/OpenFinance.InvestmentService/OpenFinance.InvestmentService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenFinance.InvestmentService.dll"]
