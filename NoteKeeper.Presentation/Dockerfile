FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NoteKeeper.Presentation/NoteKeeper.Presentation.csproj", "NoteKeeper.Presentation/"]
COPY ["NoteKeeper.Business/NoteKeeper.Business.csproj", "NoteKeeper.Business/"]
COPY ["NoteKeeper.DataAccess/NoteKeeper.DataAccess.csproj", "NoteKeeper.DataAccess/"]

COPY ["certificate.crt", "/usr/local/share/ca-certificates/certificate.crt"]
RUN update-ca-certificates

RUN dotnet restore "NoteKeeper.Presentation/NoteKeeper.Presentation.csproj"
COPY . .
WORKDIR "/src/NoteKeeper.Presentation"
RUN dotnet build "NoteKeeper.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "NoteKeeper.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "NoteKeeper.Presentation.dll"]
