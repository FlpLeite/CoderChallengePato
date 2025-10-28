import { useEffect, useState } from 'react'
import { useNavigate, type NavigateFunction } from 'react-router-dom'
import { fetchAnaliseRanking, recalcularAnalise } from '../api/client'
import type { AnaliseRankingFilters, AnaliseRankingItem, AnaliseRankingResult } from '../api/types'
import { ErrorNotice } from '../components/ErrorNotice'

const initialFilters: AnaliseRankingFilters = {
  ordem: 'prioridade',
  estado: '',
  risco: '',
  pais: '',
  page: 1,
  pageSize: 10,
}

const riscoLabels: Record<string, string> = {
  alto: 'Alto',
  medio: 'Médio',
  baixo: 'Baixo',
}

export function AnaliseRankingPage() {
  const [filters, setFilters] = useState<AnaliseRankingFilters>(initialFilters)
  const [dados, setDados] = useState<AnaliseRankingResult | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [recalculando, setRecalculando] = useState(false)
  const navigate = useNavigate()

  useEffect(() => {
    carregar()
  }, [filters])

  async function carregar() {
    try {
      setLoading(true)
      setError(null)
      const ranking = await fetchAnaliseRanking(filters)
      setDados(ranking)
    } catch (err) {
      setError((err as Error).message)
    } finally {
      setLoading(false)
    }
  }

  async function handleRecalcular() {
    try {
      setRecalculando(true)
      await recalcularAnalise()
      await carregar()
    } catch (err) {
      setError((err as Error).message)
    } finally {
      setRecalculando(false)
    }
  }

  function updateFilters(partial: Partial<AnaliseRankingFilters>) {
    setFilters((prev) => ({ ...prev, ...partial, page: 1 }))
  }

  function changePage(page: number) {
    setFilters((prev) => ({ ...prev, page }))
  }

  function renderResumo() {
    if (!dados) {
      return null
    }

    return (
      <div className="flex flex-wrap gap-3 text-xs text-slate-400">
        {dados.resumoPorRisco &&
          Object.entries(dados.resumoPorRisco).map(([classe, quantidade]) => (
            <span key={classe} className="rounded-full border border-slate-700/70 px-3 py-1 capitalize text-slate-200">
              {riscoLabels[classe] ?? classe}: <span className="font-semibold text-emerald-300">{quantidade}</span>
            </span>
          ))}
        {dados.resumoPorEstado &&
          Object.entries(dados.resumoPorEstado).map(([estado, quantidade]) => (
            <span key={estado} className="rounded-full border border-slate-700/70 px-3 py-1 capitalize text-slate-200">
              {estado}: <span className="font-semibold text-emerald-300">{quantidade}</span>
            </span>
          ))}
      </div>
    )
  }

  const totalPages = dados ? Math.max(1, Math.ceil(dados.totalCount / filters.pageSize)) : 1

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 rounded-xl border border-slate-700/60 bg-night-900/70 p-4 md:flex-row md:items-center md:justify-between">
        <div>
          <h2 className="text-lg font-semibold text-emerald-200">Prioridade de captura</h2>
          <p className="text-sm text-slate-400">
            Avaliação combinada de custo logístico, risco operativo e valor científico para cada pato primordial.
          </p>
        </div>
        <button
          type="button"
          onClick={handleRecalcular}
          disabled={recalculando}
          className="inline-flex items-center justify-center rounded-lg border border-emerald-400/40 px-4 py-2 text-sm font-semibold text-emerald-200 transition hover:bg-emerald-500/10 disabled:opacity-40"
        >
          {recalculando ? 'Recalculando…' : 'Recalcular todos'}
        </button>
      </div>

      {error && <ErrorNotice error={error} />}

      <form
        className="grid gap-4 rounded-xl border border-slate-700/60 bg-night-900/70 p-4 md:grid-cols-2 lg:grid-cols-6"
        onSubmit={(event) => {
          event.preventDefault()
          carregar()
        }}
      >
        <div className="flex flex-col">
          <label className="text-xs font-semibold uppercase tracking-wide text-slate-400">Ordenar por</label>
          <select
            value={filters.ordem}
            onChange={(event) => updateFilters({ ordem: event.target.value })}
            className="mt-1 rounded border border-slate-700 bg-night-800/60 px-3 py-2 text-sm text-slate-100 focus:border-emerald-400 focus:outline-none"
          >
            <option value="prioridade">Maior prioridade</option>
            <option value="risco">Risco total</option>
            <option value="valor">Valor científico</option>
            <option value="custo">Menor custo</option>
            <option value="poderio">Poderio necessário</option>
            <option value="dist">Distância</option>
          </select>
        </div>
        <div className="flex flex-col">
          <label className="text-xs font-semibold uppercase tracking-wide text-slate-400">Classe de risco</label>
          <select
            value={filters.risco ?? ''}
            onChange={(event) => updateFilters({ risco: event.target.value })}
            className="mt-1 rounded border border-slate-700 bg-night-800/60 px-3 py-2 text-sm text-slate-100 focus:border-emerald-400 focus:outline-none"
          >
            <option value="">Todas</option>
            <option value="alto">Alto</option>
            <option value="medio">Médio</option>
            <option value="baixo">Baixo</option>
          </select>
        </div>
        <div className="flex flex-col">
          <label className="text-xs font-semibold uppercase tracking-wide text-slate-400">Estado</label>
          <select
            value={filters.estado ?? ''}
            onChange={(event) => updateFilters({ estado: event.target.value })}
            className="mt-1 rounded border border-slate-700 bg-night-800/60 px-3 py-2 text-sm text-slate-100 focus:border-emerald-400 focus:outline-none"
          >
            <option value="">Todos</option>
            <option value="desperto">Despertos</option>
            <option value="transe">Em transe</option>
            <option value="hibernacao">Em hibernação</option>
          </select>

        </div>
        <div className="flex flex-col">
          <label className="text-xs font-semibold uppercase tracking-wide text-slate-400">País</label>
          <input
            value={filters.pais ?? ''}
            onChange={(event) => updateFilters({ pais: event.target.value })}
            placeholder="Todos"
            className="mt-1 rounded border border-slate-700 bg-night-800/60 px-3 py-2 text-sm text-slate-100 focus:border-emerald-400 focus:outline-none"
          />
        </div>
        <div className="flex flex-col">
          <label className="text-xs font-semibold uppercase tracking-wide text-slate-400">Itens por página</label>
          <select
            value={filters.pageSize}
            onChange={(event) => updateFilters({ pageSize: Number(event.target.value) })}
            className="mt-1 rounded border border-slate-700 bg-night-800/60 px-3 py-2 text-sm text-slate-100 focus:border-emerald-400 focus:outline-none"
          >
            <option value={10}>10</option>
            <option value={20}>20</option>
            <option value={50}>50</option>
          </select>
        </div>
        <div className="flex items-end">
          <button
            type="submit"
            className="w-full rounded-lg bg-emerald-500 px-4 py-2 text-sm font-semibold text-night-900 transition hover:bg-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-400"
          >
            Atualizar
          </button>
        </div>
      </form>

      {renderResumo()}

      <div className="overflow-hidden rounded-xl border border-slate-700/60 bg-night-900/70">
        <table className="min-w-full text-sm">
          <thead className="bg-night-800/80 text-xs uppercase tracking-wide text-slate-300">
            <tr>
              <th className="px-4 py-3 text-left">Prioridade</th>
              <th className="px-4 py-3 text-left">Classe</th>
              <th className="px-4 py-3 text-left">Risco</th>
              <th className="px-4 py-3 text-left">Valor científico</th>
              <th className="px-4 py-3 text-left">Custo (₢)</th>
              <th className="px-4 py-3 text-left">Poderio</th>
              <th className="px-4 py-3 text-left">Distância (km)</th>
              <th className="px-4 py-3 text-left">Localização</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-800/80">
            {loading && (
              <tr>
                <td colSpan={8} className="px-4 py-6 text-center text-slate-400">
                  Calculando rankings...
                </td>
              </tr>
            )}
            {!loading && dados && dados.items.length === 0 && (
              <tr>
                <td colSpan={8} className="px-4 py-6 text-center text-slate-400">
                  Nenhum pato atende aos filtros informados.
                </td>
              </tr>
            )}
            {!loading && dados?.items.map((item) => (
              <AnaliseRankingRow key={item.patoId} item={item} onNavigate={navigate} />
            ))}
          </tbody>
        </table>
      </div>

      {dados && dados.totalCount > filters.pageSize && (
        <div className="flex items-center justify-between rounded-xl border border-slate-700/60 bg-night-900/70 px-4 py-3 text-sm text-slate-300">
          <button
            type="button"
            onClick={() => changePage(Math.max(1, filters.page - 1))}
            disabled={filters.page <= 1}
            className="rounded border border-slate-700 px-3 py-1 transition hover:border-emerald-400 disabled:opacity-40"
          >
            Anterior
          </button>
          <span>
            Página {filters.page} de {totalPages}
          </span>
          <button
            type="button"
            onClick={() => changePage(Math.min(totalPages, filters.page + 1))}
            disabled={filters.page >= totalPages}
            className="rounded border border-slate-700 px-3 py-1 transition hover:border-emerald-400 disabled:opacity-40"
          >
            Próxima
          </button>
        </div>
      )}
    </div>
  )
}

interface AnaliseRankingRowProps {
  item: AnaliseRankingItem
  onNavigate: NavigateFunction
}

function AnaliseRankingRow({ item, onNavigate }: AnaliseRankingRowProps) {
  const destino = `/analise/${item.patoId}`

  return (
    <tr
      onClick={() => onNavigate(destino)}
      className="cursor-pointer transition hover:bg-night-800/60"
    >
      <td className="px-4 py-3 font-semibold text-emerald-200">{item.prioridade.toFixed(2)}</td>
      <td className="px-4 py-3">
        <span className="rounded bg-emerald-500/15 px-2 py-1 text-xs font-semibold uppercase tracking-wide text-emerald-200">
          {item.classePrioridade}
        </span>
      </td>
      <td className="px-4 py-3 capitalize text-slate-200">
        {item.classeRisco} ({item.riscoTotal})
      </td>
      <td className="px-4 py-3 text-slate-200">{item.valorCientifico}</td>
      <td className="px-4 py-3 text-slate-200">{item.custoTransporte.toFixed(2)}</td>
      <td className="px-4 py-3 text-slate-200">{item.poderioNecessario}</td>
      <td className="px-4 py-3 text-slate-200">{item.distKm.toFixed(2)}</td>
      <td className="px-4 py-3 text-slate-300">
        <div className="flex flex-col">
          <span>{item.cidade || 'Local desconhecido'}</span>
          <span className="text-xs text-slate-500">{item.pais}</span>
        </div>
      </td>
    </tr>
  )
}
