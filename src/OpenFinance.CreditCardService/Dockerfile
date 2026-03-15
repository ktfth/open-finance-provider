FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/OpenFinance.CreditCardService/OpenFinance.CreditCardService.csproj", "src/OpenFinance.CreditCardService/"]
COPY ["src/OpenFinance.Shared/OpenFinance.Shared.csproj", "src/OpenFinance.Shared/"]
RUN dotnet restore "src/OpenFinance.CreditCardService/OpenFinance.CreditCardService.csproj"
COPY . .
RUN dotnet build "src/OpenFinance.CreditCardService/OpenFinance.CreditCardService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/OpenFinance.CreditCardService/OpenFinance.CreditCardService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenFinance.CreditCardService.dll"]
