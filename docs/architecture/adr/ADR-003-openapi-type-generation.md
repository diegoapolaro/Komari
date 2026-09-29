# ADR-003: Geração Automatizada de Tipos e Contratos com OpenAPI / Swagger

## Status
Aceito

## Contexto
O projeto é composto por três aplicações: API (.NET), Web (React) e Mobile (React Native). Quando novos campos são adicionados ou alterados na API, escrever manualmente os tipos equivalentes em TypeScript nas pastas `web/` e `mobile/` introduz propensão a erros de digitação, tipos desatualizados e tempo desperdiçado em manutenção repetitiva.

## Decisão
A documentação da API gerada pelo **OpenAPI (Swagger)** no `Komari.Api` será a **Única Fonte da Verdade (Single Source of Truth)** para os contratos de comunicação.

- Todo endpoint e DTO na API deve ser devidamente documentado com anotações de OpenAPI e XML comments.
- As aplicações `web/` e `mobile/` utilizam ferramentas de geração de código (ex: `openapi-typescript` ou similar) para produzir interfaces TypeScript diretamente a partir do schema JSON exportado pela API.
- É expressamente proibido escrever interfaces manuais duplicadas de DTOs já existentes na API.

## Consequências

### Positivas
- **Segurança de Tipos de Ponta a Ponta**: Se um campo mudar de nome ou tipo na API, a compilação do TypeScript no frontend falhará imediatamente (`tsc --noEmit`), apontando onde a tela precisa ser ajustada.
- **Zero Duplicação de Código de Contratos**: Reduz o esforço cognitivo do desenvolvedor.
- **Documentação Sempre Viva**: A especificação OpenAPI precisa estar sempre acurada para que os frontends continuem funcionando.

### Negativas / Trade-offs
- Exige que a API esteja em execução ou gere o arquivo `swagger.json` antes de rodar o script gerador nos clientes frontend.

## Alternativas Descartadas

- **Escrever interfaces TypeScript manualmente**:
  - *Por que foi descartada:* Frágil, suscetível a desalinhamento de campos (`null` vs `undefined`, nomes em camelCase vs PascalCase) e causa retrabalho constante.
- **Monorepo com tRPC / GraphQL**:
  - *Por que foi descartada:* tRPC é centrado no ecossistema Node.js (não combina nativamente com backend C# .NET). GraphQL traria sobrecarga de infraestrutura desnecessária para o tamanho da aplicação.
