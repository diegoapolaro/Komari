# Documentação do Komari

Bem-vindo à documentação oficial de engenharia e regras de negócio do **Komari**, uma plataforma SaaS de gestão empresarial e operacional para pizzarias, restaurantes e estabelecimentos físicos.

---

## 🧭 Estrutura da Documentação

A documentação está organizada em diretórios temáticos:

```text
docs/
├── README.md                           # Este documento (visão geral do sistema)
├── architecture/                       # Decisões de Arquitetura e Modelagem
│   ├── overview.md                     # Visão geral de arquitetura do sistema e monorepo
│   ├── domain-model.md                 # Diagrama ER expandido e agregados DDD
│   └── adr/                            # Architecture Decision Records (ADRs)
│       ├── ADR-001-clean-architecture-dotnet.md
│       ├── ADR-002-postgresql-efcore.md
│       ├── ADR-003-openapi-type-generation.md
│       ├── ADR-004-state-management-web-mobile.md
│       ├── ADR-005-jwt-auth-strategy.md
│       └── ADR-006-multitenancy-and-plans.md
└── business-rules/                     # Regras de Negócio e Ciclos de Vida
    ├── 01-cardapio-e-produtos.md       # Categorias, produtos, pizzas (Broto/Grande, frações) e adicionais
    ├── 02-mesas-e-comandas.md          # Mesas, comandas numeradas, avulsas e divisão de conta
    ├── 03-pedidos-e-fluxo-cozinha.md   # Estados do pedido e fluxo de preparo (KDS)
    ├── 04-delivery-e-clientes.md       # Clientes, entregas e taxas
    ├── 05-caixa-e-pagamentos.md        # Abertura, fechamento, sangrias, suprimentos e conciliação
    └── 06-usuarios-papeis-e-planos.md  # Contas de Dono, planos SaaS, franquias e perfis de equipe
```

---

## 🎯 Visão do Produto e Modelo SaaS

O **Komari** opera no modelo SaaS Multi-tenant:
1. **Dono do Estabelecimento**: Assina um plano que determina o limite de lojas ativas (franquias/filiais) e gerencia os acessos de sua equipe.
2. **Equipe Operacional**: Contas de garçons, cozinheiros (KDS), entregadores e operadores de caixa trabalham vinculadas ao restaurante do Dono.
3. **Cardápio Inteligente para Pizzarias**: Suporte a tamanhos customizados (Broto com até 2 sabores, Grande com até 3 sabores), adicionais e bordas configuráveis com precificação pela maior fatia.
4. **Gestão Flexível de Salão e Balcão**: Comandas numeradas universais (com ou sem mesa), geração fácil de mesas em lote e divisão de conta por pagantes.
5. **Cozinha (KDS) e Ponto de Venda (PDV)**: Fila em tempo real e conciliação de caixa com auditoria de sangrias/suprimentos.

---

## 🛠️ Stack Tecnológica

| Camada | Tecnologia | Motivação Principal |
| :--- | :--- | :--- |
| **Backend** | .NET 10 (C#) | LTS, alto desempenho, tipagem forte e ecossistema maduro para Clean Architecture |
| **Banco de Dados** | PostgreSQL 17 + EF Core | Transações ACID, multi-tenancy com filtros globais (`Global Query Filters`) |
| **Painel Web** | React 19 + TypeScript + Vite + TailwindCSS | DX rápida, desempenho ótimo e ecossistema moderno para SPAs |
| **App Mobile** | React Native + Expo (Expo Router) | Solução multiplataforma para garçons e entregadores, navegação baseada em arquivos |
| **Contratos API** | OpenAPI / Swagger | Fonte única de verdade para geração automatizada de tipos no Web e Mobile |
| **Estado Cliente** | TanStack Query + Zustand | Separação entre cache de servidor assíncrono e estado de UI síncrono |
