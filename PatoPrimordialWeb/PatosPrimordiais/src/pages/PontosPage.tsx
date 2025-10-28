import { type ChangeEvent, type FormEvent, useCallback, useEffect, useState } from 'react'
import { criarPontoReferencia, fetchPontosReferencia, fetchTodosPontosReferencia } from '../api/client'
import type { PontoReferenciaDto } from '../api/types'
import { ErrorNotice } from '../components/ErrorNotice'
import { ReferenceSearch } from '../components/ReferenceSearch'

export function PontosPage() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [resultados, setResultados] = useState<PontoReferenciaDto[]>([])
  const [mostrarFormulario, setMostrarFormulario] = useState(false)
  const [formErro, setFormErro] = useState<string | null>(null)
  const [mensagemSucesso, setMensagemSucesso] = useState<string | null>(null)
  const [salvando, setSalvando] = useState(false)

  interface NovoPontoFormState {
    nome: string
    latitude: string
    longitude: string
    raioMetros: string
  }

  const [novoPonto, setNovoPonto] = useState<NovoPontoFormState>({
    nome: '',
    latitude: '',
    longitude: '',
    raioMetros: '',
  })

  const buscar = useCallback(async (lat: number, lon: number) => {
    try {
      setLoading(true)
      setError(null)
      const data = await fetchPontosReferencia(lat, lon)
      setResultados(data)
    } catch (err) {
      setError((err as Error).message)
    } finally {
      setLoading(false)
    }
  }, [])

  const carregarTodos = useCallback(async () => {
    try {
      setLoading(true)
      setError(null)
      const data = await fetchTodosPontosReferencia()
      setResultados(data)
    } catch (err) {
      setError((err as Error).message)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void carregarTodos()
  }, [carregarTodos])

  function handleNovoPontoChange(event: ChangeEvent<HTMLInputElement>) {
    const { name, value } = event.target
    setNovoPonto((prev) => ({ ...prev, [name]: value }))
  }

  async function handleCriarPonto(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setFormErro(null)
    setMensagemSucesso(null)

    const nome = novoPonto.nome.trim()
    if (!nome) {
      setFormErro('Informe um nome para o ponto de referência.')
      return
    }

    const latitude = Number(novoPonto.latitude)
    if (Number.isNaN(latitude) || latitude < -90 || latitude > 90) {
      setFormErro('Latitude inválida. Utilize valores entre -90 e 90 graus.')
      return
    }

    const longitude = Number(novoPonto.longitude)
    if (Number.isNaN(longitude) || longitude < -180 || longitude > 180) {
      setFormErro('Longitude inválida. Utilize valores entre -180 e 180 graus.')
      return
    }

    const raioMetros = Number(novoPonto.raioMetros)
    if (Number.isNaN(raioMetros) || raioMetros <= 0) {
      setFormErro('Informe um raio em metros maior que zero.')
      return
    }

    try {
      setSalvando(true)
      const criado = await criarPontoReferencia({ nome, latitude, longitude, raioMetros })
      const normalizado: PontoReferenciaDto = {
        ...criado,
        latitude: typeof criado.latitude === 'number' ? criado.latitude : Number(criado.latitude ?? latitude),
        longitude: typeof criado.longitude === 'number' ? criado.longitude : Number(criado.longitude ?? longitude),
        raioMetros:
            typeof criado.raioMetros === 'number' ? criado.raioMetros : Number(criado.raioMetros ?? raioMetros),
        distanciaMetros:
            criado.distanciaMetros !== undefined && criado.distanciaMetros !== null
                ? Number(criado.distanciaMetros)
                : undefined,
      }
      setMensagemSucesso(`Ponto de referência ${normalizado.nome} criado com sucesso.`)
      setNovoPonto({ nome: '', latitude: '', longitude: '', raioMetros: '' })
      setResultados((prev) => [normalizado, ...prev])
    } catch (err) {
      setFormErro((err as Error).message)
    } finally {
      setSalvando(false)
    }
  }

  return (
    <div className="space-y-6">
      {error && <ErrorNotice error={error} onRetry={() => void carregarTodos()} />}

      {mostrarFormulario && (
        <form
          onSubmit={handleCriarPonto}
          className="space-y-4 rounded-xl border border-slate-700/60 bg-night-900/70 p-4"
        >
          <div className="grid gap-4 sm:grid-cols-2">
            <div className="flex flex-col sm:col-span-2">
              <label className="text-xs font-semibold uppercase tracking-wide text-slate-400">Nome</label>
              <input
                name="nome"
                value={novoPonto.nome}
                onChange={handleNovoPontoChange}
                className="mt-1 rounded border border-slate-700 bg-night-800/60 px-3 py-2 text-sm text-slate-100 focus:border-emerald-400 focus:outline-none"
              />
            </div>
            <div className="flex flex-col">
              <label className="text-xs font-semibold uppercase tracking-wide text-slate-400">Latitude</label>
              <input
                name="latitude"
                value={novoPonto.latitude}
                onChange={handleNovoPontoChange}
                type="number"
                step="any"
                className="mt-1 rounded border border-slate-700 bg-night-800/60 px-3 py-2 text-sm text-slate-100 focus:border-emerald-400 focus:outline-none"
              />
            </div>
            <div className="flex flex-col">
              <label className="text-xs font-semibold uppercase tracking-wide text-slate-400">Longitude</label>
              <input
                name="longitude"
                value={novoPonto.longitude}
                onChange={handleNovoPontoChange}
                type="number"
                step="any"
                className="mt-1 rounded border border-slate-700 bg-night-800/60 px-3 py-2 text-sm text-slate-100 focus:border-emerald-400 focus:outline-none"
              />
            </div>
            <div className="flex flex-col">
              <label className="text-xs font-semibold uppercase tracking-wide text-slate-400">Raio (m)</label>
              <input
                name="raioMetros"
                value={novoPonto.raioMetros}
                onChange={handleNovoPontoChange}
                type="number"
                step="any"
                min="0"
                className="mt-1 rounded border border-slate-700 bg-night-800/60 px-3 py-2 text-sm text-slate-100 focus:border-emerald-400 focus:outline-none"
              />
            </div>
          </div>

          {formErro && (
            <div className="rounded-lg border border-rose-500/40 bg-rose-500/10 px-3 py-2 text-sm text-rose-200">{formErro}</div>
          )}

          {mensagemSucesso && (
            <div className="rounded-lg border border-emerald-500/40 bg-emerald-500/10 px-3 py-2 text-sm text-emerald-300">
              {mensagemSucesso}
            </div>
          )}

          <div className="flex justify-end">
            <button
              type="submit"
              disabled={salvando}
              className="rounded-lg bg-emerald-500 px-4 py-2 text-sm font-semibold text-night-900 transition hover:bg-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-400 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {salvando ? 'Salvando...' : 'Salvar ponto'}
            </button>
          </div>
        </form>
      )}

      <ReferenceSearch
        resultados={resultados}
        loading={loading}
        onBuscar={(lat, lon) => {
          void buscar(lat, lon)
        }}
        onLimpar={() => {
          void carregarTodos()
        }}
      />

      <div className="flex justify-end">
        <button
            type="button"
            onClick={() => setMostrarFormulario((prev) => !prev)}
            className="rounded-lg bg-emerald-500 px-4 py-2 text-sm font-semibold text-night-900 transition hover:bg-emerald-400 focus:outline-none focus:ring-2 focus:ring-emerald-400"
        >
          {mostrarFormulario ? 'Cancelar inclusão' : 'Incluir ponto de referência'}
        </button>
      </div>
    </div>
  )
}
