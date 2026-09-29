# ADR-001: Adoção de Clean Architecture no Backend (.NET 10)

## Status
Aceito

## Contexto
O Komari é um sistema de gestão operacional para restaurantes e pizzarias, envolvendo regras de negócio críticas (ciclo de vida de pedidos, conciliação de caixa, comandas e estoque). O backend precisa ser testável, sustentável ao longo do tempo e desacoplado de detalhes de implementação como banco de dados e frameworks web. Além disso, o projeto servirá como portfólio de engenharia de software para estágio.

## Decisão
Adotamos os princípios da **Clean Architecture** (Arquitetura Limpa), particionando o backend em 4 camadas bem delimitadas:

1. **`Komari.Domain`**: O núcleo do sistema. Contém entidades puras de negócio (`Product`, `Category`, `Table`), enums e contratos de repositório (`IProductRepository`). Não depende de nenhum outro projeto ou framework de persistência.
2. **`Komari.Application`**: Casos de uso e orquestração de negócios. Contém serviços de aplicação, DTOs de entrada/saída, validações e mapeamentos. Depende apenas do `Domain`.
3. **`Komari.Infrastructure`**: Implementação de detalhes técnicos. Contém o `ApplicationDbContext` (EF Core), mapeamentos ORM, repositórios concretos e integrações externas. Depende de `Domain` e implementa interfaces da `Application`.
4. **`Komari.Api`**: Ponto de entrada HTTP (ASP.NET Core). Contém Controllers REST finos, filtros, middlewares e configuração de injeção de dependências.

## Consequências

### Positivas
- **Independência de Frameworks**: As regras de negócio não dependem do ASP.NET Core nem do EF Core.
- **Alta Testabilidade**: Casos de uso e entidades podem ser testados com testes unitários puros sem necessidade de subir banco em memória ou mocks complexos de HTTP.
- **Clareza de Responsabilidades**: Facilidade para novos desenvolvedores localizarem onde uma regra deve ser implementada ou alterada.

### Negativas / Trade-offs
- Maior número inicial de arquivos e classes (DTOs, interfaces, mapeamentos) em comparação a uma estrutura simples em camada única.

## Alternativas Descartadas

- **N-Tier Tradicional (Controller -> Service -> Repository com Entidades do Banco vazando em tudo)**:
  - *Por que foi descartada:* Tende ao modelo anêmico de domínio, onde entidades do banco viram apenas sacos de `get; set;` e regras de negócio ficam espalhadas por controllers e services sem encapsulamento.
- **Minimal APIs Monolíticas em um único projeto**:
  - *Por que foi descartada:* Embora rápida para provas de conceito, mistura rotas HTTP, acesso a banco e lógica em pouquíssimos arquivos, dificultando a manutenção e a explicação conceitual em entrevistas técnicas.
