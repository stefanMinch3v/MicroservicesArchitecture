FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["TaskTronic.Drive/TaskTronic.Drive.csproj", "TaskTronic.Drive/"]
COPY ["TaskTronic/TaskTronic.csproj", "TaskTronic/"]
RUN dotnet restore "TaskTronic.Drive/TaskTronic.Drive.csproj"
COPY . .
WORKDIR "/src/TaskTronic.Drive"
RUN dotnet build "TaskTronic.Drive.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskTronic.Drive.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskTronic.Drive.dll"]