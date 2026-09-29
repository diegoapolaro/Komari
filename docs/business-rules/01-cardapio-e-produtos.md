# Regras de Negócio: 01 - Cardápio e Produtos

Este documento estabelece as regras e restrições de negócio aplicadas ao catálogo de itens, categorias, opções de pizzas e adicionais do restaurante/pizzaria.

---

## 1. Categorias

- **Identificação Única**: Toda categoria possui um identificador único (`Guid`) e um nome obrigatório (`Name`), não podendo ser vazio ou composto apenas de espaços em branco.
- **Hierarquia e Organização**: As categorias organizam o cardápio (ex: *Pizzas Tradicionais*, *Pizzas Doces*, *Bebidas*, *Sobremesas*, *Porções*).
- **Ordem de Exibição**: Cada categoria possui uma propriedade de ordenação para determinar sua posição no cardápio web e mobile.
- **Regra de Exclusão**: Uma categoria **não pode** ser excluída do sistema se houver produtos atrelados a ela, garantindo integridade referencial.

---

## 2. Produtos / Itens do Cardápio

- **Vínculo Obrigatório**: Todo produto deve pertencer a uma categoria válida e existente (`CategoryId`).
- **Preço Base**:
  - O preço deve ser maior ou igual a zero (`>= 0.00`). Não são permitidos preços negativos.
- **Disponibilidade (`IsAvailable`)**:
  - O produto pode ser marcado como indisponível/esgotado a qualquer momento pelo operador ou gerente.
  - **Invariante em Pedidos**: Um produto com `IsAvailable == false` não pode ser adicionado a um novo pedido.
  - **Preservação Histórica**: Desativar ou indisponibilizar um produto **não afeta** pedidos anteriores ou comandas abertas que já continham o item.

---

## 3. Modelagem de Pizzas: Tamanhos, Frações de Sabores e Bordas

As pizzas possuem comportamento especializado configurável pelo lojista:

### 3.1 Tamanhos e Frações Permitidas
- O estabelecimento pode definir os tamanhos ofertados (ex: **Broto** e **Grande**):
  - **Tamanho Broto**: permite a seleção de **até 2 sabores** (1 sabor inteiro ou fração de 1/2 e 1/2).
  - **Tamanho Grande**: permite a seleção de **até 3 sabores** (1 sabor inteiro, 2 sabores [1/2 cada], ou 3 sabores [1/3 cada]).
- **Validação de Frações**: A soma das frações dos sabores selecionados deve totalizar exatamente 100% (1 pizza inteira).
- **Regra de Precificação por Fração**: Na composição de múltiplos sabores, o preço base da pizza é definido pelo **maior valor entre os sabores selecionados** (padrão consolidado no mercado de pizzarias).

### 3.2 Adicionais Customizáveis e Bordas Recheadas
- O próprio lojista tem total liberdade para cadastrar adicionais no cardápio:
  - Exemplos: *Borda de Catupiry*, *Borda de Cheddar*, *Bacon Extra*, *Massa Integral*, *Sem Cebola*, etc.
- Cada adicional possui:
  - **Nome descritivo**.
  - **Preço do Adicional** (pode ser R$ 0,00 para opcionais de remoção/isenção, ou um valor positivo para acréscimos).
- Os adicionais selecionados são somados diretamente ao valor final do item do pedido.
