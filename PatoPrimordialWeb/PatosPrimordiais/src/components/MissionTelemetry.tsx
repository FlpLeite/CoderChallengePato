import type { TelemetriaDto } from '../api/types'

interface MissionTelemetryProps {
  telemetria: TelemetriaDto
  faseAtual: string
  proximaFase?: string
}

const metrics: { key: keyof TelemetriaDto; label: string; color: string }[] = [
  { key: 'bateria', label: 'Bateria', color: 'bg-emerald-400' },
  { key: 'combustivel', label: 'Combustível', color: 'bg-sky-400' },
  { key: 'integridade', label: 'Integridade', color: 'bg-orange-400' },
]

export function MissionTelemetry({ telemetria, faseAtual, proximaFase }: MissionTelemetryProps) {
  return (
    <div className="rounded-2xl border border-slate-800/70 bg-night-900/70 p-5 shadow-lg shadow-black/20">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h3 className="text-lg font-semibold text-emerald-200">Telemetria em tempo real</h3>
          <p className="text-sm text-slate-400">
            Fase atual: <span className="font-medium text-slate-100">{faseAtual}</span>
            {proximaFase ? (
              <>
                {' '}
                · Próxima fase: <span className="font-medium text-slate-200">{proximaFase}</span>
              </>
            ) : null}
          </p>
        </div>
        <div className="text-right text-xs text-slate-500">
          Distância estimada ao alvo: <span className="font-semibold text-slate-200">{telemetria.distanciaM.toFixed(1)} m</span>
        </div>
      </div>

      <div className="mt-4 grid gap-3 md:grid-cols-3">
        {metrics.map((metric) => {
          const value = Math.max(0, Math.min(100, telemetria[metric.key]))
          return (
            <div key={metric.key} className="space-y-1">
              <div className="flex items-center justify-between text-sm text-slate-300">
                <span>{metric.label}</span>
                <span className="font-semibold text-slate-100">{value.toFixed(1)}%</span>
              </div>
              <div className="h-2 rounded-full bg-night-800">
                <div className={`h-2 rounded-full ${metric.color}`} style={{ width: `${value}%` }} />
              </div>
            </div>
          )
        })}
      </div>
    </div>
  )
}
