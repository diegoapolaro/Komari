# ADR-002: Persistência com PostgreSQL e Entity Framework Core

## Status
Aceito

## Contexto
O sistema gerencia dados altamente estruturados e relacionais: produtos associados a categorias, pedidos vinculados a comandas/mesas com múltiplos itens, movimentações financeiras de caixa e pagamentos. A integridade dos dados e o suporte a transações ACID são requisitos obrigatórios para evitar inconsistências como cobranças incorretas ou quebras contábeis.

## Decisão
Utilizaremos **PostgreSQL 17** como Sistema Gerenciador de Banco de Dados Relacional (SGBD) e **Entity Framework Core** como ORM (Object-Relational Mapper), com migrações de esquema controladas via EF Core Migrations.

- O banco é provisionado localmente via `docker-compose.yml` com volume persistente.
- As configurações de mapeamento de entidades são isoladas em classes `IEntityTypeConfiguration<T>` dentro da `Komari.Infrastructure`.
- Repositórios encapsulam as queries EF Core, expondo apenas coleções de entidades ou operações bem definidas para a camada de Application.

## Consequências

### Positivas
- **Garantias ACID**: Transações atômicas nativas para fechamento de conta, baixa de itens e pagamentos.
- **Rastreabilidade de Esquema**: As migrações (`dotnet ef migrations add`) ficam versionadas no Git, permitindo deploy determinístico em qualquer ambiente.
- **Produtividade com Tipagem**: O EF Core permite consultas LINQ fortemente tipadas em C# com validação em tempo de compilação.

### Negativas / Trade-offs
- Curva de aprendizado para mapear entidades com encapsulamento rico (construtores protegidos, backing fields, setters privados) sem expor propriedades indevidamente.
- Necessidade de Docker em ambiente de desenvolvimento local para rodar o contêiner do Postgres.

## Alternativas Descartadas

- **Bancos NoSQL baseados em documentos (ex: MongoDB)**:
  - *Por que foi descartada:* Falta de suporte maduro a chaves estrangeiras rígidas e complexidade desnecessária para garantir consistência relacional estrita em cenários de fechamento de caixa e pagamentos parciais.
- **Micro-ORMs puros (ex: Dapper)**:
  - *Por que foi descartada:* Exigiria escrita manual de todos os scripts SQL de migração e mapeamentos de cada entidade. O EF Core no .NET 10 possui excelente performance para a escala do projeto, mantendo grande produtividade.
