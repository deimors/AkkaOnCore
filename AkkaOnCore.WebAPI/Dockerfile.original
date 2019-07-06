FROM mcr.microsoft.com/dotnet/core/aspnet:2.1-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.1-stretch AS build
WORKDIR /src
COPY ["AkkaOnCore/AkkaOnCore.csproj", "AkkaOnCore/"]
RUN dotnet restore "AkkaOnCore/AkkaOnCore.csproj"
COPY . .
WORKDIR "/src/AkkaOnCore"
RUN dotnet build "AkkaOnCore.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "AkkaOnCore.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "AkkaOnCore.dll"]