FROM mcr.microsoft.com/dotnet/core/aspnet:2.1-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.1-stretch AS build
WORKDIR /src
COPY ["AkkaOnCore.CommandAPI/AkkaOnCore.CommandAPI.csproj", "AkkaOnCore.CommandAPI/"]
RUN dotnet restore "AkkaOnCore.CommandAPI/AkkaOnCore.CommandAPI.csproj"
COPY . .
WORKDIR "/src/AkkaOnCore.CommandAPI"
RUN dotnet build "AkkaOnCore.CommandAPI.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "AkkaOnCore.CommandAPI.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "AkkaOnCore.CommandAPI.dll"]