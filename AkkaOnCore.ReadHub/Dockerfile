FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
COPY ["AkkaOnCore.ReadHub/AkkaOnCore.ReadHub.csproj", "AkkaOnCore.ReadHub/"]
RUN dotnet restore "AkkaOnCore.ReadHub/AkkaOnCore.ReadHub.csproj"
COPY . .
WORKDIR "/src/AkkaOnCore.ReadHub"
RUN dotnet build "AkkaOnCore.ReadHub.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "AkkaOnCore.ReadHub.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "AkkaOnCore.ReadHub.dll"]