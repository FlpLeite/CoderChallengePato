import { useEffect, useState } from 'react'
import type { PagedResult, PatoListItem, PatosFilters } from '../api/types'

interface PatosListProps {
  data?: PagedResult<PatoListItem>
  filters: PatosFilters
  onChangeFilters: (filters: PatosFilters) => void
  onSelect: (id: string) => void
  loading: boolean
}

export function PatosList({ data, filters, onChangeFilters, onSelect, loading }: PatosListProps) {
  const [localFilters, setLocalFilters] = useState(filters)

  useEffect(() => {
    setLocalFilters(filters)
  }, [filters])

  function updateFilter<K extends keyof PatosFilters>(key: K, value: PatosFilters[K]) {
    setLocalFilters((prev) => ({ ...prev, [key]: value }))
  }

  function applyFilters(event?: React.FormEvent) {
    event?.preventDefault()
    onChangeFilters({ ...localFilters, page: 1 })
  }

  function changePage(page: number) {
    onChangeFilters({ ...filters, page })
  }

  const totalPages = data ? Math.max(1, Math.ceil(data.totalCount / filters.pageSize)) : 1

  return (
    <div className="space-y-6">
      <form onSubmit={applyFilters} className="grid gap-4 rounded-xl border border-slate-700/60 bg-night-900/70 p-4 sm:grid-cols-2 lg:grid-cols-4">
        <div className="flex flex-col">
          <label className="text-xs font-semibold uppercase tracking-wide text-slate-400">Estado</label>
          <select
            value={localFilters.estado}
            onChange={(event) => updateFilter('estado', event.target.value)}
            className="mt-1 rounded border border-slate-700 bg-night-800/60 px-3 py-2 text-sm text-slate-100 focus:border-emerald-400 focus:outline-none"
          >
            <option value="">Todos</option>
            <option value="desperto">Despertos</option>
            <option value="transe">Em transe</option>
            <option value="hibernacao">Em hibernação</option>
          </select>
        </div>

        <div className="flex flex-col">
          <label className="text-xs font-semibold uppercase tracking-wide text-slate-400">Cidade</label>
          <input
            value={localFilters.cidade}
            onChange={(event) => updateFilter('cidade', event.target.value)}
            placeholder="Todas"
            className="mt-1 rounded border border-slate-700 bg-night-800/60 px-3 py-2 text-sm text-slate-100 focus:border-emerald-400 focus:outline-none"
          />
        </div>

        <div className="flex gap-3">
          <div className="flex w-full flex-col">
            <label className="text-xs font-semibold uppercase tracking-wide text-slate-400">Mut. mín.</label>
            <input
              type="number"
              value={localFilters.mutacoesMin ?? ''}
              onChange={(event) => updateFilter('mutacoesMin', event.target.value ? Number(event.target.value) : undefined)}
              className="mt-1 rounded border border-slate-700 bg-night-800/60 px-3 py-2 text-sm text-slate-100 focus:border-emerald-400 focus:outline-none"
            />
          </div>
          <div className="flex w-full flex-col">
            <label className="text-xs font-semibold uppercase tracking-wide text-slate-400">Mut. máx.</label>
            <input
              type="number"
              value={localFilters.mutacoesMax ?? ''}
              onChange={(event) => updateFilter('mutacoesMax', event.target.value ? Number(event.target.value) : undefined)}
              className="mt-1 rounded border border-slate-700 bg-night-800/60 px-3 py-2 text-sm text-slate-100 focus:border-emerald-400 focus:outline-none"
            />
          </div>
        </div>

        <div className="sm:col-span-2 lg:col-span-4">
          <button
            type="submit"
            className="w-full rounded-lg bg-emerald-500 px-4 py-2 text-sm font-semibold text-night-900 transition hover:bg-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-400"
          >
            Aplicar filtros
          </button>
        </div>
      </form>

      <div className="overflow-hidden rounded-xl border border-slate-700/60 bg-night-900/70">
        <table className="min-w-full text-sm">
          <thead className="bg-night-800/80 text-xs uppercase tracking-wide text-slate-300">
            <tr>
              <th className="px-4 py-3 text-left">Código</th>
              <th className="px-4 py-3 text-left">Localização</th>
              <th className="px-4 py-3 text-left">Estado</th>
              <th className="px-4 py-3 text-left">Mut. observadas</th>
              <th className="px-4 py-3 text-left">Precisão (m)</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-800/80">
            {loading && (
              <tr>
                <td colSpan={5} className="px-4 py-6 text-center text-slate-400">
                  Carregando catálogo...
                </td>
              </tr>
            )}
            {!loading && data && data.items.length === 0 && (
              <tr>
                <td colSpan={5} className="px-4 py-6 text-center text-slate-400">
                  Nenhum pato encontrado com os filtros aplicados.
                </td>
              </tr>
            )}
            {!loading && data?.items.map((pato) => (
              <tr
                key={pato.id}
                onClick={() => onSelect(pato.id)}
                className="cursor-pointer hover:bg-night-800/60"
              >
                <td className="px-4 py-3 font-medium text-slate-100">{pato.codigo}</td>
                <td className="px-4 py-3 text-slate-300">
                  <div className="flex flex-col">
                    <span>{pato.cidade || 'Local desconhecido'}</span>
                    <span className="text-xs text-slate-500">{pato.pais}</span>
                  </div>
                </td>
                <td className="px-4 py-3 capitalize text-slate-200">{pato.estado}</td>
                <td className="px-4 py-3 text-slate-200">{pato.mutacoesQtde ?? '-'}</td>
                <td className="px-4 py-3 text-slate-200">{pato.precisaoM.toFixed(2)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {data && data.totalCount > filters.pageSize && (
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
