# Komari

Sistema de operações para pizzarias e restaurantes (pedidos, cardápio, comandas, delivery, caixa), com API, painel web e app mobile.

## Sobre o desenvolvedor

Estudante de Ciência da Computação, aprendendo enquanto constrói. Tem experiência com C#/.NET e React (web), mas é a primeira vez com React Native. Este projeto também é peça de portfólio para estágio, então ele precisa conseguir explicar cada decisão técnica.

## Estrutura (monorepo)

- `api/`  ASP.NET Core (.NET 10, LTS), C#
- `web/`  React 19 + TypeScript + Vite
- `mobile/`  React Native + Expo (TypeScript, Expo Router)
- `docs/`  regras de negócio e decisões de arquitetura

## Stack

- Backend: .NET 10, Entity Framework Core, PostgreSQL, Swagger/OpenAPI
- Web: React 19, TypeScript, Vite, TailwindCSS, TanStack Query
- Mobile: React Native (via Expo), TypeScript, Zustand, TanStack Query
- Lint/format: ESLint + Prettier nas pastas `web/` e `mobile/`
- Não trocar de tecnologia nem de versão major sem perguntar antes.

## Como trabalhar comigo (regras mais importantes)

1. **Uma tarefa pequena por vez.** Não construa módulos inteiros de uma vez. Se o pedido for grande, proponha um plano em etapas e espere minha aprovação.
2. **Explique o porquê.** Para cada arquivo criado ou decisão tomada, explique em poucas linhas por que foi feito assim e qual a alternativa descartada.
3. **Não instale dependências novas sem avisar** e justificar.
4. **Não altere pastas que eu não pedi.** Se a tarefa é da `api/`, não mexa em `web/` nem `mobile/`.
5. Sugira uma mensagem de commit ao final de cada tarefa concluída.
6. Se algo for ambíguo, pergunte antes de assumir.

## Convenções gerais

- Código (nomes de classes, variáveis, funções, rotas) em **inglês**. Textos visíveis ao usuário em **português (pt-BR)**.
- Nomes descritivos, sem abreviações obscuras.
- Nada de segredos no código (senhas, chaves, connection strings). Use variáveis de ambiente e mantenha `.env` fora do Git.
- Validar toda entrada do usuário na API, mesmo que o front já valide.

## Backend (`api/`)

- Clean Architecture: separar Domain, Application, Infrastructure e API.
- Controllers/endpoints finos. Regra de negócio fica na camada Application/Domain.
- Nunca expor entidades diretamente: use DTOs de entrada e saída.
- Acesso a dados só via repositórios/EF Core, com migrations versionadas.
- Senhas com hash (nunca texto puro). Autenticação com JWT.
- Toda rota documentada no Swagger, porque o front gera tipos a partir do schema OpenAPI.
- Usar `async/await` de ponta a ponta e `CancellationToken` nos endpoints.

## Web (`web/`) e Mobile (`mobile/`)

- TypeScript estrito. **Proibido `any`**; se o tipo for desconhecido, use `unknown` e faça o narrowing.
- Tipos da API são gerados a partir do OpenAPI, sem reescrever à mão.
- Componentes funcionais com hooks. Não usar `useMemo`/`useCallback` sem motivo medido (o React Compiler cuida disso).
- Dados do servidor com TanStack Query; estado local/global com Zustand. Não misturar os dois.
- Enums de domínio (ex: status do pedido) centralizados em um único lugar, sem strings soltas.
- Mobile: navegação com Expo Router. No celular, `localhost` não alcança minha máquina: a URL da API vem de variável de ambiente (IP da rede local em desenvolvimento).
- Rodar `npm run lint` antes de considerar a tarefa pronta.

## Domínio (termos do negócio)

Produto/Item do cardápio, Categoria, Pedido, Comanda, Mesa, Entrega, Cliente, Caixa. Status de pedido: `Pending`, `InPreparation`, `Ready`, `OutForDelivery`, `Delivered`, `Cancelled`.

## Definição de pronto

Compila sem erros, lint passa, endpoint/tela testado manualmente, e eu consegui entender o que foi feito.