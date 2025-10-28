import { useEffect, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { criarMissao, iniciarMissao } from '../api/client'
import type { MissaoDto } from '../api/types'
import { MissionStrategyPanel } from '../components/MissionStrategyPanel'
import { MissionTelemetry } from '../components/MissionTelemetry'
import { LoadingState } from '../components/LoadingState'
import { ErrorNotice } from '../components/ErrorNotice'

export function MissaoNovaPage() {
  const [searchParams] = useSearchParams()
  const patoId = searchParams.get('patoId') ?? ''
  const [missao, setMissao] = useState<MissaoDto | null>(null)
  const [erro, setErro] = useState<string | null>(null)
  const [carregando, setCarregando] = useState(false)
  const navigate = useNavigate()

  useEffect(() => {
    async function gerar() {
      if (!patoId) {
        setErro('Informe um patoId na URL para gerar a estratégia.')
        return
      }
      try {
        setCarregando(true)
        const nova = await criarMissao(patoId)
        setMissao(nova)
        setErro(null)
      } catch (error) {
        setErro((error as Error).message)
      } finally {
        setCarregando(false)
      }
    }

    gerar()
  }, [patoId])

  const handleIniciar = async () => {
    if (!missao) return
    try {
      setCarregando(true)
      const atualizada = await iniciarMissao(missao.id)
      setMissao(atualizada)
      navigate(`/missoes/${missao.id}`)
    } catch (error) {
      setErro((error as Error).message)
    } finally {
      setCarregando(false)
    }
  }

  return (
    <div className="space-y-6">
      <header className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h2 className="text-2xl font-semibold text-emerald-200">Pré-visualização da missão</h2>
          <p className="text-sm text-slate-400">Reveja a estratégia proposta antes de iniciar a simulação.</p>
        </div>
        <div className="flex gap-2">
          <button
            type="button"
            className="rounded-full bg-night-800 px-4 py-2 text-sm font-medium text-slate-300 transition hover:bg-night-700"
            onClick={() => navigate('/missoes')}
          >
            Voltar
          </button>
          <button
            type="button"
            className="rounded-full bg-emerald-500 px-4 py-2 text-sm font-semibold text-night-900 transition hover:bg-emerald-400"
            onClick={handleIniciar}
            disabled={!missao || carregando}
          >
            Iniciar missão
          </button>
        </div>
      </header>

      {erro ? <ErrorNotice error={erro} /> : null}
      {carregando && !missao ? <LoadingState message="Gerando estratégia" /> : null}

      {missao ? (
        <div className="space-y-6">
          <MissionTelemetry telemetria={missao.telemetria} faseAtual={missao.faseAtual} proximaFase={missao.proximaFase} />
          <MissionStrategyPanel estrategia={missao.estrategia} defesas={missao.defesas} fraquezas={missao.fraquezas} />
        </div>
      ) : null}
    </div>
  )
}
