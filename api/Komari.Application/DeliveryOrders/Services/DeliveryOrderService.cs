using FluentValidation;
using Komari.Application.Common.Exceptions;
using Komari.Application.DeliveryOrders.DTOs;
using Komari.Application.DeliveryOrders.Interfaces;
using Komari.Application.Orders.DTOs;
using Komari.Domain.Entities;
using Komari.Domain.Enums;
using Komari.Domain.Repositories;

namespace Komari.Application.DeliveryOrders.Services;

public class DeliveryOrderService : IDeliveryOrderService
{
    private readonly IDeliveryOrderRepository _deliveryOrderRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IValidator<CreateDeliveryOrderRequest> _createValidator;
    private readonly IValidator<DispatchDeliveryOrderRequest> _dispatchValidator;
    private readonly IValidator<CancelDeliveryOrderRequest> _cancelValidator;

    public DeliveryOrderService(
        IDeliveryOrderRepository deliveryOrderRepository,
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        IValidator<CreateDeliveryOrderRequest> createValidator,
        IValidator<DispatchDeliveryOrderRequest> dispatchValidator,
        IValidator<CancelDeliveryOrderRequest> cancelValidator)
    {
        _deliveryOrderRepository = deliveryOrderRepository;
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _createValidator = createValidator;
        _dispatchValidator = dispatchValidator;
        _cancelValidator = cancelValidator;
    }

    public async Task<IReadOnlyList<DeliveryOrderResponse>> GetAllAsync(
        OrderStatus? status = null,
        Guid? customerId = null,
        CancellationToken cancellationToken = default)
    {
        var orders = await _deliveryOrderRepository.GetAllAsync(status, customerId, cancellationToken);
        return orders.Select(MapToResponse).ToList();
    }

    public async Task<DeliveryOrderResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deliveryOrder = await _deliveryOrderRepository.GetByIdAsync(id, true, cancellationToken)
            ?? throw new NotFoundException(nameof(DeliveryOrder), id);

        return MapToResponse(deliveryOrder);
    }

    public async Task<DeliveryOrderResponse> CreateAsync(CreateDeliveryOrderRequest request, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var customer = await _customerRepository.GetByIdWithAddressesAsync(request.CustomerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), request.CustomerId);

        if (!customer.IsActive)
        {
            throw new ConflictException($"O cliente '{customer.Name}' está inativo no sistema.");
        }

        var address = customer.Addresses.FirstOrDefault(a => a.Id == request.CustomerAddressId && a.IsActive)
            ?? throw new NotFoundException(nameof(CustomerAddress), request.CustomerAddressId);

        var order = Order.CreateDeliveryOrder(request.Notes);

        foreach (var itemReq in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(itemReq.ProductId, cancellationToken)
                ?? throw new NotFoundException(nameof(Product), itemReq.ProductId);

            if (!product.IsActive || !product.IsAvailable)
            {
                throw new ConflictException($"O produto '{product.Name}' está indisponível para novos pedidos.");
            }

            order.AddItem(product.Id, product.Price, itemReq.Quantity, itemReq.Notes, itemReq.Size);
        }

        var deliveryOrder = new DeliveryOrder(
            order.Id,
            customer.Id,
            address.Id,
            address.Street,
            address.Number,
            address.Neighborhood,
            address.ZipCode,
            address.Complement,
            address.ReferencePoint,
            order.Total,
            request.DeliveryFee,
            request.Discount,
            request.PaymentMethod,
            request.ChangeFor,
            request.EstimatedMinutes
        );

        await _orderRepository.AddAsync(order, cancellationToken);
        await _deliveryOrderRepository.AddAsync(deliveryOrder, cancellationToken);

        var created = await _deliveryOrderRepository.GetByIdAsync(deliveryOrder.Id, true, cancellationToken);
        return MapToResponse(created ?? deliveryOrder);
    }

    public async Task<DeliveryOrderResponse> DispatchAsync(Guid id, DispatchDeliveryOrderRequest request, CancellationToken cancellationToken = default)
    {
        await _dispatchValidator.ValidateAndThrowAsync(request, cancellationToken);

        var deliveryOrder = await _deliveryOrderRepository.GetByIdAsync(id, true, cancellationToken)
            ?? throw new NotFoundException(nameof(DeliveryOrder), id);

        deliveryOrder.Dispatch(request.DriverName);
        await _deliveryOrderRepository.UpdateAsync(deliveryOrder, cancellationToken);

        return MapToResponse(deliveryOrder);
    }

    public async Task<DeliveryOrderResponse> DeliverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deliveryOrder = await _deliveryOrderRepository.GetByIdAsync(id, true, cancellationToken)
            ?? throw new NotFoundException(nameof(DeliveryOrder), id);

        deliveryOrder.Deliver();
        await _deliveryOrderRepository.UpdateAsync(deliveryOrder, cancellationToken);

        return MapToResponse(deliveryOrder);
    }

    public async Task<DeliveryOrderResponse> CancelAsync(Guid id, CancelDeliveryOrderRequest request, CancellationToken cancellationToken = default)
    {
        await _cancelValidator.ValidateAndThrowAsync(request, cancellationToken);

        var deliveryOrder = await _deliveryOrderRepository.GetByIdAsync(id, true, cancellationToken)
            ?? throw new NotFoundException(nameof(DeliveryOrder), id);

        deliveryOrder.Cancel(request.Reason);
        await _deliveryOrderRepository.UpdateAsync(deliveryOrder, cancellationToken);

        return MapToResponse(deliveryOrder);
    }

    public async Task<DeliveryOrderResponse> UpdateEstimatedMinutesAsync(Guid id, UpdateEstimatedMinutesRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Minutes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request.Minutes), "O tempo estimado deve ser maior que zero minutos.");
        }

        var deliveryOrder = await _deliveryOrderRepository.GetByIdAsync(id, true, cancellationToken)
            ?? throw new NotFoundException(nameof(DeliveryOrder), id);

        deliveryOrder.UpdateEstimatedMinutes(request.Minutes);
        await _deliveryOrderRepository.UpdateAsync(deliveryOrder, cancellationToken);

        return MapToResponse(deliveryOrder);
    }

    private static DeliveryOrderResponse MapToResponse(DeliveryOrder deliveryOrder)
    {
        var order = deliveryOrder.Order;
        var items = order?.Items
            .Where(i => i.IsActive)
            .Select(i => new OrderItemResponse(
                i.Id,
                i.ProductId,
                i.Product?.Name ?? string.Empty,
                i.Quantity,
                i.UnitPrice,
                i.TotalPrice,
                i.Notes,
                i.Size,
                i.IsActive,
                i.CreatedAt
            )).ToList() ?? new List<OrderItemResponse>();

        return new DeliveryOrderResponse(
            deliveryOrder.Id,
            deliveryOrder.OrderId,
            deliveryOrder.CustomerId,
            deliveryOrder.Customer?.Name ?? string.Empty,
            deliveryOrder.Customer?.Phone,
            deliveryOrder.CustomerAddressId,
            deliveryOrder.Street,
            deliveryOrder.Number,
            deliveryOrder.Neighborhood,
            deliveryOrder.ZipCode,
            deliveryOrder.Complement,
            deliveryOrder.ReferencePoint,
            order?.Total ?? 0m,
            deliveryOrder.DeliveryFee,
            deliveryOrder.Discount,
            deliveryOrder.TotalAmount,
            deliveryOrder.PaymentMethod,
            deliveryOrder.ChangeFor,
            deliveryOrder.DriverName,
            deliveryOrder.EstimatedMinutes,
            order?.Status ?? OrderStatus.Pending,
            order?.Notes,
            order?.CancellationReason,
            items,
            deliveryOrder.DispatchedAt,
            deliveryOrder.DeliveredAt,
            deliveryOrder.CreatedAt,
            deliveryOrder.UpdatedAt
        );
    }
}
