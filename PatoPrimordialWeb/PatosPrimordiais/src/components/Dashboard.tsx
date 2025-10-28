import type { AvistamentoDto, PatosKpiDto } from '../api/types'

interface DashboardProps {
  kpis?: PatosKpiDto
  avistamentos: AvistamentoDto[]
}

const estadoLabels: Record<string, string> = {
  desperto: 'Despertos',
  transe: 'Em Transe',
  hibernacao: 'Em Hibernação',
}

export function Dashboard({ kpis, avistamentos }: DashboardProps) {
  const totalPatos = kpis?.total ?? 0
  const porEstado = kpis?.porEstado ?? {}

  return (
    <div className="space-y-6">
      <section>
        <h2 className="text-lg font-semibold text-slate-100">Indicadores principais</h2>
        <div className="mt-4 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <KpiCard label="Total de patos" value={totalPatos.toLocaleString('pt-BR')} />
          {Object.entries(porEstado).map(([estado, valor]) => (
            <KpiCard
              key={estado}
              label={estadoLabels[estado] ?? estado}
              value={valor.toLocaleString('pt-BR')}
            />
          ))}
          {totalPatos === 0 && <KpiCard label="Nenhum pato catalogado" value="-" />}
        </div>
      </section>

      <section>
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-semibold text-slate-100">Avistamentos recentes</h2>
          <span className="text-xs uppercase tracking-wide text-slate-400">Últimos {avistamentos.length}</span>
        </div>
        <div className="mt-4 overflow-hidden rounded-xl border border-slate-700/60 bg-night-900/60">
          {avistamentos.length === 0 ? (
            <div className="p-6 text-sm text-slate-400">Nenhum avistamento registrado.</div>
          ) : (
            <table className="min-w-full text-left text-sm">
              <thead className="bg-night-800/80 text-xs uppercase tracking-wide text-slate-300">
                <tr>
                  <th className="px-4 py-3">Data</th>
                  <th className="px-4 py-3">Localização</th>
                  <th className="px-4 py-3">Drone</th>
                  <th className="px-4 py-3">Estado</th>
                  <th className="px-4 py-3">Precisão (m)</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-800/80">
                {avistamentos.map((item) => (
                  <tr key={item.id} className="hover:bg-night-800/60">
                    <td className="px-4 py-3 text-slate-200">
                      {new Date(item.criadoEm).toLocaleString('pt-BR')}
                    </td>
                    <td className="px-4 py-3 text-slate-300">
                      <div className="flex flex-col">
                        <span>{item.cidade}</span>
                        <span className="text-xs text-slate-500">{item.pais}</span>
                      </div>
                    </td>
                    <td className="px-4 py-3 text-slate-300">
                      {item.droneNumeroSerie}
                      <div className="text-xs text-slate-500">#{item.droneId.slice(0, 8)}</div>
                    </td>
                    <td className="px-4 py-3 capitalize text-slate-200">{item.estadoPato}</td>
                    <td className="px-4 py-3 text-slate-200">{item.precisaoM.toFixed(2)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </section>
    </div>
  )
}

function KpiCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-xl border border-emerald-500/20 bg-night-900/80 px-4 py-5 shadow-lg shadow-emerald-500/5">
      <p className="text-xs uppercase tracking-wide text-emerald-300/80">{label}</p>
      <p className="mt-2 text-2xl font-semibold text-emerald-200">{value}</p>
    </div>
  )
}
