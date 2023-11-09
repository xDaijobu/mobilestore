#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

# FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
# WORKDIR /app
# EXPOSE 8080
# EXPOSE 443

# FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
# WORKDIR /src
# COPY ["PlaystoreAPI.csproj", "."]
# RUN dotnet restore "./PlaystoreAPI.csproj"
# COPY . .
# WORKDIR "/src/."
# RUN dotnet build "PlaystoreAPI.csproj" -c Release -o /app/build

# FROM build AS publish
# RUN dotnet publish "PlaystoreAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# FROM base AS final
# WORKDIR /app
# COPY --from=publish /app/publish .
# ENTRYPOINT ["dotnet", "PlaystoreAPI.dll"]



FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /src
COPY ["PlaystoreAPI.csproj", "."]

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