import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { BillStatus, OrderStatus, TableStatus } from '../constants/enums'
import {
  api,
  type BillResponse,
  type OrderResponse,
  type TableResponse,
} from '../services/api'

export function TablesTab() {
  const queryClient = useQueryClient()
  const [selectedTableId, setSelectedTableId] = useState<string | null>(null)
  const [feedbackMessage, setFeedbackMessage] = useState<string | null>(null)

  // 1. Consulta de mesas cadastradas
  const {
    data: tables = [],
    isLoading: isLoadingTables,
    error: tablesError,
  } = useQuery({
    queryKey: ['tables'],
    queryFn: () => api.getTables(),
  })

  // 2. Consulta de comandas ativas
  const { data: openBills = [] } = useQuery({
    queryKey: ['bills', 'open'],
    queryFn: () => api.getBills({ status: BillStatus.Open }),
    refetchInterval: 4000,
  })

  // 3. Inicialização de mesas se o restaurante ainda não tiver nenhuma cadastrada
  const initMutation = useMutation({
    mutationFn: () => api.initializeTables({ totalTables: 12, defaultCapacity: 4 }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tables'] })
      setFeedbackMessage('12 mesas foram inicializadas com sucesso!')
    },
    onError: (err: Error) => {
      setFeedbackMessage(`Erro ao inicializar mesas: ${err.message}`)
    },
  })

  // Mapa rápido de mesaId -> comanda aberta
  const activeBillByTableId = new Map<string, BillResponse>()
  for (const bill of openBills) {
    if (bill.tableId) {
      activeBillByTableId.set(bill.tableId, bill)
    }
  }

  // Ordena mesas pelo número
  const sortedTables = [...tables].sort((a, b) => (a.number ?? 0) - (b.number ?? 0))

  // Mesa atualmente selecionada para o modal
  const selectedTable = sortedTables.find((t) => t.id === selectedTableId) || null

  return (
    <div className="space-y-6">
      {/* Cabeçalho limpo */}
      <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h2 className="text-xl font-bold tracking-tight text-zinc-900">
            Salão & Mesas
          </h2>
          <p className="text-xs text-zinc-500">
            Mesas verdes estão abertas para atendimento. Mesas laranja avermelhado estão fechadas.
          </p>
        </div>

        {/* Inicializador de contingência caso o banco de dados esteja vazio */}
        {tables.length === 0 && !isLoadingTables && (
          <button
            type="button"
            onClick={() => initMutation.mutate()}
            disabled={initMutation.isPending}
            className="rounded bg-emerald-600 px-3 py-1.5 text-xs font-semibold text-white shadow-sm hover:bg-emerald-700 disabled:opacity-50"
          >
            {initMutation.isPending ? 'Criando mesas...' : 'Inicializar 12 Mesas'}
          </button>
        )}
      </div>

      {feedbackMessage && (
        <div className="flex items-center justify-between rounded border border-zinc-300 bg-zinc-100 px-4 py-2 text-xs text-zinc-800">
          <span>{feedbackMessage}</span>
          <button
            type="button"
            onClick={() => setFeedbackMessage(null)}
            className="font-bold text-zinc-500 hover:text-zinc-800"
          >
            ×
          </button>
        </div>
      )}

      {/* Grade de Mesas: Quadrados verdes (abertas) ou laranja quase vermelho (fechadas) com fonte branca no meio */}
      {isLoadingTables ? (
        <div className="rounded-xl border border-zinc-200 bg-white p-12 text-center text-xs text-zinc-500">
          Carregando mesas do salão...
        </div>
      ) : tablesError ? (
        <div className="rounded-xl border border-red-200 bg-red-50 p-6 text-center text-xs text-red-700">
          Erro ao carregar mesas: {(tablesError as Error).message}
        </div>
      ) : sortedTables.length === 0 ? (
        <div className="rounded-xl border border-zinc-200 bg-white p-12 text-center text-xs text-zinc-500">
          Nenhuma mesa cadastrada. Clique no botão acima para inicializar as mesas do restaurante.
        </div>
      ) : (
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6">
          {sortedTables.map((table) => {
            // Mesa fechada = status Closing (4). Naturalmente aberta = qualquer outro status.
            const isClosed = table.status === TableStatus.Closing
            const tileBg = isClosed
              ? 'bg-orange-600 hover:bg-orange-700 focus:ring-orange-400/50'
              : 'bg-emerald-600 hover:bg-emerald-700 focus:ring-emerald-400/50'

            return (
              <button
                key={table.id}
                type="button"
                onClick={() => setSelectedTableId(table.id ?? null)}
                className={`group relative flex aspect-square cursor-pointer flex-col items-center justify-center rounded-2xl ${tileBg} p-4 shadow-md transition-all duration-150 hover:shadow-lg hover:scale-105 active:scale-95 focus:outline-none focus:ring-4`}
              >
                {/* Número da mesa em fonte branca grande no centro */}
                <span className="font-mono text-3xl sm:text-4xl font-black text-white select-none">
                  {table.number}
                </span>

                {/* Rótulo de status em branco */}
                <span className="mt-1 text-[11px] font-semibold text-white/90 select-none">
                  {isClosed ? 'Fechada' : 'Aberta'}
                </span>
              </button>
            )
          })}
        </div>
      )}

      {/* Guia Pequena (Modal de Operação Direta da Mesa) */}
      {selectedTable && (
        <TableDetailsModal
          table={selectedTable}
          activeBill={selectedTable.id ? activeBillByTableId.get(selectedTable.id) : undefined}
          onClose={() => setSelectedTableId(null)}
        />
      )}
    </div>
  )
}

interface TableDetailsModalProps {
  table: TableResponse
  activeBill?: BillResponse
  onClose: () => void
}

function TableDetailsModal({ table, activeBill, onClose }: TableDetailsModalProps) {
  const queryClient = useQueryClient()
  const [selectedProductId, setSelectedProductId] = useState<string>('')
  const [quantity, setQuantity] = useState<number>(1)
  const [itemNotes, setItemNotes] = useState<string>('')
  const [actionMessage, setActionMessage] = useState<string | null>(null)
  const [customerModalNotice, setCustomerModalNotice] = useState(false)

  // Mesa fechada se o status dela for Closing
  const isClosed = table.status === TableStatus.Closing

  // 1. Busca produtos do cardápio para adicionar na mesa
  const { data: products = [] } = useQuery({
    queryKey: ['products'],
    queryFn: () => api.getProducts(),
    enabled: !isClosed,
  })

  // 2. Busca pedidos da comanda ativa (se existir) para listar o consumo
  const { data: orders = [], isLoading: isLoadingOrders } = useQuery({
    queryKey: ['orders', activeBill?.id],
    queryFn: () => (activeBill?.id ? api.getOrdersByBillId(activeBill.id) : Promise.resolve([])),
    enabled: Boolean(activeBill?.id),
    refetchInterval: 3000,
  })

  // Consolidação de itens consumidos
  const consumedItems = orders.flatMap((order: OrderResponse) => order.items || [])
  const totalAmount = consumedItems.reduce(
    (sum, item) => sum + (item.totalPrice ?? (item.unitPrice ?? 0) * (item.quantity ?? 1)),
    0,
  )

  // Mutação: Reabrir Mesa (quando estiver fechada)
  const reopenTableMutation = useMutation({
    mutationFn: async () => {
      if (!table.id) return
      // Retorna o status da mesa para Disponível (Aberta)
      await api.updateTableStatus(table.id, { status: TableStatus.Available })
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tables'] })
      queryClient.invalidateQueries({ queryKey: ['bills'] })
      setActionMessage('Mesa reaberta com sucesso! Pronta para novos lançamentos.')
    },
    onError: (err: Error) => {
      setActionMessage(`Erro ao reabrir mesa: ${err.message}`)
    },
  })

  // Mutação: Lançar Item na Mesa (cria comanda automaticamente se ainda não existir)
  const addItemMutation = useMutation({
    mutationFn: async () => {
      if (!table.id || !selectedProductId) return

      let currentBillId = activeBill?.id

      // Se a mesa ainda não tiver comanda ativa, cria uma automaticamente de forma transparente
      if (!currentBillId) {
        const newBill = await api.openBill({
          tableId: table.id,
          notes: `Atendimento Mesa #${table.number}`,
        })
        currentBillId = newBill.id
      }

      if (!currentBillId) {
        throw new Error('Não foi possível identificar a comanda da mesa.')
      }

      await api.createOrder({
        billId: currentBillId,
        items: [
          {
            productId: selectedProductId,
            quantity: quantity,
            notes: itemNotes.trim() || null,
          },
        ],
      })
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tables'] })
      queryClient.invalidateQueries({ queryKey: ['bills'] })
      queryClient.invalidateQueries({ queryKey: ['orders'] })
      setSelectedProductId('')
      setQuantity(1)
      setItemNotes('')
      setActionMessage('Item adicionado à mesa!')
    },
    onError: (err: Error) => {
      setActionMessage(`Erro ao adicionar item: ${err.message}`)
    },
  })

  // Mutação: Fechar Mesa (finaliza pedidos pendentes, fecha comanda e muda cor para laranja quase vermelho)
  const closeTableMutation = useMutation({
    mutationFn: async () => {
      if (!table.id) return

      // 1. Se houver comanda com pedidos pendentes, finaliza os pedidos para que o backend permita o fechamento
      if (activeBill?.id) {
        try {
          const currentOrders = await api.getOrdersByBillId(activeBill.id)
          for (const order of currentOrders) {
            if (
              order.id &&
              (order.status === OrderStatus.Pending || order.status === OrderStatus.InPreparation)
            ) {
              await api.updateOrderStatus(order.id, { status: OrderStatus.Delivered })
            }
          }
          await api.closeBill(activeBill.id)
        } catch {
          // Continua mesmo se a comanda já tiver sido fechada
        }
      }

      // 2. Define o status da mesa como Closing (Fechada -> cor laranja quase vermelho)
      await api.updateTableStatus(table.id, { status: TableStatus.Closing })
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tables'] })
      queryClient.invalidateQueries({ queryKey: ['bills'] })
      queryClient.invalidateQueries({ queryKey: ['orders'] })
      setActionMessage('Mesa fechada com sucesso.')
    },
    onError: (err: Error) => {
      setActionMessage(`Erro ao fechar mesa: ${err.message}`)
    },
  })

  const handleAddItem = (e: React.FormEvent) => {
    e.preventDefault()
    if (!selectedProductId || quantity <= 0) return
    addItemMutation.mutate()
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-xs p-4">
      <div className="flex max-h-[92vh] w-full max-w-lg flex-col overflow-hidden rounded-2xl bg-white shadow-2xl animate-in fade-in zoom-in-95 duration-150">
        {/* Cabeçalho da Guia Pequena */}
        <div className="flex items-center justify-between border-b border-zinc-200 bg-zinc-50 px-6 py-4">
          <div className="flex items-center gap-3">
            <div
              className={`flex h-10 w-10 items-center justify-center rounded-xl font-mono text-lg font-black text-white shadow-sm ${
                isClosed ? 'bg-orange-600' : 'bg-emerald-600'
              }`}
            >
              {table.number}
            </div>
            <div>
              <h3 className="text-base font-bold text-zinc-900">
                Mesa {table.number}
              </h3>
              <div className="flex items-center gap-2">
                <span
                  className={`inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-[10px] font-bold ${
                    isClosed
                      ? 'bg-orange-100 text-orange-800'
                      : 'bg-emerald-100 text-emerald-800'
                  }`}
                >
                  <span
                    className={`h-1.5 w-1.5 rounded-full ${
                      isClosed ? 'bg-orange-600' : 'bg-emerald-600'
                    }`}
                  />
                  {isClosed ? 'Mesa Fechada' : 'Mesa Aberta'}
                </span>
                {!isClosed && activeBill && (
                  <span className="text-[11px] text-zinc-400">
                    • Comanda #{activeBill.number}
                  </span>
                )}
              </div>
            </div>
          </div>

          <button
            type="button"
            onClick={onClose}
            className="flex h-8 w-8 items-center justify-center rounded-lg text-zinc-400 transition hover:bg-zinc-200 hover:text-zinc-700"
          >
            ✕
          </button>
        </div>

        {/* Notificação temporária de ação */}
        {actionMessage && (
          <div className="mx-6 mt-4 flex items-center justify-between rounded-lg bg-zinc-100 px-3 py-2 text-xs text-zinc-800">
            <span>{actionMessage}</span>
            <button
              type="button"
              onClick={() => setActionMessage(null)}
              className="text-zinc-400 hover:text-zinc-700"
            >
              ✕
            </button>
          </div>
        )}

        {/* Barra superior: Botão Vincular Cliente (não funcional por enquanto) */}
        <div className="border-b border-zinc-100 px-6 py-2.5 bg-zinc-50/50 flex items-center justify-between">
          <button
            type="button"
            onClick={() => setCustomerModalNotice(true)}
            className="inline-flex items-center gap-1.5 rounded-lg border border-zinc-300 bg-white px-3 py-1 text-xs font-medium text-zinc-700 shadow-xs hover:bg-zinc-50 active:scale-98"
          >
            <span>👤</span>
            <span>Vincular Cliente</span>
            <span className="rounded bg-zinc-100 px-1 py-0.2 text-[9px] font-semibold text-zinc-500 uppercase tracking-wider">
              Em breve
            </span>
          </button>

          {customerModalNotice && (
            <span className="text-[11px] text-zinc-500 italic">
              Funcionalidade em desenvolvimento!
            </span>
          )}
        </div>

        {/* Corpo principal da Guia Pequena */}
        <div className="flex-1 overflow-y-auto px-6 py-4 space-y-5">
          {isClosed ? (
            /* CENÁRIO 1: MESA FECHADA */
            <div className="flex flex-col items-center justify-center py-8 text-center space-y-4">
              <div className="flex h-16 w-16 items-center justify-center rounded-full bg-orange-100 text-3xl text-orange-600">
                🔒
              </div>
              <div>
                <h4 className="text-sm font-bold text-zinc-900">
                  A Mesa {table.number} está Fechada
                </h4>
                <p className="mt-1 text-xs text-zinc-500 max-w-xs">
                  A conta desta mesa foi encerrada. Clique no botão abaixo para reabri-la e receber novos pedidos.
                </p>
              </div>

              <button
                type="button"
                onClick={() => reopenTableMutation.mutate()}
                disabled={reopenTableMutation.isPending}
                className="inline-flex items-center gap-2 rounded-xl bg-emerald-600 px-5 py-2.5 text-xs font-bold text-white shadow-md hover:bg-emerald-700 active:scale-95 disabled:opacity-50 transition"
              >
                <span>🟢</span>
                <span>
                  {reopenTableMutation.isPending ? 'Reabrindo...' : 'Reabrir Mesa (Ficar Aberta)'}
                </span>
              </button>
            </div>
          ) : (
            /* CENÁRIO 2: MESA ABERTA NATURALMENTE */
            <div className="space-y-5">
              {/* Formulário: Lançar Itens na Mesa */}
              <div className="rounded-xl border border-zinc-200 bg-zinc-50/50 p-4">
                <h4 className="text-xs font-bold uppercase tracking-wider text-zinc-700 mb-3">
                  + Lançar Itens na Mesa
                </h4>

                <form onSubmit={handleAddItem} className="space-y-3">
                  <div className="grid grid-cols-1 sm:grid-cols-3 gap-2">
                    {/* Seletor de Produto */}
                    <div className="sm:col-span-2">
                      <label htmlFor="modal-product" className="block text-[11px] font-medium text-zinc-600 mb-1">
                        Produto do Cardápio *
                      </label>
                      <select
                        id="modal-product"
                        required
                        value={selectedProductId}
                        onChange={(e) => setSelectedProductId(e.target.value)}
                        className="w-full rounded-lg border border-zinc-300 bg-white px-2.5 py-1.5 text-xs text-zinc-900 focus:border-emerald-600 focus:outline-none"
                      >
                        <option value="">Selecione um produto...</option>
                        {products.map((p) => (
                          <option key={p.id} value={p.id}>
                            {p.name} — R$ {(p.price ?? 0).toFixed(2).replace('.', ',')}
                          </option>
                        ))}
                      </select>
                    </div>

                    {/* Quantidade */}
                    <div>
                      <label htmlFor="modal-quantity" className="block text-[11px] font-medium text-zinc-600 mb-1">
                        Qtd *
                      </label>
                      <div className="flex items-center">
                        <button
                          type="button"
                          onClick={() => setQuantity((q) => Math.max(1, q - 1))}
                          className="flex h-7 w-7 items-center justify-center rounded-l border border-r-0 border-zinc-300 bg-white text-xs font-bold text-zinc-700 hover:bg-zinc-100"
                        >
                          -
                        </button>
                        <input
                          id="modal-quantity"
                          type="number"
                          min="1"
                          required
                          value={quantity}
                          onChange={(e) => setQuantity(Math.max(1, parseInt(e.target.value, 10) || 1))}
                          className="h-7 w-12 border border-zinc-300 bg-white text-center text-xs text-zinc-900 focus:outline-none"
                        />
                        <button
                          type="button"
                          onClick={() => setQuantity((q) => q + 1)}
                          className="flex h-7 w-7 items-center justify-center rounded-r border border-l-0 border-zinc-300 bg-white text-xs font-bold text-zinc-700 hover:bg-zinc-100"
                        >
                          +
                        </button>
                      </div>
                    </div>
                  </div>

                  {/* Observação opcional */}
                  <div>
                    <input
                      type="text"
                      placeholder="Observações do item (Ex: Sem cebola, gelo e limão...)"
                      value={itemNotes}
                      onChange={(e) => setItemNotes(e.target.value)}
                      className="w-full rounded-lg border border-zinc-300 bg-white px-2.5 py-1.5 text-xs text-zinc-900 placeholder-zinc-400 focus:border-emerald-600 focus:outline-none"
                    />
                  </div>

                  <div className="flex justify-end pt-1">
                    <button
                      type="submit"
                      disabled={!selectedProductId || addItemMutation.isPending}
                      className="inline-flex items-center gap-1.5 rounded-lg bg-emerald-600 px-3.5 py-1.5 text-xs font-bold text-white shadow-xs hover:bg-emerald-700 active:scale-95 disabled:opacity-40 transition"
                    >
                      {addItemMutation.isPending ? 'Lançando...' : '+ Adicionar à Mesa'}
                    </button>
                  </div>
                </form>
              </div>

              {/* Itens Já Consumidos */}
              <div>
                <div className="flex items-center justify-between mb-2">
                  <h4 className="text-xs font-bold uppercase tracking-wider text-zinc-700">
                    Consumo Atual ({consumedItems.length} {consumedItems.length === 1 ? 'item' : 'itens'})
                  </h4>
                  {isLoadingOrders && (
                    <span className="text-[10px] text-zinc-400 animate-pulse">Atualizando...</span>
                  )}
                </div>

                {consumedItems.length === 0 ? (
                  <div className="rounded-xl border border-dashed border-zinc-300 p-6 text-center text-xs text-zinc-400">
                    Nenhum item consumido ainda nesta mesa. Lance o primeiro produto acima!
                  </div>
                ) : (
                  <div className="overflow-hidden rounded-xl border border-zinc-200">
                    <div className="max-h-48 overflow-y-auto divide-y divide-zinc-100">
                      {consumedItems.map((item, idx) => (
                        <div key={item.id || idx} className="flex items-center justify-between px-3 py-2 bg-white text-xs">
                          <div>
                            <div className="font-medium text-zinc-900">
                              <span className="font-bold text-emerald-700 mr-1.5">{item.quantity}x</span>
                              {item.productName}
                            </div>
                            {item.notes && (
                              <div className="text-[11px] text-zinc-500 italic">
                                Obs: {item.notes}
                              </div>
                            )}
                          </div>
                          <div className="text-right font-mono font-semibold text-zinc-800">
                            R$ {(item.totalPrice ?? (item.unitPrice ?? 0) * (item.quantity ?? 1)).toFixed(2).replace('.', ',')}
                          </div>
                        </div>
                      ))}
                    </div>

                    {/* Totalizador */}
                    <div className="flex items-center justify-between border-t border-zinc-200 bg-zinc-50 px-4 py-2.5">
                      <span className="text-xs font-bold uppercase tracking-wider text-zinc-700">
                        Total da Conta:
                      </span>
                      <span className="font-mono text-base font-extrabold text-emerald-700">
                        R$ {totalAmount.toFixed(2).replace('.', ',')}
                      </span>
                    </div>
                  </div>
                )}
              </div>
            </div>
          )}
        </div>

        {/* Rodapé da Guia Pequena */}
        <div className="border-t border-zinc-200 bg-zinc-50 px-6 py-3.5 flex items-center justify-between">
          <button
            type="button"
            onClick={onClose}
            className="rounded-lg border border-zinc-300 bg-white px-3 py-1.5 text-xs font-medium text-zinc-700 hover:bg-zinc-50"
          >
            Fechar Janela
          </button>

          {!isClosed && (
            <button
              type="button"
              onClick={() => {
                if (confirm(`Deseja realmente fechar a Mesa #${table.number}? A cor da mesa mudará para laranja avermelhado.`)) {
                  closeTableMutation.mutate()
                }
              }}
              disabled={closeTableMutation.isPending}
              className="inline-flex items-center gap-1.5 rounded-lg bg-orange-600 px-4 py-1.5 text-xs font-bold text-white shadow-xs hover:bg-orange-700 active:scale-95 disabled:opacity-50 transition"
            >
              <span>🔒</span>
              <span>{closeTableMutation.isPending ? 'Fechando Mesa...' : 'Fechar Mesa'}</span>
            </button>
          )}
        </div>
      </div>
    </div>
  )
}
