import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useRef, useState } from 'react'
import {
  api,
  fetchAddressByCep,
  type CreateCustomerAddressRequest,
  type CustomerAddressResponse,
  type CustomerResponse,
} from '../services/api'

interface CustomerAddressesModalProps {
  customer: CustomerResponse | null
  isOpen: boolean
  onClose: () => void
}

export function CustomerAddressesModal({
  customer,
  isOpen,
  onClose,
}: CustomerAddressesModalProps) {
  const queryClient = useQueryClient()
  const numberInputRef = useRef<HTMLInputElement>(null)

  // Estados de controle do formulário
  const [isFormOpen, setIsFormOpen] = useState(false)
  const [editingAddress, setEditingAddress] = useState<CustomerAddressResponse | null>(null)
  const [feedbackMessage, setFeedbackMessage] = useState<string | null>(null)
  const [cepLoading, setCepLoading] = useState(false)
  const [cepFeedback, setCepFeedback] = useState<string | null>(null)

  // Estado do formulário de endereço
  const [formData, setFormData] = useState<CreateCustomerAddressRequest>({
    street: '',
    number: '',
    neighborhood: '',
    zipCode: '',
    complement: '',
    referencePoint: '',
    isDefault: false,
  })

  // Consulta de endereços do cliente via TanStack Query
  const customerId = customer?.id
  const {
    data: addresses = [],
    isLoading,
    isError,
    error,
  } = useQuery({
    queryKey: ['customer-addresses', customerId],
    queryFn: () => (customerId ? api.getCustomerAddresses(customerId) : Promise.resolve([])),
    enabled: isOpen && !!customerId,
  })

  const resetForm = () => {
    setFormData({
      street: '',
      number: '',
      neighborhood: '',
      zipCode: '',
      complement: '',
      referencePoint: '',
      isDefault: false,
    })
    setEditingAddress(null)
    setCepFeedback(null)
    setCepLoading(false)
  }

  // Mutation: Criar endereço
  const createMutation = useMutation({
    mutationFn: (data: CreateCustomerAddressRequest) => {
      if (!customerId) throw new Error('Cliente inválido.')
      return api.createCustomerAddress(customerId, data)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['customer-addresses', customerId] })
      setIsFormOpen(false)
      resetForm()
      setFeedbackMessage('Endereço adicionado com sucesso!')
    },
    onError: (err: Error) => {
      setFeedbackMessage(`Erro ao cadastrar endereço: ${err.message}`)
    },
  })

  // Mutation: Atualizar endereço
  const updateMutation = useMutation({
    mutationFn: ({ addressId, data }: { addressId: string; data: CreateCustomerAddressRequest }) => {
      if (!customerId) throw new Error('Cliente inválido.')
      return api.updateCustomerAddress(customerId, addressId, data)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['customer-addresses', customerId] })
      setIsFormOpen(false)
      resetForm()
      setFeedbackMessage('Endereço atualizado com sucesso!')
    },
    onError: (err: Error) => {
      setFeedbackMessage(`Erro ao atualizar endereço: ${err.message}`)
    },
  })

  // Mutation: Definir endereço padrão
  const setDefaultMutation = useMutation({
    mutationFn: (addressId: string) => {
      if (!customerId) throw new Error('Cliente inválido.')
      return api.setDefaultCustomerAddress(customerId, addressId)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['customer-addresses', customerId] })
      setFeedbackMessage('Endereço padrão atualizado!')
    },
    onError: (err: Error) => {
      setFeedbackMessage(`Erro ao definir padrão: ${err.message}`)
    },
  })

  // Mutation: Excluir endereço
  const deleteMutation = useMutation({
    mutationFn: (addressId: string) => {
      if (!customerId) throw new Error('Cliente inválido.')
      return api.deleteCustomerAddress(customerId, addressId)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['customer-addresses', customerId] })
      setFeedbackMessage('Endereço removido com sucesso.')
    },
    onError: (err: Error) => {
      setFeedbackMessage(`Erro ao remover endereço: ${err.message}`)
    },
  })

  if (!isOpen || !customer) return null

  // Manipulador de digitação do CEP com autopreenchimento instantâneo pelo ViaCEP
  const handleCepChange = async (rawCep: string) => {
    // Formata o CEP com máscara 00000-000
    const digitsOnly = rawCep.replace(/\D/g, '').slice(0, 8)
    const formatted =
      digitsOnly.length > 5 ? `${digitsOnly.slice(0, 5)}-${digitsOnly.slice(5)}` : digitsOnly

    setFormData((prev) => ({ ...prev, zipCode: formatted }))

    // Quando o usuário completa 8 dígitos, consulta o ViaCEP
    if (digitsOnly.length === 8) {
      setCepLoading(true)
      setCepFeedback('Buscando CEP no ViaCEP...')

      const result = await fetchAddressByCep(digitsOnly)
      setCepLoading(false)

      if (result) {
        setFormData((prev) => ({
          ...prev,
          street: result.street || prev.street,
          neighborhood: result.neighborhood || prev.neighborhood,
        }))
        setCepFeedback('✓ Endereço localizado! Preencha o número.')
        // Posiciona o foco no campo número automaticamente para o operador
        setTimeout(() => numberInputRef.current?.focus(), 80)
      } else {
        setCepFeedback('⚠️ CEP não encontrado. Preencha a rua e bairro manualmente.')
      }
    } else {
      setCepFeedback(null)
    }
  }

  const handleStartEdit = (address: CustomerAddressResponse) => {
    setEditingAddress(address)
    setFormData({
      street: address.street ?? '',
      number: address.number ?? '',
      neighborhood: address.neighborhood ?? '',
      zipCode: address.zipCode ?? '',
      complement: address.complement ?? '',
      referencePoint: address.referencePoint ?? '',
      isDefault: address.isDefault ?? false,
    })
    setIsFormOpen(true)
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()

    const trimmedStreet = formData.street?.trim() ?? ''
    const trimmedNumber = formData.number?.trim() ?? ''
    const trimmedNeighborhood = formData.neighborhood?.trim() ?? ''

    if (!trimmedStreet || !trimmedNumber || !trimmedNeighborhood) {
      setFeedbackMessage('Rua, Número e Bairro são campos obrigatórios.')
      return
    }

    const payload: CreateCustomerAddressRequest = {
      street: trimmedStreet,
      number: trimmedNumber,
      neighborhood: trimmedNeighborhood,
      zipCode: formData.zipCode?.trim() || null,
      complement: formData.complement?.trim() || null,
      referencePoint: formData.referencePoint?.trim() || null,
      isDefault: formData.isDefault ?? false,
    }

    if (editingAddress?.id) {
      updateMutation.mutate({ addressId: editingAddress.id, data: payload })
    } else {
      createMutation.mutate(payload)
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4 backdrop-blur-xs">
      <div className="relative flex max-h-[90vh] w-full max-w-2xl flex-col rounded-2xl bg-white shadow-2xl overflow-hidden border border-zinc-200 animate-in fade-in zoom-in-95 duration-150">
        {/* Cabeçalho do Modal */}
        <div className="flex items-center justify-between border-b border-zinc-200 px-6 py-4 bg-zinc-50/80">
          <div>
            <div className="flex items-center gap-2">
              <span className="text-lg">📍</span>
              <h3 className="text-base font-bold text-zinc-900">
                Endereços de Entrega
              </h3>
            </div>
            <p className="text-xs text-zinc-500 mt-0.5">
              Cliente: <span className="font-semibold text-zinc-800">{customer.name}</span>
              {customer.phone && ` • ${customer.phone}`}
            </p>
          </div>

          <button
            type="button"
            onClick={onClose}
            className="flex h-8 w-8 items-center justify-center rounded-lg text-zinc-400 hover:bg-zinc-200 hover:text-zinc-700 transition-colors"
            title="Fechar"
          >
            ✕
          </button>
        </div>

        {/* Corpo do Modal */}
        <div className="flex-1 overflow-y-auto p-6 space-y-4">
          {/* Feedback de sucesso ou erro */}
          {feedbackMessage && (
            <div className="flex items-center justify-between rounded-lg border border-zinc-200 bg-zinc-50 p-3 text-xs text-zinc-800">
              <span>{feedbackMessage}</span>
              <button
                type="button"
                onClick={() => setFeedbackMessage(null)}
                className="font-bold text-zinc-400 hover:text-zinc-600"
              >
                ✕
              </button>
            </div>
          )}

          {/* Botão de Abertura do Formulário */}
          {!isFormOpen && (
            <div className="flex items-center justify-between">
              <span className="text-xs font-semibold text-zinc-700">
                Endereços cadastrados ({addresses.length})
              </span>
              <button
                type="button"
                onClick={() => {
                  resetForm()
                  setIsFormOpen(true)
                }}
                className="inline-flex items-center gap-1 rounded-lg bg-zinc-900 px-3 py-1.5 text-xs font-semibold text-white shadow-sm hover:bg-zinc-800 transition-colors"
              >
                + Novo Endereço
              </button>
            </div>
          )}

          {/* Formulário de Cadastro / Edição de Endereço */}
          {isFormOpen && (
            <div className="rounded-xl border border-zinc-300 bg-zinc-50/70 p-4 shadow-inner space-y-3">
              <div className="flex items-center justify-between border-b border-zinc-200 pb-2">
                <h4 className="text-xs font-bold text-zinc-900 uppercase tracking-wide">
                  {editingAddress ? 'Editar Endereço' : 'Cadastrar Novo Endereço'}
                </h4>
                <button
                  type="button"
                  onClick={() => {
                    setIsFormOpen(false)
                    resetForm()
                  }}
                  className="text-xs text-zinc-500 hover:text-zinc-800"
                >
                  Cancelar
                </button>
              </div>

              <form onSubmit={handleSubmit} className="space-y-3">
                {/* Linha 1: CEP com Busca Automática */}
                <div>
                  <div className="flex items-center justify-between">
                    <label className="block text-[11px] font-semibold text-zinc-700">
                      CEP (busca automática)
                    </label>
                    {cepLoading && (
                      <span className="text-[10px] font-medium text-amber-700 animate-pulse">
                        Buscando ViaCEP...
                      </span>
                    )}
                  </div>
                  <div className="relative mt-1">
                    <input
                      type="text"
                      placeholder="00000-000"
                      value={formData.zipCode ?? ''}
                      onChange={(e) => handleCepChange(e.target.value)}
                      maxLength={9}
                      className="w-full rounded-md border border-zinc-300 bg-white px-3 py-1.5 text-xs text-zinc-900 placeholder:text-zinc-400 focus:border-zinc-900 focus:outline-none"
                    />
                    <span className="absolute inset-y-0 right-0 flex items-center pr-3 text-xs text-zinc-400">
                      🔍
                    </span>
                  </div>
                  {cepFeedback && (
                    <p
                      className={`text-[10px] mt-1 font-medium ${
                        cepFeedback.startsWith('✓')
                          ? 'text-emerald-700'
                          : cepFeedback.startsWith('⚠️')
                            ? 'text-amber-700'
                            : 'text-zinc-500'
                      }`}
                    >
                      {cepFeedback}
                    </p>
                  )}
                </div>

                {/* Linha 2: Rua / Logradouro e Número */}
                <div className="grid grid-cols-1 gap-3 sm:grid-cols-4">
                  <div className="sm:col-span-3">
                    <label className="block text-[11px] font-semibold text-zinc-700">
                      Rua / Logradouro *
                    </label>
                    <input
                      type="text"
                      required
                      placeholder="Ex: Av. Paulista ou Rua das Flores"
                      value={formData.street ?? ''}
                      onChange={(e) =>
                        setFormData((prev) => ({ ...prev, street: e.target.value }))
                      }
                      className="mt-1 w-full rounded-md border border-zinc-300 bg-white px-3 py-1.5 text-xs text-zinc-900 placeholder:text-zinc-400 focus:border-zinc-900 focus:outline-none"
                    />
                  </div>
                  <div className="sm:col-span-1">
                    <label className="block text-[11px] font-semibold text-zinc-700">
                      Número *
                    </label>
                    <input
                      ref={numberInputRef}
                      type="text"
                      required
                      placeholder="Ex: 120"
                      value={formData.number ?? ''}
                      onChange={(e) =>
                        setFormData((prev) => ({ ...prev, number: e.target.value }))
                      }
                      className="mt-1 w-full rounded-md border border-zinc-300 bg-white px-3 py-1.5 text-xs text-zinc-900 placeholder:text-zinc-400 focus:border-zinc-900 focus:outline-none"
                    />
                  </div>
                </div>

                {/* Linha 3: Bairro e Complemento */}
                <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                  <div>
                    <label className="block text-[11px] font-semibold text-zinc-700">
                      Bairro *
                    </label>
                    <input
                      type="text"
                      required
                      placeholder="Ex: Bela Vista"
                      value={formData.neighborhood ?? ''}
                      onChange={(e) =>
                        setFormData((prev) => ({ ...prev, neighborhood: e.target.value }))
                      }
                      className="mt-1 w-full rounded-md border border-zinc-300 bg-white px-3 py-1.5 text-xs text-zinc-900 placeholder:text-zinc-400 focus:border-zinc-900 focus:outline-none"
                    />
                  </div>
                  <div>
                    <label className="block text-[11px] font-semibold text-zinc-700">
                      Complemento (opcional)
                    </label>
                    <input
                      type="text"
                      placeholder="Ex: Apto 42, Bloco C"
                      value={formData.complement ?? ''}
                      onChange={(e) =>
                        setFormData((prev) => ({ ...prev, complement: e.target.value }))
                      }
                      className="mt-1 w-full rounded-md border border-zinc-300 bg-white px-3 py-1.5 text-xs text-zinc-900 placeholder:text-zinc-400 focus:border-zinc-900 focus:outline-none"
                    />
                  </div>
                </div>

                {/* Linha 4: Ponto de Referência */}
                <div>
                  <label className="block text-[11px] font-semibold text-zinc-700">
                    Ponto de Referência (opcional)
                  </label>
                  <input
                    type="text"
                    placeholder="Ex: Em frente à farmácia ou portão verde"
                    value={formData.referencePoint ?? ''}
                    onChange={(e) =>
                      setFormData((prev) => ({ ...prev, referencePoint: e.target.value }))
                    }
                    className="mt-1 w-full rounded-md border border-zinc-300 bg-white px-3 py-1.5 text-xs text-zinc-900 placeholder:text-zinc-400 focus:border-zinc-900 focus:outline-none"
                  />
                </div>

                {/* Linha 5: Checkbox de Endereço Padrão */}
                <div className="flex items-center gap-2 pt-1">
                  <input
                    id="isDefaultCheckbox"
                    type="checkbox"
                    checked={formData.isDefault ?? false}
                    onChange={(e) =>
                      setFormData((prev) => ({ ...prev, isDefault: e.target.checked }))
                    }
                    className="h-4 w-4 rounded border-zinc-300 text-zinc-900 focus:ring-zinc-900"
                  />
                  <label
                    htmlFor="isDefaultCheckbox"
                    className="text-xs text-zinc-700 font-medium select-none cursor-pointer"
                  >
                    Definir como endereço padrão de entrega
                  </label>
                </div>

                {/* Botões do Formulário */}
                <div className="flex items-center justify-end gap-2 pt-2 border-t border-zinc-200">
                  <button
                    type="button"
                    onClick={() => {
                      setIsFormOpen(false)
                      resetForm()
                    }}
                    className="rounded-md border border-zinc-300 bg-white px-3 py-1.5 text-xs font-medium text-zinc-700 hover:bg-zinc-50"
                  >
                    Cancelar
                  </button>
                  <button
                    type="submit"
                    disabled={createMutation.isPending || updateMutation.isPending}
                    className="rounded-md bg-zinc-900 px-4 py-1.5 text-xs font-semibold text-white hover:bg-zinc-800 disabled:opacity-50 transition-colors"
                  >
                    {createMutation.isPending || updateMutation.isPending
                      ? 'Salvando...'
                      : editingAddress
                        ? 'Salvar Alterações'
                        : 'Cadastrar Endereço'}
                  </button>
                </div>
              </form>
            </div>
          )}

          {/* Listagem de Endereços */}
          {isLoading ? (
            <div className="p-8 text-center text-xs text-zinc-500">
              Carregando endereços...
            </div>
          ) : isError ? (
            <div className="rounded-lg border border-rose-200 bg-rose-50 p-4 text-center text-xs text-rose-700">
              Erro ao listar endereços: {error instanceof Error ? error.message : 'Erro desconhecido'}
            </div>
          ) : addresses.length === 0 ? (
            <div className="rounded-xl border border-dashed border-zinc-300 p-8 text-center space-y-2">
              <span className="text-2xl">📦</span>
              <p className="text-xs font-semibold text-zinc-800">
                Nenhum endereço cadastrado para este cliente
              </p>
              <p className="text-[11px] text-zinc-500 max-w-xs mx-auto">
                Adicione um endereço residencial ou de trabalho para agilizar os pedidos de delivery.
              </p>
              {!isFormOpen && (
                <button
                  type="button"
                  onClick={() => {
                    resetForm()
                    setIsFormOpen(true)
                  }}
                  className="inline-flex items-center rounded-md bg-zinc-900 px-3 py-1.5 text-xs font-semibold text-white shadow-sm hover:bg-zinc-800"
                >
                  + Adicionar Primeiro Endereço
                </button>
              )}
            </div>
          ) : (
            <div className="space-y-3">
              {addresses.map((address) => (
                <div
                  key={address.id}
                  className={`flex flex-col sm:flex-row sm:items-center justify-between gap-3 rounded-xl border p-3.5 transition-colors ${
                    address.isDefault
                      ? 'border-emerald-300 bg-emerald-50/40 shadow-xs'
                      : 'border-zinc-200 bg-white hover:border-zinc-300'
                  }`}
                >
                  {/* Informações de Localização */}
                  <div className="space-y-1 text-xs">
                    <div className="flex items-center gap-2 flex-wrap">
                      <span className="font-semibold text-zinc-900">
                        {address.street}, {address.number}
                      </span>
                      {address.isDefault ? (
                        <span className="inline-flex items-center gap-0.5 rounded-full bg-emerald-100 px-2 py-0.5 text-[10px] font-bold text-emerald-800">
                          ★ Principal
                        </span>
                      ) : (
                        <span className="inline-flex items-center rounded-full bg-zinc-100 px-2 py-0.5 text-[10px] font-medium text-zinc-600">
                          Secundário
                        </span>
                      )}
                    </div>

                    <p className="text-zinc-600 text-[11px]">
                      <span className="font-medium text-zinc-700">Bairro:</span> {address.neighborhood}
                      {address.zipCode && ` • CEP: ${address.zipCode}`}
                    </p>

                    {(address.complement || address.referencePoint) && (
                      <p className="text-zinc-500 text-[11px]">
                        {address.complement && <span>Comp: {address.complement}</span>}
                        {address.complement && address.referencePoint && ' • '}
                        {address.referencePoint && <span>Ref: {address.referencePoint}</span>}
                      </p>
                    )}
                  </div>

                  {/* Ações do Endereço */}
                  <div className="flex items-center gap-2 self-end sm:self-center border-t sm:border-t-0 pt-2 sm:pt-0 border-zinc-100">
                    {!address.isDefault && (
                      <button
                        type="button"
                        onClick={() => {
                          if (address.id) setDefaultMutation.mutate(address.id)
                        }}
                        disabled={setDefaultMutation.isPending}
                        className="rounded-md border border-zinc-200 bg-white px-2 py-1 text-[11px] font-medium text-zinc-700 hover:bg-zinc-50 hover:text-zinc-900 transition-colors shadow-2xs"
                        title="Definir este endereço como o principal para entregas"
                      >
                        ★ Tornar Padrão
                      </button>
                    )}

                    <button
                      type="button"
                      onClick={() => handleStartEdit(address)}
                      className="rounded-md border border-zinc-200 bg-white px-2 py-1 text-[11px] font-medium text-zinc-700 hover:bg-zinc-50 hover:text-zinc-900 transition-colors shadow-2xs"
                    >
                      Editar
                    </button>

                    <button
                      type="button"
                      onClick={() => {
                        if (
                          address.id &&
                          window.confirm(
                            `Deseja remover o endereço "${address.street}, ${address.number}"?`
                          )
                        ) {
                          deleteMutation.mutate(address.id)
                        }
                      }}
                      className="rounded-md border border-rose-100 bg-white px-2 py-1 text-[11px] font-medium text-rose-600 hover:bg-rose-50 hover:text-rose-700 transition-colors shadow-2xs"
                    >
                      Excluir
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Rodapé do Modal */}
        <div className="border-t border-zinc-200 px-6 py-3 bg-zinc-50 flex items-center justify-between text-xs text-zinc-500">
          <span>
            {addresses.length} {addresses.length === 1 ? 'endereço registrado' : 'endereços registrados'}
          </span>
          <button
            type="button"
            onClick={onClose}
            className="rounded-lg bg-zinc-200 px-4 py-1.5 text-xs font-semibold text-zinc-800 hover:bg-zinc-300 transition-colors"
          >
            Fechar
          </button>
        </div>
      </div>
    </div>
  )
}
