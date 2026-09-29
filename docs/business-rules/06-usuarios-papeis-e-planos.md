# Regras de Negócio: 06 - Usuários, Perfis (Roles) e Planos SaaS

Este documento especifica a estrutura hierárquica de usuários, planos de assinatura do Komari e perfis de colaboradores.

---

## 1. Planos de Assinatura e Cotas de Estabelecimentos

- **Conta Titular (Dono / Proprietário)**:
  - É a conta principal e responsável financeira pelo pagamento da assinatura do Komari.
  - Possui um plano vinculado (`Plan`), que determina os limites da conta.
- **Limite de Lojas / Filiais**:
  - Cada plano estipula uma quantidade máxima de restaurantes/comércios ativos simultaneamente (ex: Plano Básico = 1 comércio; Plano Pro = até 3 comércios; Plano Franquia = 5+ comércios).
  - A tentativa de cadastrar um novo comércio além do limite do plano é rejeitada com erro amigável de negócio, orientando o upgrade de plano.
- **Multi-comércios pelo mesmo Dono**:
  - O Dono autenticado pode alternar o contexto de trabalho entre qualquer um dos seus estabelecimentos através de uma listagem no painel.

---

## 2. Perfis de Colaboradores (Roles) e Vínculo

Todos os colaboradores operacionais trabalham vinculados à conta do Dono e a um ou mais restaurantes específicos:

| Papel (`Role`) | Finalidade | Principais Permissões e Telas |
| :--- | :--- | :--- |
| **`Owner` (Dono)** | Titular da conta e pagante | Acesso irrestrito a configurações, relatórios financeiros consolidados, contratação de planos e criação de lojas |
| **`Manager` (Gerente)** | Gestão operacional da loja | Cadastro de cardápio, gestão de equipe, cancelamentos justificados, sangrias/suprimentos e fechamento de caixa |
| **`Waiter` (Garçom)** | Atendimento de salão e balcão | Acesso ao app mobile: visualização do mapa de mesas, abertura e lançamento de itens em comandas, envio de pedidos à cozinha |
| **`Cook` (Cozinheiro / KDS)** | Produção na cozinha | Acesso à tela de KDS: visualização de pedidos pendentes, início do preparo (`InPreparation`) e marcação de pronto (`Ready`) |
| **`DeliveryDriver` (Entregador)**| Logística de delivery | Acesso ao app mobile: listagem de entregas atribuídas, endereço do cliente com link para rotas, alteração de status para `Delivered` |
| **`Cashier` (Operador de Caixa)**| Ponto de Venda e pagamentos | Abertura/fechamento de sessão de caixa, recebimento de pagamentos em múltiplas formas, emissão de comprovantes |

---

## 3. Regras de Isolamento e Vinculação

1. **Vínculo Restrito**: Um colaborador só tem acesso aos dados operacionais (cardápio, pedidos, mesas) dos estabelecimentos para os quais foi expressamente convidado/cadastrado pelo Dono ou Gerente.
2. **Desativação Imediata**: Se o Dono ou Gerente inativar um colaborador, seus tokens perdem o acesso aos recursos do restaurante no próximo ciclo de requisição.
3. **Sem Acesso ao Faturamento Global**: Perfis operacionais (Garçom, Cozinha, Entregador) não têm visibilidade sobre faturamento geral, limites de plano ou dados de cobrança do Dono.
