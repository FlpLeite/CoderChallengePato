import { useEffect, useMemo, useState, type ReactNode } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { fetchAnaliseDetalhe } from '../api/client'
import type { AnaliseDetalheDto } from '../api/types'
import { ErrorNotice } from '../components/ErrorNotice'
import { LoadingState } from '../components/LoadingState'

interface PesosPrioridade {
  valor: number
  custo: number
  risco: number
}

export function AnaliseDetalhePage() {
  const { patoId } = useParams<{ patoId: string }>()
  const navigate = useNavigate()
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [detalhe, setDetalhe] = useState<AnaliseDetalheDto | null>(null)
  const [pesos, setPesos] = useState<PesosPrioridade>({ valor: 0.45, custo: -0.35, risco: -0.2 })

  useEffect(() => {
    if (!patoId) {
      return
    }
    carregar(patoId)
  }, [patoId])

  async function carregar(id: string) {
    try {
      setLoading(true)
      setError(null)
      const data = await fetchAnaliseDetalhe(id)
      setDetalhe(data)
      setPesos({
        valor: data.parametros.pesoValorCientifico,
        custo: data.parametros.pesoCusto,
        risco: data.parametros.pesoRisco,
      })
    } catch (err) {
      setError((err as Error).message)
    } finally {
      setLoading(false)
    }
  }

  const simulacao = useMemo(() => {
    if (!detalhe) {
      return null
    }

    const prioridade =
      pesos.valor * detalhe.scores.valorCientifico +
      pesos.custo * (detalhe.scores.custoTransporte / 1000) +
      pesos.risco * detalhe.scores.riscoTotal

    const classe = prioridade >= 20 ? 'A' : prioridade >= 0 ? 'B' : 'C'

    return {
      prioridade,
      classe,
      diferenca: prioridade - detalhe.scores.prioridade,
    }
  }, [detalhe, pesos])

  function atualizarPeso(chave: keyof PesosPrioridade, valor: number) {
    setPesos((prev) => ({ ...prev, [chave]: valor }))
  }

  if (loading) {
    return <LoadingState message="Carregando análise..." />
  }

  if (error) {
    return <ErrorNotice error={error} />
  }

  if (!detalhe) {
    return null
  }

  return (
    <div className="space-y-6">
      <button
        type="button"
        onClick={() => navigate('/analise')}
        className="inline-flex items-center text-sm text-emerald-300 transition hover:text-emerald-100"
      >
        ← Voltar ao ranking
      </button>

      <header className="rounded-xl border border-slate-700/60 bg-night-900/70 p-4">
        <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
          <div>
            <h2 className="text-xl font-semibold text-emerald-200">{detalhe.codigo}</h2>
            <p className="text-sm text-slate-400">Último cálculo em {new Date(detalhe.calculadoEm).toLocaleString('pt-BR')}</p>
          </div>
          <div className="flex flex-wrap gap-2 text-xs text-slate-300">
            {detalhe.poderNome && (
              <span className="rounded-full border border-emerald-400/40 px-3 py-1 text-emerald-200">{detalhe.poderNome}</span>
            )}
            {detalhe.poderTags.map((tag) => (
              <span key={tag} className="rounded-full border border-slate-700/60 px-3 py-1 capitalize text-slate-200">
                {tag}
              </span>
            ))}
          </div>
        </div>
      </header>

      <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <ScoreCard titulo="Prioridade" destaque>{detalhe.scores.prioridade.toFixed(2)}</ScoreCard>
        <ScoreCard titulo="Classe" destaque>{detalhe.scores.classePrioridade}</ScoreCard>
        <ScoreCard titulo="Risco total">{detalhe.scores.riscoTotal}</ScoreCard>
        <ScoreCard titulo="Custo de transporte">₢ {detalhe.scores.custoTransporte.toFixed(2)}</ScoreCard>
        <ScoreCard titulo="Valor científico">{detalhe.scores.valorCientifico}</ScoreCard>
        <ScoreCard titulo="Poderio necessário">{detalhe.scores.poderioNecessario}</ScoreCard>
        <ScoreCard titulo="Distância">{detalhe.scores.distKm.toFixed(2)} km</ScoreCard>
      </section>

      <section className="grid gap-6 lg:grid-cols-2">
        <div className="rounded-xl border border-slate-700/60 bg-night-900/70 p-4">
          <h3 className="text-sm font-semibold uppercase tracking-wide text-slate-300">Entradas do cálculo</h3>
          <dl className="mt-4 grid gap-2 text-sm text-slate-200">
            <div className="flex items-center justify-between">
              <dt>Massa</dt>
              <dd>{detalhe.entradas.massaToneladas.toFixed(4)} t</dd>
            </div>
            <div className="flex items-center justify-between">
              <dt>Altura</dt>
              <dd>{detalhe.entradas.tamanhoMetros.toFixed(2)} m</dd>
            </div>
            <div className="flex items-center justify-between">
              <dt>Distância DSIN</dt>
              <dd>{detalhe.entradas.distanciaKm.toFixed(2)} km</dd>
            </div>
            <div className="flex items-center justify-between">
              <dt>Mut. observadas</dt>
              <dd>{detalhe.entradas.mutacoes}</dd>
            </div>
            <div className="flex items-center justify-between">
              <dt>Estado</dt>
              <dd className="capitalize">{detalhe.entradas.estado}</dd>
            </div>
            {detalhe.entradas.bpm && (
              <div className="flex items-center justify-between">
                <dt>Batimentos</dt>
                <dd>{detalhe.entradas.bpm} bpm</dd>
              </div>
            )}
          </dl>
        </div>

        <div className="rounded-xl border border-slate-700/60 bg-night-900/70 p-4">
          <h3 className="text-sm font-semibold uppercase tracking-wide text-slate-300">Componentes</h3>
          <dl className="mt-4 grid gap-2 text-sm text-slate-200">
            <div className="flex items-center justify-between">
              <dt>Custo base</dt>
              <dd>₢ {detalhe.componentes.custoBase.toFixed(2)}</dd>
            </div>
            <div className="flex items-center justify-between">
              <dt>Custo por massa</dt>
              <dd>₢ {detalhe.componentes.custoPorMassa.toFixed(2)}</dd>
            </div>
            <div className="flex items-center justify-between">
              <dt>Custo por distância</dt>
              <dd>₢ {detalhe.componentes.custoPorDistancia.toFixed(2)}</dd>
            </div>
            <div className="flex items-center justify-between">
              <dt>Custo por tamanho</dt>
              <dd>₢ {detalhe.componentes.custoPorTamanho.toFixed(2)}</dd>
            </div>
            <div className="flex items-center justify-between">
              <dt>Risco estado</dt>
              <dd>{detalhe.componentes.riscoEstado}</dd>
            </div>
            <div className="flex items-center justify-between">
              <dt>Risco poder</dt>
              <dd>{detalhe.componentes.riscoPoder}</dd>
            </div>
            <div className="flex items-center justify-between">
              <dt>Risco BPM</dt>
              <dd>{detalhe.componentes.riscoBpm}</dd>
            </div>
            <div className="flex items-center justify-between">
              <dt>Risco mutações</dt>
              <dd>{detalhe.componentes.riscoMutacoes}</dd>
            </div>
            <div className="flex items-center justify-between">
              <dt>Rareza poder</dt>
              <dd>{detalhe.componentes.rarezaPoder}</dd>
            </div>
            <div className="flex items-center justify-between">
              <dt>Valor base</dt>
              <dd>{detalhe.componentes.valorCientificoBase}</dd>
            </div>
          </dl>
        </div>
      </section>

      <section className="rounded-xl border border-slate-700/60 bg-night-900/70 p-4">
        <h3 className="text-sm font-semibold uppercase tracking-wide text-slate-300">Sensibilidade dos pesos</h3>
        <p className="mt-2 text-xs text-slate-400">
          Ajuste os pesos locais para simular novas prioridades sem alterar os parâmetros oficiais.
        </p>

        <div className="mt-4 grid gap-4 md:grid-cols-3">
          <SliderCard
            titulo="Peso do valor científico"
            valor={pesos.valor}
            min={0}
            max={1}
            step={0.01}
            onChange={(value) => atualizarPeso('valor', value)}
          />
          <SliderCard
            titulo="Peso do custo"
            valor={pesos.custo}
            min={-1}
            max={0}
            step={0.01}
            onChange={(value) => atualizarPeso('custo', value)}
          />
          <SliderCard
            titulo="Peso do risco"
            valor={pesos.risco}
            min={-1}
            max={0}
            step={0.01}
            onChange={(value) => atualizarPeso('risco', value)}
          />
        </div>

        {simulacao && (
          <div className="mt-6 grid gap-3 rounded-lg border border-emerald-400/30 bg-emerald-500/10 p-4 text-sm text-emerald-100 md:grid-cols-3">
            <div>
              <div className="text-xs uppercase tracking-wide text-emerald-300">Prioridade simulada</div>
              <div className="text-2xl font-semibold">{simulacao.prioridade.toFixed(2)}</div>
            </div>
            <div>
              <div className="text-xs uppercase tracking-wide text-emerald-300">Classe prevista</div>
              <div className="text-lg font-semibold">{simulacao.classe}</div>
            </div>
            <div>
              <div className="text-xs uppercase tracking-wide text-emerald-300">Diferença</div>
              <div className="text-lg font-semibold">
                {simulacao.diferenca >= 0 ? '+' : ''}
                {simulacao.diferenca.toFixed(2)}
              </div>
            </div>
          </div>
        )}
      </section>
    </div>
  )
}

interface ScoreCardProps {
  titulo: string
  children: ReactNode
  destaque?: boolean
}

function ScoreCard({ titulo, children, destaque }: ScoreCardProps) {
  return (
    <div className={`rounded-xl border ${destaque ? 'border-emerald-400/50 bg-emerald-500/10' : 'border-slate-700/60 bg-night-900/70'} p-4`}>
      <div className="text-xs font-semibold uppercase tracking-wide text-slate-400">{titulo}</div>
      <div className={`mt-2 text-lg ${destaque ? 'font-semibold text-emerald-100' : 'text-slate-100'}`}>{children}</div>
    </div>
  )
}

interface SliderCardProps {
  titulo: string
  valor: number
  min: number
  max: number
  step: number
  onChange: (value: number) => void
}

function SliderCard({ titulo, valor, min, max, step, onChange }: SliderCardProps) {
  return (
    <div className="rounded-xl border border-slate-700/60 bg-night-900/70 p-4">
      <div className="text-xs font-semibold uppercase tracking-wide text-slate-300">{titulo}</div>
      <div className="mt-2 text-sm text-slate-200">{valor.toFixed(2)}</div>
      <input
        type="range"
        min={min}
        max={max}
        step={step}
        value={valor}
        onChange={(event) => onChange(Number(event.target.value))}
        className="mt-3 w-full"
      />
    </div>
  )
}
