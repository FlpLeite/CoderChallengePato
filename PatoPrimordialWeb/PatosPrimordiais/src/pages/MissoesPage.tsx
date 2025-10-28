import { useEffect, useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { listarMissoes } from '../api/client'
import type { MissaoListItemDto } from '../api/types'
import { LoadingState } from '../components/LoadingState'
import { ErrorNotice } from '../components/ErrorNotice'

export function MissoesPage() {
  const [missoes, setMissoes] = useState<MissaoListItemDto[]>([])
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState<string | null>(null)
  const [patoId, setPatoId] = useState('')
  const navigate = useNavigate()

  useEffect(() => {
    async function carregar() {
      try {
        setCarregando(true)
        const data = await listarMissoes()
        setMissoes(data)
      } catch (error) {
        setErro((error as Error).message)
      } finally {
        setCarregando(false)
      }
    }

    carregar()
  }, [])

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!patoId) return
    navigate(`/missoes/nova?patoId=${encodeURIComponent(patoId)}`)
  }

  return (
    <div className="space-y-6">
      <header className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h2 className="text-2xl font-semibold text-emerald-200">Missões de contenção</h2>
          <p className="text-sm text-slate-400">
            Monitore operações simuladas e revisite estratégias aplicadas para cada pato primordial.
          </p>
        </div>
        <form onSubmit={handleSubmit} className="flex flex-col gap-2 sm:flex-row sm:items-center">
          <label className="text-xs uppercase tracking-wide text-slate-400">Pato ID</label>
          <input
            className="rounded-full border border-slate-800 bg-night-900 px-4 py-2 text-sm text-slate-100 focus:border-emerald-400 focus:outline-none"
            placeholder="UUID do pato"
            value={patoId}
            onChange={(event) => setPatoId(event.target.value)}
          />
          <button
            type="submit"
            className="rounded-full bg-emerald-500 px-4 py-2 text-sm font-semibold text-night-900 transition hover:bg-emerald-400"
          >
            Nova missão
          </button>
        </form>
      </header>

      {erro ? <ErrorNotice error={erro} /> : null}
      {carregando ? <LoadingState message="Carregando missões" /> : null}

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
        {missoes.map((missao) => (
          <article key={missao.id} className="rounded-2xl border border-slate-800/70 bg-night-900/70 p-5 shadow-lg shadow-black/20">
            <div className="flex items-center justify-between text-xs text-slate-400">
              <span>ID: {missao.id}</span>
              <span className={`rounded-full px-2 py-0.5 font-semibold uppercase ${statusColor(missao.status)}`}>
                {missao.status.replace('_', ' ')}
              </span>
            </div>
            <div className="mt-3 space-y-2 text-sm text-slate-200">
              <p>
                <span className="text-slate-400">Pato:</span> {missao.patoId}
              </p>
              <p>
                <span className="text-slate-400">Poderio alocado:</span> {missao.poderioAlocado.toFixed(1)}
              </p>
              <p>
                <span className="text-slate-400">Criada em:</span> {new Date(missao.criadoEm).toLocaleString('pt-BR')}
              </p>
              {missao.iniciadoEm ? (
                <p>
                  <span className="text-slate-400">Iniciada:</span> {new Date(missao.iniciadoEm).toLocaleString('pt-BR')}
                </p>
              ) : null}
              {missao.finalizadoEm ? (
                <p>
                  <span className="text-slate-400">Finalizada:</span> {new Date(missao.finalizadoEm).toLocaleString('pt-BR')}
                </p>
              ) : null}
              {missao.resultado ? (
                <p className="text-xs text-emerald-200/80">{missao.resultado}</p>
              ) : null}
            </div>
            <div className="mt-4 flex gap-2">
              <button
                type="button"
                className="flex-1 rounded-full bg-night-800 px-3 py-2 text-sm font-semibold text-emerald-200 transition hover:bg-night-700"
                onClick={() => navigate(`/missoes/${missao.id}`)}
              >
                Visualizar
              </button>
              <button
                type="button"
                className="rounded-full bg-night-800 px-3 py-2 text-sm font-medium text-slate-300 transition hover:bg-night-700"
                onClick={() => navigate(`/missoes/nova?patoId=${missao.patoId}`)}
              >
                Replicar
              </button>
            </div>
          </article>
        ))}
        {!carregando && missoes.length === 0 ? (
          <div className="col-span-full rounded-2xl border border-dashed border-slate-700 p-8 text-center text-sm text-slate-400">
            Nenhuma missão criada ainda. Informe o ID de um pato para gerar uma nova estratégia.
          </div>
        ) : null}
      </div>
    </div>
  )
}

function statusColor(status: string) {
  switch (status) {
    case 'planejada':
      return 'bg-slate-800 text-slate-200'
    case 'em_execucao':
      return 'bg-amber-500/20 text-amber-300'
    case 'concluida':
      return 'bg-emerald-500/20 text-emerald-300'
    case 'abortada':
      return 'bg-rose-500/20 text-rose-300'
    default:
      return 'bg-slate-800 text-slate-200'
  }
}
