# 💻 Sistema de Retirada de Notebooks

> Aplicação web full-stack para **controlar a retirada e a devolução de notebooks** em uma escola — do pedido do professor à devolução registrada pela gestão.

<p align="center">
  <img alt=".NET" src="https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white">
  <img alt="C#" src="https://img.shields.io/badge/C%23-ASP.NET%20Core-239120?logo=csharp&logoColor=white">
  <img alt="Angular" src="https://img.shields.io/badge/Angular-22-DD0031?logo=angular&logoColor=white">
  <img alt="PostgreSQL" src="https://img.shields.io/badge/PostgreSQL-Npgsql-4169E1?logo=postgresql&logoColor=white">
  <img alt="JWT" src="https://img.shields.io/badge/Auth-JWT-000000?logo=jsonwebtokens&logoColor=white">
  <img alt="Docker" src="https://img.shields.io/badge/Deploy-Docker-2496ED?logo=docker&logoColor=white">
</p>

<!-- 🔗 Demo ao vivo: adicione aqui o link quando quiser -->
<!-- 🖼️ Dica: coloque prints/GIF em uma pasta /docs e referencie aqui -->

---

## 🎯 O problema que ele resolve

Em uma escola, vários notebooks são emprestados a professores todos os dias. Esse controle era feito em uma **ficha de papel** — da solicitação até a devolução. O resultado:

- ❌ Difícil saber **quem está com qual equipamento** neste momento
- ❌ Nenhuma garantia de que o notebook foi realmente retirado ou devolvido
- ❌ Sem histórico confiável nem controle de atrasos

Este sistema substitui a ficha por um **fluxo digital rastreável**, com autenticação, aprovação de pedidos e controle de inventário em tempo real.

> 🎓 Projeto real, criado para resolver uma demanda do dia a dia de uma unidade escolar.

---

## Funcionalidades

- 🔐 **Autenticação com JWT** e dois perfis: **Comum** (professores) e **Admin** (gestão)
- 📝 **Cadastro com aprovação** — novos usuários entram como *pendentes* e só acessam depois que um admin aprova
- 🛒 **Inventário por carrinhos** — notebooks organizados em carrinhos, cada um com seu status
- 📨 **Solicitação de retirada** — o professor pede um ou mais notebooks informando as aulas previstas
- ✅ **Fluxo de aprovação/devolução** — o admin aprova, recusa (com motivo) ou registra a devolução
- ⏰ **Previsão de devolução e atraso** — calculado automaticamente a partir das aulas previstas (50 min por aula)
- 🧰 **Gestão completa** — CRUD de notebooks e usuários restrito a administradores

---

## 🔄 Como o sistema funciona

Do cadastro à devolução, o caminho que um professor percorre:

```
1. Professor se cadastra ........................ Usuário: PendenteAprovacao
2. Admin aprova o cadastro ...................... Usuário: Ativo  → já pode logar
3. Professor cria uma solicitação de notebook ... Solicitação: Pendente
4. Admin aprova ................................. Solicitação: EmUso   | Notebook: Emprestado
   ou recusa (com motivo) ....................... Solicitação: Recusada
5. Admin registra a devolução ................... Solicitação: Devolvida | Notebook: Disponivel
```

### Máquina de estados da solicitação

```
          aprovar                 devolver
Pendente ──────────►  EmUso  ──────────────►  Devolvida
   │
   │ recusar(motivo)
   ▼
Recusada
```

As transições são **garantidas pelo domínio**: tentar devolver algo que não está em uso, ou aprovar algo já aprovado, lança uma exceção com mensagem clara. A regra de negócio vive no modelo (`Solicitacao.cs`), não espalhada pelos controllers.

### Estados possíveis de um notebook

`Disponivel` · `Emprestado` · `EmManutencao` · `ManutencaoSemGarantia` · `Quebrado` · `ChamadoAberto` · `UsoAdministrativo` · `Furtado`

---

## 🛠️ Stack e decisões técnicas

| Camada | Tecnologia | Por quê |
|--------|-----------|---------|
| **Back-end** | C# / ASP.NET Core Web API (.NET 10) | API robusta, tipada e com ótimo ecossistema |
| **ORM / Banco** | Entity Framework Core + PostgreSQL (Npgsql) | Migrations versionadas e banco relacional confiável |
| **Autenticação** | JWT (JSON Web Tokens) | Stateless, ideal para separar front e back |
| **Front-end** | Angular 22 (standalone components) | SPA organizada, com rotas, guards e injeção de dependência |
| **Deploy** | Docker (back-end) + build estático (front-end) | Portável e fácil de publicar em qualquer cloud |

---

## 🧱 Arquitetura

```
┌─────────────────────┐        HTTP + JWT        ┌──────────────────────────┐        ┌──────────────┐
│   Angular 22 (SPA)  │ ───────────────────────► │  ASP.NET Core Web API    │ ─────► │  PostgreSQL  │
│  rotas · guards ·   │ ◄─────────────────────── │  controllers · domínio · │ ◄───── │  (EF Core)   │
│  interceptor JWT    │        JSON              │  EF Core · JWT           │        │              │
└─────────────────────┘                          └──────────────────────────┘        └──────────────┘
    localhost:4200                                      localhost:5172
```

---

## 📁 Estrutura do projeto

```
sistema-retirada-notebooks/
├── Dockerfile                        # Build do back-end (imagem .NET)
├── railway.json                      # Exemplo de config de deploy do back-end
├── SistemaRetiradaNotebooks.slnx
└── src/
    ├── SistemaRetiradaNotebooks.Api/ # 🔙 BACK-END (ASP.NET Core)
    └── SistemaRetiradaNotebooks.Web/ # 🔜 FRONT-END (Angular)
```

<details>
<summary><strong>🔙 Back-end — <code>src/SistemaRetiradaNotebooks.Api/</code></strong></summary>

| Pasta / Arquivo | O que faz |
|-----------------|-----------|
| `Program.cs` | Ponto de entrada. Configura CORS, JWT, EF Core, aplica migrations e popula dados iniciais. |
| **`Models/`** | Entidades do domínio. |
| ├─ `Usuario.cs` | Usuário do sistema (nome, e-mail, hash de senha, perfil e status de cadastro). |
| ├─ `Carrinho.cs` | Carrinho que agrupa notebooks. |
| ├─ `Notebook.cs` | Notebook do inventário (modelo, série, patrimônio, garantia, status). |
| ├─ `Solicitacao.cs` | Pedido de retirada **+ máquina de estados** (`Aprovar`/`Devolver`/`Recusar`) e cálculo de atraso. |
| ├─ `SolicitacaoNotebook.cs` | Relação N:N entre solicitação e notebooks. |
| └─ `Enums.cs` | Enums de perfil, status de cadastro, status de notebook e status de solicitação. |
| **`Dtos/`** | Objetos de entrada/saída da API — isolam a API do modelo interno. |
| **`Controllers/`** | Endpoints HTTP (`Auth`, `Usuarios`, `Carrinhos`, `Notebooks`, `Solicitacoes`). |
| **`Data/`** | `AppDbContext.cs` (contexto EF Core) e `DbSeeder.cs` (dados de exemplo na 1ª execução). |
| **`Services/`** | `TokenService.cs` — geração dos tokens JWT no login. |
| **`Migrations/`** | Histórico de migrations do EF Core. |

</details>

<details>
<summary><strong>🔜 Front-end — <code>src/SistemaRetiradaNotebooks.Web/</code></strong></summary>

| Pasta / Arquivo | O que faz |
|-----------------|-----------|
| `src/app/app.routes.ts` | Mapa de rotas, com guards de autenticação e de admin. |
| `src/app/app.config.ts` | Configuração raiz (HTTP client, interceptors, router). |
| **`pages/`** | Telas: `login/`, `cadastro/`, `notebooks/`, `solicitacoes/`, `admin/`. |
| **`components/`** | `shell/` (layout base) e `navbar/` (barra de navegação). |
| **`services/`** | Comunicação com a API (`auth`, `notebooks`, `solicitacoes`, `usuarios`). |
| **`guards/`** | `authGuard` (exige login) e `adminGuard` (exige perfil Admin). |
| **`interceptors/`** | `auth.interceptor` injeta o token JWT em toda requisição. |
| **`models/`** | Tipagens TypeScript compartilhadas entre as telas. |
| **`environments/`** | `environment.ts` (dev) e `environment.prod.ts` (prod) — URL da API por ambiente. |

</details>

---

## 🌐 Endpoints da API

| Método | Rota | Acesso |
|--------|------|--------|
| `POST` | `/api/auth/registrar` | Público |
| `POST` | `/api/auth/login` | Público |
| `GET` | `/api/notebooks`, `/api/notebooks/por-carrinho`, `/api/notebooks/{id}` | Autenticado |
| `POST` `PUT` `DELETE` | `/api/notebooks` | Admin |
| `GET` | `/api/carrinhos`, `/api/carrinhos/{id}` | Autenticado |
| `POST` `PUT` `DELETE` | `/api/carrinhos` | Admin |
| `GET` | `/api/solicitacoes`, `/api/solicitacoes/{id}` | Autenticado |
| `POST` | `/api/solicitacoes` | Autenticado |
| `POST` | `/api/solicitacoes/{id}/aprovar` · `/devolver` · `/recusar` | Admin |
| `GET` `PUT` `DELETE` | `/api/usuarios...` (incl. `/aprovar`, `/recusar`) | Admin |
| `GET` | `/health` | Público (healthcheck) |

---

## 🚀 Como rodar localmente

### Pré-requisitos
- [.NET SDK 10](https://dotnet.microsoft.com/)
- [Node.js](https://nodejs.org/) + npm (Angular CLI 22)
- [PostgreSQL](https://www.postgresql.org/) rodando localmente

### 1. Back-end

Configure os segredos locais **sem tocar no `appsettings.json`** (recomendado — usa o *User Secrets* do .NET):

```bash
cd src/SistemaRetiradaNotebooks.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=sistema_retirada;Username=postgres;Password=SUA_SENHA"
dotnet user-secrets set "Jwt:Key" "uma-chave-aleatoria-longa-com-32-caracteres-ou-mais"
dotnet user-secrets set "Admin:Senha" "SuaSenhaDeAdmin"
```

Depois, rode a API:

```bash
dotnet run --project src/SistemaRetiradaNotebooks.Api
```

Na primeira execução, o sistema cria o banco (migrations), cadastra o admin e popula dados de exemplo. API em **`http://localhost:5172`** (docs OpenAPI em ambiente de desenvolvimento).

### 2. Front-end

```bash
cd src/SistemaRetiradaNotebooks.Web
npm install
npm start
```

Aplicação em **`http://localhost:4200`**, consumindo a API local.

### Login inicial
Use o e-mail e a senha de admin que você configurou em `Admin:Email` / `Admin:Senha`.

---

## ⚙️ Configuração (variáveis de ambiente do back-end)

| Variável | Descrição |
|----------|-----------|
| `ConnectionStrings__DefaultConnection` / `DATABASE_URL` | Conexão com o PostgreSQL |
| `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience` | Configuração do token JWT |
| `Admin__Email`, `Admin__Senha` | Credenciais do administrador inicial |
| `AllowedOrigins` | Origens liberadas no CORS (separadas por vírgula) |
| `PORT` | Porta de escuta (injetada por alguns provedores de cloud) |

> 🔒 **Segurança:** este repositório usa apenas valores de exemplo/placeholder. Os dados de inventário do seeder são **fictícios**, e nenhum segredo real está versionado.

---

<p align="center">
  Desenvolvido por <strong>Roberto Almeida</strong> — projeto de aplicação real em ambiente escolar.
</p>
