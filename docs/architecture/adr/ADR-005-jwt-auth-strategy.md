# ADR-005: Estrutura de Autenticação Stateless com JWT e RBAC

## Status
Aceito

## Contexto
O ecossistema do Komari atende a múltiplos clientes simultaneamente: o painel administrativo Web (acessado via navegadores desktop/tablets) e o aplicativo operacional Mobile (executado em smartphones Android e iOS pela equipe de garçons e entregadores). O sistema requer uma estratégia de autenticação unificada, segura e compatível com clientes móveis nativos e aplicações web modernas.

## Decisão
Adotamos autenticação **Stateless baseada em JSON Web Tokens (JWT)** combinada com **Controle de Acesso Baseado em Papéis (RBAC - Role-Based Access Control)**:

1. **Tokens JWT com Assinatura HMAC-SHA256**: Emitidos pelo `Komari.Api` no endpoint de login contendo claims padronizadas (`sub` com o UserId, `email`, `role`, `name`).
2. **Tempo de Expiração Curto**: Access Tokens com vida útil curta, acompanhados de mecanismo de renovação via Refresh Token persistido com hash no banco de dados.
3. **RBAC no ASP.NET Core**: Uso de atributos `[Authorize(Roles = "...")]` e Policies nos Controllers para segregação de privilégios.
4. **Armazenamento Seguro nos Clientes**:
   - Web: Armazenamento em memória com refresh automático e/ou cookies seguros (`HttpOnly`, `SameSite=Lax`).
   - Mobile: Armazenamento do token através de `expo-secure-store` (chaveiro seguro do iOS Keychain e Android Keystore).

## Consequências

### Positivas
- **Desacoplamento de Sessão de Servidor**: A API não precisa de sessão em memória nem de servidor de cache compartilhado (Redis) para validar a autenticidade do token a cada requisição.
- **Uniformidade entre Web e Mobile**: O mesmo endpoint de autenticação e formato de cabeçalho (`Authorization: Bearer <token>`) atende a ambos os frontends.
- **Segurança de Nível Profissional**: Em consonância com as práticas recomendadas de segurança da OWASP.

### Negativas / Trade-offs
- Invalidação imediata de um Access Token já emitido antes de sua expiração requer estratégias como lista de revogação (*token blacklist*) ou conferência da versão do token no refresh.

## Alternativas Descartadas

- **Sessões clássicas baseadas em cookies com afinidade de servidor**:
  - *Por que foi descartada:* Dificulta a autenticação em aplicativos móveis React Native e restringe a escalabilidade horizontal futura.
- **Autenticação Básica (HTTP Basic Auth)**:
  - *Por que foi descartada:* Insegura e inadequada para aplicações modernas.
