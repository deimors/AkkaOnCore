FROM mcr.microsoft.com/dotnet/core/aspnet:2.1-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.1-stretch AS build
WORKDIR /src
COPY ["AkkaOnCore.QueryAPI/AkkaOnCore.QueryAPI.csproj", "AkkaOnCore.QueryAPI/"]
RUN dotnet restore "AkkaOnCore.QueryAPI/AkkaOnCore.QueryAPI.csproj"
COPY . .
WORKDIR "/src/AkkaOnCore.QueryAPI"
RUN dotnet build "AkkaOnCore.QueryAPI.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "AkkaOnCore.QueryAPI.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "AkkaOnCore.QueryAPI.dll"]