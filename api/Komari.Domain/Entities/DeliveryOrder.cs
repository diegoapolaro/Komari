using Komari.Domain.Common;
using Komari.Domain.Enums;

namespace Komari.Domain.Entities;

/// <summary>
/// Representa os detalhes e o ciclo logístico de um pedido de entrega em domicílio (Delivery).
/// Atua em conjunto com a entidade Order (1-para-1), encapsulando cliente, snapshot de endereço,
/// taxa de entrega, meio de pagamento na entrega e rastreamento de despacho com motoboy.
/// </summary>
public class DeliveryOrder : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Order? Order { get; private set; }

    public Guid CustomerId { get; private set; }
    public Customer? Customer { get; private set; }

    public Guid CustomerAddressId { get; private set; }
    public CustomerAddress? CustomerAddress { get; private set; }

    // Snapshot imutável do endereço de entrega no momento do pedido
    public string Street { get; private set; } = string.Empty;
    public string Number { get; private set; } = string.Empty;
    public string Neighborhood { get; private set; } = string.Empty;
    public string? ZipCode { get; private set; }
    public string? Complement { get; private set; }
    public string? ReferencePoint { get; private set; }

    // Componentes financeiros da entrega
    public decimal DeliveryFee { get; private set; }
    public decimal Discount { get; private set; }
    public decimal TotalAmount { get; private set; }

    // Informações de recebimento na entrega
    public DeliveryPaymentMethod PaymentMethod { get; private set; }
    public decimal? ChangeFor { get; private set; }

    // Logística e despacho
    public string? DriverName { get; private set; }
    public int? EstimatedMinutes { get; private set; }
    public DateTime? DispatchedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }

    // Construtor protegido exigido pelo EF Core
    protected DeliveryOrder() { }

    public DeliveryOrder(
        Guid orderId,
        Guid customerId,
        Guid customerAddressId,
        string street,
        string number,
        string neighborhood,
        string? zipCode,
        string? complement,
        string? referencePoint,
        decimal itemsTotal,
        decimal deliveryFee,
        decimal discount,
        DeliveryPaymentMethod paymentMethod,
        decimal? changeFor = null,
        int? estimatedMinutes = null)
    {
        SetOrderId(orderId);
        SetCustomerId(customerId);
        SetCustomerAddressId(customerAddressId);
        SetAddressSnapshot(street, number, neighborhood, zipCode, complement, referencePoint);
        SetFinancials(itemsTotal, deliveryFee, discount);
        SetPayment(paymentMethod, changeFor);
        SetEstimatedMinutes(estimatedMinutes);
    }

    public void Dispatch(string driverName)
    {
        if (Order != null && Order.Status != OrderStatus.Ready)
        {
            throw new InvalidOperationException(
                $"O pedido só pode ser despachado para entrega quando estiver com status 'Ready' (Pronto na cozinha). Status atual: '{Order.Status}'.");
        }

        if (string.IsNullOrWhiteSpace(driverName))
        {
            throw new ArgumentException("O nome do entregador/motoboy é obrigatório para realizar o despacho.", nameof(driverName));
        }

        var trimmed = driverName.Trim();
        if (trimmed.Length < 2 || trimmed.Length > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(driverName), "O nome do entregador deve conter entre 2 e 100 caracteres.");
        }

        DriverName = trimmed;
        DispatchedAt = DateTime.UtcNow;

        Order?.UpdateStatus(OrderStatus.OutForDelivery);
        TouchUpdated();
    }

    public void Deliver()
    {
        if (Order != null && Order.Status != OrderStatus.OutForDelivery)
        {
            throw new InvalidOperationException(
                $"O pedido só pode ser confirmado como entregue se estiver em trânsito com o entregador ('OutForDelivery'). Status atual: '{Order.Status}'.");
        }

        DeliveredAt = DateTime.UtcNow;

        Order?.UpdateStatus(OrderStatus.Delivered);
        TouchUpdated();
    }

    public void Cancel(string reason)
    {
        Order?.Cancel(reason);
        TouchUpdated();
    }

    public void UpdateEstimatedMinutes(int? minutes)
    {
        SetEstimatedMinutes(minutes);
        TouchUpdated();
    }

    public void RecalculateTotal(decimal itemsTotal)
    {
        SetFinancials(itemsTotal, DeliveryFee, Discount);
        TouchUpdated();
    }

    private void SetOrderId(Guid orderId)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException("O identificador do pedido (OrderId) não pode ser vazio.", nameof(orderId));
        }
        OrderId = orderId;
    }

    private void SetCustomerId(Guid customerId)
    {
        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("O identificador do cliente (CustomerId) não pode ser vazio.", nameof(customerId));
        }
        CustomerId = customerId;
    }

    private void SetCustomerAddressId(Guid customerAddressId)
    {
        if (customerAddressId == Guid.Empty)
        {
            throw new ArgumentException("O identificador do endereço de entrega (CustomerAddressId) não pode ser vazio.", nameof(customerAddressId));
        }
        CustomerAddressId = customerAddressId;
    }

    private void SetAddressSnapshot(
        string street,
        string number,
        string neighborhood,
        string? zipCode,
        string? complement,
        string? referencePoint)
    {
        if (string.IsNullOrWhiteSpace(street))
        {
            throw new ArgumentException("O logradouro/rua é obrigatório no snapshot de entrega.", nameof(street));
        }
        if (string.IsNullOrWhiteSpace(number))
        {
            throw new ArgumentException("O número é obrigatório no snapshot de entrega.", nameof(number));
        }
        if (string.IsNullOrWhiteSpace(neighborhood))
        {
            throw new ArgumentException("O bairro é obrigatório no snapshot de entrega.", nameof(neighborhood));
        }

        Street = street.Trim();
        Number = number.Trim();
        Neighborhood = neighborhood.Trim();
        ZipCode = string.IsNullOrWhiteSpace(zipCode) ? null : zipCode.Trim();
        Complement = string.IsNullOrWhiteSpace(complement) ? null : complement.Trim();
        ReferencePoint = string.IsNullOrWhiteSpace(referencePoint) ? null : referencePoint.Trim();
    }

    private void SetFinancials(decimal itemsTotal, decimal deliveryFee, decimal discount)
    {
        if (itemsTotal < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(itemsTotal), "O total dos itens não pode ser negativo.");
        }
        if (deliveryFee < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deliveryFee), "A taxa de entrega não pode ser negativa.");
        }
        if (discount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(discount), "O desconto não pode ser negativo.");
        }

        var grossTotal = itemsTotal + deliveryFee;
        if (discount > grossTotal)
        {
            throw new ArgumentOutOfRangeException(nameof(discount), "O desconto não pode exceder o valor total do pedido com a taxa de entrega.");
        }

        DeliveryFee = deliveryFee;
        Discount = discount;
        TotalAmount = grossTotal - discount;
    }

    private void SetPayment(DeliveryPaymentMethod paymentMethod, decimal? changeFor)
    {
        if (paymentMethod != DeliveryPaymentMethod.Cash && changeFor.HasValue)
        {
            throw new ArgumentException("O valor de troco só pode ser informado para pagamentos em dinheiro.", nameof(changeFor));
        }

        if (paymentMethod == DeliveryPaymentMethod.Cash && changeFor.HasValue && changeFor.Value < TotalAmount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(changeFor),
                $"O valor informado para troco (R$ {changeFor.Value:F2}) não pode ser inferior ao valor total do pedido (R$ {TotalAmount:F2}).");
        }

        PaymentMethod = paymentMethod;
        ChangeFor = changeFor;
    }

    private void SetEstimatedMinutes(int? estimatedMinutes)
    {
        if (estimatedMinutes.HasValue && estimatedMinutes.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(estimatedMinutes), "O tempo estimado de entrega deve ser maior que zero minutos.");
        }

        EstimatedMinutes = estimatedMinutes;
    }
}
