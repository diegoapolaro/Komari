# Komari 🍕

> Plataforma SaaS de gestão empresarial e operacional especializada em pizzarias, restaurantes e estabelecimentos físicos.

O **Komari** integra em uma única solução moderna e escalável toda a esteira operacional de um restaurante: cardápio dinâmico com regras de frações e adicionais, gestão flexível de mesas e comandas, fluxo de cozinha (KDS) em tempo real, delivery com geolocalização e frente de caixa (PDV) com conciliação financeira e auditoria de sangrias.

Projetado sob a arquitetura **Multi-tenant**, o sistema permite que estabelecimentos gerenciem matrizes e franquias com controle de planos e permissões granulares por função da equipe.

---

## 📌 Sumário

- [Visão Geral dos Módulos](#-visão-geral-dos-módulos)
- [Estrutura do Monorepo](#-estrutura-do-monorepo)
- [Stack Tecnológica](#-stack-tecnológica)
- [Arquitetura e Boas Práticas](#-arquitetura-e-boas-práticas)
- [Como Executar Localmente](#-como-executar-localmente)
  - [Pré-requisitos](#pré-requisitos)
  - [1. Banco de Dados (PostgreSQL via Docker)](#1-banco-de-dados-postgresql-via-docker)
  - [2. Backend (.NET 10 API)](#2-backend-net-10-api)
  - [3. Painel Web (React + Vite)](#3-painel-web-react--vite)
  - [4. Aplicativo Mobile (React Native + Expo)](#4-aplicativo-mobile-react-native--expo)
- [Documentação Técnica](#-documentação-técnica)

---

## 🎯 Visão Geral dos Módulos

- **Cardápio Inteligente**: Gestão de categorias e produtos com suporte nativo a pizzas fracionadas (Broto com até 2 sabores, Grande com até 3 sabores), cálculo pelo maior valor de fatia, bordas recheadas e adicionais customizáveis.
- **Salão, Mesas e Comandas**: Comandas numeradas universais utilizáveis tanto vinculadas a mesas quanto em atendimento avulso/balcão, geração de mesas em lote e divisão de conta flexível por pagantes.
- **KDS & Cozinha**: Fila de pedidos em tempo real com ciclo de vida rigoroso (`Pending` ➔ `InPreparation` ➔ `Ready` ➔ `OutForDelivery` ➔ `Delivered` / `Cancelled`).
- **Delivery & Clientes**: Cadastro simplificado de clientes, múltiplos endereços e cálculo de taxa de entrega configurável por distância ou raio fixo.
- **Frente de Caixa (PDV) & Financeiro**: Abertura e fechamento de turno, conferência cega de valores, sangrias, suprimentos e pagamento particionado (múltiplas formas de pagamento no mesmo pedido).
- **Multi-Tenancy e Assinaturas**: Isolamento estrito de dados por estabelecimento via `TenantId` no banco, limite de unidades por plano do Dono e perfis de acesso (Dono, Gerente, Garçom, Cozinha, Caixa, Entregador).

---

## 📂 Estrutura do Monorepo

```text
komari-app/
├── api/                    # Backend ASP.NET Core (.NET 10 LTS, C#)
│   ├── Komari.Domain/      # Entidades de domínio, Enums, Regras de negócio puras
│   ├── Komari.Application/ # Use cases, DTOs, interfaces e orquestração
│   ├── Komari.Infrastructure/ # EF Core, PostgreSQL, Repositórios, JWT, Hash
│   └── Komari.Api/         # Controllers REST, Middlewares, Configurações Swagger
├── web/                    # Painel Administrativo e PDV (React 19, TypeScript, Vite)
├── mobile/                 # App de Garçons e Entregadores (React Native, Expo, Expo Router)
├── docs/                   # Documentação detalhada de regras e decisões arquiteturais
│   ├── architecture/       # ADRs, visão geral de arquitetura e modelo de dados DDD
│   └── business-rules/     # Especificação detalhada de cada fluxo de negócio
└── docker-compose.yml      # Infraestrutura local de banco de dados
```

---

## 🛠️ Stack Tecnológica

| Camada | Tecnologias | Justificativa / Papel |
| :--- | :--- | :--- |
| **Backend** | .NET 10 (C#), ASP.NET Core | Alto desempenho, suporte LTS, tipagem robusta e padrão Clean Architecture |
| **Banco de Dados** | PostgreSQL 17 + EF Core | Confiabilidade ACID, migrações versionadas e isolamento multi-tenant via *Global Query Filters* |
| **Painel Web** | React 19, TypeScript, Vite, TailwindCSS | SPA moderna, carregamento instantâneo via Vite e estilização utilitária ágil |
| **App Mobile** | React Native, Expo, Expo Router | Experiência nativa para garçons e entregadores, com roteamento declarativo por arquivos |
| **Gerenciamento de Estado** | TanStack Query + Zustand | TanStack Query para cache e sincronização assíncrona; Zustand para estados locais síncronos da UI |
| **Contratos de API** | OpenAPI (Swagger) | Fonte única da verdade para tipagem, eliminando discrepâncias entre frontend e backend |

---

## 🏛️ Arquitetura e Boas Práticas

- **Clean Architecture no Backend**: Divisão estrita de responsabilidades entre *Domain*, *Application*, *Infrastructure* e *Api*. O núcleo do domínio é isolado de frameworks externos e bibliotecas de persistência.
- **DTOs e Isolamento**: Endpoints nunca expõem entidades de banco diretamente. Todas as entradas e saídas utilizam DTOs validados.
- **Contratos Tipados de Ponta a Ponta**: Tipos TypeScript no front-end são sincronizados a partir da especificação OpenAPI gerada pelo backend, evitando interfaces manuais sujeitas a erros humanos.
- **Segurança e Isolamento**: Autenticação stateless via JWT, hash de senhas criptograficamente seguro e garantia de multi-tenancy a nível de ORM (nenhuma consulta vaza dados entre estabelecimentos).
- **Sem Segredos em Código**: Todas as chaves e connection strings são fornecidas por variáveis de ambiente via arquivos `.env` ignorados pelo versionamento.

---

## 🚀 Como Executar Localmente

### Pré-requisitos

- [Git](https://git-scm.com/)
- [Docker & Docker Compose](https://www.docker.com/)
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (versão LTS 20+)

---

### 1. Banco de Dados (PostgreSQL via Docker)

Suba o container do PostgreSQL configurado no projeto:

```bash
docker compose up -d
```

O banco de dados estará acessível em `localhost:5432` com credenciais padrão (`komari_db`, usuário `postgres`, senha `postgres`).

---

### 2. Backend (.NET 10 API)

Entre no diretório da API e execute o projeto:

```bash
cd api
dotnet restore
dotnet run --project Komari.Api
```

A API iniciará e a documentação interativa do **Swagger** estará disponível em:
👉 `http://localhost:5000/swagger` ou `https://localhost:5001/swagger`

---

### 3. Painel Web (React + Vite)

Em outro terminal, instale as dependências e inicie o servidor de desenvolvimento:

```bash
cd web
npm install
npm run dev
```

Acesse o painel em:
👉 `http://localhost:5173`

---

### 4. Aplicativo Mobile (React Native + Expo)

Em outro terminal, configure as dependências do app:

```bash
cd mobile
npm install
npm start
```

> **Nota para execução em dispositivo físico**: Como o celular não acessa `localhost` da sua máquina host, aponte a URL da API para o IP local da sua máquina na rede local (ex: `http://192.168.x.x:5000`).

---

## 📚 Documentação Técnica

Para entender a fundo as regras de negócio e as decisões técnicas tomadas, consulte o diretório [`docs/`](./docs/README.md):

- [Visão Geral de Arquitetura](./docs/architecture/overview.md)
- [Modelo de Domínio e Entidades (DDD)](./docs/architecture/domain-model.md)
- [Registros de Decisão de Arquitetura (ADRs)](./docs/architecture/adr/)
- [Regras de Negócio e Ciclos de Vida](./docs/business-rules/)

---

## 📄 Licença

Este projeto é desenvolvido para fins educacionais e de portfólio profissional.
