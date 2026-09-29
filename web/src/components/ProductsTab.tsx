import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { api, type CreateProductRequest, type ProductResponse } from '../services/api'

export function ProductsTab() {
  const queryClient = useQueryClient()
  const [selectedCategoryFilter, setSelectedCategoryFilter] = useState<string>('')
  const [isCreating, setIsCreating] = useState(false)
  const [editingProduct, setEditingProduct] = useState<ProductResponse | null>(null)
  const [feedbackMessage, setFeedbackMessage] = useState<string | null>(null)

  // Consulta de categorias para o filtro e para o select do form
  const { data: categories = [] } = useQuery({
    queryKey: ['categories'],
    queryFn: api.getCategories,
  })

  // Consulta de produtos (filtrada ou total)
  const {
    data: products = [],
    isLoading,
    error,
  } = useQuery({
    queryKey: ['products', selectedCategoryFilter],
    queryFn: () => api.getProducts(selectedCategoryFilter || undefined),
  })

  // Form State
  const [formData, setFormData] = useState<CreateProductRequest>({
    name: '',
    price: 0,
    categoryId: '',
    description: '',
  })

  // Mutation: Criar
  const createMutation = useMutation({
    mutationFn: api.createProduct,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] })
      setIsCreating(false)
      resetForm()
      setFeedbackMessage('Produto cadastrado com sucesso.')
    },
    onError: (err: Error) => {
      setFeedbackMessage(`Erro ao criar produto: ${err.message}`)
    },
  })

  // Mutation: Atualizar
  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: CreateProductRequest }) =>
      api.updateProduct(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] })
      setEditingProduct(null)
      resetForm()
      setFeedbackMessage('Produto atualizado com sucesso.')
    },
    onError: (err: Error) => {
      setFeedbackMessage(`Erro ao atualizar produto: ${err.message}`)
    },
  })

  // Mutation: Alternar disponibilidade
  const toggleAvailabilityMutation = useMutation({
    mutationFn: ({ id, isAvailable }: { id: string; isAvailable: boolean }) =>
      api.updateProductAvailability(id, { isAvailable }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] })
      setFeedbackMessage('Disponibilidade do produto atualizada.')
    },
    onError: (err: Error) => {
      setFeedbackMessage(`Erro ao atualizar disponibilidade: ${err.message}`)
    },
  })

  // Mutation: Deletar
  const deleteMutation = useMutation({
    mutationFn: api.deleteProduct,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] })
      setFeedbackMessage('Produto removido com sucesso.')
    },
    onError: (err: Error) => {
      setFeedbackMessage(`Erro ao excluir: ${err.message}`)
    },
  })

  const resetForm = () => {
    setIsCreating(false)
    setEditingProduct(null)
    setFormData({
      name: '',
      price: 0,
      categoryId: categories[0]?.id || '',
      description: '',
    })
  }

  const startCreate = () => {
    setIsCreating(true)
    setEditingProduct(null)
    setFormData({
      name: '',
      price: 0,
      categoryId: selectedCategoryFilter || categories[0]?.id || '',
      description: '',
    })
  }

  const startEdit = (prod: ProductResponse) => {
    setEditingProduct(prod)
    setFormData({
      name: prod.name || '',
      price: prod.price ?? 0,
      categoryId: prod.categoryId || '',
      description: prod.description || '',
    })
    setIsCreating(true)
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (!formData.name?.trim() || !formData.categoryId) return

    if (editingProduct?.id) {
      updateMutation.mutate({ id: editingProduct.id, data: formData })
    } else {
      createMutation.mutate(formData)
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h2 className="text-lg font-semibold text-zinc-900">
            Produtos do Cardápio
          </h2>
          <p className="text-xs text-zinc-500">
            Itens comercializados, seus preços e status operacional de estoque
          </p>
        </div>

        <div className="flex items-center gap-2">
          {/* Filtro por Categoria */}
          <select
            value={selectedCategoryFilter}
            onChange={(e) => setSelectedCategoryFilter(e.target.value)}
            className="rounded border border-zinc-300 bg-white px-3 py-1.5 text-xs text-zinc-800 focus:border-zinc-800 focus:outline-none"
          >
            <option value="">Todas as Categorias</option>
            {categories.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>

          {!isCreating && (
            <button
              type="button"
              onClick={startCreate}
              disabled={categories.length === 0}
              className="rounded bg-zinc-900 px-3 py-1.5 text-xs font-semibold text-white shadow-sm transition hover:bg-zinc-800 disabled:opacity-50"
            >
              + Novo Produto
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

      {categories.length === 0 && (
        <div className="rounded border border-dashed border-zinc-300 bg-zinc-50 p-4 text-center text-xs text-zinc-600">
          Você precisa cadastrar pelo menos uma <strong>Categoria</strong> antes de cadastrar produtos.
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
              {editingProduct ? 'Editar Produto' : 'Cadastrar Novo Produto'}
            </h3>
          </div>

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
            <div>
              <label htmlFor="product-name" className="block text-xs font-medium text-zinc-700">
                Nome do Produto *
              </label>
              <input
                id="product-name"
                type="text"
                required
                value={formData.name || ''}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                placeholder="Ex: Pizza Quatro Queijos"
                className="mt-1 w-full rounded border border-zinc-300 px-3 py-1.5 text-xs text-zinc-900 focus:border-zinc-800 focus:outline-none"
              />
            </div>

            <div>
              <label htmlFor="product-price" className="block text-xs font-medium text-zinc-700">
                Preço (R$) *
              </label>
              <input
                id="product-price"
                type="number"
                step="0.01"
                min="0"
                required
                value={formData.price ?? 0}
                onChange={(e) =>
                  setFormData({ ...formData, price: parseFloat(e.target.value) || 0 })
                }
                className="mt-1 w-full rounded border border-zinc-300 px-3 py-1.5 text-xs text-zinc-900 focus:border-zinc-800 focus:outline-none"
              />
            </div>

            <div>
              <label htmlFor="product-category" className="block text-xs font-medium text-zinc-700">
                Categoria *
              </label>
              <select
                id="product-category"
                required
                value={formData.categoryId || ''}
                onChange={(e) => setFormData({ ...formData, categoryId: e.target.value })}
                className="mt-1 w-full rounded border border-zinc-300 bg-white px-3 py-1.5 text-xs text-zinc-900 focus:border-zinc-800 focus:outline-none"
              >
                <option value="" disabled>Selecione uma categoria...</option>
                {categories.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.name}
                  </option>
                ))}
              </select>
            </div>

            <div className="sm:col-span-3">
              <label htmlFor="product-desc" className="block text-xs font-medium text-zinc-700">
                Descrição / Ingredientes
              </label>
              <input
                id="product-desc"
                type="text"
                value={formData.description || ''}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                placeholder="Ex: Mussarela, provolone, gorgonzola e parmesão fresco"
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

      {/* Tabela de Produtos */}
      <div className="overflow-hidden rounded-lg border border-zinc-300 bg-white shadow-sm">
        <div className="border-b border-zinc-200 bg-zinc-50 px-4 py-3">
          <span className="text-xs font-semibold uppercase tracking-wider text-zinc-600">
            Total de Produtos: {products.length}
          </span>
        </div>

        {isLoading ? (
          <div className="p-8 text-center text-xs text-zinc-500">
            Carregando produtos...
          </div>
        ) : error ? (
          <div className="p-8 text-center text-xs text-zinc-700">
            Erro ao carregar produtos: {(error as Error).message}
          </div>
        ) : products.length === 0 ? (
          <div className="p-8 text-center text-xs text-zinc-500">
            Nenhum produto cadastrado {selectedCategoryFilter ? 'para a categoria selecionada' : ''}.
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-xs text-zinc-800">
              <thead className="border-b border-zinc-200 bg-zinc-50 font-semibold text-zinc-700">
                <tr>
                  <th className="px-4 py-3">Produto</th>
                  <th className="px-4 py-3">Categoria</th>
                  <th className="px-4 py-3">Preço</th>
                  <th className="px-4 py-3">Disponibilidade</th>
                  <th className="px-4 py-3 text-right">Ações</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-zinc-200">
                {products.map((prod) => (
                  <tr key={prod.id} className="hover:bg-zinc-50">
                    <td className="px-4 py-3">
                      <div className="font-medium text-zinc-900">{prod.name}</div>
                      {prod.description && (
                        <div className="text-[11px] text-zinc-500 line-clamp-1">{prod.description}</div>
                      )}
                    </td>
                    <td className="px-4 py-3 text-zinc-600">
                      {prod.categoryName || '-'}
                    </td>
                    <td className="px-4 py-3 font-mono font-medium text-zinc-900">
                      {(prod.price ?? 0).toLocaleString('pt-BR', {
                        style: 'currency',
                        currency: 'BRL',
                      })}
                    </td>
                    <td className="px-4 py-3">
                      <button
                        type="button"
                        onClick={() => {
                          if (prod.id) {
                            toggleAvailabilityMutation.mutate({
                              id: prod.id,
                              isAvailable: !prod.isAvailable,
                            })
                          }
                        }}
                        title="Clique para alternar disponibilidade"
                        className={`inline-flex items-center gap-1.5 rounded border px-2 py-0.5 text-[11px] font-medium transition ${
                          prod.isAvailable
                            ? 'border-zinc-400 bg-zinc-100 text-zinc-900 hover:bg-zinc-200'
                            : 'border-zinc-300 bg-zinc-50 text-zinc-400 line-through hover:bg-zinc-100'
                        }`}
                      >
                        <span
                          className={`h-1.5 w-1.5 rounded-full ${
                            prod.isAvailable ? 'bg-zinc-800' : 'bg-zinc-300'
                          }`}
                        />
                        {prod.isAvailable ? 'Disponível' : 'Indisponível'}
                      </button>
                    </td>
                    <td className="px-4 py-3 text-right space-x-2">
                      <button
                        type="button"
                        onClick={() => startEdit(prod)}
                        className="rounded border border-zinc-300 bg-white px-2 py-1 text-[11px] font-medium text-zinc-700 hover:bg-zinc-100"
                      >
                        Editar
                      </button>
                      <button
                        type="button"
                        onClick={() => {
                          if (prod.id && confirm(`Deseja excluir o produto "${prod.name}"?`)) {
                            deleteMutation.mutate(prod.id)
                          }
                        }}
                        className="rounded border border-zinc-300 bg-zinc-100 px-2 py-1 text-[11px] font-medium text-zinc-700 hover:bg-zinc-200"
                      >
                        Excluir
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  )
}
