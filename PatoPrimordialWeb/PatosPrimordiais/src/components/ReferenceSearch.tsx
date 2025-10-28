import { useState } from 'react'
import type { PontoReferenciaDto } from '../api/types'

interface ReferenceSearchProps {
  resultados: PontoReferenciaDto[]
  loading: boolean
  onBuscar: (lat: number, lon: number) => void
  onLimpar: () => void
}

export function ReferenceSearch({ resultados, loading, onBuscar, onLimpar }: ReferenceSearchProps) {
  const [lat, setLat] = useState('')
  const [lon, setLon] = useState('')

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    const latTrimmed = lat.trim()
    const lonTrimmed = lon.trim()
    if (latTrimmed === '' || lonTrimmed === '') {
      onLimpar()
      return
    }

    const latValue = Number(latTrimmed)
    const lonValue = Number(lonTrimmed)
    if (!Number.isNaN(latValue) && !Number.isNaN(lonValue)) {
      onBuscar(latValue, lonValue)
    }
  }

  return (
    <div className="space-y-6">
      <form onSubmit={handleSubmit} className="grid gap-4 rounded-xl border border-slate-700/60 bg-night-900/70 p-4 sm:grid-cols-2">
        <div className="flex flex-col">
          <label className="text-xs font-semibold uppercase tracking-wide text-slate-400">Latitude</label>
          <input
            value={lat}
            onChange={(event) => setLat(event.target.value)}
            placeholder="Ex: -23.5505"
            className="mt-1 rounded border border-slate-700 bg-night-800/60 px-3 py-2 text-sm text-slate-100 focus:border-emerald-400 focus:outline-none"
          />
        </div>
        <div className="flex flex-col">
          <label className="text-xs font-semibold uppercase tracking-wide text-slate-400">Longitude</label>
          <input
            value={lon}
            onChange={(event) => setLon(event.target.value)}
            placeholder="Ex: -46.6333"
            className="mt-1 rounded border border-slate-700 bg-night-800/60 px-3 py-2 text-sm text-slate-100 focus:border-emerald-400 focus:outline-none"
          />
        </div>
        <div className="sm:col-span-2">
          <button
            type="submit"
            className="w-full rounded-lg bg-emerald-500 px-4 py-2 text-sm font-semibold text-night-900 transition hover:bg-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-400"
          >
            Buscar próximos
          </button>
        </div>
      </form>

      <div className="overflow-hidden rounded-xl border border-slate-700/60 bg-night-900/70">
        <table className="min-w-full text-sm">
          <thead className="bg-night-800/80 text-xs uppercase tracking-wide text-slate-300">
            <tr>
              <th className="px-4 py-3 text-left">Nome</th>
              <th className="px-4 py-3 text-left">Coordenadas</th>
              <th className="px-4 py-3 text-left">Raio (m)</th>
              <th className="px-4 py-3 text-left">Distância (m)</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-800/80">
            {loading && (
              <tr>
                <td colSpan={4} className="px-4 py-6 text-center text-slate-400">
                  Calculando proximidade...
                </td>
              </tr>
            )}
            {!loading && resultados.length === 0 && (
              <tr>
                <td colSpan={4} className="px-4 py-6 text-center text-slate-400">
                  Nenhum ponto encontrado no raio informado.
                </td>
              </tr>
            )}
            {!loading &&
              resultados.map((ponto) => (
                <tr key={ponto.id} className="hover:bg-night-800/60">
                  <td className="px-4 py-3 text-slate-200">{ponto.nome}</td>
                  <td className="px-4 py-3 text-slate-300">
                    {ponto.latitude.toFixed(4)}, {ponto.longitude.toFixed(4)}
                  </td>
                  <td className="px-4 py-3 text-slate-200">{ponto.raioMetros.toFixed(0)}</td>
                  <td className="px-4 py-3 text-slate-200">
                    {typeof ponto.distanciaMetros === 'number' ? ponto.distanciaMetros.toFixed(1) : '—'}
                  </td>
                </tr>
              ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
