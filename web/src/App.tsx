import { useState } from 'react'
import { CategoriesTab } from './components/CategoriesTab'
import { CustomersTab } from './components/CustomersTab'
import { Header, type TabType } from './components/Header'
import { ProductsTab } from './components/ProductsTab'
import { TablesTab } from './components/TablesTab'

export function App() {
  const [activeTab, setActiveTab] = useState<TabType>('categories')

  return (
    <div className="min-h-screen bg-zinc-100 text-zinc-900 flex flex-col font-sans">
      {/* Header com navegação e status da API */}
      <Header activeTab={activeTab} onTabChange={setActiveTab} />

      {/* Conteúdo Principal */}
      <main className="mx-auto w-full max-w-7xl flex-1 px-6 py-8">
        {activeTab === 'categories' && <CategoriesTab />}
        {activeTab === 'products' && <ProductsTab />}
        {activeTab === 'tables' && <TablesTab />}
        {activeTab === 'customers' && <CustomersTab />}
      </main>

      {/* Rodapé monocromático */}
      <footer className="border-t border-zinc-200 bg-white py-4 text-center text-xs text-zinc-500">
        <div className="mx-auto max-w-7xl px-6 flex flex-col sm:flex-row items-center justify-between gap-2">
          <span>Komari Operations • Console de Testes Funcionais</span>
          <span>Backend conectado em <code className="bg-zinc-100 px-1 py-0.5 rounded text-[11px]">http://localhost:5105</code></span>
        </div>
      </footer>
    </div>
  )
}

export default App
