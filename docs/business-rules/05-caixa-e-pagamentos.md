# Regras de Negócio: 05 - Caixa e Pagamentos

Este documento define as regras contábeis e de conciliação financeira do Ponto de Venda (PDV), incluindo sessões de caixa, recebimentos, sangrias e suprimentos.

---

## 1. Sessão de Caixa (`CashRegisterSession`)

- **Abertura Obrigatória**: Nenhuma operação financeira (recebimento de comandas ou delivery) pode ser processada sem que haja uma sessão de caixa aberta por um operador responsável.
- **Fundo de Troco (Saldo Inicial)**:
  - A abertura exige a declaração do valor inicial em dinheiro disponível na gaveta (`InitialAmount >= 0.00`).
- **Unicidade por Operador/Terminal**: Cada terminal ou operador possui no máximo uma sessão de caixa aberta concorrentemente.

---

## 2. Movimentações de Caixa (`CashMovement`)

Além dos recebimentos de pedidos, o caixa suporta duas movimentações manuais:

1. **Suprimento (Aporte)**:
   - Entrada manual de dinheiro na gaveta (ex: adição de troco no meio do expediente).
   - Exige valor positivo e descrição/justificativa.
2. **Sangria (Retirada)**:
   - Saída manual de dinheiro da gaveta por motivo de segurança ou recolhimento gerencial para cofre.
   - O valor não pode exceder o saldo total em dinheiro existente na gaveta no momento da operação.
   - Exige justificativa obrigatória.

---

## 3. Pagamentos e Métodos (`Payment`)

- Cada pagamento é registrado com sua respectiva forma:
  - `Cash` (Dinheiro em espécie)
  - `Pix`
  - `CreditCard` (Cartão de Crédito)
  - `DebitCard` (Cartão de Débito)
  - `Voucher` (Vale Refeição / Alimentação)
- **Pagamentos Parciais / Múltiplos**: Uma conta pode ser quitada com mais de uma modalidade (ex: metade em PIX e metade em Dinheiro).
- **Cálculo de Troco**: Em pagamentos em dinheiro onde o valor entregue é superior ao valor devido:
  $$\text{Troco} = \text{Valor Recebido} - \text{Valor da Parcela}$$
  O troco reduz o saldo em dinheiro da gaveta.

---

## 4. Fechamento e Conciliação (`CloseSession`)

- Ao final do expediente ou turno, o operador realiza a contagem física dos valores e declara os totais apurados por modalidade (Dinheiro, PIX, Cartão).
- **Cálculo do Saldo Esperado**:
  $$\text{Esperado em Dinheiro} = \text{Saldo Inicial} + \sum \text{Recebimentos Dinheiro} + \sum \text{Suprimentos} - \sum \text{Sangrias} - \sum \text{Trocos}$$
- **Divergência (Quebra / Sobra de Caixa)**:
  $$\text{Divergência} = \text{Valor Declarado} - \text{Valor Esperado}$$
- O fechamento registra a auditoria com data/hora, operador e justificativa caso haja diferença contábil. Uma sessão fechada torna-se **estritamente imutável**.
