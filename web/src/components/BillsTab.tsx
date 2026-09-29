import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { BillStatus, BillStatusLabels } from '../constants/enums'
import {
  api,
  type BillStatusType,
  type OpenBillRequest,
} from '../services/api'

export function BillsTab() {
  const queryClient = useQueryClient()
  const [statusFilter, setStatusFilter] = useState<string>('')
  const [searchNumber, setSearchNumber] = useState<string>('')
  const [isOpening, setIsOpening] = useState(false)
  const [feedbackMessage, setFeedbackMessage] = useState<string | null>(null)

  // Consulta de mesas para vincular na abertura de comanda
  const { data: tables = [] } = useQuery({
    queryKey: ['tables'],
    queryFn: () => api.getTables(),
  })

  // Consulta de comandas
  const {
    data: bills = [],
    isLoading,
    error,
  } = useQuery({
    queryKey: ['bills', statusFilter],
    queryFn: () =>
      api.getBills(
        statusFilter
          ? { status: parseInt(statusFilter, 10) as BillStatusType }
          : undefined,
      ),
  })

  // Formulário State
  const [formData, setFormData] = useState<OpenBillRequest>({
    number: 101,
    tableId: undefined,
    customerName: '',
    notes: '',
  })

  // Mutation: Abrir Comanda
  const openMutation = useMutation({
    mutationFn: api.openBill,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['bills'] })
      queryClient.invalidateQueries({ queryKey: ['tables'] }) // Atualiza status da mesa se vinculada
      setIsOpening(false)
      setFormData({ number: 101, tableId: undefined, customerName: '', notes: '' })
      setFeedbackMessage('Comanda aberta com sucesso.')
    },
    onError: (err: Error) => {
      setFeedbackMessage(`Erro ao abrir comanda: ${err.message}`)
    },
  })

  // Mutation: Fechar Comanda
  const closeMutation = useMutation({
    mutationFn: ({ id, notes }: { id: string; notes?: string }) =>
      api.closeBill(id, { notes }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['bills'] })
      queryClient.invalidateQueries({ queryKey: ['tables'] })
      setFeedbackMessage('Comanda encerrada com sucesso.')
    },
    onError: (err: Error) => {
      setFeedbackMessage(`Erro ao encerrar comanda: ${err.message}`)
    },
  })

  // Mutation: Cancelar Comanda
  const cancelMutation = useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) =>
      api.cancelBill(id, { reason }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['bills'] })
      queryClient.invalidateQueries({ queryKey: ['tables'] })
      setFeedbackMessage('Comanda cancelada com sucesso.')
    },
    onError: (err: Error) => {
      setFeedbackMessage(`Erro ao cancelar comanda: ${err.message}`)
    },
  })

  const handleOpenSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (!formData.number || formData.number <= 0) return

    openMutation.mutate({
      number: formData.number,
      tableId: formData.tableId || null,
      customerName: formData.customerName?.trim() || null,
      notes: formData.notes?.trim() || null,
    })
  }

  // Filtragem local se o usuário pesquisar por número específico
  const displayedBills = searchNumber.trim()
    ? bills.filter((b) => b.number?.toString().includes(searchNumber.trim()))
    : bills

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h2 className="text-lg font-semibold text-zinc-900">
            Comandas de Consumo
          </h2>
          <p className="text-xs text-zinc-500">
            Comandas universais para mesas do salão ou atendimento avulso/balcão
          </p>
        </div>

        <div className="flex flex-wrap items-center gap-2">
          {/* Busca por Número */}
          <input
            type="number"
            placeholder="Buscar por Nº..."
            value={searchNumber}
            onChange={(e) => setSearchNumber(e.target.value)}
            className="w-32 rounded border border-zinc-300 bg-white px-2.5 py-1.5 text-xs text-zinc-800 placeholder-zinc-400 focus:border-zinc-800 focus:outline-none"
          />

          {/* Filtro por Status */}
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            className="rounded border border-zinc-300 bg-white px-3 py-1.5 text-xs text-zinc-800 focus:border-zinc-800 focus:outline-none"
          >
            <option value="">Todos os Status</option>
            <option value={BillStatus.Open}>Abertas</option>
            <option value={BillStatus.Closed}>Fechadas</option>
            <option value={BillStatus.Cancelled}>Canceladas</option>
          </select>

          {!isOpening && (
            <button
              type="button"
              onClick={() => setIsOpening(true)}
              className="rounded bg-zinc-900 px-3 py-1.5 text-xs font-semibold text-white shadow-sm transition hover:bg-zinc-800"
            >
              + Abrir Comanda
            </button>
          )}
        </div>
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

      {/* Formulário de Abertura de Comanda */}
      {isOpening && (
        <form
          onSubmit={handleOpenSubmit}
          className="rounded-lg border border-zinc-300 bg-white p-5 shadow-sm space-y-4"
        >
          <div className="border-b border-zinc-200 pb-2">
            <h3 className="text-sm font-semibold text-zinc-900">
              Abertura de Nova Comanda
            </h3>
          </div>

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
            <div>
              <label htmlFor="bill-number" className="block text-xs font-medium text-zinc-700">
                Número da Comanda *
              </label>
              <input
                id="bill-number"
                type="number"
                min="1"
                required
                value={formData.number ?? 101}
                onChange={(e) =>
                  setFormData({ ...formData, number: parseInt(e.target.value, 10) || 1 })
                }
                className="mt-1 w-full rounded border border-zinc-300 px-3 py-1.5 text-xs text-zinc-900 focus:border-zinc-800 focus:outline-none"
              />
            </div>

            <div>
              <label htmlFor="bill-table" className="block text-xs font-medium text-zinc-700">
                Mesa / Posição (Opcional)
              </label>
              <select
                id="bill-table"
                value={formData.tableId || ''}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    tableId: e.target.value ? e.target.value : undefined,
                  })
                }
                className="mt-1 w-full rounded border border-zinc-300 bg-white px-3 py-1.5 text-xs text-zinc-900 focus:border-zinc-800 focus:outline-none"
              >
                <option value="">Atendimento Avulso / Balcão</option>
                {tables.map((t) => (
                  <option key={t.id} value={t.id}>
                    Mesa #{t.number} ({t.capacity} lugares)
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label htmlFor="bill-customer" className="block text-xs font-medium text-zinc-700">
                Nome do Cliente (Opcional)
              </label>
              <input
                id="bill-customer"
                type="text"
                value={formData.customerName || ''}
                onChange={(e) => setFormData({ ...formData, customerName: e.target.value })}
                placeholder="Ex: Carlos Oliveira"
                className="mt-1 w-full rounded border border-zinc-300 px-3 py-1.5 text-xs text-zinc-900 focus:border-zinc-800 focus:outline-none"
              />
            </div>

            <div className="sm:col-span-3">
              <label htmlFor="bill-notes" className="block text-xs font-medium text-zinc-700">
                Observações
              </label>
              <input
                id="bill-notes"
                type="text"
                value={formData.notes || ''}
                onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
                placeholder="Ex: Cliente aguardando na varanda"
                className="mt-1 w-full rounded border border-zinc-300 px-3 py-1.5 text-xs text-zinc-900 focus:border-zinc-800 focus:outline-none"
              />
            </div>
          </div>

          <div className="flex justify-end gap-2 pt-2">
            <button
              type="button"
              onClick={() => setIsOpening(false)}
              className="rounded border border-zinc-300 bg-white px-3 py-1.5 text-xs font-medium text-zinc-700 hover:bg-zinc-50"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={openMutation.isPending}
              className="rounded bg-zinc-900 px-4 py-1.5 text-xs font-semibold text-white hover:bg-zinc-800 disabled:opacity-50"
            >
              {openMutation.isPending ? 'Abrindo...' : 'Abrir Comanda'}
            </button>
          </div>
        </form>
      )}

      {/* Lista / Tabela de Comandas */}
      <div className="overflow-hidden rounded-lg border border-zinc-300 bg-white shadow-sm">
        <div className="border-b border-zinc-200 bg-zinc-50 px-4 py-3">
          <span className="text-xs font-semibold uppercase tracking-wider text-zinc-600">
            Comandas Encontradas: {displayedBills.length}
          </span>
        </div>

        {isLoading ? (
          <div className="p-8 text-center text-xs text-zinc-500">
            Carregando comandas...
          </div>
        ) : error ? (
          <div className="p-8 text-center text-xs text-zinc-700">
            Erro ao carregar comandas: {(error as Error).message}
          </div>
        ) : displayedBills.length === 0 ? (
          <div className="p-8 text-center text-xs text-zinc-500">
            Nenhuma comanda encontrada. Clique em "+ Abrir Comanda" acima para iniciar um atendimento.
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-xs text-zinc-800">
              <thead className="border-b border-zinc-200 bg-zinc-50 font-semibold text-zinc-700">
                <tr>
                  <th className="px-4 py-3">Comanda</th>
                  <th className="px-4 py-3">Vínculo</th>
                  <th className="px-4 py-3">Cliente</th>
                  <th className="px-4 py-3">Status</th>
                  <th className="px-4 py-3">Horário Abertura</th>
                  <th className="px-4 py-3 text-right">Ações Operacionais</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-zinc-200">
                {displayedBills.map((bill) => {
                  const statusVal = bill.status ?? BillStatus.Open
                  const isOpen = statusVal === BillStatus.Open
                  const statusText = BillStatusLabels[statusVal] || 'Desconhecido'

                  return (
                    <tr key={bill.id} className="hover:bg-zinc-50">
                      <td className="px-4 py-3 font-mono font-bold text-zinc-900">
                        #{bill.number}
                      </td>
                      <td className="px-4 py-3 text-zinc-600">
                        {bill.tableNumber ? (
                          <span className="inline-flex items-center gap-1 rounded bg-zinc-100 px-2 py-0.5 text-[11px] font-medium text-zinc-800 border border-zinc-200">
                            Mesa #{bill.tableNumber}
                          </span>
                        ) : (
                          <span className="text-zinc-400">Avulso / Balcão</span>
                        )}
                      </td>
                      <td className="px-4 py-3 text-zinc-700">
                        {bill.customerName || '-'}
                      </td>
                      <td className="px-4 py-3">
                        <span
                          className={`inline-flex rounded px-2 py-0.5 text-[11px] font-semibold border ${
                            statusVal === BillStatus.Open
                              ? 'border-zinc-400 bg-zinc-100 text-zinc-900'
                              : statusVal === BillStatus.Closed
                                ? 'border-zinc-300 bg-zinc-50 text-zinc-600'
                                : 'border-zinc-200 bg-zinc-50 text-zinc-400 line-through'
                          }`}
                        >
                          {statusText}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-zinc-500 font-mono text-[11px]">
                        {bill.openedAt
                          ? new Date(bill.openedAt).toLocaleTimeString('pt-BR', {
                              hour: '2-digit',
                              minute: '2-digit',
                            })
                          : '-'}
                      </td>
                      <td className="px-4 py-3 text-right space-x-2">
                        {isOpen ? (
                          <>
                            <button
                              type="button"
                              onClick={() => {
                                if (
                                  bill.id &&
                                  confirm(`Deseja encerrar a comanda #${bill.number}?`)
                                ) {
                                  closeMutation.mutate({ id: bill.id })
                                }
                              }}
                              className="rounded border border-zinc-300 bg-zinc-900 px-2.5 py-1 text-[11px] font-semibold text-white hover:bg-zinc-800"
                            >
                              Encerrar
                            </button>
                            <button
                              type="button"
                              onClick={() => {
                                if (!bill.id) return
                                const reason = prompt(
                                  `Informe o motivo do cancelamento da comanda #${bill.number}:`,
                                  'Abertura indevida / cliente desistiu',
                                )
                                if (reason && reason.trim()) {
                                  cancelMutation.mutate({ id: bill.id, reason: reason.trim() })
                                }
                              }}
                              className="rounded border border-zinc-300 bg-zinc-100 px-2.5 py-1 text-[11px] font-medium text-zinc-700 hover:bg-zinc-200"
                            >
                              Cancelar
                            </button>
                          </>
                        ) : (
                          <span className="text-[11px] text-zinc-400 italic">
                            Encerrada
                          </span>
                        )}
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  )
}
