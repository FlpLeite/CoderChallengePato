import { useMemo, useState } from 'react'
import type { MissaoTickDto } from '../api/types'

interface MissionTimelineProps {
  ticks: MissaoTickDto[]
  onLoadMore?: () => void
  hasMore: boolean
  carregando?: boolean
}

interface ResumoFase {
  primeiroTick: number
  ultimoTick: number
  totalEventos: number
  bateria: { min: number; max: number }
  combustivel: { min: number; max: number }
  integridade: { min: number; max: number }
  distancia: { min: number; max: number }
  possuiSucesso: boolean
}

export function MissionTimeline({ ticks, onLoadMore, hasMore, carregando }: MissionTimelineProps) {
  const [faseAberta, setFaseAberta] = useState<string | null>(null)

  const fasesAgrupadas = useMemo(() => {
    const ordem: string[] = []
    const grupos = new Map<string, MissaoTickDto[]>()

    for (const tick of ticks) {
      if (!grupos.has(tick.fase)) {
        grupos.set(tick.fase, [])
        ordem.push(tick.fase)
      }

      grupos.get(tick.fase)?.push(tick)
    }

    return ordem.map((fase) => {
      const faseTicks = grupos.get(fase) ?? []
      const ordenados = [...faseTicks].sort((a, b) => a.tick - b.tick)

      if (ordenados.length === 0) {
        return {
          fase,
          ticks: ordenados,
          resumo: {
            primeiroTick: 0,
            ultimoTick: 0,
            totalEventos: 0,
            bateria: { min: 0, max: 0 },
            combustivel: { min: 0, max: 0 },
            integridade: { min: 0, max: 0 },
            distancia: { min: 0, max: 0 },
            possuiSucesso: false,
          } satisfies ResumoFase,
        }
      }

      let bateriaMin = ordenados[0].bateriaPct
      let bateriaMax = ordenados[0].bateriaPct
      let combustivelMin = ordenados[0].combustivelPct
      let combustivelMax = ordenados[0].combustivelPct
      let integridadeMin = ordenados[0].integridadePct
      let integridadeMax = ordenados[0].integridadePct
      let distanciaMin = ordenados[0].distanciaM
      let distanciaMax = ordenados[0].distanciaM

      for (const tick of ordenados) {
        bateriaMin = Math.min(bateriaMin, tick.bateriaPct)
        bateriaMax = Math.max(bateriaMax, tick.bateriaPct)
        combustivelMin = Math.min(combustivelMin, tick.combustivelPct)
        combustivelMax = Math.max(combustivelMax, tick.combustivelPct)
        integridadeMin = Math.min(integridadeMin, tick.integridadePct)
        integridadeMax = Math.max(integridadeMax, tick.integridadePct)
        distanciaMin = Math.min(distanciaMin, tick.distanciaM)
        distanciaMax = Math.max(distanciaMax, tick.distanciaM)
      }

      const resumo: ResumoFase = {
        primeiroTick: ordenados[0].tick,
        ultimoTick: ordenados[ordenados.length - 1].tick,
        totalEventos: ordenados.length,
        bateria: { min: bateriaMin, max: bateriaMax },
        combustivel: { min: combustivelMin, max: combustivelMax },
        integridade: { min: integridadeMin, max: integridadeMax },
        distancia: { min: distanciaMin, max: distanciaMax },
        possuiSucesso: ordenados.some((tick) => tick.sucesso),
      }

      return {
        fase,
        ticks: ordenados,
        resumo,
      }
    })
  }, [ticks])

  return (
    <div className="rounded-2xl border border-slate-800/70 bg-night-900/70 p-5 shadow-lg shadow-black/20">
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-semibold text-emerald-200">Linha do tempo</h3>
        {carregando ? <span className="text-xs text-slate-400">Atualizando…</span> : null}
      </div>
      <div className="mt-4 space-y-4">
        {fasesAgrupadas.length === 0 ? (
          <p className="text-sm text-slate-400">Nenhum evento registrado até o momento.</p>
        ) : null}
        {fasesAgrupadas.map(({ fase, ticks: faseTicks, resumo }) => {
          const aberta = faseAberta === fase
          const faseTitulo = fase.replace(/_/g, ' ')

          return (
            <div
              key={fase}
              className="rounded-xl border border-slate-800/60 bg-night-950/40 p-4 transition hover:border-slate-700/60"
            >
              <button
                type="button"
                onClick={() => setFaseAberta((atual) => (atual === fase ? null : fase))}
                className="flex w-full items-start justify-between gap-4 text-left"
              >
                <div>
                  <p className="text-xs uppercase tracking-wide text-slate-400">Fase</p>
                  <h4 className="text-lg font-semibold text-emerald-200">{faseTitulo}</h4>
                  <p className="mt-1 text-xs text-slate-400">
                    {resumo.totalEventos} evento{resumo.totalEventos === 1 ? '' : 's'} · ticks {resumo.primeiroTick}–
                    {resumo.ultimoTick}
                  </p>
                </div>
                <div className="flex flex-col items-end text-xs text-slate-400">
                  <span>🔋 {resumo.bateria.min.toFixed(0)}% – {resumo.bateria.max.toFixed(0)}%</span>
                  <span>⛽ {resumo.combustivel.min.toFixed(0)}% – {resumo.combustivel.max.toFixed(0)}%</span>
                  <span>🛡️ {resumo.integridade.min.toFixed(0)}% – {resumo.integridade.max.toFixed(0)}%</span>
                  <span>📏 {resumo.distancia.min.toFixed(1)}m – {resumo.distancia.max.toFixed(1)}m</span>
                  <span className="mt-1 text-emerald-300">
                    {resumo.possuiSucesso ? 'Eventos bem-sucedidos' : 'Em andamento'}
                  </span>
                  <span className="mt-2 text-base text-emerald-200">{aberta ? '−' : '+'}</span>
                </div>
              </button>
              {aberta ? (
                <ul className="mt-4 space-y-3 border-t border-slate-800/60 pt-4 text-sm text-slate-200">
                  {faseTicks.map((tick) => (
                    <li
                      key={tick.id}
                      className={`rounded-lg border border-slate-800/50 bg-night-900/70 p-3 ${
                        tick.sucesso ? 'border-emerald-500/40' : ''
                      }`}
                    >
                      <div className="flex flex-wrap items-baseline justify-between gap-2 text-xs text-slate-400">
                        <span className="font-mono">Tick {tick.tick}</span>
                        <div className="flex flex-wrap gap-2">
                          <span>🔋 {tick.bateriaPct.toFixed(0)}%</span>
                          <span>⛽ {tick.combustivelPct.toFixed(0)}%</span>
                          <span>🛡️ {tick.integridadePct.toFixed(0)}%</span>
                          <span>📏 {tick.distanciaM.toFixed(1)}m</span>
                        </div>
                      </div>
                      <p className="mt-2 text-sm text-slate-100">{tick.evento}</p>
                      {tick.detalhe ? (
                        <details className="mt-2 text-[11px] text-slate-400">
                          <summary className="cursor-pointer text-xs text-emerald-300">Ver detalhes</summary>
                          <pre className="mt-2 whitespace-pre-wrap break-words bg-night-950/60 p-2 text-[11px]">
                            {JSON.stringify(tick.detalhe, null, 2)}
                          </pre>
                        </details>
                      ) : null}
                    </li>
                  ))}
                </ul>
              ) : null}
            </div>
          )
        })}
      </div>
      {hasMore ? (
        <div className="mt-4 text-center">
          <button
            type="button"
            className="rounded-full bg-night-800 px-4 py-2 text-sm font-medium text-emerald-200 transition hover:bg-night-700"
            onClick={onLoadMore}
            disabled={carregando}
          >
            Carregar mais
          </button>
        </div>
      ) : null}
    </div>
  )
}
