import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useDeferredValue, useState } from 'react'
import {
  api,
  type CreateCustomerRequest,
  type CustomerResponse,
  type UpdateCustomerRequest,
} from '../services/api'

// Função auxiliar para destacar o trecho do texto correspondente à busca
function HighlightMatch({ text, term }: { text: string; term: string }) {
  if (!term || !text) return <>{text}</>

  // Se o termo de busca for numérico, limpa caracteres especiais para encontrar a ocorrência
  const cleanTerm = term.replace(/[^\w\d]/g, '')
  if (!cleanTerm) return <>{text}</>

  const index = text.toLowerCase().indexOf(term.toLowerCase())
  if (index === -1) {
    // Tenta encontrar por dígitos puros se for número
    const digitsOnly = term.replace(/\D/g, '')
    if (digitsOnly.length >= 2) {
      const digitIndex = text.indexOf(digitsOnly)
      if (digitIndex !== -1) {
        const before = text.slice(0, digitIndex)
        const match = text.slice(digitIndex, digitIndex + digitsOnly.length)
        const after = text.slice(digitIndex + digitsOnly.length)
        return (
          <>
            {before}
            <mark className="rounded bg-amber-200 px-0.5 font-bold text-zinc-900">{match}</mark>
            {after}
          </>
        )
      }
    }
    return <>{text}</>
  }

  const before = text.slice(0, index)
  const match = text.slice(index, index + term.length)
  const after = text.slice(index + term.length)

  return (
    <>
      {before}
      <mark className="rounded bg-amber-200 px-0.5 font-bold text-zinc-900">{match}</mark>
      {after}
    </>
  )
}

export function CustomersTab() {
  const queryClient = useQueryClient()

  // Estados de busca em tempo real
  const [searchTerm, setSearchTerm] = useState('')
  const deferredSearchTerm = useDeferredValue(searchTerm)
  const [includeInactive, setIncludeInactive] = useState(false)

  // Estados de formulário e modal
  const [isFormOpen, setIsFormOpen] = useState(false)
  const [editingCustomer, setEditingCustomer] = useState<CustomerResponse | null>(null)
  const [feedbackMessage, setFeedbackMessage] = useState<string | null>(null)

  // Estado do formulário
  const [formData, setFormData] = useState<CreateCustomerRequest>({
    name: '',
    phone: '',
    email: '',
    document: '',
    notes: '',
  })

  // Consulta de clientes com busca instantânea conforme digitação
  const {
    data: customers = [],
    isLoading,
    isFetching,
    isError,
    error,
  } = useQuery({
    queryKey: ['customers', { searchTerm: deferredSearchTerm, includeInactive }],
    queryFn: () => api.getCustomers({ searchTerm: deferredSearchTerm, includeInactive }),
  })

  // Mutation: Criar cliente
  const createMutation = useMutation({
    mutationFn: (data: CreateCustomerRequest) => api.createCustomer(data),
    onSuccess: (created) => {
      queryClient.invalidateQueries({ queryKey: ['customers'] })
      setIsFormOpen(false)
      resetForm()
      setFeedbackMessage(`Cliente "${created.name}" cadastrado com sucesso!`)
    },
    onError: (err: Error) => {
      setFeedbackMessage(`Erro ao cadastrar: ${err.message}`)
    },
  })

  // Mutation: Atualizar cliente
  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateCustomerRequest }) =>
      api.updateCustomer(id, data),
    onSuccess: (updated) => {
      queryClient.invalidateQueries({ queryKey: ['customers'] })
      setIsFormOpen(false)
      setEditingCustomer(null)
      resetForm()
      setFeedbackMessage(`Cliente "${updated.name}" atualizado com sucesso!`)
    },
    onError: (err: Error) => {
      setFeedbackMessage(`Erro ao atualizar: ${err.message}`)
    },
  })

  // Mutation: Desativar cliente (Soft delete)
  const deleteMutation = useMutation({
    mutationFn: (id: string) => api.deleteCustomer(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['customers'] })
      setFeedbackMessage('Cliente desativado com sucesso.')
    },
    onError: (err: Error) => {
      setFeedbackMessage(`Erro ao desativar: ${err.message}`)
    },
  })

  const resetForm = () => {
    setFormData({
      name: '',
      phone: '',
      email: '',
      document: '',
      notes: '',
    })
    setEditingCustomer(null)
  }

  const handleOpenCreateWithPrefill = () => {
    resetForm()
    const cleanDigits = searchTerm.replace(/\D/g, '')
    // Se o termo digitado na busca for majoritariamente números, preenche no telefone
    if (cleanDigits.length >= 3) {
      setFormData((prev) => ({ ...prev, phone: cleanDigits }))
    } else if (searchTerm.trim().length > 0) {
      setFormData((prev) => ({ ...prev, name: searchTerm.trim() }))
    }
    setIsFormOpen(true)
  }

  const handleStartEdit = (customer: CustomerResponse) => {
    setEditingCustomer(customer)
    setFormData({
      name: customer.name ?? '',
      phone: customer.phone ?? '',
      email: customer.email ?? '',
      document: customer.document ?? '',
      notes: customer.notes ?? '',
    })
    setIsFormOpen(true)
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  const handleCancelForm = () => {
    setIsFormOpen(false)
    resetForm()
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()

    const trimmedName = formData.name?.trim() ?? ''
    if (!trimmedName) {
      setFeedbackMessage('O nome do cliente é obrigatório.')
      return
    }

    const payload: CreateCustomerRequest = {
      name: trimmedName,
      phone: formData.phone?.trim() || null,
      email: formData.email?.trim() || null,
      document: formData.document?.trim() || null,
      notes: formData.notes?.trim() || null,
    }

    if (editingCustomer?.id) {
      updateMutation.mutate({ id: editingCustomer.id, data: payload })
    } else {
      createMutation.mutate(payload)
    }
  }

  return (
    <div className="space-y-6">
      {/* Cabeçalho da Aba */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h2 className="text-xl font-bold tracking-tight text-zinc-900">
            Cadastro e Gestão de Clientes
          </h2>
          <p className="text-xs text-zinc-500">
            Busca instantânea por nome ou telefone em tempo real para salão, balcão e delivery.
          </p>
        </div>

        <button
          type="button"
          onClick={handleOpenCreateWithPrefill}
          className="inline-flex items-center justify-center rounded-lg bg-zinc-900 px-4 py-2 text-xs font-semibold text-white shadow-sm hover:bg-zinc-800 transition-colors"
        >
          + Novo Cliente
        </button>
      </div>

      {/* Feedback Message */}
      {feedbackMessage && (
        <div className="flex items-center justify-between rounded-lg border border-zinc-200 bg-white p-3 text-xs text-zinc-800 shadow-sm">
          <span>{feedbackMessage}</span>
          <button
            type="button"
            onClick={() => setFeedbackMessage(null)}
            className="text-zinc-400 hover:text-zinc-600 font-bold"
          >
            ✕
          </button>
        </div>
      )}

      {/* Campo de Busca Unificada Instantânea */}
      <div className="rounded-xl border border-zinc-200 bg-white p-4 shadow-sm space-y-3">
        <div className="flex flex-col sm:flex-row items-stretch sm:items-center justify-between gap-3">
          <div className="relative flex-1">
            <span className="absolute inset-y-0 left-0 flex items-center pl-3 text-zinc-400 pointer-events-none text-sm">
              🔍
            </span>
            <input
              type="text"
              autoFocus
              placeholder="Digite o nome ou telefone... (ex: Carlos ou 1197968...)"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full rounded-lg border border-zinc-300 bg-zinc-50/50 py-2.5 pl-10 pr-10 text-sm text-zinc-900 placeholder:text-zinc-400 focus:border-zinc-900 focus:bg-white focus:outline-none shadow-inner"
            />
            {searchTerm && (
              <button
                type="button"
                onClick={() => setSearchTerm('')}
                className="absolute inset-y-0 right-0 flex items-center pr-3 text-xs text-zinc-400 hover:text-zinc-700"
                title="Limpar busca"
              >
                ✕
              </button>
            )}
          </div>

          <label className="inline-flex items-center gap-2 text-xs text-zinc-600 cursor-pointer select-none self-end sm:self-center">
            <input
              type="checkbox"
              checked={includeInactive}
              onChange={(e) => setIncludeInactive(e.target.checked)}
              className="rounded border-zinc-300 text-zinc-900 focus:ring-0"
            />
            <span>Mostrar inativos</span>
          </label>
        </div>

        {/* Status da busca ao digitar */}
        <div className="flex items-center justify-between text-[11px] text-zinc-500 pt-1">
          <div className="flex items-center gap-2">
            {searchTerm.trim() ? (
              <span>
                Filtrando por:{' '}
                <strong className="text-zinc-800 font-semibold bg-zinc-100 px-1.5 py-0.5 rounded">
                  {searchTerm}
                </strong>
              </span>
            ) : (
              <span>Exibindo todos os clientes cadastrados</span>
            )}
            {isFetching && (
              <span className="inline-flex items-center text-zinc-400 animate-pulse">
                • Atualizando...
              </span>
            )}
          </div>

          <span>
            {customers.length} {customers.length === 1 ? 'cliente encontrado' : 'clientes encontrados'}
          </span>
        </div>
      </div>

      {/* Formulário de Cadastro / Edição */}
      {isFormOpen && (
        <div className="rounded-xl border border-zinc-300 bg-white p-5 shadow-sm space-y-4">
          <div className="flex items-center justify-between border-b border-zinc-200 pb-3">
            <div>
              <h3 className="text-sm font-bold text-zinc-900">
                {editingCustomer ? 'Editar Cliente' : 'Cadastrar Novo Cliente'}
              </h3>
              <p className="text-[11px] text-zinc-500">
                Preencha os dados do cliente. O nome é obrigatório para registro.
              </p>
            </div>
            <button
              type="button"
              onClick={handleCancelForm}
              className="text-xs text-zinc-500 hover:text-zinc-800"
            >
              Cancelar
            </button>
          </div>

          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
              {/* Nome */}
              <div className="sm:col-span-2">
                <label className="block text-xs font-medium text-zinc-700">
                  Nome Completo <span className="text-rose-500">*</span>
                </label>
                <input
                  type="text"
                  required
                  placeholder="Ex: Carlos Eduardo dos Santos"
                  value={formData.name ?? ''}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  className="mt-1 w-full rounded-md border border-zinc-300 px-3 py-1.5 text-xs text-zinc-900 placeholder:text-zinc-400 focus:border-zinc-900 focus:outline-none"
                />
              </div>

              {/* Telefone */}
              <div>
                <label className="block text-xs font-medium text-zinc-700">
                  Telefone / WhatsApp (DDD + Número)
                </label>
                <input
                  type="text"
                  placeholder="Ex: 11987654321"
                  value={formData.phone ?? ''}
                  onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
                  className="mt-1 w-full rounded-md border border-zinc-300 px-3 py-1.5 text-xs text-zinc-900 placeholder:text-zinc-400 focus:border-zinc-900 focus:outline-none"
                />
              </div>

              {/* E-mail */}
              <div>
                <label className="block text-xs font-medium text-zinc-700">
                  E-mail
                </label>
                <input
                  type="email"
                  placeholder="Ex: cliente@email.com"
                  value={formData.email ?? ''}
                  onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                  className="mt-1 w-full rounded-md border border-zinc-300 px-3 py-1.5 text-xs text-zinc-900 placeholder:text-zinc-400 focus:border-zinc-900 focus:outline-none"
                />
              </div>

              {/* CPF / CNPJ */}
              <div>
                <label className="block text-xs font-medium text-zinc-700">
                  CPF / CNPJ (Opcional para Nota Fiscal)
                </label>
                <input
                  type="text"
                  placeholder="Ex: 123.456.789-00"
                  value={formData.document ?? ''}
                  onChange={(e) => setFormData({ ...formData, document: e.target.value })}
                  className="mt-1 w-full rounded-md border border-zinc-300 px-3 py-1.5 text-xs text-zinc-900 placeholder:text-zinc-400 focus:border-zinc-900 focus:outline-none"
                />
              </div>

              {/* Observações */}
              <div className="sm:col-span-2">
                <label className="block text-xs font-medium text-zinc-700">
                  Observações e Preferências
                </label>
                <textarea
                  rows={2}
                  placeholder="Ex: Prefere massa fina bem assada, não colocar cebola nas pizzas..."
                  value={formData.notes ?? ''}
                  onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
                  className="mt-1 w-full rounded-md border border-zinc-300 px-3 py-1.5 text-xs text-zinc-900 placeholder:text-zinc-400 focus:border-zinc-900 focus:outline-none"
                />
              </div>
            </div>

            <div className="flex justify-end gap-2 pt-2 border-t border-zinc-100">
              <button
                type="button"
                onClick={handleCancelForm}
                className="rounded-md border border-zinc-300 px-3 py-1.5 text-xs font-medium text-zinc-600 hover:bg-zinc-50"
              >
                Cancelar
              </button>
              <button
                type="submit"
                disabled={createMutation.isPending || updateMutation.isPending}
                className="rounded-md bg-zinc-900 px-4 py-1.5 text-xs font-medium text-white hover:bg-zinc-800 disabled:opacity-50"
              >
                {createMutation.isPending || updateMutation.isPending
                  ? 'Salvando...'
                  : editingCustomer
                    ? 'Salvar Alterações'
                    : 'Cadastrar Cliente'}
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Resultados da Listagem de Clientes */}
      {isLoading ? (
        <div className="rounded-xl border border-zinc-200 bg-white p-12 text-center text-xs text-zinc-500">
          Carregando registros de clientes...
        </div>
      ) : isError ? (
        <div className="rounded-xl border border-rose-200 bg-rose-50 p-6 text-center text-xs text-rose-700">
          Erro ao carregar clientes: {error instanceof Error ? error.message : 'Erro desconhecido'}
        </div>
      ) : customers.length === 0 ? (
        <div className="rounded-xl border border-dashed border-zinc-300 bg-white p-12 text-center space-y-3">
          <p className="text-sm font-semibold text-zinc-900">
            Nenhum cliente encontrado
          </p>
          <p className="text-xs text-zinc-500 max-w-sm mx-auto">
            {searchTerm.trim()
              ? `Nenhum cadastro bate com o termo "${searchTerm}". Deseja cadastrar agora?`
              : 'Nenhum cliente cadastrado no sistema.'}
          </p>
          {searchTerm.trim() && (
            <button
              type="button"
              onClick={handleOpenCreateWithPrefill}
              className="inline-flex items-center rounded-md bg-zinc-900 px-3.5 py-1.5 text-xs font-semibold text-white shadow-sm hover:bg-zinc-800"
            >
              + Cadastrar com este dado
            </button>
          )}
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
          {customers.map((customer) => (
            <div
              key={customer.id}
              className={`flex flex-col justify-between rounded-xl border p-4 shadow-sm transition-all ${
                customer.isActive
                  ? 'border-zinc-200 bg-white hover:border-zinc-400'
                  : 'border-zinc-200 bg-zinc-50 opacity-75'
              }`}
            >
              <div className="space-y-2">
                <div className="flex items-start justify-between gap-2">
                  <div>
                    <h4 className="font-semibold text-zinc-900 text-sm">
                      <HighlightMatch text={customer.name ?? ''} term={searchTerm} />
                    </h4>
                    <p className="font-mono text-xs text-zinc-700 font-medium mt-0.5">
                      {customer.phone ? (
                        <span>
                          📞 <HighlightMatch text={customer.phone} term={searchTerm} />
                        </span>
                      ) : (
                        <span className="text-zinc-400 text-[11px]">Sem telefone</span>
                      )}
                    </p>
                  </div>
                  <span
                    className={`rounded-full px-2 py-0.5 text-[10px] font-semibold ${
                      customer.isActive
                        ? 'bg-emerald-100 text-emerald-800'
                        : 'bg-zinc-200 text-zinc-700'
                    }`}
                  >
                    {customer.isActive ? 'Ativo' : 'Inativo'}
                  </span>
                </div>

                <div className="space-y-1 text-[11px] text-zinc-500 pt-1">
                  {customer.email && (
                    <p className="truncate">
                      ✉️ <span className="text-zinc-700">{customer.email}</span>
                    </p>
                  )}
                  {customer.document && (
                    <p>
                      📄 CPF/CNPJ: <span className="text-zinc-700">{customer.document}</span>
                    </p>
                  )}
                  {customer.notes && (
                    <div className="mt-2 rounded bg-zinc-50 p-2 text-zinc-600 border border-zinc-100">
                      <span className="font-medium text-zinc-700">Obs:</span> {customer.notes}
                    </div>
                  )}
                </div>
              </div>

              {/* Ações do Card */}
              <div className="mt-4 flex items-center justify-between border-t border-zinc-100 pt-3 text-[11px]">
                <span className="text-zinc-400">
                  {customer.createdAt
                    ? new Date(customer.createdAt).toLocaleDateString('pt-BR')
                    : ''}
                </span>

                <div className="flex items-center gap-2">
                  <button
                    type="button"
                    onClick={() => handleStartEdit(customer)}
                    className="font-medium text-zinc-700 hover:text-zinc-900 hover:underline"
                  >
                    Editar
                  </button>
                  {customer.isActive && (
                    <button
                      type="button"
                      onClick={() => {
                        if (
                          customer.id &&
                          window.confirm(`Deseja desativar o cliente "${customer.name}"?`)
                        ) {
                          deleteMutation.mutate(customer.id)
                        }
                      }}
                      className="font-medium text-rose-600 hover:text-rose-800 hover:underline"
                    >
                      Desativar
                    </button>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
