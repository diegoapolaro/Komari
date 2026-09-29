import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import {
  TableStatus,
  TableStatusLabels,
  TableType,
  TableTypeLabels,
} from '../constants/enums'
import {
  api,
  type CreateTableRequest,
  type TableResponse,
  type TableStatusType,
  type TableTypeType,
} from '../services/api'

export function TablesTab() {
  const queryClient = useQueryClient()
  const [statusFilter, setStatusFilter] = useState<string>('')
  const [isCreating, setIsCreating] = useState(false)
  const [editingTable, setEditingTable] = useState<TableResponse | null>(null)
  const [feedbackMessage, setFeedbackMessage] = useState<string | null>(null)

  // Consulta de mesas
  const {
    data: tables = [],
    isLoading,
    error,
  } = useQuery({
    queryKey: ['tables', statusFilter],
    queryFn: () =>
      api.getTables(
        statusFilter
          ? { status: parseInt(statusFilter, 10) as TableStatusType }
          : undefined,
      ),
  })

  // Form State
  const [formData, setFormData] = useState<CreateTableRequest>({
    number: 1,
    capacity: 4,
    type: TableType.DiningTable,
    location: '',
  })

  // Mutation: Criar
  const createMutation = useMutation({
    mutationFn: api.createTable,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tables'] })
      resetForm()
      setFeedbackMessage('Ponto de atendimento cadastrado com sucesso.')
    },
    onError: (err: Error) => {
      setFeedbackMessage(`Erro ao cadastrar mesa: ${err.message}`)
    },
  })

  // Mutation: Atualizar
  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: CreateTableRequest }) =>
      api.updateTable(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tables'] })
      resetForm()
      setFeedbackMessage('Dados da mesa atualizados com sucesso.')
    },
    onError: (err: Error) => {
      setFeedbackMessage(`Erro ao atualizar mesa: ${err.message}`)
    },
  })

  // Mutation: Alterar Status
  const statusMutation = useMutation({
    mutationFn: ({ id, status }: { id: string; status: TableStatusType }) =>
      api.updateTableStatus(id, { status }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tables'] })
      setFeedbackMessage('Status da mesa atualizado.')
    },
    onError: (err: Error) => {
      setFeedbackMessage(`Erro ao alterar status: ${err.message}`)
    },
  })

  // Mutation: Deletar
  const deleteMutation = useMutation({
    mutationFn: api.deleteTable,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tables'] })
      setFeedbackMessage('Mesa removida com sucesso.')
    },
    onError: (err: Error) => {
      setFeedbackMessage(`Erro ao excluir: ${err.message}`)
    },
  })

  const resetForm = () => {
    setIsCreating(false)
    setEditingTable(null)
    setFormData({
      number: 1,
      capacity: 4,
      type: TableType.DiningTable,
      location: '',
    })
  }

  const startEdit = (table: TableResponse) => {
    setEditingTable(table)
    setFormData({
      number: table.number ?? 1,
      capacity: table.capacity ?? 4,
      type: (table.type ?? TableType.DiningTable) as TableTypeType,
      location: table.location || '',
    })
    setIsCreating(true)
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (!formData.number || formData.number <= 0) return

    if (editingTable?.id) {
      updateMutation.mutate({ id: editingTable.id, data: formData })
    } else {
      createMutation.mutate(formData)
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h2 className="text-lg font-semibold text-zinc-900">
            Salão & Mesas
          </h2>
          <p className="text-xs text-zinc-500">
            Controle de capacidade física, mesas e posições de balcão
          </p>
        </div>

        <div className="flex items-center gap-2">
          {/* Filtro por Status */}
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            className="rounded border border-zinc-300 bg-white px-3 py-1.5 text-xs text-zinc-800 focus:border-zinc-800 focus:outline-none"
          >
            <option value="">Todos os Status</option>
            <option value={TableStatus.Available}>Disponíveis</option>
            <option value={TableStatus.Occupied}>Ocupadas</option>
            <option value={TableStatus.Reserved}>Reservadas</option>
          </select>

          {!isCreating && (
            <button
              type="button"
              onClick={() => {
                resetForm()
                setIsCreating(true)
              }}
              className="rounded bg-zinc-900 px-3 py-1.5 text-xs font-semibold text-white shadow-sm transition hover:bg-zinc-800"
            >
              + Nova Mesa / Balcão
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

      {/* Formulário de Criação / Edição */}
      {isCreating && (
        <form
          onSubmit={handleSubmit}
          className="rounded-lg border border-zinc-300 bg-white p-5 shadow-sm space-y-4"
        >
          <div className="border-b border-zinc-200 pb-2">
            <h3 className="text-sm font-semibold text-zinc-900">
              {editingTable ? 'Editar Ponto de Atendimento' : 'Cadastrar Mesa ou Balcão'}
            </h3>
          </div>

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-4">
            <div>
              <label htmlFor="table-number" className="block text-xs font-medium text-zinc-700">
                Número da Mesa *
              </label>
              <input
                id="table-number"
                type="number"
                min="1"
                required
                value={formData.number ?? 1}
                onChange={(e) =>
                  setFormData({ ...formData, number: parseInt(e.target.value, 10) || 1 })
                }
                className="mt-1 w-full rounded border border-zinc-300 px-3 py-1.5 text-xs text-zinc-900 focus:border-zinc-800 focus:outline-none"
              />
            </div>

            <div>
              <label htmlFor="table-capacity" className="block text-xs font-medium text-zinc-700">
                Capacidade (Lugares) *
              </label>
              <input
                id="table-capacity"
                type="number"
                min="1"
                required
                value={formData.capacity ?? 4}
                onChange={(e) =>
                  setFormData({ ...formData, capacity: parseInt(e.target.value, 10) || 1 })
                }
                className="mt-1 w-full rounded border border-zinc-300 px-3 py-1.5 text-xs text-zinc-900 focus:border-zinc-800 focus:outline-none"
              />
            </div>

            <div>
              <label htmlFor="table-type" className="block text-xs font-medium text-zinc-700">
                Modalidade *
              </label>
              <select
                id="table-type"
                value={formData.type ?? TableType.DiningTable}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    type: parseInt(e.target.value, 10) as TableTypeType,
                  })
                }
                className="mt-1 w-full rounded border border-zinc-300 bg-white px-3 py-1.5 text-xs text-zinc-900 focus:border-zinc-800 focus:outline-none"
              >
                <option value={TableType.DiningTable}>Mesa de Salão</option>
                <option value={TableType.Counter}>Posição de Balcão</option>
              </select>
            </div>

            <div>
              <label htmlFor="table-location" className="block text-xs font-medium text-zinc-700">
                Localização / Descrição
              </label>
              <input
                id="table-location"
                type="text"
                value={formData.location || ''}
                onChange={(e) => setFormData({ ...formData, location: e.target.value })}
                placeholder="Ex: Próximo à janela / Varanda"
                className="mt-1 w-full rounded border border-zinc-300 px-3 py-1.5 text-xs text-zinc-900 focus:border-zinc-800 focus:outline-none"
              />
            </div>
          </div>

          <div className="flex justify-end gap-2 pt-2">
            <button
              type="button"
              onClick={resetForm}
              className="rounded border border-zinc-300 bg-white px-3 py-1.5 text-xs font-medium text-zinc-700 hover:bg-zinc-50"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={createMutation.isPending || updateMutation.isPending}
              className="rounded bg-zinc-900 px-4 py-1.5 text-xs font-semibold text-white hover:bg-zinc-800 disabled:opacity-50"
            >
              {createMutation.isPending || updateMutation.isPending ? 'Salvando...' : 'Salvar'}
            </button>
          </div>
        </form>
      )}

      {/* Grade de Mesas */}
      <div className="rounded-lg border border-zinc-300 bg-white p-4 shadow-sm">
        <div className="mb-4 flex items-center justify-between border-b border-zinc-200 pb-3">
          <span className="text-xs font-semibold uppercase tracking-wider text-zinc-600">
            Total de Pontos de Atendimento: {tables.length}
          </span>
        </div>

        {isLoading ? (
          <div className="p-8 text-center text-xs text-zinc-500">
            Carregando mesas...
          </div>
        ) : error ? (
          <div className="p-8 text-center text-xs text-zinc-700">
            Erro ao carregar mesas: {(error as Error).message}
          </div>
        ) : tables.length === 0 ? (
          <div className="p-8 text-center text-xs text-zinc-500">
            Nenhuma mesa cadastrada {statusFilter ? 'com esse status' : ''}. Clique em "+ Nova Mesa" acima para cadastrar.
          </div>
        ) : (
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4">
            {tables.map((table) => {
              const statusVal = (table.status ?? TableStatus.Available) as TableStatusType
              const statusText = TableStatusLabels[statusVal] || 'Desconhecido'
              const typeVal = (table.type ?? TableType.DiningTable) as TableTypeType
              const typeText = TableTypeLabels[typeVal] || 'Mesa'

              return (
                <div
                  key={table.id}
                  className={`flex flex-col justify-between rounded border p-4 transition ${
                    statusVal === TableStatus.Occupied
                      ? 'border-zinc-400 bg-zinc-100'
                      : statusVal === TableStatus.Reserved
                        ? 'border-zinc-300 bg-zinc-50'
                        : 'border-zinc-200 bg-white'
                  }`}
                >
                  <div>
                    <div className="flex items-center justify-between">
                      <span className="text-base font-bold text-zinc-900">
                        {typeText} #{table.number}
                      </span>
                      <span className="rounded border border-zinc-300 bg-white px-2 py-0.5 text-[10px] font-semibold text-zinc-700">
                        {table.capacity} lugares
                      </span>
                    </div>

                    {table.location && (
                      <p className="mt-1 text-xs text-zinc-500">
                        {table.location}
                      </p>
                    )}

                    {/* Status e Seletor Rápido */}
                    <div className="mt-3">
                      <label htmlFor={`table-status-${table.id}`} className="block text-[11px] font-medium text-zinc-500">
                        Status atual:
                      </label>
                      <select
                        id={`table-status-${table.id}`}
                        value={statusVal}
                        onChange={(e) => {
                          if (table.id) {
                            statusMutation.mutate({
                              id: table.id,
                              status: parseInt(e.target.value, 10) as TableStatusType,
                            })
                          }
                        }}
                        className="mt-1 w-full rounded border border-zinc-300 bg-white px-2 py-1 text-xs font-semibold text-zinc-900 focus:border-zinc-800 focus:outline-none"
                      >
                        <option value={TableStatus.Available}>🟢 {TableStatusLabels[TableStatus.Available]}</option>
                        <option value={TableStatus.Occupied}>🔴 {TableStatusLabels[TableStatus.Occupied]}</option>
                        <option value={TableStatus.Reserved}>🟡 {TableStatusLabels[TableStatus.Reserved]}</option>
                      </select>
                    </div>
                  </div>

                  {/* Ações */}
                  <div className="mt-4 flex items-center justify-between border-t border-zinc-200 pt-3">
                    <span className="text-[11px] text-zinc-400">
                      {statusText}
                    </span>
                    <div className="space-x-1">
                      <button
                        type="button"
                        onClick={() => startEdit(table)}
                        className="rounded border border-zinc-300 bg-white px-2 py-1 text-[11px] font-medium text-zinc-700 hover:bg-zinc-100"
                      >
                        Editar
                      </button>
                      <button
                        type="button"
                        onClick={() => {
                          if (table.id && confirm(`Excluir a ${typeText} #${table.number}?`)) {
                            deleteMutation.mutate(table.id)
                          }
                        }}
                        className="rounded border border-zinc-300 bg-zinc-100 px-2 py-1 text-[11px] font-medium text-zinc-700 hover:bg-zinc-200"
                      >
                        Excluir
                      </button>
                    </div>
                  </div>
                </div>
              )
            })}
          </div>
        )}
      </div>
    </div>
  )
}
