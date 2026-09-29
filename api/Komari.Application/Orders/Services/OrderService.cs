using FluentValidation;
using Komari.Application.Common.Exceptions;
using Komari.Application.Orders.DTOs;
using Komari.Application.Orders.Interfaces;
using Komari.Domain.Entities;
using Komari.Domain.Enums;
using Komari.Domain.Repositories;

namespace Komari.Application.Orders.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IBillRepository _billRepository;
    private readonly IProductRepository _productRepository;
    private readonly IValidator<CreateOrderRequest> _createOrderValidator;
    private readonly IValidator<CancelOrderRequest> _cancelOrderValidator;
    private readonly IValidator<UpdateOrderStatusRequest> _updateStatusValidator;

    public OrderService(
        IOrderRepository orderRepository,
        IBillRepository billRepository,
        IProductRepository productRepository,
        IValidator<CreateOrderRequest> createOrderValidator,
        IValidator<CancelOrderRequest> cancelOrderValidator,
        IValidator<UpdateOrderStatusRequest> updateStatusValidator)
    {
        _orderRepository = orderRepository;
        _billRepository = billRepository;
        _productRepository = productRepository;
        _createOrderValidator = createOrderValidator;
        _cancelOrderValidator = cancelOrderValidator;
        _updateStatusValidator = updateStatusValidator;
    }

    public async Task<IReadOnlyList<OrderResponse>> GetAllAsync(
        Guid? billId = null,
        OrderStatus? status = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetAllAsync(billId, status, includeInactive, cancellationToken);
        return orders.Select(MapToResponse).ToList();
    }

    public async Task<OrderResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, true, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), id);

        return MapToResponse(order);
    }

    public async Task<IReadOnlyList<OrderResponse>> GetByBillIdAsync(Guid billId, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetByBillIdAsync(billId, cancellationToken);
        return orders.Select(MapToResponse).ToList();
    }

    public async Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        await _createOrderValidator.ValidateAndThrowAsync(request, cancellationToken);

        var bill = await _billRepository.GetByIdAsync(request.BillId, cancellationToken)
            ?? throw new NotFoundException(nameof(Bill), request.BillId);

        if (bill.Status != BillStatus.Open)
        {
            throw new ConflictException($"Não é possível lançar pedidos na comanda #{bill.Number} com status '{bill.Status}'. A comanda deve estar aberta.");
        }

        var order = new Order(request.BillId, request.Notes);

        foreach (var itemReq in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(itemReq.ProductId, cancellationToken)
                ?? throw new NotFoundException(nameof(Product), itemReq.ProductId);

            if (!product.IsActive || !product.IsAvailable)
            {
                throw new ConflictException($"O produto '{product.Name}' está indisponível para novos pedidos.");
            }

            // Snapshot imutável do preço unitário atual do produto
            order.AddItem(product.Id, product.Price, itemReq.Quantity, itemReq.Notes, itemReq.Size);
        }

        await _orderRepository.AddAsync(order, cancellationToken);

        // Recarrega o pedido completo com relacionamentos para resposta consistente
        var createdOrder = await _orderRepository.GetByIdAsync(order.Id, true, cancellationToken);
        return MapToResponse(createdOrder ?? order);
    }

    public async Task<OrderResponse> AddItemsAsync(Guid orderId, AddItemsToOrderRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            throw new ArgumentException("A lista de itens não pode ser vazia.", nameof(request));
        }

        var order = await _orderRepository.GetByIdAsync(orderId, true, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), orderId);

        var bill = await _billRepository.GetByIdAsync(order.BillId, cancellationToken)
            ?? throw new NotFoundException(nameof(Bill), order.BillId);

        if (bill.Status != BillStatus.Open)
        {
            throw new ConflictException($"Não é possível alterar pedidos de uma comanda que não está aberta.");
        }

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

        await _orderRepository.UpdateAsync(order, cancellationToken);

        var updatedOrder = await _orderRepository.GetByIdAsync(order.Id, true, cancellationToken);
        return MapToResponse(updatedOrder ?? order);
    }

    public async Task<OrderResponse> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default)
    {
        await _updateStatusValidator.ValidateAndThrowAsync(request, cancellationToken);

        var order = await _orderRepository.GetByIdAsync(id, true, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), id);

        order.UpdateStatus(request.Status, request.CancellationReason);
        await _orderRepository.UpdateAsync(order, cancellationToken);

        return MapToResponse(order);
    }

    public async Task<OrderResponse> CancelAsync(Guid id, CancelOrderRequest request, CancellationToken cancellationToken = default)
    {
        await _cancelOrderValidator.ValidateAndThrowAsync(request, cancellationToken);

        var order = await _orderRepository.GetByIdAsync(id, true, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), id);

        order.Cancel(request.Reason);
        await _orderRepository.UpdateAsync(order, cancellationToken);

        return MapToResponse(order);
    }

    private static OrderResponse MapToResponse(Order order)
    {
        var itemResponses = order.Items
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
            )).ToList();

        return new OrderResponse(
            order.Id,
            order.BillId,
            order.Bill?.Number ?? 0,
            order.Bill?.TableId,
            order.Bill?.Table?.Number,
            order.Bill?.CounterName,
            order.Status,
            order.Total,
            order.Notes,
            order.CancellationReason,
            itemResponses,
            order.CreatedAt,
            order.UpdatedAt
        );
    }
}
