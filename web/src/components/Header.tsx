import { useQuery } from '@tanstack/react-query'
import { api } from '../services/api'

export type TabType = 'categories' | 'products' | 'tables' | 'customers' | 'delivery'

interface HeaderProps {
  activeTab: TabType
  onTabChange: (tab: TabType) => void
}

export function Header({ activeTab, onTabChange }: HeaderProps) {
  const { data: health, isError, isLoading } = useQuery({
    queryKey: ['health'],
    queryFn: api.checkHealth,
    refetchInterval: 15000,
  })

  return (
    <header className="border-b border-zinc-300 bg-white">
      <div className="mx-auto flex max-w-7xl flex-col gap-4 px-6 py-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <div className="flex items-center gap-3">
            <h1 className="text-xl font-bold tracking-tight text-zinc-900">
              KOMARI
            </h1>
            <span className="rounded bg-zinc-200 px-2 py-0.5 text-xs font-semibold text-zinc-700">
              Console de Operações
            </span>
          </div>
          <p className="mt-1 text-xs text-zinc-500">
            Ambiente monocromático de teste funcional ponta a ponta
          </p>
        </div>

        {/* Indicador de Status da API */}
        <div className="flex items-center gap-2 self-start rounded-full border border-zinc-200 bg-zinc-50 px-3 py-1 text-xs sm:self-center">
          <span
            className={`h-2 w-2 rounded-full ${
              isLoading
                ? 'bg-zinc-400 animate-pulse'
                : isError
                  ? 'bg-zinc-800'
                  : 'bg-zinc-600'
            }`}
          />
          <span className="text-zinc-600 font-medium">
            API Status:
          </span>
          <span className="font-semibold text-zinc-900">
            {isLoading
              ? 'Verificando...'
              : isError
                ? 'Desconectada (localhost:5105)'
                : `${health?.status || 'Online'}`}
          </span>
        </div>
      </div>

      {/* Navegação por Abas */}
      <div className="mx-auto max-w-7xl px-6">
        <nav className="flex space-x-2 border-t border-zinc-100 pt-2">
          {(
            [
              { id: 'categories', label: '1. Categorias' },
              { id: 'products', label: '2. Produtos / Cardápio' },
              { id: 'tables', label: '3. Salão & Mesas' },
              { id: 'customers', label: '4. Clientes' },
              { id: 'delivery', label: '5. Pedidos de Delivery' },
            ] as const
          ).map((tab) => {
            const isActive = activeTab === tab.id
            return (
              <button
                key={tab.id}
                type="button"
                onClick={() => onTabChange(tab.id)}
                className={`border-b-2 px-4 py-2.5 text-sm font-medium transition-colors ${
                  isActive
                    ? 'border-zinc-900 text-zinc-900'
                    : 'border-transparent text-zinc-500 hover:border-zinc-300 hover:text-zinc-800'
                }`}
              >
                {tab.label}
              </button>
            )
          })}
        </nav>
      </div>
    </header>
  )
}
