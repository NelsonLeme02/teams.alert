# Usa a imagem oficial do .NET SDK para compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia os arquivos do projeto e restaura as dependências
COPY *.csproj ./
RUN dotnet restore

# Copia o código fonte e compila
COPY . ./
RUN dotnet publish -c Release -o /out

# Usa a imagem base oficial do Google Cloud Functions para execução
FROM gcr.io/google-appengine/aspnetcore:8.0 AS runtime
WORKDIR /app
COPY --from=build /out .

# Define o ponto de entrada para a função
CMD ["dotnet", "rpnet.teamsalert.function.dll"]
