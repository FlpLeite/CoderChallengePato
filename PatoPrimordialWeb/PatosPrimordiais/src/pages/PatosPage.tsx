import { useEffect, useMemo, useState } from 'react'
import {
  fetchPatos,
  fetchPatoDetalhe,
  fetchPatoHistorico,
} from '../api/client'
import type { AvistamentoDto, PagedResult, PatoDto, PatoListItem, PatosFilters } from '../api/types'
import { ErrorNotice } from '../components/ErrorNotice'
import { LoadingState } from '../components/LoadingState'
import { PatoDetail } from '../components/PatoDetail'
import { PatosList } from '../components/PatosList'

const initialFilters: PatosFilters = {
  estado: '',
  pais: '',
  cidade: '',
  mutacoesMin: undefined,
  mutacoesMax: undefined,
  page: 1,
  pageSize: 10,
}

export function PatosPage() {
  const [filters, setFilters] = useState<PatosFilters>(initialFilters)
  const [patosLoading, setPatosLoading] = useState(true)
  const [detailLoading, setDetailLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [patosPage, setPatosPage] = useState<PagedResult<PatoListItem> | undefined>()
  const [patoSelecionadoId, setPatoSelecionadoId] = useState<string | null>(null)
  const [patoSelecionado, setPatoSelecionado] = useState<PatoDto | null>(null)
  const [historico, setHistorico] = useState<AvistamentoDto[]>([])

  useEffect(() => {
    carregarPatos()
  }, [filters])

  useEffect(() => {
    if (patoSelecionadoId) {
      carregarDetalhe(patoSelecionadoId)
    } else {
      setPatoSelecionado(null)
      setHistorico([])
    }
  }, [patoSelecionadoId])

  const resumoEstados = useMemo(() => patosPage?.resumoPorEstado ?? {}, [patosPage])

  async function carregarPatos() {
    try {
      setPatosLoading(true)
      setError(null)
      const dados = await fetchPatos(filters)
      setPatosPage(dados)
      if (dados.items.length > 0 && !patoSelecionadoId) {
        setPatoSelecionadoId(dados.items[0].id)
      }
    } catch (err) {
      setError((err as Error).message)
    } finally {
      setPatosLoading(false)
    }
  }

  async function carregarDetalhe(id: string) {
    try {
      setDetailLoading(true)
      setError(null)
      const [pato, historicoData] = await Promise.all([fetchPatoDetalhe(id), fetchPatoHistorico(id)])
      setPatoSelecionado(pato)
      setHistorico(historicoData)
    } catch (err) {
      setError((err as Error).message)
    } finally {
      setDetailLoading(false)
    }
  }

  return (
    <div className="space-y-6">
      {error && <ErrorNotice error={error} />}
      <div className="flex flex-wrap gap-3 text-xs text-slate-400">
        {Object.entries(resumoEstados).map(([estado, quantidade]) => (
          <span key={estado} className="rounded-full border border-slate-700/70 px-3 py-1 capitalize text-slate-200">
            {estado}: <span className="font-semibold text-emerald-300">{quantidade}</span>
          </span>
        ))}
      </div>
      <div className="grid gap-6 lg:grid-cols-2">
        <div>
          <PatosList
            data={patosPage}
            filters={filters}
            loading={patosLoading}
            onChangeFilters={setFilters}
            onSelect={(id) => setPatoSelecionadoId(id)}
          />
        </div>
        <div>
          {detailLoading ? (
            <LoadingState message="Carregando pato..." />
          ) : (
            <PatoDetail pato={patoSelecionado} historico={historico} onClose={() => setPatoSelecionadoId(null)} />
          )}
        </div>
      </div>
    </div>
  )
}
