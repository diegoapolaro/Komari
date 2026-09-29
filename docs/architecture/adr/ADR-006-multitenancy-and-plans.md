# ADR-006: Estratégia de Multi-Tenancy e Planos de Assinatura

## Status
Aceito

## Contexto
O Komari foi concebido como um modelo SaaS (Software as a Service) comercial. Um usuário proprietário (Dono) assina a plataforma e contrata um plano que define o limite de comércios/lojas (ex: pizzaria matriz e filiais). Além disso, cada restaurante possui seus próprios funcionários (gerentes, garçons, cozinheiros, entregadores), cardápios, mesas, pedidos e caixas, exigindo isolamento de dados rigoroso.

## Decisão
Adotamos o padrão de **Multi-tenancy com Banco Compartilhado e Esquema Compartilhado (Discriminator Column / `RestaurantId`)** gerenciado por **Global Query Filters do Entity Framework Core**:

1. **Hierarquia de Contas e Lojas**:
   - `User (Dono / Proprietário)`: Usuário titular da assinatura comercial.
   - `Plan / Subscription`: Define as cotas contratadas (ex: quantidade máxima de estabelecimentos ativos).
   - `Restaurant (Tenant)`: Cada unidade comercial cadastrada pelo dono.
   - `RestaurantMembership / Staff`: Associação de usuários colaboradores (garçons, cozinheiros, entregadores) ao restaurante do dono, com seu respectivo papel (`Role`).

2. **Isolamento de Dados no EF Core**:
   - Todas as entidades operacionais (`Category`, `Product`, `Table`, `Bill`, `Order`, `CashSession`) herdam uma propriedade `RestaurantId`.
   - Um serviço injetado por requisição (`ICurrentRestaurantContext`) extrai o identificador do restaurante atual a partir do Token JWT ou do cabeçalho `X-Restaurant-Id`.
   - O `DbContext` aplica automaticamente filtros globais (`builder.Entity<T>().HasQueryFilter(e => e.RestaurantId == currentRestaurantId)`), garantindo que consultas nunca vazem dados de um estabelecimento para outro.

## Consequências

### Positivas
- **Custo e Operação Otimizados**: Um único banco de dados PostgreSQL atende a múltiplos clientes e restaurantes, reduzindo custos com infraestrutura no início do SaaS.
- **Segurança Nativa**: O desenvolvedor não precisa lembrar de adicionar `.Where(x => x.RestaurantId == id)` em cada query; o EF Core injeta o filtro automaticamente em tempo de compilação da consulta.
- **Suporte a Franquias e Filiais**: O dono acessa um painel onde alterna entre seus restaurantes autorizados usando a mesma credencial.

### Negativas / Trade-offs
- Consultas administrativas globais de auditoria ou faturamento exigem desativação explícita do filtro com `.IgnoreQueryFilters()`.

## Alternativas Descartadas

- **Banco de Dados Isolado por Restaurante (Database per Tenant)**:
  - *Por que foi descartada:* Custo elevado e complexidade desnecessária para gerenciar centenas de conexões, migrations simultâneas e provisionamento dinâmico no estágio atual do projeto.
- **Mono-loja com deploy isolado por cliente**:
  - *Por que foi descartada:* Inviabilizaria o modelo SaaS escalável, onde novos restaurantes devem ser criados instantaneamente após o pagamento.
