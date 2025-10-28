import { useEffect, useMemo, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { abortarMissao, obterMissao, timelineMissao } from '../api/client'
import type { MissaoDto, MissaoTickDto } from '../api/types'
import { MissionTelemetry } from '../components/MissionTelemetry'
import { MissionStrategyPanel } from '../components/MissionStrategyPanel'
import { MissionTimeline } from '../components/MissionTimeline'
import { LoadingState } from '../components/LoadingState'
import { ErrorNotice } from '../components/ErrorNotice'

export function MissaoDetalhePage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [missao, setMissao] = useState<MissaoDto | null>(null)
  const [erro, setErro] = useState<string | null>(null)
  const [carregando, setCarregando] = useState(true)
  const [timeline, setTimeline] = useState<MissaoTickDto[]>([])
  const [timelinePage, setTimelinePage] = useState(1)
  const [timelineTotal, setTimelineTotal] = useState(0)
  const [timelineLoading, setTimelineLoading] = useState(false)

  const podeAbortar = useMemo(() => missao?.status === 'em_execucao', [missao])
  const podeReiniciar = useMemo(() => missao && missao.status !== 'em_execucao', [missao])

  const carregarTimeline = async (missaoId: string, page: number, reset = false) => {
    try {
      setTimelineLoading(true)
      const data = await timelineMissao(missaoId, page, 100)
      setTimelineTotal(data.totalCount)
      setTimelinePage(page)
      setTimeline((prev) => (reset ? data.items : [...prev, ...data.items]))
    } catch (error) {
      setErro((error as Error).message)
    } finally {
      setTimelineLoading(false)
    }
  }

  useEffect(() => {
    if (!id) return

    const missaoId = id

    async function carregar() {
      try {
        setCarregando(true)
        const data = await obterMissao(missaoId)
        setMissao(data)
        await carregarTimeline(missaoId, 1, true)
      } catch (error) {
        setErro((error as Error).message)
      } finally {
        setCarregando(false)
      }
    }

    carregar()
  }, [id])

  useEffect(() => {
    if (!id || missao?.status !== 'em_execucao') return
    const missaoId = id
    const interval = window.setInterval(async () => {
      try {
        const data = await obterMissao(missaoId)
        setMissao(data)
        await carregarTimeline(missaoId, 1, true)
      } catch (error) {
        setErro((error as Error).message)
      }
    }, 2000)

    return () => window.clearInterval(interval)
  }, [id, missao?.status])

  const handleAbortar = async () => {
    if (!missao) return
    try {
      const data = await abortarMissao(missao.id)
      setMissao(data)
    } catch (error) {
      setErro((error as Error).message)
    }
  }

  if (carregando && !missao) {
    return <LoadingState message="Carregando missão" />
  }

  if (erro && !missao) {
    return <ErrorNotice error={erro} />
  }

  if (!missao || !id) {
    return (
      <div className="rounded-2xl border border-slate-800/70 bg-night-900/70 p-8 text-center text-sm text-slate-400">
        Missão não encontrada.
      </div>
    )
  }

  const hasMore = timeline.length < timelineTotal

  return (
    <div className="space-y-6">
      <header className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h2 className="text-2xl font-semibold text-emerald-200">
            Missão #{String(missao.id).slice(0, 8)}
          </h2>
          <p className="text-sm text-slate-400">
            Status atual: <span className="font-semibold text-slate-100">{missao.status.replace('_', ' ')}</span>{' '}
            {missao.resultado ? `· ${missao.resultado}` : null}
          </p>
        </div>
        <div className="flex flex-wrap gap-2">
          <button
            type="button"
            className="rounded-full bg-night-800 px-4 py-2 text-sm font-medium text-slate-300 transition hover:bg-night-700"
            onClick={() => navigate('/missoes')}
          >
            Voltar
          </button>
          {podeAbortar ? (
            <button
              type="button"
              className="rounded-full bg-rose-500 px-4 py-2 text-sm font-semibold text-night-900 transition hover:bg-rose-400"
              onClick={handleAbortar}
            >
              Abortar missão
            </button>
          ) : null}
          {podeReiniciar ? (
            <button
              type="button"
              className="rounded-full bg-emerald-500 px-4 py-2 text-sm font-semibold text-night-900 transition hover:bg-emerald-400"
              onClick={() => navigate(`/missoes/nova?patoId=${missao.patoId}`)}
            >
              Reiniciar
            </button>
          ) : null}
        </div>
      </header>

      {erro ? <ErrorNotice error={erro} /> : null}

      <MissionTelemetry telemetria={missao.telemetria} faseAtual={missao.faseAtual} proximaFase={missao.proximaFase} />
      <MissionStrategyPanel estrategia={missao.estrategia} defesas={missao.defesas} fraquezas={missao.fraquezas} />

      <MissionTimeline
        ticks={timeline}
        hasMore={hasMore}
        carregando={timelineLoading}
        onLoadMore={() => carregarTimeline(missao.id, timelinePage + 1)}
      />
    </div>
  )
}
