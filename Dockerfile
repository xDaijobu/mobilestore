FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /src

COPY . ./

RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /src
COPY --from=build-env /app/out .

ARG PORT
ENV PORT $PORT
ENV ASPNETCORE_URLS=http://+:$PORT

EXPOSE $PORT/tcp

# TODO: remove debug
RUN echo "ASPNETCORE_URLS: [$ASPNETCORE_URLS]"
RUN echo "PORT: [$PORT]"

# FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
# WORKDIR /src
# COPY ["PlaystoreAPI.csproj", "."]
# RUN dotnet restore "./PlaystoreAPI.csproj"
# COPY . .
# WORKDIR "/src/."
# RUN dotnet build "PlaystoreAPI.csproj" -c Release -o /app/build

# FROM build AS publish
# RUN dotnet publish "PlaystoreAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PlaystoreAPI.dll"]