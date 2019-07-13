FROM mcr.microsoft.com/dotnet/core/aspnet:2.1-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.1-stretch AS build
WORKDIR /src
COPY ["AkkaOnCore.WebAPI/AkkaOnCore.WebAPI.csproj", "AkkaOnCore.WebAPI/"]
COPY ["AkkaOnCore.Actors/AkkaOnCore.Actors.csproj", "AkkaOnCore.Actors/"]
RUN dotnet restore "AkkaOnCore.WebAPI/AkkaOnCore.WebAPI.csproj"
COPY . .
WORKDIR "/src/AkkaOnCore.WebAPI"
RUN dotnet build "AkkaOnCore.WebAPI.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "AkkaOnCore.WebAPI.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "AkkaOnCore.WebAPI.dll"]
