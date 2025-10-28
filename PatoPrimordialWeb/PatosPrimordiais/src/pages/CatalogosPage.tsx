import { useEffect, useState } from 'react'
import { obterFraquezasCatalogo, obterRegrasDefesas, obterRegrasTaticas } from '../api/client'
import type { FraquezaCatalogoDto, RegraDefesaDto, RegraTaticaDto } from '../api/types'
import { LoadingState } from '../components/LoadingState'
import { ErrorNotice } from '../components/ErrorNotice'

export function CatalogosPage() {
  const [taticas, setTaticas] = useState<RegraTaticaDto[]>([])
  const [defesas, setDefesas] = useState<RegraDefesaDto[]>([])
  const [fraquezas, setFraquezas] = useState<FraquezaCatalogoDto[]>([])
  const [erro, setErro] = useState<string | null>(null)
  const [carregando, setCarregando] = useState(true)

  useEffect(() => {
    async function carregar() {
      try {
        setCarregando(true)
        const [taticasResp, defesasResp, fraquezasResp] = await Promise.all([
          obterRegrasTaticas(),
          obterRegrasDefesas(),
          obterFraquezasCatalogo(),
        ])
        setTaticas(taticasResp)
        setDefesas(defesasResp)
        setFraquezas(fraquezasResp)
      } catch (error) {
        setErro((error as Error).message)
      } finally {
        setCarregando(false)
      }
    }

    carregar()
  }, [])

  return (
    <div className="space-y-6">
      <header>
        <h2 className="text-2xl font-semibold text-emerald-200">Catálogos táticos e defensivos</h2>
        <p className="text-sm text-slate-400">
          Referência das regras usadas pelos simuladores SGDA para planejar missões contra patos primordiais.
        </p>
      </header>

      {erro ? <ErrorNotice error={erro} /> : null}
      {carregando ? <LoadingState message="Carregando catálogos" /> : null}

      <section className="grid gap-4 lg:grid-cols-2">
        <div className="rounded-2xl border border-slate-800/70 bg-night-900/70 p-5 shadow-lg shadow-black/20">
          <h3 className="text-lg font-semibold text-emerald-200">Regras táticas</h3>
          <ul className="mt-3 space-y-3 text-sm text-slate-200">
            {taticas.map((tatica) => (
              <li key={tatica.id} className="rounded-xl border border-slate-800/70 bg-night-950/40 p-4">
                <div className="flex items-center justify-between">
                  <span className="font-semibold">{tatica.nome}</span>
                  <span className="text-xs text-emerald-300">Prioridade {tatica.prioridade}</span>
                </div>
                <p className="mt-1 text-xs text-slate-400">{tatica.descricao}</p>
                <pre className="mt-2 whitespace-pre-wrap break-words text-[11px] text-slate-500">
                  Condição: {JSON.stringify(tatica.condicao, null, 2)}
                </pre>
                <pre className="mt-1 whitespace-pre-wrap break-words text-[11px] text-slate-500">
                  Ação: {JSON.stringify(tatica.acao, null, 2)}
                </pre>
              </li>
            ))}
          </ul>
        </div>
        <div className="rounded-2xl border border-slate-800/70 bg-night-900/70 p-5 shadow-lg shadow-black/20">
          <h3 className="text-lg font-semibold text-emerald-200">Defesas SGDA</h3>
          <ul className="mt-3 space-y-3 text-sm text-slate-200">
            {defesas.map((defesa) => (
              <li key={defesa.id} className="rounded-xl border border-slate-800/70 bg-night-950/40 p-4">
                <div className="flex items-center justify-between">
                  <span className="font-semibold">{defesa.nome}</span>
                  <span className="text-xs text-slate-400">Rareza {defesa.rareza}</span>
                </div>
                <p className="mt-1 text-xs text-slate-400">{defesa.contramedida}</p>
                <p className="mt-2 text-[11px] uppercase tracking-wide text-emerald-300">
                  Tags: {defesa.tagsAmeaca.length > 0 ? defesa.tagsAmeaca.join(', ') : 'neutra'}
                </p>
                <p className="text-[11px] text-slate-500">Mitigação {(defesa.mitigacao * 100).toFixed(0)}%</p>
                {defesa.fallback ? <p className="text-[11px] text-slate-500">Fallback neutra</p> : null}
              </li>
            ))}
          </ul>
        </div>
      </section>

      <section className="rounded-2xl border border-slate-800/70 bg-night-900/70 p-5 shadow-lg shadow-black/20">
        <h3 className="text-lg font-semibold text-emerald-200">Fraquezas catalogadas</h3>
        <ul className="mt-3 space-y-3 text-sm text-slate-200">
          {fraquezas.map((fraqueza) => (
            <li key={fraqueza.id} className="rounded-xl border border-emerald-500/30 bg-emerald-500/10 p-4">
              <div className="flex items-center justify-between">
                <span className="font-semibold text-emerald-200">{fraqueza.nome}</span>
                <span className="text-xs text-emerald-300">Bônus {(fraqueza.efeito.bonusSucesso * 100).toFixed(0)}%</span>
              </div>
              <p className="mt-1 text-xs text-emerald-100/80">{fraqueza.efeito.descricao}</p>
              <pre className="mt-2 whitespace-pre-wrap break-words text-[11px] text-emerald-200/70">
                Condição: {JSON.stringify(fraqueza.condicao, null, 2)}
              </pre>
            </li>
          ))}
        </ul>
      </section>
    </div>
  )
}
