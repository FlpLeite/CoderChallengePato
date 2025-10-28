import { type FormEvent, useEffect, useMemo, useState } from 'react'
import { criarDrone, fetchDroneSaude, registrarDroneAvistamento } from '../api/client'
import type { DroneAvistamentoPayload, DroneSaudeDto, DroneSaudeItemDto } from '../api/types'
import { DroneHealthPanel } from '../components/DroneHealthPanel'
import { ErrorNotice } from '../components/ErrorNotice'
import { LoadingState } from '../components/LoadingState'

export function DronesPage() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [dados, setDados] = useState<DroneSaudeDto | undefined>()
  const [criando, setCriando] = useState(false)
  const [registrandoAvistamento, setRegistrandoAvistamento] = useState(false)
  const [formErro, setFormErro] = useState<string | null>(null)
  const [avistamentoErro, setAvistamentoErro] = useState<string | null>(null)
  const [mensagemCriacao, setMensagemCriacao] = useState<string | null>(null)
  const [mensagemAvistamento, setMensagemAvistamento] = useState<string | null>(null)
  const [ultimoAvistamentoGerado, setUltimoAvistamentoGerado] = useState<{
    payload: DroneAvistamentoPayload
    patoCodigo: string
  } | null>(null)

  const droneStatusOptions = useMemo(
    () => [
      { label: 'Operacional', value: 'operacional' },
      { label: 'Quebrado', value: 'quebrado' },
      { label: 'Indisponivel', value: 'indisponivel' },
    ],
    [],
  )

  interface NovoDroneFormState {
    numeroSerie: string
    status: string
    fabricanteNome: string
    marca: string
    paisOrigem: string
    precisaoNominalMinCm: string
    precisaoNominalMaxM: string
  }

  const [novoDrone, setNovoDrone] = useState<NovoDroneFormState>({
    numeroSerie: '',
    status: 'operacional',
    fabricanteNome: '',
    marca: '',
    paisOrigem: '',
    precisaoNominalMinCm: '',
    precisaoNominalMaxM: '',
  })

  useEffect(() => {
    carregar()
  }, [])

  async function carregar() {
    try {
      setLoading(true)
      setError(null)
      const data = await fetchDroneSaude()
      setDados(data)
    } catch (err) {
      setError((err as Error).message)
    } finally {
      setLoading(false)
    }
  }

  const handleCriarDrone = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setFormErro(null)
    setMensagemCriacao(null)

    if (!novoDrone.numeroSerie.trim()) {
      setFormErro('Informe um número de série para registrar o drone.')
      return
    }

    try {
      setCriando(true)
      const payload = {
        numeroSerie: novoDrone.numeroSerie.trim(),
        status: novoDrone.status.trim() || undefined,
        fabricanteNome: novoDrone.fabricanteNome.trim() || undefined,
        marca: novoDrone.marca.trim() || undefined,
        paisOrigem: novoDrone.paisOrigem.trim() || undefined,
        precisaoNominalMinCm: novoDrone.precisaoNominalMinCm
          ? Number(novoDrone.precisaoNominalMinCm)
          : undefined,
        precisaoNominalMaxM: novoDrone.precisaoNominalMaxM
          ? Number(novoDrone.precisaoNominalMaxM)
          : undefined,
      }

      await criarDrone(payload)
      setMensagemCriacao(`Drone ${payload.numeroSerie} registrado com sucesso.`)
      setNovoDrone({
        numeroSerie: '',
        status: 'operacional',
        fabricanteNome: '',
        marca: '',
        paisOrigem: '',
        precisaoNominalMinCm: '',
        precisaoNominalMaxM: '',
      })
      await carregar()
    } catch (err) {
      setFormErro((err as Error).message)
    } finally {
      setCriando(false)
    }
  }

  const gerarNumeroSerie = () => {
    const numero = Math.floor(1000 + Math.random() * 9000)
    const sufixo = Math.floor(100 + Math.random() * 900)
    return `SIM-DRN-${numero}-${sufixo}`
  }

  const locaisPredefinidos = useMemo(
    () => [
      { cidade: 'São Paulo', pais: 'Brasil', lat: -23.5505, lon: -46.6333 },
      { cidade: 'Curitiba', pais: 'Brasil', lat: -25.4284, lon: -49.2733 },
      { cidade: 'Rio de Janeiro', pais: 'Brasil', lat: -22.9068, lon: -43.1729 },
      { cidade: 'Lisboa', pais: 'Portugal', lat: 38.7223, lon: -9.1393 },
      { cidade: 'Porto', pais: 'Portugal', lat: 41.1579, lon: -8.6291 },
      { cidade: 'Miami', pais: 'Estados Unidos', lat: 25.7617, lon: -80.1918 },
      { cidade: 'Toronto', pais: 'Canadá', lat: 43.6532, lon: -79.3832 },
      { cidade: 'Tóquio', pais: 'Japão', lat: 35.6762, lon: 139.6503 },
      { cidade: 'Sydney', pais: 'Austrália', lat: -33.8688, lon: 151.2093 },
      { cidade: 'Cidade do Cabo', pais: 'África do Sul', lat: -33.9249, lon: 18.4241 },
    ],
    [],
  )

  const estadosPato = useMemo(() => ['desperto', 'transe', 'hibernação'], [])

  const poderesObservados = useMemo(
    () => [
      { nome: 'Rajada Sônica', tags: ['som', 'onda'] },
      { nome: 'Câmbio Camaleão', tags: ['furtividade'] },
      { nome: 'Chama Criogênica', tags: ['gelo', 'controle'] },
      { nome: 'Mimetismo Neural', tags: ['psíquico'] },
      { nome: 'Campo Antigravitacional', tags: ['voo', 'campo'] },
      { nome: 'Eco Transdimensional', tags: ['energia'] },
    ],
    [],
  )

  function escolherAleatorio<T>(itens: T[]): T {
    return itens[Math.floor(Math.random() * itens.length)]
  }

  function gerarCoordenada(base: number) {
    const variacao = (Math.random() - 0.5) * 0.2
    return Number((base + variacao).toFixed(4))
  }

  function gerarValor(min: number, max: number, precision = 1) {
    const valor = min + Math.random() * (max - min)
    return Number(valor.toFixed(precision))
  }

  function obterDroneAleatorio(): DroneSaudeItemDto | undefined {
    const dronesDisponiveis = dados?.drones ?? []
    if (!dronesDisponiveis.length) return undefined
    return escolherAleatorio(dronesDisponiveis)
  }

  const gerarAvistamentoAleatorio = (): DroneAvistamentoPayload => {
    const drone = obterDroneAleatorio()
    const local = escolherAleatorio(locaisPredefinidos)
    const incluirPoder = Math.random() > 0.5
    const poderSelecionado = incluirPoder ? escolherAleatorio(poderesObservados) : null
    const estadoPato = escolherAleatorio(estadosPato)
    const bpmObrigatorio = estadoPato === 'transe' || estadoPato === 'hibernação'
    const bpm = bpmObrigatorio
        ? Math.floor(gerarValor(60, 220, 0))
        : Math.random() > 0.3
            ? Math.floor(gerarValor(60, 220, 0))
            : undefined

    return {
      drone_id: drone?.id ? Number(drone.id) : undefined,
      numero_serie: drone?.numeroSerie ?? gerarNumeroSerie(),
      fabricante: undefined,
      marca: undefined,
      pais_origem: undefined,
      altura_valor: gerarValor(0.6, 2.2, 2),
      altura_unidade: 'm',
      peso_valor: gerarValor(3, 25, 1),
      peso_unidade: 'kg',
      lat: gerarCoordenada(local.lat),
      lon: gerarCoordenada(local.lon),
      precisao_valor: gerarValor(1, 50, 1),
      precisao_unidade: 'm',
      cidade: local.cidade,
      pais: local.pais,
      estado_pato: estadoPato,
      bpm,
      mutacoes_qtde: Math.random() > 0.4 ? Math.floor(gerarValor(0, 6, 0)) : undefined,
      poder: poderSelecionado
        ? {
            nome: poderSelecionado.nome,
            tags: poderSelecionado.tags,
          }
        : undefined,
    }
  }

  const handleSimularAvistamento = async () => {
    setAvistamentoErro(null)
    setMensagemAvistamento(null)

    try {
      setRegistrandoAvistamento(true)
      const payload = gerarAvistamentoAleatorio()
      const pato = await registrarDroneAvistamento(payload)
      setUltimoAvistamentoGerado({ payload, patoCodigo: pato.codigo })
      setMensagemAvistamento(
        `Avistamento registrado para o pato ${pato.codigo} em ${payload.cidade}, ${payload.pais}.`,
      )
      await carregar()
    } catch (err) {
      setAvistamentoErro((err as Error).message)
    } finally {
      setRegistrandoAvistamento(false)
    }
  }

  return (
    <div className="space-y-6">
      {error && <ErrorNotice error={error} />}
      {loading && !dados ? (
        <LoadingState message="Carregando drones..." />
      ) : (
        <DroneHealthPanel data={dados} loading={loading} onRefresh={carregar} />
      )}
      <div className="grid gap-6 lg:grid-cols-2">
        <section className="space-y-4 rounded-xl border border-slate-800 bg-night-900/70 p-6">
          <header>
            <h3 className="text-lg font-semibold text-emerald-200">Cadastrar drone</h3>
            <p className="text-sm text-slate-400">
              Registre rapidamente um novo drone operacional antes de iniciar missões ou avistamentos.
            </p>
          </header>
          {formErro ? <ErrorNotice error={formErro} /> : null}
          {mensagemCriacao ? (
            <div className="rounded border border-emerald-500/40 bg-emerald-500/10 px-3 py-2 text-sm text-emerald-200">
              {mensagemCriacao}
            </div>
          ) : null}
          <form className="space-y-4" onSubmit={handleCriarDrone}>
            <div className="space-y-2">
              <label className="block text-sm font-medium text-slate-200" htmlFor="numeroSerie">
                Número de série
              </label>
              <input
                id="numeroSerie"
                type="text"
                value={novoDrone.numeroSerie}
                onChange={(event) => setNovoDrone((prev) => ({ ...prev, numeroSerie: event.target.value }))}
                className="w-full rounded border border-slate-700 bg-night-900 px-3 py-2 text-sm text-slate-100 focus:border-emerald-400 focus:outline-none"
                placeholder="EX-DRN-2048"
              />
            </div>
            <div className="grid gap-3 sm:grid-cols-2">
              <div className="space-y-2">
                <label className="block text-sm font-medium text-slate-200" htmlFor="status">
                  Status
                </label>
                <select
                  id="status"
                  value={novoDrone.status}
                  onChange={(event) => setNovoDrone((prev) => ({ ...prev, status: event.target.value }))}
                  className="w-full rounded border border-slate-700 bg-night-900 px-3 py-2 text-sm text-slate-100 focus:border-emerald-400 focus:outline-none"
                >
                  {droneStatusOptions.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </select>
              </div>
              <div className="space-y-2">
                <label className="block text-sm font-medium text-slate-200" htmlFor="paisOrigem">
                  País de origem do fabricante
                </label>
                <input
                  id="paisOrigem"
                  type="text"
                  value={novoDrone.paisOrigem}
                  onChange={(event) => setNovoDrone((prev) => ({ ...prev, paisOrigem: event.target.value }))}
                  className="w-full rounded border border-slate-700 bg-night-900 px-3 py-2 text-sm text-slate-100 focus:border-emerald-400 focus:outline-none"
                  placeholder="Brasil"
                />
              </div>
            </div>
            <div className="grid gap-3 sm:grid-cols-2">
              <div className="space-y-2">
                <label className="block text-sm font-medium text-slate-200" htmlFor="fabricanteNome">
                  Fabricante
                </label>
                <input
                  id="fabricanteNome"
                  type="text"
                  value={novoDrone.fabricanteNome}
                  onChange={(event) => setNovoDrone((prev) => ({ ...prev, fabricanteNome: event.target.value }))}
                  className="w-full rounded border border-slate-700 bg-night-900 px-3 py-2 text-sm text-slate-100 focus:border-emerald-400 focus:outline-none"
                  placeholder="Aurora Dynamics"
                />
              </div>
              <div className="space-y-2">
                <label className="block text-sm font-medium text-slate-200" htmlFor="marca">
                  Marca
                </label>
                <input
                  id="marca"
                  type="text"
                  value={novoDrone.marca}
                  onChange={(event) => setNovoDrone((prev) => ({ ...prev, marca: event.target.value }))}
                  className="w-full rounded border border-slate-700 bg-night-900 px-3 py-2 text-sm text-slate-100 focus:border-emerald-400 focus:outline-none"
                  placeholder="Falcão"
                />
              </div>
            </div>
            <div className="grid gap-3 sm:grid-cols-2">
              <div className="space-y-2">
                <label className="block text-sm font-medium text-slate-200" htmlFor="precisaoMin">
                  Precisão nominal mínima (cm)
                </label>
                <input
                  id="precisaoMin"
                  type="number"
                  value={novoDrone.precisaoNominalMinCm}
                  onChange={(event) => setNovoDrone((prev) => ({ ...prev, precisaoNominalMinCm: event.target.value }))}
                  className="w-full rounded border border-slate-700 bg-night-900 px-3 py-2 text-sm text-slate-100 focus:border-emerald-400 focus:outline-none"
                  min="0"
                />
              </div>
              <div className="space-y-2">
                <label className="block text-sm font-medium text-slate-200" htmlFor="precisaoMax">
                  Precisão nominal máxima (m)
                </label>
                <input
                  id="precisaoMax"
                  type="number"
                  value={novoDrone.precisaoNominalMaxM}
                  onChange={(event) => setNovoDrone((prev) => ({ ...prev, precisaoNominalMaxM: event.target.value }))}
                  className="w-full rounded border border-slate-700 bg-night-900 px-3 py-2 text-sm text-slate-100 focus:border-emerald-400 focus:outline-none"
                  min="0"
                />
              </div>
            </div>
            <button
              type="submit"
              className="w-full rounded-full bg-emerald-500 px-4 py-2 text-sm font-semibold text-night-900 transition hover:bg-emerald-400 disabled:cursor-not-allowed disabled:opacity-60"
              disabled={criando}
            >
              {criando ? 'Registrando...' : 'Registrar drone'}
            </button>
          </form>
        </section>

        <section className="space-y-4 rounded-xl border border-slate-800 bg-night-900/70 p-6">
          <header>
            <h3 className="text-lg font-semibold text-emerald-200">Simular avistamento</h3>
            <p className="text-sm text-slate-400">
              Deixe o sistema gerar um novo avistamento automático para popular o mapa de patos e atualizar a
              telemetria dos drones.
            </p>
          </header>
          {avistamentoErro ? <ErrorNotice error={avistamentoErro} /> : null}
          {mensagemAvistamento ? (
            <div className="rounded border border-emerald-500/40 bg-emerald-500/10 px-3 py-2 text-sm text-emerald-200">
              {mensagemAvistamento}
            </div>
          ) : null}
          <div className="space-y-4">
            <p className="text-sm text-slate-300">
              Clique no botão abaixo para que o sistema gere automaticamente um conjunto de dados de
              avistamento com base em drones disponíveis e locais pré-definidos. O registro será
              enviado imediatamente para a central e gravado na tabela de patos.
            </p>
            <button
              type="button"
              onClick={handleSimularAvistamento}
              className="w-full rounded-full bg-indigo-500 px-4 py-2 text-sm font-semibold text-night-900 transition hover:bg-indigo-400 disabled:cursor-not-allowed disabled:opacity-60"
              disabled={registrandoAvistamento}
            >
              {registrandoAvistamento ? 'Gerando avistamento...' : 'Simular avistamento automático'}
            </button>
            {ultimoAvistamentoGerado ? (
              <div className="space-y-2 rounded border border-slate-700/70 bg-night-900/70 p-4 text-sm text-slate-200">
                <p className="font-semibold text-indigo-200">Último cenário gerado</p>
                <dl className="grid grid-cols-1 gap-1 text-xs sm:grid-cols-2">
                  <div>
                    <dt className="uppercase tracking-wide text-slate-400">Pato</dt>
                    <dd className="text-slate-100">{ultimoAvistamentoGerado.patoCodigo}</dd>
                  </div>
                  <div>
                    <dt className="uppercase tracking-wide text-slate-400">Drone</dt>
                    <dd className="text-slate-100">{ultimoAvistamentoGerado.payload.numero_serie}</dd>
                  </div>
                  <div>
                    <dt className="uppercase tracking-wide text-slate-400">Localização</dt>
                    <dd className="text-slate-100">
                      {ultimoAvistamentoGerado.payload.cidade}, {ultimoAvistamentoGerado.payload.pais}
                    </dd>
                  </div>
                  <div>
                    <dt className="uppercase tracking-wide text-slate-400">Coordenadas</dt>
                    <dd className="text-slate-100">
                      {ultimoAvistamentoGerado.payload.lat.toFixed(4)}, {ultimoAvistamentoGerado.payload.lon.toFixed(4)}
                    </dd>
                  </div>
                  <div>
                    <dt className="uppercase tracking-wide text-slate-400">Estado do pato</dt>
                    <dd className="text-slate-100">{ultimoAvistamentoGerado.payload.estado_pato}</dd>
                  </div>
                  <div>
                    <dt className="uppercase tracking-wide text-slate-400">Precisão do sensor</dt>
                    <dd className="text-slate-100">
                      {ultimoAvistamentoGerado.payload.precisao_valor} {ultimoAvistamentoGerado.payload.precisao_unidade}
                    </dd>
                  </div>
                  {ultimoAvistamentoGerado.payload.poder?.nome ? (
                    <div className="sm:col-span-2">
                      <dt className="uppercase tracking-wide text-slate-400">Poder detectado</dt>
                      <dd className="text-slate-100">
                        {ultimoAvistamentoGerado.payload.poder.nome}
                        {ultimoAvistamentoGerado.payload.poder.tags?.length ? (
                          <span className="text-slate-400">
                            {' '}
                            ({ultimoAvistamentoGerado.payload.poder.tags.join(', ')})
                          </span>
                        ) : null}
                      </dd>
                    </div>
                  ) : null}
                </dl>
              </div>
            ) : null}
          </div>
        </section>
      </div>
    </div>
  )
}
