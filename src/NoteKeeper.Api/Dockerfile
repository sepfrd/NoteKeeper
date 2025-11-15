FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/NoteKeeper.Api/NoteKeeper.Api.csproj", "src/NoteKeeper.Api/"]
COPY ["src/NoteKeeper.Infrastructure/NoteKeeper.Infrastructure.csproj", "src/NoteKeeper.Infrastructure/"]
COPY ["src/NoteKeeper.Application/NoteKeeper.Application.csproj", "src/NoteKeeper.Application/"]
COPY ["src/NoteKeeper.Domain/NoteKeeper.Domain.csproj", "src/NoteKeeper.Domain/"]
COPY ["src/NoteKeeper.Shared/NoteKeeper.Shared.csproj", "src/NoteKeeper.Shared/"]
RUN dotnet restore "src/NoteKeeper.Api/NoteKeeper.Api.csproj"
COPY . .
WORKDIR "/src/src/NoteKeeper.Api"
RUN dotnet build "./NoteKeeper.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./NoteKeeper.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NoteKeeper.Api.dll"]
