export interface PagedResult<T> {
  page: number
  pageSize: number
  totalCount: number
  items: T[]
  resumoPorEstado?: Record<string, number>
}

export interface PatoListItem {
  id: string
  codigo: string
  cidade: string
  pais: string
  estado: string
  mutacoesQtde?: number
  precisaoM: number
}

export interface PatoDto {
  id: string
  codigo: string
  alturaCm: number
  pesoG: number
  cidade: string
  pais: string
  latitude: number
  longitude: number
  precisaoM: number
  estado: string
  bpm?: number
  mutacoesQtde?: number
  poderNome?: string
  poderDescricao?: string
  poderTags: string[]
  pontoReferenciaId?: string
  pontoReferenciaNome?: string
  criadoEm: string
  atualizadoEm: string
}

export interface AvistamentoDto {
  id: string
  droneId: string
  droneNumeroSerie: string
  alturaValor: number
  alturaUnidade: string
  pesoValor: number
  pesoUnidade: string
  precisaoValor: number
  precisaoUnidade: string
  precisaoM: number
  latitude: number
  longitude: number
  cidade: string
  pais: string
  estadoPato: string
  bpm?: number
  mutacoesQtde?: number
  criadoEm: string
}

export interface PatosKpiDto {
  total: number
  porEstado: Record<string, number>
}

export interface DroneSaudeDto {
  contagemPorStatus: Record<string, number>
  drones: DroneSaudeItemDto[]
}

export interface DroneSaudeItemDto {
  id: string
  numeroSerie: string
  status: string
  ultimoContatoEm?: string
  ultimoAvistamento?: {
    id: string
    criadoEm: string
    estadoPato: string
    precisaoM: number
  }
}

export interface PontoReferenciaDto {
  id: string
  nome: string
  latitude: number
  longitude: number
  raioMetros: number
  distanciaMetros?: number | null
}

export interface CriarPontoReferenciaPayload {
  nome: string
  latitude: number
  longitude: number
  raioMetros: number
}

export interface PatosFilters {
  estado: string
  pais: string
  cidade: string
  mutacoesMin?: number
  mutacoesMax?: number
  page: number
  pageSize: number
}

export interface AnaliseRankingFilters {
  ordem?: string
  estado?: string
  risco?: string
  pais?: string
  page: number
  pageSize: number
}

export interface AnaliseRankingItem {
  patoId: string
  codigo: string
  estado: string
  pais: string
  cidade: string
  prioridade: number
  classePrioridade: string
  riscoTotal: number
  classeRisco: string
  valorCientifico: number
  custoTransporte: number
  poderioNecessario: number
  distKm: number
  calculadoEm: string
  poderTags: string[]
}

export interface AnaliseRankingResult {
  page: number
  pageSize: number
  totalCount: number
  items: AnaliseRankingItem[]
  resumoPorEstado?: Record<string, number>
  resumoPorRisco?: Record<string, number>
}

export interface AnaliseScoresDto {
  custoTransporte: number
  riscoTotal: number
  valorCientifico: number
  poderioNecessario: number
  distKm: number
  prioridade: number
  classePrioridade: string
  classeRisco: string
}

export interface AnaliseDetalheEntradasDto {
  massaToneladas: number
  tamanhoMetros: number
  distanciaKm: number
  mutacoes: number
  bpm?: number
  estado: string
}

export interface AnaliseDetalheComponentesDto {
  custoBase: number
  custoPorMassa: number
  custoPorDistancia: number
  custoPorTamanho: number
  riscoEstado: number
  riscoPoder: number
  riscoBpm: number
  riscoMutacoes: number
  rarezaPoder: number
  valorCientificoBase: number
}

export interface AnaliseParametrosDto {
  custoBase: number
  custoPorTonelada: number
  custoPorKm: number
  custoPorMetro: number
  pesoValorCientifico: number
  pesoCusto: number
  pesoRisco: number
  dsinLat: number
  dsinLon: number
}

export interface AnaliseDetalheDto {
  patoId: string
  codigo: string
  poderNome?: string
  poderTags: string[]
  scores: AnaliseScoresDto
  entradas: AnaliseDetalheEntradasDto
  componentes: AnaliseDetalheComponentesDto
  parametros: AnaliseParametrosDto
  calculadoEm: string
}

export interface ParametroAnaliseDto {
  id: string
  chave: string
  valorNum?: number
  valorTxt?: string
  atualizadoEm: string
}

export type MissaoStatus = 'planejada' | 'em_execucao' | 'abortada' | 'concluida'

export interface TelemetriaDto {
  bateria: number
  combustivel: number
  integridade: number
  distanciaM: number
}

export interface TaticaPlanoDto {
  nome: string
  descricao: string
  prioridade: number
  bonusFraqueza: number
  explorandoFraqueza: boolean
}

export interface PlanoEstrategiaDto {
  porte: string
  classeRisco: string
  taticas: TaticaPlanoDto[]
}

export interface DefesaSelecionadaDto {
  nome: string
  contramedida: string
  tagsAmeaca: string[]
  rareza: number
  mitigacao: number
}

export interface PlanoDefesaDto {
  primaria?: DefesaSelecionadaDto
  fallback?: DefesaSelecionadaDto
}

export interface FraquezaAplicadaDto {
  id: string
  nome: string
  descricao: string
  bonusSucesso: number
}

export interface MissaoDto {
  id: string
  patoId: string
  status: MissaoStatus
  poderioAlocado: number
  criadoEm: string
  iniciadoEm?: string
  finalizadoEm?: string
  telemetria: TelemetriaDto
  faseAtual: string
  proximaFase?: string
  estrategia: PlanoEstrategiaDto
  defesas: PlanoDefesaDto
  fraquezas: FraquezaAplicadaDto[]
  resultado?: string
}

export interface MissaoListItemDto {
  id: string
  patoId: string
  status: MissaoStatus
  poderioAlocado: number
  criadoEm: string
  iniciadoEm?: string
  finalizadoEm?: string
  resultado?: string
}

export interface MissaoTickDto {
  id: string
  tick: number
  fase: string
  evento: string
  detalhe?: unknown
  bateriaPct: number
  combustivelPct: number
  integridadePct: number
  distanciaM: number
  sucesso: boolean
}

export interface MissaoTimelineDto {
  page: number
  pageSize: number
  totalCount: number
  items: MissaoTickDto[]
}

export interface RegraTaticaDto {
  id: string
  nome: string
  descricao: string
  condicao: Record<string, unknown>
  acao: Record<string, unknown>
  prioridade: number
}

export interface RegraDefesaDto {
  id: string
  nome: string
  tagsAmeaca: string[]
  contramedida: string
  rareza: number
  mitigacao: number
  fallback: boolean
}

export interface FraquezaCatalogoDto {
  id: string
  nome: string
  condicao: Record<string, unknown>
  efeito: { bonusSucesso: number; descricao: string }
}

export interface CriarDronePayload {
  numeroSerie: string
  status?: string
  fabricanteNome?: string
  marca?: string
  paisOrigem?: string
  precisaoNominalMinCm?: number
  precisaoNominalMaxM?: number
}

export interface DroneAvistamentoPayload {
  drone_id?: number
  numero_serie: string
  fabricante?: string
  marca?: string
  pais_origem?: string
  altura_valor: number
  altura_unidade: string
  peso_valor: number
  peso_unidade: string
  lat: number
  lon: number
  precisao_valor: number
  precisao_unidade: string
  cidade: string
  pais: string
  estado_pato: string
  bpm?: number
  mutacoes_qtde?: number
  poder?: {
    nome?: string
    descricao?: string
    tags?: string[]
  }
}
