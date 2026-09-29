# Modelo de Domínio e Relacionamentos (ER)

Este documento ilustra a arquitetura de entidades do Komari, incorporando suporte a Multi-Tenancy (SaaS), composição de pizzas fracionadas, adicionais customizados e comandas flexíveis.

---

## 📊 Diagrama Entidade-Relacionamento (ER)

```mermaid
erDiagram
    OWNER_USER ||--o{ RESTAURANT : owns
    PLAN ||--o{ OWNER_USER : subscribes
    RESTAURANT ||--o{ RESTAURANT_STAFF : employs
    USER ||--o{ RESTAURANT_STAFF : acts_as
    
    RESTAURANT ||--o{ CATEGORY : contains
    CATEGORY ||--o{ PRODUCT : contains
    PRODUCT ||--o{ PRODUCT_EXTRA : offers
    
    RESTAURANT ||--o{ TABLE : arranges
    RESTAURANT ||--o{ BILL : manages
    TABLE ||--o{ BILL : hosts
    
    BILL ||--o{ ORDER : includes
    ORDER ||--|{ ORDER_ITEM : has
    ORDER_ITEM ||--o{ ORDER_ITEM_FLAVOR : splits_into
    ORDER_ITEM ||--o{ ORDER_ITEM_EXTRA : includes_extra
    
    RESTAURANT ||--o{ CASH_SESSION : runs
    CASH_SESSION ||--o{ PAYMENT : registers
    BILL ||--o{ PAYMENT : settles
    
    OWNER_USER {
        guid Id PK
        string Email
        string FullName
        guid PlanId FK
    }

    PLAN {
        guid Id PK
        string Name
        int MaxRestaurants
        decimal MonthlyPrice
    }

    RESTAURANT {
        guid Id PK
        guid OwnerId FK
        string TradeName
        string DocumentCnpj
        boolean IsActive
    }

    RESTAURANT_STAFF {
        guid Id PK
        guid RestaurantId FK
        guid UserId FK
        int Role "Manager, Waiter, Cook, DeliveryDriver, Cashier"
        boolean IsActive
    }

    PRODUCT {
        guid Id PK
        guid RestaurantId FK
        guid CategoryId FK
        string Name
        decimal BasePrice
        boolean IsPizza
        boolean IsAvailable
    }

    PRODUCT_EXTRA {
        guid Id PK
        guid RestaurantId FK
        string Name
        decimal Price
    }

    TABLE {
        guid Id PK
        guid RestaurantId FK
        int Number
        int Capacity
        int Type "DiningTable, Counter"
        int Status "Available, Occupied, Reserved"
        string Location
    }

    BILL {
        guid Id PK
        guid RestaurantId FK
        guid TableId FK "nullable"
        int Number "Universal sequential identifier"
        int Status "Open, Closed, Cancelled"
        datetime OpenedAt
        datetime ClosedAt
    }

    ORDER {
        guid Id PK
        guid RestaurantId FK
        guid BillId FK "nullable"
        int Status "Pending, InPrep, Ready, OutForDeliv, Delivered, Cancelled"
        decimal Total
        datetime CreatedAt
    }

    ORDER_ITEM {
        guid Id PK
        guid OrderId FK
        guid ProductId FK
        int Quantity
        string Size "Broto, Grande, Standard"
        decimal UnitPrice
    }

    ORDER_ITEM_FLAVOR {
        guid Id PK
        guid OrderItemId FK
        guid ProductId FK "Flavor product reference"
        decimal Fraction "0.5, 0.333, etc."
    }

    ORDER_ITEM_EXTRA {
        guid Id PK
        guid OrderItemId FK
        guid ProductExtraId FK
        decimal Price
    }

    PAYMENT {
        guid Id PK
        guid RestaurantId FK
        guid BillId FK
        guid CashSessionId FK
        int Method "Cash, Pix, Credit, Debit, Voucher"
        decimal Amount
        datetime PaidAt
    }
```

---

## 🧩 Agregados DDD Principais

1. **Conta e Assinatura SaaS (`Subscription Aggregate`)**:
   - `OwnerUser`, `Plan`, `Restaurant`.
   - Limita a criação de restaurantes ao teto do plano contratado.

2. **Equipe e Autorização (`Staff Aggregate`)**:
   - `RestaurantStaff`, vinculando `User` com uma `Role` restrita a um `RestaurantId`.

3. **Catálogo & Adicionais (`Catalog Aggregate`)**:
   - `Category`, `Product` e `ProductExtra`.
   - Suporte a itens convencionais e itens do tipo pizza com múltiplos sabores e bordas.

4. **Mesas e Comandas (`Table & Bill Aggregate`)**:
   - `Table` e `Bill` (numerada, com ou sem mesa associada, suporte a divisão por pagantes).

5. **Produção e Pedidos (`Order Aggregate`)**:
   - `Order`, `OrderItem`, frações de sabores (`OrderItemFlavor`) e adicionais (`OrderItemExtra`).

6. **Caixa e Pagamentos (`Cashier Aggregate`)**:
   - `CashSession`, `Payment` e `CashMovement`.
