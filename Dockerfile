# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["PaymentService.csproj", "./"]
RUN dotnet restore "PaymentService.csproj"

COPY . .
RUN dotnet build "PaymentService.csproj" -c Release -o /app/build

RUN dotnet publish "PaymentService.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 3000

ENV ASPNETCORE_URLS=http://+:3000

ENTRYPOINT ["dotnet", "PaymentService.dll"]
