# Usa a imagem oficial do .NET SDK para compilar
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copia os arquivos do projeto e restaura as dependências
COPY rpnet.teamsalert.function/rpnet.teamsalert.function.csproj ./
RUN dotnet restore

# Copia o código fonte e compila
COPY . ./
RUN dotnet publish -c Release -o /out

# Usa a imagem base oficial do Google Cloud Functions para execução
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /out .

# Define o ponto de entrada para a função
CMD ["dotnet", "rpnet.teamsalert.function.dll"]
