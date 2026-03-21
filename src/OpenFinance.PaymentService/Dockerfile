FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# Non-root user for security
RUN adduser --disabled-password --gecos "" appuser

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/OpenFinance.PaymentService/OpenFinance.PaymentService.csproj", "src/OpenFinance.PaymentService/"]
COPY ["src/OpenFinance.Shared/OpenFinance.Shared.csproj", "src/OpenFinance.Shared/"]
RUN dotnet restore "src/OpenFinance.PaymentService/OpenFinance.PaymentService.csproj"
COPY . .
RUN dotnet build "src/OpenFinance.PaymentService/OpenFinance.PaymentService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/OpenFinance.PaymentService/OpenFinance.PaymentService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER appuser
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health/live || exit 1
ENTRYPOINT ["dotnet", "OpenFinance.PaymentService.dll"]
