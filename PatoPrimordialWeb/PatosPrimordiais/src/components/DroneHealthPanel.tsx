import type { DroneSaudeDto } from '../api/types'

interface DroneHealthPanelProps {
  data?: DroneSaudeDto
  loading: boolean
  onRefresh: () => void
}

export function DroneHealthPanel({ data, loading, onRefresh }: DroneHealthPanelProps) {
  const contagem = data?.contagemPorStatus ?? {}
  const drones = data?.drones ?? []

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-lg font-semibold text-slate-100">Saúde dos drones</h2>
        <button
          type="button"
          onClick={onRefresh}
          className="rounded border border-slate-700 px-3 py-1 text-xs uppercase tracking-wide text-slate-300 transition hover:border-emerald-400"
        >
          Atualizar
        </button>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {Object.entries(contagem).map(([status, total]) => (
          <div key={status} className="rounded-xl border border-slate-700/60 bg-night-900/70 px-4 py-4">
            <p className="text-xs uppercase tracking-wide text-slate-400">{status || 'desconhecido'}</p>
            <p className="mt-2 text-2xl font-semibold text-slate-100">{total}</p>
          </div>
        ))}
        {Object.keys(contagem).length === 0 && (
          <div className="rounded-xl border border-slate-700/60 bg-night-900/70 px-4 py-4 text-sm text-slate-400">
            Nenhum drone monitorado.
          </div>
        )}
      </div>

      <div className="overflow-hidden rounded-xl border border-slate-700/60 bg-night-900/70">
        <table className="min-w-full text-sm">
          <thead className="bg-night-800/80 text-xs uppercase tracking-wide text-slate-300">
            <tr>
              <th className="px-4 py-3 text-left">Drone</th>
              <th className="px-4 py-3 text-left">Status</th>
              <th className="px-4 py-3 text-left">Último contato</th>
              <th className="px-4 py-3 text-left">Último avistamento</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-800/80">
            {loading && (
              <tr>
                <td colSpan={4} className="px-4 py-6 text-center text-slate-400">
                  Carregando drones...
                </td>
              </tr>
            )}
            {!loading && drones.length === 0 && (
              <tr>
                <td colSpan={4} className="px-4 py-6 text-center text-slate-400">
                  Nenhum drone disponível.
                </td>
              </tr>
            )}
            {!loading &&
              drones.map((drone) => (
                <tr key={drone.id} className="hover:bg-night-800/60">
                  <td className="px-4 py-3 text-slate-200">
                    <div className="flex flex-col">
                      <span>{drone.numeroSerie}</span>
                      <span className="text-xs text-slate-500">#{drone.id.slice(0, 8)}</span>
                    </div>
                  </td>
                  <td className="px-4 py-3 capitalize text-slate-200">{drone.status}</td>
                  <td className="px-4 py-3 text-slate-300">
                    {drone.ultimoContatoEm ? new Date(drone.ultimoContatoEm).toLocaleString('pt-BR') : '—'}
                  </td>
                  <td className="px-4 py-3 text-slate-300">
                    {drone.ultimoAvistamento ? (
                      <div className="flex flex-col">
                        <span>{new Date(drone.ultimoAvistamento.criadoEm).toLocaleString('pt-BR')}</span>
                        <span className="text-xs text-slate-500">
                          Estado {drone.ultimoAvistamento.estadoPato}, precisão {drone.ultimoAvistamento.precisaoM.toFixed(2)} m
                        </span>
                      </div>
                    ) : (
                      '—'
                    )}
                  </td>
                </tr>
              ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
