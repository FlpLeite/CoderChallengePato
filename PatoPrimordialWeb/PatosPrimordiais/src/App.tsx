import { NavLink, Outlet, Route, Routes } from 'react-router-dom'
import './App.css'
import { AnaliseDetalhePage } from './pages/AnaliseDetalhePage'
import { AnaliseRankingPage } from './pages/AnaliseRankingPage'
import { CatalogosPage } from './pages/CatalogosPage'
import { DashboardPage } from './pages/DashboardPage'
import { DronesPage } from './pages/DronesPage'
import { MissaoDetalhePage } from './pages/MissaoDetalhePage'
import { MissaoNovaPage } from './pages/MissaoNovaPage'
import { MissoesPage } from './pages/MissoesPage'
import { PatosPage } from './pages/PatosPage'
import { PontosPage } from './pages/PontosPage'

function App() {
  return (
    <Routes>
      <Route element={<AppLayout />}>
        <Route index element={<DashboardPage />} />
        <Route path="patos" element={<PatosPage />} />
        <Route path="drones" element={<DronesPage />} />
        <Route path="pontos" element={<PontosPage />} />
        <Route path="analise" element={<AnaliseRankingPage />} />
        <Route path="analise/:patoId" element={<AnaliseDetalhePage />} />
        <Route path="missoes" element={<MissoesPage />} />
        <Route path="missoes/nova" element={<MissaoNovaPage />} />
        <Route path="missoes/:id" element={<MissaoDetalhePage />} />
        <Route path="catalogos" element={<CatalogosPage />} />
      </Route>
    </Routes>
  )
}

function AppLayout() {
  const links = [
    { to: '/', label: 'Dashboard', end: true },
    { to: '/patos', label: 'Patos' },
    { to: '/drones', label: 'Drones' },
    { to: '/pontos', label: 'Pontos de referência' },
    { to: '/analise', label: 'Análise' },
    { to: '/missoes', label: 'Missões' },
    { to: '/catalogos', label: 'Catálogos' },
  ]

  return (
    <div className="app-shell bg-night-950">
      <header className="app-header border-b border-slate-800/80 bg-night-900/80 backdrop-blur">
        <div className="mx-auto flex max-w-7xl flex-col gap-4 px-4 py-6 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <h1 className="text-2xl font-semibold text-emerald-200">Catálogo de Patos Primordiais</h1>
            <p className="text-sm text-slate-400">Monitoramento em tempo real dos patos extraordinários.</p>
          </div>
          <nav className="flex flex-wrap gap-2">
            {links.map(({ to, label, end }) => (
              <NavLink
                key={to}
                to={to}
                end={end}
                className={({ isActive }) =>
                  `rounded-full px-4 py-2 text-sm font-medium transition ${
                    isActive
                      ? 'bg-emerald-500 text-night-900 shadow-lg shadow-emerald-500/20'
                      : 'bg-night-800/60 text-slate-300 hover:bg-night-800/90'
                  }`
                }
              >
                {label}
              </NavLink>
            ))}
          </nav>
        </div>
      </header>

      <main className="app-main mx-auto max-w-7xl space-y-6 px-4 py-8">
        <Outlet />
      </main>

      <footer className="app-footer border-t border-slate-800/80 bg-night-900/80 px-4 py-4 text-center text-xs text-slate-500">
        Dados normalizados para unidades SI. Última atualização: {new Date().toLocaleString('pt-BR')}
      </footer>
    </div>
  )
}

export default App
