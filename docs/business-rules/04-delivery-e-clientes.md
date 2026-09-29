# Regras de Negócio: 04 - Delivery e Clientes

Este documento estabelece as regras para pedidos de entrega em domicílio, identificação de clientes e logística de despacho.

---

## 1. Cadastro de Clientes (`Customer`)

- **Identificação**: O cliente é identificado principalmente pelo número de telefone / WhatsApp (com DDD) e nome.
- **Histórico**: O histórico de pedidos anteriores fica vinculado ao cliente para facilitar recompra rápida e análise de fidelidade.
- **Endereços de Entrega**: Um cliente pode ter um ou mais endereços cadastrados (Logradouro, Número, Bairro, CEP, Complemento e Ponto de Referência).

---

## 2. Pedidos de Delivery (`DeliveryOrder`)

- **Vínculo Obrigatório**: Todo pedido de delivery deve obrigatoriamente possuir:
  1. Um **Cliente** associado.
  2. Um **Endereço de Entrega** completo.
  3. Uma **Taxa de Entrega** (`DeliveryFee`), que pode ser fixa por bairro ou calculada por distância.
- **Total do Pedido de Delivery**:
  $$\text{Total} = \sum (\text{Preço do Item} \times \text{Quantidade}) + \text{Taxa de Entrega} - \text{Descontos}$$
- **Despacho e Rastreamento**:
  - O pedido transiciona para `OutForDelivery` no momento em que é atribuído a um entregador e sai do estabelecimento.
  - O operador deve registrar a forma de recebimento combinada (ex: *"Pagar na entrega em dinheiro com troco para R$ 50"*, *"Cartão maquininha"* ou *"Pago antecipado via PIX"*).
- **Finalização**: A entrega é dada como concluída (`Delivered`) mediante confirmação do entregador ou retorno da rota ao caixa.
