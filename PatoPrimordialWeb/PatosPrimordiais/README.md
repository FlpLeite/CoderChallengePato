# Sistema de Defesa Global - Guia de Execução

Este repositório contém dois projetos que trabalham em conjunto:

- **PatoPrimordialAPI** – API ASP.NET Core responsável pela ingestão de dados e exposição dos endpoints.
- **PatosPrimordiais** – aplicação web construída com React + Vite que consome a API para exibir os painéis.

O passo a passo abaixo descreve todo o fluxo necessário para clonar o repositório, preparar as dependências e executar tanto a API quanto o frontend.

## 1. Pré-requisitos

Antes de começar, garanta que o ambiente possui as ferramentas abaixo instaladas:

- [Git](https://git-scm.com/) para clonar o repositório.
- [PostgreSQL](https://www.postgresql.org/) (versão 13 ou superior) em execução e acessível localmente ou via rede.
- [.NET SDK 8.0](https://dotnet.microsoft.com/pt-br/download) para compilar e executar a API. O projeto **PatoPrimordialAPI** está direcionado para o `net8.0`.
- [Node.js](https://nodejs.org/) versão 18 ou superior (o Vite recomenda Node 18+) e o **npm** correspondente para executar o frontend React. As dependências estão descritas em `PatoPrimordialWeb/PatosPrimordiais/package.json`.

> 💡 Caso utilize Windows, é recomendado executar os comandos a seguir no **PowerShell** ou **Windows Terminal**. Em macOS/Linux, use o **Terminal** padrão.

## 2. Clonar o repositório

```bash
git clone https://github.com/<seu-usuario>/SistemaDefesaGlobal.git
cd SistemaDefesaGlobal
```

Os diretórios relevantes após o clone serão:

```
SistemaDefesaGlobal/
├── PatoPrimordialAPI          # API ASP.NET Core
├── PatoPrimordialAPI.Tests    # Projeto de testes da API (opcional)
└── PatoPrimordialWeb/
    └── PatosPrimordiais       # Frontend React + Vite
```

## 3. Configurar e executar a API (PatoPrimordialAPI)

1. **Configurar a conexão com o banco**
    - A API utiliza uma connection string chamada `Default`. Por padrão ela aponta para `Host=localhost;Port=5432;Database=patos_primordiais;Username=postgres;Password=40028922`. Ajuste usuário, senha, host e porta conforme o seu ambiente de PostgreSQL em `PatoPrimordialAPI/appsettings.Development.json` ou `appsettings.json`.
    - Alternativamente, defina a variável de ambiente `ConnectionStrings__Default` antes de executar a aplicação para evitar versionar senhas.

2. **Instalar as dependências .NET**
    - Abra um terminal na pasta da API:
      ```bash
      cd PatoPrimordialAPI
      dotnet restore
      ```

3. **Aplicar as migrações do Entity Framework**
    - Certifique-se de que o [dotnet-ef](https://learn.microsoft.com/ef/core/cli/dotnet) está disponível. Caso não esteja, instale com:
      ```bash
      dotnet tool install --global dotnet-ef
      ```
    - Ainda na pasta `PatoPrimordialAPI`, execute:
      ```bash
      dotnet ef database update
      ```
      Esse comando cria (ou atualiza) o banco de dados configurado, aplicando todas as migrações presentes em `PatoPrimordialAPI/Migrations`.

4. **Executar a API**
    - Com o banco preparado, suba a API:
      ```bash
      dotnet run
      ```
    - Por padrão a aplicação usa **Kestrel** e expõe a API em `http://localhost:5138` (a porta pode variar; verifique o terminal). A configuração de CORS permite que qualquer origem acesse os endpoints.
    - Com o ambiente de desenvolvimento, a documentação Swagger ficará disponível em `http://localhost:5138/swagger`.

## 4. Configurar e executar o frontend (PatosPrimordiais)

1. **Ajustar a URL da API**
    - O frontend lê a URL base da API da variável `VITE_API_BASE_URL`, caindo em `http://localhost:5138` se nada for informado.
    - Para garantir a comunicação, crie um arquivo `.env` dentro de `PatoPrimordialWeb/PatosPrimordiais` (mesmo nível do `package.json`) com o conteúdo:
      ```env
      VITE_API_BASE_URL=http://localhost:5138
      ```
      Ajuste a porta conforme a indicada pela API.

2. **Instalar dependências**
    - Em um novo terminal (ou outra aba) na pasta do frontend:
      ```bash
      cd PatoPrimordialWeb/PatosPrimordiais
      npm install
      ```

3. **Executar em modo desenvolvimento**
    - Após instalar as dependências, execute:
      ```bash
      npm run dev
      ```
    - O Vite iniciará um servidor (geralmente em `http://localhost:5173`). Abra o navegador e acesse a URL informada no terminal. Ao abrir a aplicação, ela buscará dados da API configurada anteriormente.

## 5. Testes (opcional)

- O projeto **PatoPrimordialAPI.Tests** contém os testes automatizados da API. Para executá-los, certifique-se de estar na pasta raiz do repositório e rode:
  ```bash
  dotnet test
  ```

## 6. Dicas e solução de problemas

- **Erro de conexão com o banco**: revise a connection string e verifique se o PostgreSQL está aceitando conexões (serviço ativo e porta liberada).
- **`dotnet ef` não encontrado**: confirme a instalação do `dotnet-ef` (reinicie o terminal após instalar, se necessário) ou execute `dotnet tool restore` caso utilize manifestos de ferramentas.
- **CORS ou falha ao buscar dados no frontend**: confirme que a API está em execução e que `VITE_API_BASE_URL` aponta para o mesmo host/porta exibidos por `dotnet run`.
- **Portas ocupadas**: se `dotnet run` ou `npm run dev` indicarem conflito de portas, finalize o processo que está usando a porta ou altere a configuração de URL/porta (via `appsettings.json` para a API e argumento `--port` no Vite).

Seguindo os passos acima, o ambiente completo do Sistema de Defesa Global estará pronto para uso local. Bons testes! 🚀