import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { api, type CategoryResponse, type CreateCategoryRequest } from '../services/api'

export function CategoriesTab() {
  const queryClient = useQueryClient()
  const [isCreating, setIsCreating] = useState(false)
  const [editingCategory, setEditingCategory] = useState<CategoryResponse | null>(null)
  const [feedbackMessage, setFeedbackMessage] = useState<string | null>(null)

  // Formulário State
  const [formData, setFormData] = useState<CreateCategoryRequest>({
    name: '',
    description: '',
    displayOrder: 0,
  })

  // Consulta de Categorias
  const { data: categories = [], isLoading, error } = useQuery({
    queryKey: ['categories'],
    queryFn: api.getCategories,
  })

  // Mutation: Criar
  const createMutation = useMutation({
    mutationFn: api.createCategory,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] })
      setIsCreating(false)
      setFormData({ name: '', description: '', displayOrder: 0 })
      setFeedbackMessage('Categoria cadastrada com sucesso.')
    },
    onError: (err: Error) => {
      setFeedbackMessage(`Erro ao cadastrar: ${err.message}`)
    },
  })

  // Mutation: Atualizar
  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: CreateCategoryRequest }) =>
      api.updateCategory(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] })
      setEditingCategory(null)
      setFeedbackMessage('Categoria atualizada com sucesso.')
    },
    onError: (err: Error) => {
      setFeedbackMessage(`Erro ao atualizar: ${err.message}`)
    },
  })

  // Mutation: Deletar
  const deleteMutation = useMutation({
    mutationFn: api.deleteCategory,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] })
      setFeedbackMessage('Categoria excluída com sucesso.')
    },
    onError: (err: Error) => {
      setFeedbackMessage(`Erro ao excluir: ${err.message}`)
    },
  })

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (!formData.name?.trim()) return

    if (editingCategory?.id) {
      updateMutation.mutate({ id: editingCategory.id, data: formData })
    } else {
      createMutation.mutate(formData)
    }
  }

  const startEdit = (cat: CategoryResponse) => {
    setEditingCategory(cat)
    setFormData({
      name: cat.name || '',
      description: cat.description || '',
      displayOrder: cat.displayOrder ?? 0,
    })
    setIsCreating(true)
  }

  const cancelForm = () => {
    setIsCreating(false)
    setEditingCategory(null)
    setFormData({ name: '', description: '', displayOrder: 0 })
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h2 className="text-lg font-semibold text-zinc-900">
            Categorias do Cardápio
          </h2>
          <p className="text-xs text-zinc-500">
            Gerencie o agrupamento de itens do cardápio (ex: Pizzas, Bebidas, Sobremesas)
          </p>
        </div>

        {!isCreating && (
          <button
            type="button"
            onClick={() => setIsCreating(true)}
            className="rounded bg-zinc-900 px-3 py-1.5 text-xs font-semibold text-white shadow-sm transition hover:bg-zinc-800"
          >
            + Nova Categoria
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

      {/* Formulário de Criação / Edição */}
      {isCreating && (
        <form
          onSubmit={handleSubmit}
          className="rounded-lg border border-zinc-300 bg-white p-5 shadow-sm space-y-4"
        >
          <div className="border-b border-zinc-200 pb-2">
            <h3 className="text-sm font-semibold text-zinc-900">
              {editingCategory ? 'Editar Categoria' : 'Cadastrar Nova Categoria'}
            </h3>
          </div>

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
            <div>
              <label htmlFor="category-name" className="block text-xs font-medium text-zinc-700">
                Nome *
              </label>
              <input
                id="category-name"
                type="text"
                required
                value={formData.name || ''}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                placeholder="Ex: Pizzas Tradicionais"
                className="mt-1 w-full rounded border border-zinc-300 px-3 py-1.5 text-xs text-zinc-900 focus:border-zinc-800 focus:outline-none"
              />
            </div>

            <div>
              <label htmlFor="category-order" className="block text-xs font-medium text-zinc-700">
                Ordem de Exibição
              </label>
              <input
                id="category-order"
                type="number"
                value={formData.displayOrder ?? 0}
                onChange={(e) =>
                  setFormData({ ...formData, displayOrder: parseInt(e.target.value, 10) || 0 })
                }
                className="mt-1 w-full rounded border border-zinc-300 px-3 py-1.5 text-xs text-zinc-900 focus:border-zinc-800 focus:outline-none"
              />
            </div>

            <div className="sm:col-span-3">
              <label htmlFor="category-desc" className="block text-xs font-medium text-zinc-700">
                Descrição
              </label>
              <input
                id="category-desc"
                type="text"
                value={formData.description || ''}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                placeholder="Ex: Pizzas com massa tradicional fermentada 48h"
                className="mt-1 w-full rounded border border-zinc-300 px-3 py-1.5 text-xs text-zinc-900 focus:border-zinc-800 focus:outline-none"
              />
            </div>
          </div>

          <div className="flex justify-end gap-2 pt-2">
            <button
              type="button"
              onClick={cancelForm}
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

      {/* Tabela de Categorias */}
      <div className="overflow-hidden rounded-lg border border-zinc-300 bg-white shadow-sm">
        <div className="border-b border-zinc-200 bg-zinc-50 px-4 py-3">
          <span className="text-xs font-semibold uppercase tracking-wider text-zinc-600">
            Total de Categorias: {categories.length}
          </span>
        </div>

        {isLoading ? (
          <div className="p-8 text-center text-xs text-zinc-500">
            Carregando categorias...
          </div>
        ) : error ? (
          <div className="p-8 text-center text-xs text-zinc-700">
            Erro ao carregar categorias: {(error as Error).message}
          </div>
        ) : categories.length === 0 ? (
          <div className="p-8 text-center text-xs text-zinc-500">
            Nenhuma categoria cadastrada ainda. Clique em "+ Nova Categoria" acima para criar a primeira.
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-xs text-zinc-800">
              <thead className="border-b border-zinc-200 bg-zinc-50 font-semibold text-zinc-700">
                <tr>
                  <th className="px-4 py-3">Ordem</th>
                  <th className="px-4 py-3">Nome</th>
                  <th className="px-4 py-3">Descrição</th>
                  <th className="px-4 py-3">Status</th>
                  <th className="px-4 py-3 text-right">Ações</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-zinc-200">
                {categories.map((cat) => (
                  <tr key={cat.id} className="hover:bg-zinc-50">
                    <td className="px-4 py-3 font-mono text-zinc-500">
                      {cat.displayOrder ?? 0}
                    </td>
                    <td className="px-4 py-3 font-medium text-zinc-900">
                      {cat.name}
                    </td>
                    <td className="px-4 py-3 text-zinc-500">
                      {cat.description || '-'}
                    </td>
                    <td className="px-4 py-3">
                      <span className="inline-flex rounded bg-zinc-100 px-2 py-0.5 text-[11px] font-medium text-zinc-800 border border-zinc-200">
                        {cat.isActive ? 'Ativa' : 'Inativa'}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-right space-x-2">
                      <button
                        type="button"
                        onClick={() => startEdit(cat)}
                        className="rounded border border-zinc-300 bg-white px-2 py-1 text-[11px] font-medium text-zinc-700 hover:bg-zinc-100"
                      >
                        Editar
                      </button>
                      <button
                        type="button"
                        onClick={() => {
                          if (cat.id && confirm(`Deseja realmente excluir a categoria "${cat.name}"?`)) {
                            deleteMutation.mutate(cat.id)
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
