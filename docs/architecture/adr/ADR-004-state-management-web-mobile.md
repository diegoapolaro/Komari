# ADR-004: Gerenciamento de Estado com TanStack Query e Zustand

## Status
Aceito

## Contexto
Tanto o painel gerencial Web (React 19) quanto o aplicativo Mobile (React Native + Expo) lidam com dois tipos fundamentalmente distintos de estado:
1. **Server State (Estado de Servidor)**: Dados que pertencem à API remota (produtos, mesas, comandas, relatórios), sujeitos a latência, cache, revalidação e dessincronização por mutações concorrentes.
2. **Client State (Estado de Cliente)**: Dados efêmeros da interface e do usuário local (tema claro/escuro, gaveta aberta/fechada, token de autenticação, filtros selecionados).

## Decisão
Adotamos uma divisão clara e estrita de bibliotecas de estado:

- **TanStack Query (React Query)**: Responsável **exclusivamente** por Server State:
  - Cacheamento de requisições HTTP.
  - Invalidação automática após mutações (`useMutation` -> `invalidateQueries`).
  - Estados nativos de carregamento e erro (`isLoading`, `isError`, `data`).
  - Polling e refetch em segundo plano (essencial para KDS e status de mesas).
- **Zustand**: Responsável **exclusivamente** por Client State global:
  - Tokens de autenticação e sessão do usuário.
  - Estado do carrinho local de pedido antes do envio para a API.
  - Preferências da interface de usuário.

**Regra mandatória do projeto**: Nunca armazenar respostas de requisições da API dentro do Zustand; use o cache do TanStack Query.

## Consequências

### Positivas
- **Eliminação de Sincronização Manual**: O TanStack Query cuida de desduplicar requisições, refetch em foco de janela e expiração de cache.
- **Zustand Simples e Leve**: O Zustand atua apenas onde é realmente necessário compartilhar estado entre componentes sem prop drilling, sem o peso ou boilerplate de bibliotecas maiores.
- **Previsibilidade**: Código limpo e padronizado em todo o frontend (web e mobile).

### Negativas / Trade-offs
- Os desenvolvedores precisam ter discernimento claro sobre a fronteira entre dados de servidor e estado de UI.

## Alternativas Descartadas

- **Redux / Redux Toolkit**:
  - *Por que foi descartada:* Boilerplate elevado (actions, reducers, slices), curva de aprendizado mais íngreme e mistura desnecessária de cache de rede com estado de UI.
- **Context API pura do React para tudo**:
  - *Por que foi descartada:* Causa re-renderizações desnecessárias de árvores inteiras de componentes e não possui suporte nativo a cache assíncrono ou invalidação de requisições.
