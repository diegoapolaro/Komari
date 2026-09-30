import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import {
  DeliveryPaymentMethod,
  DeliveryPaymentMethodLabels,
  OrderStatus,
  OrderStatusLabels,
} from '../constants/enums'
import {
  api,
  type CreateDeliveryOrderRequest,
  type CustomerResponse,
  type DeliveryOrderResponse,
  type DeliveryPaymentMethodType,
  type OrderStatusType,
  type ProductResponse,
} from '../services/api'

interface SelectedItemDraft {
  productId: string
  productName: string
  unitPrice: number
  quantity: number
  notes?: string
}

export function DeliveryTab() {
  const queryClient = useQueryClient()

  // Filtro de status
  const [selectedStatusFilter, setSelectedStatusFilter] = useState<number | undefined>(undefined)

  // Modais
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false)
  const [dispatchOrderTarget, setDispatchOrderTarget] = useState<DeliveryOrderResponse | null>(null)
  const [cancelOrderTarget, setCancelOrderTarget] = useState<DeliveryOrderResponse | null>(null)

  // Estado dos formulários de modal
  const [driverNameInput, setDriverNameInput] = useState('')
  const [cancelReasonInput, setCancelReasonInput] = useState('')
  const [actionError, setActionError] = useState<string | null>(null)
  const [feedbackMessage, setFeedbackMessage] = useState<string | null>(null)

  // Estado do formulário de criação de pedido
  const [selectedCustomerId, setSelectedCustomerId] = useState<string>('')
  const [selectedAddressId, setSelectedAddressId] = useState<string>('')
  const [deliveryFee, setDeliveryFee] = useState<number>(8)
  const [discount, setDiscount] = useState<number>(0)
  const [paymentMethod, setPaymentMethod] = useState<DeliveryPaymentMethodType>(DeliveryPaymentMethod.Cash)
  const [changeFor, setChangeFor] = useState<string>('')
  const [estimatedMinutes, setEstimatedMinutes] = useState<number>(40)
  const [generalNotes, setGeneralNotes] = useState<string>('')

  // Rascunho de itens adicionados
  const [itemsDraft, setItemsDraft] = useState<SelectedItemDraft[]>([])
  const [currentProductId, setCurrentProductId] = useState<string>('')
  const [currentQuantity, setCurrentQuantity] = useState<number>(1)
  const [currentItemNotes, setCurrentItemNotes] = useState<string>('')

  // Queries
  const { data: deliveryOrders = [], isLoading: isLoadingOrders } = useQuery({
    queryKey: ['deliveryOrders', selectedStatusFilter],
    queryFn: () => api.getDeliveryOrders({ status: selectedStatusFilter }),
    refetchInterval: 5000,
  })

  const { data: customers = [] } = useQuery({
    queryKey: ['customers'],
    queryFn: () => api.getCustomers(),
  })

  const { data: products = [] } = useQuery({
    queryKey: ['products'],
    queryFn: () => api.getProducts(),
  })

  // Consulta de endereços do cliente selecionado no modal
  const { data: customerAddresses = [], isLoading: isLoadingAddresses } = useQuery({
    queryKey: ['customerAddresses', selectedCustomerId],
    queryFn: () => (selectedCustomerId ? api.getCustomerAddresses(selectedCustomerId) : Promise.resolve([])),
    enabled: !!selectedCustomerId,
  })

  // Endereço padrão ou o selecionado manualmente pelo usuário
  const defaultAddressId = customerAddresses.find((a) => a.isDefault)?.id ?? customerAddresses[0]?.id ?? ''
  const effectiveAddressId = selectedAddressId || defaultAddressId

  // Mutações
  const createOrderMutation = useMutation({
    mutationFn: (data: CreateDeliveryOrderRequest) => api.createDeliveryOrder(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['deliveryOrders'] })
      setIsCreateModalOpen(false)
      resetCreateForm()
      setFeedbackMessage('Pedido de delivery criado com sucesso!')
      setTimeout(() => setFeedbackMessage(null), 4000)
    },
    onError: (err: unknown) => {
      setActionError(extractErrorMessage(err))
    },
  })

  const updateStatusMutation = useMutation({
    mutationFn: ({ orderId, status }: { orderId: string; status: OrderStatusType }) =>
      api.updateOrderStatus(orderId, { status }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['deliveryOrders'] })
      setFeedbackMessage('Status de cozinha do pedido atualizado!')
      setTimeout(() => setFeedbackMessage(null), 3000)
    },
    onError: (err: unknown) => {
      alert(`Erro ao atualizar status: ${extractErrorMessage(err)}`)
    },
  })

  const dispatchMutation = useMutation({
    mutationFn: ({ id, driverName }: { id: string; driverName: string }) =>
      api.dispatchDeliveryOrder(id, { driverName }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['deliveryOrders'] })
      setDispatchOrderTarget(null)
      setDriverNameInput('')
      setFeedbackMessage('Pedido despachado para entrega com sucesso!')
      setTimeout(() => setFeedbackMessage(null), 4000)
    },
    onError: (err: unknown) => {
      setActionError(extractErrorMessage(err))
    },
  })

  const deliverMutation = useMutation({
    mutationFn: (id: string) => api.deliverDeliveryOrder(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['deliveryOrders'] })
      setFeedbackMessage('Entrega confirmada com sucesso!')
      setTimeout(() => setFeedbackMessage(null), 4000)
    },
    onError: (err: unknown) => {
      alert(`Erro ao confirmar entrega: ${extractErrorMessage(err)}`)
    },
  })

  const cancelMutation = useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) =>
      api.cancelDeliveryOrder(id, { reason }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['deliveryOrders'] })
      setCancelOrderTarget(null)
      setCancelReasonInput('')
      setFeedbackMessage('Pedido de delivery cancelado.')
      setTimeout(() => setFeedbackMessage(null), 4000)
    },
    onError: (err: unknown) => {
      setActionError(extractErrorMessage(err))
    },
  })

  // Funções utilitárias
  function extractErrorMessage(err: unknown): string {
    if (typeof err === 'object' && err !== null && 'detail' in err) {
      return String((err as { detail: unknown }).detail)
    }
    if (typeof err === 'object' && err !== null && 'title' in err) {
      return String((err as { title: unknown }).title)
    }
    return 'Ocorreu um erro ao processar a operação.'
  }

  function resetCreateForm() {
    setSelectedCustomerId('')
    setSelectedAddressId('')
    setDeliveryFee(8)
    setDiscount(0)
    setPaymentMethod(DeliveryPaymentMethod.Cash)
    setChangeFor('')
    setEstimatedMinutes(40)
    setGeneralNotes('')
    setItemsDraft([])
    setCurrentProductId('')
    setCurrentQuantity(1)
    setCurrentItemNotes('')
    setActionError(null)
  }

  // Adição de item ao rascunho
  function handleAddItem() {
    if (!currentProductId) return
    const prod: ProductResponse | undefined = products.find((p) => p.id === currentProductId)
    if (!prod || !prod.id) return

    setItemsDraft((prev) => [
      ...prev,
      {
        productId: prod.id ?? '',
        productName: prod.name ?? 'Produto sem nome',
        unitPrice: prod.price ?? 0,
        quantity: currentQuantity,
        notes: currentItemNotes.trim() || undefined,
      },
    ])

    setCurrentProductId('')
    setCurrentQuantity(1)
    setCurrentItemNotes('')
  }

  function handleRemoveItem(index: number) {
    setItemsDraft((prev) => prev.filter((_, i) => i !== index))
  }

  // Cálculos de totais no modal de criação
  const itemsTotal = itemsDraft.reduce((acc, item) => acc + item.unitPrice * item.quantity, 0)
  const calculatedTotal = Math.max(0, itemsTotal + Number(deliveryFee || 0) - Number(discount || 0))

  function handleCreateOrderSubmit(e: React.FormEvent) {
    e.preventDefault()
    setActionError(null)

    if (!selectedCustomerId) {
      setActionError('Selecione um cliente para o pedido.')
      return
    }

    if (!effectiveAddressId) {
      setActionError('Selecione um endereço de entrega.')
      return
    }

    if (itemsDraft.length === 0) {
      setActionError('Adicione pelo menos um item ao pedido.')
      return
    }

    const parsedChangeFor =
      paymentMethod === DeliveryPaymentMethod.Cash && changeFor.trim() !== ''
        ? Number(changeFor.replace(',', '.'))
        : null

    if (parsedChangeFor !== null && parsedChangeFor < calculatedTotal) {
      setActionError(
        `O troco (R$ ${parsedChangeFor.toFixed(2)}) deve ser maior ou igual ao total (R$ ${calculatedTotal.toFixed(2)}).`
      )
      return
    }

    createOrderMutation.mutate({
      customerId: selectedCustomerId,
      customerAddressId: effectiveAddressId,
      deliveryFee: Number(deliveryFee),
      discount: Number(discount),
      paymentMethod,
      changeFor: parsedChangeFor,
      estimatedMinutes: Number(estimatedMinutes) || undefined,
      notes: generalNotes.trim() || undefined,
      items: itemsDraft.map((i) => ({
        productId: i.productId,
        quantity: i.quantity,
        notes: i.notes,
      })),
    })
  }

  function getStatusBadgeClass(status?: number) {
    switch (status) {
      case OrderStatus.Pending:
        return 'bg-amber-100 text-amber-900 border-amber-300'
      case OrderStatus.InPreparation:
        return 'bg-blue-100 text-blue-900 border-blue-300'
      case OrderStatus.Ready:
        return 'bg-emerald-100 text-emerald-900 border-emerald-300'
      case OrderStatus.OutForDelivery:
        return 'bg-purple-100 text-purple-900 border-purple-300 animate-pulse'
      case OrderStatus.Delivered:
        return 'bg-zinc-100 text-zinc-700 border-zinc-300'
      case OrderStatus.Cancelled:
        return 'bg-red-100 text-red-800 border-red-300'
      default:
        return 'bg-zinc-100 text-zinc-700 border-zinc-200'
    }
  }

  return (
    <div className="space-y-6">
      {/* Topo com Título e Ação */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h2 className="text-lg font-bold tracking-tight text-zinc-900">
            Pedidos de Delivery
          </h2>
          <p className="text-xs text-zinc-500">
            Gerenciamento de entregas em domicílio, despacho de motoboys e rastreamento de rotas.
          </p>
        </div>

        <button
          type="button"
          onClick={() => {
            resetCreateForm()
            setIsCreateModalOpen(true)
          }}
          className="inline-flex items-center gap-2 rounded bg-zinc-900 px-4 py-2 text-xs font-semibold text-white shadow hover:bg-zinc-800 transition-colors"
        >
          <span className="text-sm font-bold">+</span>
          Novo Pedido de Delivery
        </button>
      </div>

      {/* Mensagem de Feedback */}
      {feedbackMessage && (
        <div className="rounded border border-emerald-300 bg-emerald-50 px-4 py-2 text-xs font-medium text-emerald-800 shadow-sm transition-all">
          {feedbackMessage}
        </div>
      )}

      {/* Filtros de Status */}
      <div className="flex flex-wrap items-center gap-1.5 border-b border-zinc-200 pb-3">
        <span className="text-xs font-semibold text-zinc-500 mr-2">Filtrar:</span>
        <button
          type="button"
          onClick={() => setSelectedStatusFilter(undefined)}
          className={`rounded px-3 py-1 text-xs font-medium transition-colors ${
            selectedStatusFilter === undefined
              ? 'bg-zinc-900 text-white'
              : 'bg-zinc-200 text-zinc-700 hover:bg-zinc-300'
          }`}
        >
          Todos ({deliveryOrders.length})
        </button>
        {[
          { id: OrderStatus.Pending, label: 'Pendentes' },
          { id: OrderStatus.InPreparation, label: 'Em Preparo' },
          { id: OrderStatus.Ready, label: 'Prontos' },
          { id: OrderStatus.OutForDelivery, label: 'Em Rota' },
          { id: OrderStatus.Delivered, label: 'Entregues' },
          { id: OrderStatus.Cancelled, label: 'Cancelados' },
        ].map((s) => {
          const count = deliveryOrders.filter((o) => o.status === s.id).length
          const isSelected = selectedStatusFilter === s.id
          return (
            <button
              key={s.id}
              type="button"
              onClick={() => setSelectedStatusFilter(s.id)}
              className={`rounded px-3 py-1 text-xs font-medium transition-colors ${
                isSelected
                  ? 'bg-zinc-900 text-white'
                  : 'bg-zinc-200 text-zinc-700 hover:bg-zinc-300'
              }`}
            >
              {s.label} ({count})
            </button>
          )
        })}
      </div>

      {/* Listagem de Pedidos de Delivery */}
      {isLoadingOrders ? (
        <div className="py-12 text-center text-xs text-zinc-500">
          Carregando pedidos de delivery...
        </div>
      ) : deliveryOrders.length === 0 ? (
        <div className="rounded border border-dashed border-zinc-300 bg-white py-12 text-center text-xs text-zinc-500">
          Nenhum pedido de delivery encontrado para este filtro.
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
          {deliveryOrders.map((order) => {
            const orderIdSafe = order.id ?? ''
            const statusLabel =
              OrderStatusLabels[order.status as keyof typeof OrderStatusLabels] || 'Desconhecido'
            const paymentLabel =
              DeliveryPaymentMethodLabels[
                order.paymentMethod as keyof typeof DeliveryPaymentMethodLabels
              ] || 'Outro'

            return (
              <div
                key={orderIdSafe}
                className="flex flex-col justify-between rounded-lg border border-zinc-200 bg-white p-5 shadow-sm transition-shadow hover:shadow"
              >
                <div>
                  {/* Cabeçalho do Card */}
                  <div className="flex items-start justify-between border-b border-zinc-100 pb-3">
                    <div>
                      <div className="flex items-center gap-2">
                        <span className="text-xs font-bold text-zinc-900">
                          Entrega #{orderIdSafe ? orderIdSafe.slice(0, 8) : '---'}
                        </span>
                        <span
                          className={`rounded border px-2 py-0.5 text-[11px] font-semibold ${getStatusBadgeClass(
                            order.status
                          )}`}
                        >
                          {statusLabel}
                        </span>
                      </div>
                      <p className="mt-1 text-xs font-medium text-zinc-700">
                        {order.customerName} {order.customerPhone ? `• ${order.customerPhone}` : ''}
                      </p>
                    </div>

                    <div className="text-right">
                      <span className="text-base font-extrabold text-zinc-900">
                        R$ {Number(order.totalAmount ?? 0).toFixed(2)}
                      </span>
                      <p className="text-[11px] text-zinc-400">
                        {order.createdAt
                          ? new Date(order.createdAt).toLocaleTimeString('pt-BR', {
                              hour: '2-digit',
                              minute: '2-digit',
                            })
                          : ''}
                      </p>
                    </div>
                  </div>

                  {/* Endereço de Entrega (Snapshot) */}
                  <div className="mt-3 rounded bg-zinc-50 p-2.5 text-xs text-zinc-700 border border-zinc-100">
                    <div className="flex items-center gap-1.5 font-semibold text-zinc-900">
                      <span>📍 Destino:</span>
                      <span>
                        {order.street}, {order.number}
                      </span>
                    </div>
                    <div className="text-zinc-600 mt-0.5">
                      {order.neighborhood}
                      {order.complement ? ` • ${order.complement}` : ''}
                      {order.zipCode ? ` • CEP ${order.zipCode}` : ''}
                    </div>
                    {order.referencePoint && (
                      <div className="text-zinc-500 italic mt-0.5 text-[11px]">
                        Ref: {order.referencePoint}
                      </div>
                    )}
                  </div>

                  {/* Itens do Pedido */}
                  <div className="mt-3 border-t border-zinc-100 pt-3">
                    <span className="text-[11px] font-bold uppercase tracking-wider text-zinc-400">
                      Itens do Pedido:
                    </span>
                    <ul className="mt-1.5 divide-y divide-zinc-50 text-xs">
                      {(order.items ?? []).map((item) => (
                        <li key={item.id ?? Math.random().toString()} className="py-1 flex justify-between items-start">
                          <div>
                            <span className="font-semibold text-zinc-800">
                              {item.quantity ?? 1}x {item.productName}
                            </span>
                            {item.notes && (
                              <p className="text-[11px] text-zinc-500 italic">Obs: {item.notes}</p>
                            )}
                          </div>
                          <span className="text-zinc-600 font-medium">
                            R$ {Number(item.totalPrice ?? 0).toFixed(2)}
                          </span>
                        </li>
                      ))}
                    </ul>
                  </div>

                  {/* Resumo Financeiro e Pagamento */}
                  <div className="mt-3 flex flex-wrap items-center justify-between gap-2 border-t border-zinc-100 pt-3 text-xs text-zinc-600">
                    <div>
                      <span>Itens: R$ {Number(order.itemsTotal ?? 0).toFixed(2)}</span>
                      <span className="mx-1">•</span>
                      <span>Taxa: R$ {Number(order.deliveryFee ?? 0).toFixed(2)}</span>
                      {Number(order.discount ?? 0) > 0 && (
                        <>
                          <span className="mx-1">•</span>
                          <span className="text-emerald-700">
                            Desc: -R$ {Number(order.discount ?? 0).toFixed(2)}
                          </span>
                        </>
                      )}
                    </div>
                    <div className="font-medium text-zinc-800">
                      <span>{paymentLabel}</span>
                      {order.changeFor && (
                        <span className="ml-1 text-zinc-500 text-[11px]">
                          (Troco p/ R$ {Number(order.changeFor).toFixed(2)})
                        </span>
                      )}
                    </div>
                  </div>

                  {/* Detalhes de Despacho / Motorista */}
                  {(order.driverName || order.estimatedMinutes) && (
                    <div className="mt-3 flex items-center justify-between rounded bg-zinc-100 px-3 py-1.5 text-xs text-zinc-700">
                      {order.driverName && (
                        <span>
                          🛵 Entregador: <strong>{order.driverName}</strong>
                        </span>
                      )}
                      {order.estimatedMinutes && (
                        <span>Previsão: ~{order.estimatedMinutes} min</span>
                      )}
                    </div>
                  )}

                  {order.notes && (
                    <p className="mt-2 text-xs text-zinc-500 italic">
                      Nota da entrega: {order.notes}
                    </p>
                  )}

                  {order.cancellationReason && (
                    <p className="mt-2 text-xs text-red-600 bg-red-50 p-2 rounded border border-red-200">
                      Motivo do cancelamento: {order.cancellationReason}
                    </p>
                  )}
                </div>

                {/* Ações de Transição de Estado */}
                <div className="mt-4 flex flex-wrap items-center justify-end gap-2 border-t border-zinc-100 pt-3">
                  {/* Se Pendente: Cozinha pode aceitar */}
                  {order.status === OrderStatus.Pending && order.orderId && (
                    <button
                      type="button"
                      onClick={() =>
                        updateStatusMutation.mutate({
                          orderId: order.orderId ?? '',
                          status: OrderStatus.InPreparation,
                        })
                      }
                      className="rounded bg-blue-600 px-3 py-1.5 text-xs font-semibold text-white hover:bg-blue-700 transition-colors"
                    >
                      ▶ Iniciar Preparo na Cozinha
                    </button>
                  )}

                  {/* Se Em Preparação: Cozinha marca como Pronto */}
                  {order.status === OrderStatus.InPreparation && order.orderId && (
                    <button
                      type="button"
                      onClick={() =>
                        updateStatusMutation.mutate({
                          orderId: order.orderId ?? '',
                          status: OrderStatus.Ready,
                        })
                      }
                      className="rounded bg-emerald-600 px-3 py-1.5 text-xs font-semibold text-white hover:bg-emerald-700 transition-colors"
                    >
                      ✔ Marcar como Pronto
                    </button>
                  )}

                  {/* Se Pronto: Despachar com Motoboy */}
                  {order.status === OrderStatus.Ready && (
                    <button
                      type="button"
                      onClick={() => {
                        setDispatchOrderTarget(order)
                        setDriverNameInput('')
                        setActionError(null)
                      }}
                      className="rounded bg-purple-600 px-3 py-1.5 text-xs font-semibold text-white hover:bg-purple-700 transition-colors"
                    >
                      🛵 Despachar com Motoboy
                    </button>
                  )}

                  {/* Se Em Rota: Confirmar Entrega */}
                  {order.status === OrderStatus.OutForDelivery && order.id && (
                    <button
                      type="button"
                      onClick={() => deliverMutation.mutate(order.id ?? '')}
                      className="rounded bg-emerald-700 px-3 py-1.5 text-xs font-semibold text-white hover:bg-emerald-800 transition-colors"
                    >
                      🏁 Confirmar Entrega
                    </button>
                  )}

                  {/* Cancelamento permitido se não estiver entregue nem cancelado */}
                  {order.status !== OrderStatus.Delivered &&
                    order.status !== OrderStatus.Cancelled && (
                      <button
                        type="button"
                        onClick={() => {
                          setCancelOrderTarget(order)
                          setCancelReasonInput('')
                          setActionError(null)
                        }}
                        className="rounded border border-red-300 bg-white px-2.5 py-1.5 text-xs font-semibold text-red-700 hover:bg-red-50 transition-colors"
                      >
                        Cancelar
                      </button>
                    )}
                </div>
              </div>
            )
          })}
        </div>
      )}

      {/* Modal: Novo Pedido de Delivery */}
      {isCreateModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4 backdrop-blur-xs">
          <div className="max-h-[90vh] w-full max-w-2xl overflow-y-auto rounded-lg bg-white p-6 shadow-xl border border-zinc-200">
            <div className="flex items-center justify-between border-b border-zinc-200 pb-3">
              <h3 className="text-sm font-bold text-zinc-900">Novo Pedido de Delivery</h3>
              <button
                type="button"
                onClick={() => setIsCreateModalOpen(false)}
                className="text-zinc-400 hover:text-zinc-600 text-lg font-bold"
              >
                ✕
              </button>
            </div>

            {actionError && (
              <div className="mt-3 rounded border border-red-300 bg-red-50 p-2.5 text-xs text-red-800">
                {actionError}
              </div>
            )}

            <form onSubmit={handleCreateOrderSubmit} className="mt-4 space-y-4 text-xs">
              {/* Seleção do Cliente */}
              <div>
                <label className="block font-semibold text-zinc-700 mb-1">
                  Cliente <span className="text-red-500">*</span>
                </label>
                <select
                  value={selectedCustomerId}
                  onChange={(e) => {
                    setSelectedCustomerId(e.target.value)
                  }}
                  className="w-full rounded border border-zinc-300 bg-white px-3 py-2 text-xs focus:border-zinc-900 focus:outline-none"
                  required
                >
                  <option value="">Selecione o cliente...</option>
                  {customers.map((c: CustomerResponse) => (
                    <option key={c.id ?? ''} value={c.id ?? ''}>
                      {c.name} {c.phone ? `(${c.phone})` : ''}
                    </option>
                  ))}
                </select>
              </div>

              {/* Endereço de Entrega */}
              {selectedCustomerId && (
                <div>
                  <label className="block font-semibold text-zinc-700 mb-1">
                    Endereço de Entrega <span className="text-red-500">*</span>
                  </label>
                  {isLoadingAddresses ? (
                    <p className="text-zinc-500 text-xs">Carregando endereços do cliente...</p>
                  ) : customerAddresses.length === 0 ? (
                    <div className="rounded border border-amber-300 bg-amber-50 p-2.5 text-amber-900 text-xs">
                      Este cliente não possui nenhum endereço cadastrado. Acesse a aba <strong>Clientes</strong> para adicionar um endereço antes de abrir o pedido.
                    </div>
                  ) : (
                    <select
                      value={effectiveAddressId}
                      onChange={(e) => setSelectedAddressId(e.target.value)}
                      className="w-full rounded border border-zinc-300 bg-white px-3 py-2 text-xs focus:border-zinc-900 focus:outline-none"
                      required
                    >
                      <option value="">Selecione o endereço...</option>
                      {customerAddresses.map((addr) => (
                        <option key={addr.id ?? ''} value={addr.id ?? ''}>
                          {addr.street}, {addr.number} - {addr.neighborhood} {addr.complement ? `(${addr.complement})` : ''} {addr.isDefault ? '★ Padrão' : ''}
                        </option>
                      ))}
                    </select>
                  )}
                </div>
              )}

              {/* Seção de Adicionar Itens ao Pedido */}
              <div className="rounded border border-zinc-200 bg-zinc-50 p-3.5 space-y-3">
                <span className="block font-bold text-zinc-800">Itens do Cardápio</span>

                <div className="grid grid-cols-1 sm:grid-cols-12 gap-2">
                  <div className="sm:col-span-6">
                    <label className="block font-medium text-zinc-600 mb-0.5">Produto</label>
                    <select
                      value={currentProductId}
                      onChange={(e) => setCurrentProductId(e.target.value)}
                      className="w-full rounded border border-zinc-300 bg-white px-2.5 py-1.5 text-xs focus:border-zinc-900 focus:outline-none"
                    >
                      <option value="">Selecione o produto...</option>
                      {products.map((p) => (
                        <option key={p.id ?? ''} value={p.id ?? ''}>
                          {p.name} - R$ {Number(p.price ?? 0).toFixed(2)}
                        </option>
                      ))}
                    </select>
                  </div>

                  <div className="sm:col-span-2">
                    <label className="block font-medium text-zinc-600 mb-0.5">Qtd</label>
                    <input
                      type="number"
                      min={1}
                      max={99}
                      value={currentQuantity}
                      onChange={(e) => setCurrentQuantity(Math.max(1, Number(e.target.value)))}
                      className="w-full rounded border border-zinc-300 bg-white px-2.5 py-1.5 text-xs focus:border-zinc-900 focus:outline-none"
                    />
                  </div>

                  <div className="sm:col-span-4">
                    <label className="block font-medium text-zinc-600 mb-0.5">Obs do Item</label>
                    <input
                      type="text"
                      placeholder="Ex: sem cebola"
                      value={currentItemNotes}
                      onChange={(e) => setCurrentItemNotes(e.target.value)}
                      className="w-full rounded border border-zinc-300 bg-white px-2.5 py-1.5 text-xs focus:border-zinc-900 focus:outline-none"
                    />
                  </div>
                </div>

                <button
                  type="button"
                  onClick={handleAddItem}
                  disabled={!currentProductId}
                  className="rounded bg-zinc-800 px-3 py-1.5 font-semibold text-white hover:bg-zinc-700 disabled:opacity-50 text-xs"
                >
                  + Adicionar Item
                </button>

                {/* Lista de itens já adicionados */}
                {itemsDraft.length > 0 && (
                  <div className="border-t border-zinc-200 pt-2">
                    <span className="text-[11px] font-bold text-zinc-600 uppercase">
                      Itens Adicionados ({itemsDraft.length}):
                    </span>
                    <ul className="mt-1 divide-y divide-zinc-200">
                      {itemsDraft.map((item, idx) => (
                        <li key={idx} className="flex items-center justify-between py-1.5 text-xs">
                          <div>
                            <span className="font-semibold text-zinc-800">
                              {item.quantity}x {item.productName}
                            </span>
                            <span className="ml-2 text-zinc-500">
                              (R$ {(item.unitPrice * item.quantity).toFixed(2)})
                            </span>
                            {item.notes && (
                              <p className="text-[11px] text-zinc-400 italic">Obs: {item.notes}</p>
                            )}
                          </div>
                          <button
                            type="button"
                            onClick={() => handleRemoveItem(idx)}
                            className="text-xs text-red-600 hover:text-red-800 font-semibold"
                          >
                            Remover
                          </button>
                        </li>
                      ))}
                    </ul>
                  </div>
                )}
              </div>

              {/* Informações Financeiras e de Pagamento */}
              <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
                <div>
                  <label className="block font-semibold text-zinc-700 mb-1">
                    Taxa de Entrega (R$)
                  </label>
                  <input
                    type="number"
                    step="0.50"
                    min="0"
                    value={deliveryFee}
                    onChange={(e) => setDeliveryFee(Number(e.target.value))}
                    className="w-full rounded border border-zinc-300 px-2.5 py-1.5 focus:border-zinc-900 focus:outline-none"
                    required
                  />
                </div>

                <div>
                  <label className="block font-semibold text-zinc-700 mb-1">
                    Desconto (R$)
                  </label>
                  <input
                    type="number"
                    step="0.50"
                    min="0"
                    value={discount}
                    onChange={(e) => setDiscount(Number(e.target.value))}
                    className="w-full rounded border border-zinc-300 px-2.5 py-1.5 focus:border-zinc-900 focus:outline-none"
                  />
                </div>

                <div>
                  <label className="block font-semibold text-zinc-700 mb-1">
                    Previsão (Minutos)
                  </label>
                  <input
                    type="number"
                    min="5"
                    step="5"
                    value={estimatedMinutes}
                    onChange={(e) => setEstimatedMinutes(Number(e.target.value))}
                    className="w-full rounded border border-zinc-300 px-2.5 py-1.5 focus:border-zinc-900 focus:outline-none"
                  />
                </div>
              </div>

              {/* Meio de Pagamento */}
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                <div>
                  <label className="block font-semibold text-zinc-700 mb-1">
                    Forma de Pagamento
                  </label>
                  <select
                    value={paymentMethod}
                    onChange={(e) => setPaymentMethod(Number(e.target.value) as DeliveryPaymentMethodType)}
                    className="w-full rounded border border-zinc-300 bg-white px-2.5 py-1.5 focus:border-zinc-900 focus:outline-none"
                  >
                    <option value={DeliveryPaymentMethod.Cash}>Dinheiro na Entrega</option>
                    <option value={DeliveryPaymentMethod.Pix}>PIX</option>
                    <option value={DeliveryPaymentMethod.CreditCard}>Cartão de Crédito na Maquininha</option>
                    <option value={DeliveryPaymentMethod.DebitCard}>Cartão de Débito na Maquininha</option>
                  </select>
                </div>

                {paymentMethod === DeliveryPaymentMethod.Cash && (
                  <div>
                    <label className="block font-semibold text-zinc-700 mb-1">
                      Troco para quanto? (R$)
                    </label>
                    <input
                      type="text"
                      placeholder="Ex: 50,00"
                      value={changeFor}
                      onChange={(e) => setChangeFor(e.target.value)}
                      className="w-full rounded border border-zinc-300 px-2.5 py-1.5 focus:border-zinc-900 focus:outline-none"
                    />
                  </div>
                )}
              </div>

              {/* Observações da Entrega */}
              <div>
                <label className="block font-semibold text-zinc-700 mb-1">
                  Observações Gerais da Entrega
                </label>
                <textarea
                  rows={2}
                  placeholder="Ex: Portaria 2, bloco B, interfone quebrado..."
                  value={generalNotes}
                  onChange={(e) => setGeneralNotes(e.target.value)}
                  className="w-full rounded border border-zinc-300 px-2.5 py-1.5 focus:border-zinc-900 focus:outline-none"
                />
              </div>

              {/* Resumo Totalizador */}
              <div className="rounded bg-zinc-900 p-3 text-white flex items-center justify-between">
                <div>
                  <span className="text-xs text-zinc-400 block">Total do Pedido:</span>
                  <span className="text-xs text-zinc-300">
                    Itens (R$ {itemsTotal.toFixed(2)}) + Taxa (R$ {Number(deliveryFee).toFixed(2)}) - Desc (R$ {Number(discount).toFixed(2)})
                  </span>
                </div>
                <span className="text-xl font-black">
                  R$ {calculatedTotal.toFixed(2)}
                </span>
              </div>

              {/* Botões do Rodapé */}
              <div className="flex justify-end gap-2 border-t border-zinc-200 pt-3">
                <button
                  type="button"
                  onClick={() => setIsCreateModalOpen(false)}
                  className="rounded border border-zinc-300 px-4 py-2 font-semibold text-zinc-700 hover:bg-zinc-100"
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  disabled={createOrderMutation.isPending || itemsDraft.length === 0}
                  className="rounded bg-zinc-900 px-4 py-2 font-semibold text-white hover:bg-zinc-800 disabled:opacity-50"
                >
                  {createOrderMutation.isPending ? 'Lançando...' : 'Lançar Pedido de Delivery'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal: Despacho com Motoboy */}
      {dispatchOrderTarget && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4 backdrop-blur-xs">
          <div className="w-full max-w-md rounded-lg bg-white p-6 shadow-xl border border-zinc-200">
            <h3 className="text-sm font-bold text-zinc-900">Despachar Pedido para Entrega</h3>
            <p className="mt-1 text-xs text-zinc-500">
              Informe o nome do motoboy ou entregador que sairá com a rota.
            </p>

            {actionError && (
              <div className="mt-3 rounded border border-red-300 bg-red-50 p-2 text-xs text-red-800">
                {actionError}
              </div>
            )}

            <div className="mt-4 space-y-3 text-xs">
              <div className="rounded bg-zinc-50 p-2.5 border border-zinc-100">
                <span className="font-semibold text-zinc-800">
                  Pedido #{dispatchOrderTarget.id ? dispatchOrderTarget.id.slice(0, 8) : '---'} • {dispatchOrderTarget.customerName}
                </span>
                <p className="text-zinc-600 mt-0.5">
                  {dispatchOrderTarget.street}, {dispatchOrderTarget.number} - {dispatchOrderTarget.neighborhood}
                </p>
                <span className="font-bold text-zinc-900 block mt-1">
                  Total: R$ {Number(dispatchOrderTarget.totalAmount ?? 0).toFixed(2)}
                </span>
              </div>

              <div>
                <label className="block font-semibold text-zinc-700 mb-1">
                  Nome do Entregador / Motoboy <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  placeholder="Ex: Carlos Motoboy"
                  value={driverNameInput}
                  onChange={(e) => setDriverNameInput(e.target.value)}
                  className="w-full rounded border border-zinc-300 px-3 py-2 focus:border-zinc-900 focus:outline-none"
                  autoFocus
                  required
                />
              </div>

              <div className="flex justify-end gap-2 border-t border-zinc-200 pt-3">
                <button
                  type="button"
                  onClick={() => setDispatchOrderTarget(null)}
                  className="rounded border border-zinc-300 px-3 py-1.5 font-semibold text-zinc-700 hover:bg-zinc-100"
                >
                  Fechar
                </button>
                <button
                  type="button"
                  onClick={() => {
                    if (!driverNameInput.trim()) {
                      setActionError('O nome do entregador é obrigatório.')
                      return
                    }
                    if (dispatchOrderTarget.id) {
                      dispatchMutation.mutate({
                        id: dispatchOrderTarget.id,
                        driverName: driverNameInput.trim(),
                      })
                    }
                  }}
                  disabled={dispatchMutation.isPending}
                  className="rounded bg-purple-700 px-4 py-1.5 font-semibold text-white hover:bg-purple-800 disabled:opacity-50"
                >
                  {dispatchMutation.isPending ? 'Despachando...' : 'Confirmar Saída para Rota'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Modal: Cancelar Pedido */}
      {cancelOrderTarget && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4 backdrop-blur-xs">
          <div className="w-full max-w-md rounded-lg bg-white p-6 shadow-xl border border-zinc-200">
            <h3 className="text-sm font-bold text-red-900">Cancelar Pedido de Delivery</h3>
            <p className="mt-1 text-xs text-zinc-500">
              O cancelamento exige o preenchimento de uma justificativa para auditoria.
            </p>

            {actionError && (
              <div className="mt-3 rounded border border-red-300 bg-red-50 p-2 text-xs text-red-800">
                {actionError}
              </div>
            )}

            <div className="mt-4 space-y-3 text-xs">
              <div>
                <label className="block font-semibold text-zinc-700 mb-1">
                  Justificativa do Cancelamento <span className="text-red-500">*</span>
                </label>
                <textarea
                  rows={3}
                  placeholder="Ex: Cliente desistiu antes do preparo..."
                  value={cancelReasonInput}
                  onChange={(e) => setCancelReasonInput(e.target.value)}
                  className="w-full rounded border border-zinc-300 px-3 py-2 focus:border-red-600 focus:outline-none"
                  autoFocus
                  required
                />
              </div>

              <div className="flex justify-end gap-2 border-t border-zinc-200 pt-3">
                <button
                  type="button"
                  onClick={() => setCancelOrderTarget(null)}
                  className="rounded border border-zinc-300 px-3 py-1.5 font-semibold text-zinc-700 hover:bg-zinc-100"
                >
                  Voltar
                </button>
                <button
                  type="button"
                  onClick={() => {
                    if (!cancelReasonInput.trim()) {
                      setActionError('Informe a justificativa do cancelamento.')
                      return
                    }
                    if (cancelOrderTarget.id) {
                      cancelMutation.mutate({
                        id: cancelOrderTarget.id,
                        reason: cancelReasonInput.trim(),
                      })
                    }
                  }}
                  disabled={cancelMutation.isPending}
                  className="rounded bg-red-600 px-4 py-1.5 font-semibold text-white hover:bg-red-700 disabled:opacity-50"
                >
                  {cancelMutation.isPending ? 'Cancelando...' : 'Confirmar Cancelamento'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
