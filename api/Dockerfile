# Stage 1 Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

ARG CONFIGURATION=Release
ARG BUILD_VERSION
# If BUILD_VERSION is set/non-empty, use it, otherwise use a default value
ARG VERSION=${BUILD_VERSION:-0.1.0}

# Copy the main source project files
COPY *.csproj ./

RUN dotnet restore 

# Copy everything else and build
COPY . ./

RUN dotnet build /p:Version=$VERSION -c ${CONFIGURATION}

RUN dotnet publish -c ${CONFIGURATION} -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
# Valor default Production

ENV ASPNETCORE_URLS http://+:5000
ENV LD_LIBRARY_PATH='/app'
COPY --from=build /app/out .

EXPOSE 5000

ENTRYPOINT ["dotnet", "api.dll"]