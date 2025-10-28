import type {
  AnaliseDetalheDto,
  AnaliseRankingFilters,
  AnaliseRankingResult,
  AvistamentoDto,
  CriarDronePayload,
  CriarPontoReferenciaPayload,
  DroneSaudeDto,
  DroneSaudeItemDto,
  FraquezaCatalogoDto,
  DroneAvistamentoPayload,
  MissaoDto,
  MissaoListItemDto,
  MissaoTimelineDto,
  PagedResult,
  PatosKpiDto,
  PatoDto,
  PatoListItem,
  PatosFilters,
  PontoReferenciaDto,
  ParametroAnaliseDto,
  RegraDefesaDto,
  RegraTaticaDto,
} from './types'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5099'

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const url = `${API_BASE_URL}${path}`
  let response: Response

  try {
    response = await fetch(url, {
      ...init,
      headers: {
        'Content-Type': 'application/json',
        ...(init?.headers ?? {}),
      },
    })
  } catch (error) {
    if (error instanceof TypeError) {
      throw new Error('Não foi possível conectar com a API. Verifique se o serviço está em execução.')
    }

    throw error
  }

  if (!response.ok) {
    const message = await response.text()
    throw new Error(message || `Falha ao carregar ${path}`)
  }

  return (await response.json()) as T
}

function buildQuery(params: Record<string, string | number | undefined | null>) {
  const entries = Object.entries(params).filter(([, value]) => value !== undefined && value !== null && value !== '')
  if (entries.length === 0) {
    return ''
  }

  const search = new URLSearchParams()
  for (const [key, value] of entries) {
    search.append(key, String(value))
  }

  return `?${search.toString()}`
}

export async function fetchPatos(filters: PatosFilters) {
  const query = buildQuery({
    estado: filters.estado,
    pais: filters.pais,
    cidade: filters.cidade,
    mutMin: filters.mutacoesMin,
    mutMax: filters.mutacoesMax,
    page: filters.page,
    pageSize: filters.pageSize,
  })

  return await request<PagedResult<PatoListItem>>(`/api/patos${query}`)
}

export async function fetchPatosKpis() {
  return await request<PatosKpiDto>('/api/patos/kpis')
}

export async function fetchPatoDetalhe(id: string) {
  return await request<PatoDto>(`/api/patos/${id}`)
}

export async function fetchPatoHistorico(id: string) {
  return await request<AvistamentoDto[]>(`/api/patos/${id}/historico`)
}

export async function fetchAvistamentosRecentes() {
  return await request<AvistamentoDto[]>('/api/avistamentos/recentes')
}

export async function fetchDroneSaude() {
  return await request<DroneSaudeDto>('/api/drones/saude')
}

export async function criarDrone(payload: CriarDronePayload) {
  return await request<DroneSaudeItemDto>('/api/drones', {
    method: 'POST',
    body: JSON.stringify(payload),
  })
}

export async function registrarDroneAvistamento(payload: DroneAvistamentoPayload) {
  return await request<PatoDto>('/api/ingestao/drone-avistamento', {
    method: 'POST',
    body: JSON.stringify(payload),
  })
}

export async function fetchPontosReferencia(lat: number, lon: number) {
  const query = buildQuery({ lat, lon })
  return await request<PontoReferenciaDto[]>(`/api/pontos-referencia/near${query}`)
}

export async function fetchTodosPontosReferencia() {
  return await request<PontoReferenciaDto[]>(`/api/pontos-referencia`)
}

export async function criarPontoReferencia(payload: CriarPontoReferenciaPayload) {
  return await request<PontoReferenciaDto>('/api/pontos-referencia', {
    method: 'POST',
    body: JSON.stringify(payload),
  })
}

export async function fetchAnaliseRanking(filters: AnaliseRankingFilters) {
  const query = buildQuery({
    ordem: filters.ordem,
    estado: filters.estado,
    risco: filters.risco,
    pais: filters.pais,
    page: filters.page,
    pageSize: filters.pageSize,
  })

  return await request<AnaliseRankingResult>(`/api/analise/patos${query}`)
}

export async function fetchAnaliseDetalhe(patoId: string) {
  return await request<AnaliseDetalheDto>(`/api/analise/patos/${patoId}`)
}

export async function fetchAnaliseParametros() {
  return await request<ParametroAnaliseDto[]>(`/api/analise/parametros`)
}

export async function recalcularAnalise() {
  return await request<{ atualizados: number }>(`/api/analise/recalcular`, { method: 'POST' })
}

export async function criarMissao(patoId: string) {
  return await request<MissaoDto>(`/api/missoes`, {
    method: 'POST',
    body: JSON.stringify({ patoId }),
  })
}

export async function listarMissoes() {
  return await request<MissaoListItemDto[]>(`/api/missoes`)
}

export async function obterMissao(id: string) {
  return await request<MissaoDto>(`/api/missoes/${id}`)
}

export async function iniciarMissao(id: string) {
  return await request<MissaoDto>(`/api/missoes/${id}/iniciar`, { method: 'POST' })
}

export async function abortarMissao(id: string) {
  return await request<MissaoDto>(`/api/missoes/${id}/abortar`, { method: 'POST' })
}

export async function timelineMissao(id: string, page: number, size: number) {
  const query = buildQuery({ page, size })
  return await request<MissaoTimelineDto>(`/api/missoes/${id}/timeline${query}`)
}

export async function obterRegrasTaticas() {
  return await request<RegraTaticaDto[]>(`/api/regras/taticas`)
}

export async function obterRegrasDefesas() {
  return await request<RegraDefesaDto[]>(`/api/regras/defesas`)
}

export async function obterFraquezasCatalogo() {
  return await request<FraquezaCatalogoDto[]>(`/api/catalogos/fraquezas`)
}
