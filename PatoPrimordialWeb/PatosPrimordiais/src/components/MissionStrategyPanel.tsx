import type { FraquezaAplicadaDto, PlanoDefesaDto, PlanoEstrategiaDto } from '../api/types'

interface MissionStrategyPanelProps {
  estrategia: PlanoEstrategiaDto
  defesas: PlanoDefesaDto
  fraquezas: FraquezaAplicadaDto[]
}

export function MissionStrategyPanel({ estrategia, defesas, fraquezas }: MissionStrategyPanelProps) {
  return (
    <div className="grid gap-4 lg:grid-cols-2">
      <div className="rounded-2xl border border-slate-800/70 bg-night-900/70 p-5 shadow-lg shadow-black/20">
        <h3 className="text-lg font-semibold text-emerald-200">Plano tático</h3>
        <p className="text-xs text-slate-400">
          Porte: <span className="font-medium text-slate-200 uppercase">{estrategia.porte}</span> · Risco:{' '}
          <span className="font-medium text-slate-200 uppercase">{estrategia.classeRisco}</span>
        </p>
        <ul className="mt-4 space-y-3">
          {estrategia.taticas.map((tatica) => (
            <li key={tatica.nome} className="rounded-xl border border-slate-800/70 bg-night-950/40 p-4">
              <div className="flex items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-slate-100">{tatica.nome}</p>
                  <p className="text-xs text-slate-400">{tatica.descricao}</p>
                </div>
                <span className="text-xs font-semibold text-emerald-300">P{tatica.prioridade}</span>
              </div>
              <div className="mt-2 text-xs text-slate-400">
                {tatica.explorandoFraqueza ? (
                  <span className="text-emerald-300">Bônus de fraqueza {Math.round(tatica.bonusFraqueza * 100)} p.p.</span>
                ) : (
                  <span className="text-slate-500">Sem sinergia direta</span>
                )}
              </div>
            </li>
          ))}
        </ul>
      </div>
      <div className="space-y-4">
        <div className="rounded-2xl border border-slate-800/70 bg-night-900/70 p-5 shadow-lg shadow-black/20">
          <h3 className="text-lg font-semibold text-emerald-200">Defesas ativas</h3>
          <div className="mt-3 space-y-3 text-sm text-slate-300">
            {defesas.primaria ? (
              <DefesaItem titulo="Primária" defesa={defesas.primaria} destaque />
            ) : (
              <p className="text-xs text-slate-500">Nenhuma defesa primária selecionada.</p>
            )}
            {defesas.fallback ? (
              <DefesaItem titulo="Fallback" defesa={defesas.fallback} />
            ) : (
              <p className="text-xs text-slate-500">Fallback neutro indisponível.</p>
            )}
          </div>
        </div>
        <div className="rounded-2xl border border-slate-800/70 bg-night-900/70 p-5 shadow-lg shadow-black/20">
          <h3 className="text-lg font-semibold text-emerald-200">Fraquezas exploradas</h3>
          {fraquezas.length === 0 ? (
            <p className="text-xs text-slate-500">Nenhuma fraqueza catalogada se aplica a este alvo.</p>
          ) : (
            <ul className="mt-3 space-y-3">
              {fraquezas.map((fraqueza) => (
                <li key={fraqueza.id} className="rounded-xl border border-emerald-500/30 bg-emerald-500/10 p-4">
                  <p className="text-sm font-semibold text-emerald-200">{fraqueza.nome}</p>
                  <p className="text-xs text-emerald-100/80">{fraqueza.descricao}</p>
                  <p className="mt-1 text-xs font-semibold text-emerald-300">
                    Bônus aplicado: {Math.round(fraqueza.bonusSucesso * 100)} p.p.
                  </p>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </div>
  )
}

interface DefesaItemProps {
  titulo: string
  defesa: PlanoDefesaDto['primaria']
  destaque?: boolean
}

function DefesaItem({ titulo, defesa, destaque }: DefesaItemProps) {
  if (!defesa) return null
  return (
    <div className={`rounded-xl border p-4 ${destaque ? 'border-emerald-500/40 bg-emerald-500/5' : 'border-slate-800/70 bg-night-950/40'}`}>
      <div className="flex items-center justify-between text-sm">
        <span className="font-semibold text-slate-100">{titulo}: {defesa.nome}</span>
        <span className="text-xs text-slate-400">Mitigação {(defesa.mitigacao * 100).toFixed(0)}%</span>
      </div>
      <p className="mt-1 text-xs text-slate-400">{defesa.contramedida}</p>
      {defesa.tagsAmeaca.length > 0 ? (
        <div className="mt-2 flex flex-wrap gap-1">
          {defesa.tagsAmeaca.map((tag) => (
            <span key={tag} className="rounded-full bg-night-800 px-2 py-0.5 text-[10px] uppercase tracking-wide text-emerald-200">
              {tag}
            </span>
          ))}
        </div>
      ) : (
        <p className="mt-2 text-[10px] uppercase tracking-wide text-slate-500">Neutra</p>
      )}
    </div>
  )
}
