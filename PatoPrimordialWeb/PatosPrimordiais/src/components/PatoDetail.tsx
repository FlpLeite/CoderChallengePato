import type { AvistamentoDto, PatoDto } from '../api/types'

interface PatoDetailProps {
  pato?: PatoDto | null
  historico: AvistamentoDto[]
  onClose: () => void
}

export function PatoDetail({ pato, historico, onClose }: PatoDetailProps) {
  if (!pato) {
    return (
      <div className="rounded-xl border border-slate-700/60 bg-night-900/70 p-6 text-sm text-slate-400">
        Selecione um pato para visualizar os detalhes.
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div className="rounded-xl border border-emerald-500/20 bg-night-900/80 p-6">
        <div className="flex items-start justify-between">
          <div>
            <h3 className="text-xl font-semibold text-emerald-200">{pato.codigo}</h3>
            <p className="mt-1 text-sm text-slate-400">
              Atualizado em {new Date(pato.atualizadoEm).toLocaleString('pt-BR')}
            </p>
          </div>
          <button
            onClick={onClose}
            className="rounded border border-slate-700 px-3 py-1 text-xs uppercase tracking-wide text-slate-300 transition hover:border-emerald-400"
          >
            Fechar
          </button>
        </div>

        <div className="mt-4 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          <DetailCard label="Estado" value={capitalize(pato.estado)} emphasis />
          <DetailCard label="BPM" value={pato.bpm ? pato.bpm.toString() : '—'} />
          <DetailCard label="Mut. registradas" value={pato.mutacoesQtde?.toString() ?? '—'} />
          <DetailCard label="Altura" value={`${pato.alturaCm.toFixed(1)} cm`} />
          <DetailCard label="Peso" value={`${pato.pesoG.toFixed(1)} g`} />
          <DetailCard label="Precisão" value={`${pato.precisaoM.toFixed(2)} m`} />
          <DetailCard label="Localização" value={`${pato.cidade}, ${pato.pais}`} />
          <DetailCard label="Coordenadas" value={`${pato.latitude.toFixed(4)}, ${pato.longitude.toFixed(4)}`} />
          <DetailCard label="Ponto de referência" value={pato.pontoReferenciaNome ?? 'Não associado'} />
        </div>

        {(pato.poderNome || pato.poderDescricao || pato.poderTags.length > 0) && (
          <div className="mt-6 rounded-lg border border-emerald-500/10 bg-night-800/70 p-4">
            <h4 className="text-sm font-semibold text-emerald-200">Poder especial</h4>
            <div className="mt-2 text-sm text-slate-200">
              <p className="font-medium text-emerald-100">{pato.poderNome ?? 'Não identificado'}</p>
              {pato.poderDescricao && <p className="mt-2 text-slate-300">{pato.poderDescricao}</p>}
              {pato.poderTags.length > 0 && (
                <div className="mt-3 flex flex-wrap gap-2 text-xs text-emerald-200">
                  {pato.poderTags.map((tag) => (
                    <span key={tag} className="rounded-full border border-emerald-500/40 px-2 py-0.5">
                      #{tag}
                    </span>
                  ))}
                </div>
              )}
            </div>
          </div>
        )}
      </div>

      <div className="rounded-xl border border-slate-700/60 bg-night-900/70">
        <div className="flex items-center justify-between px-4 py-3">
          <h4 className="text-sm font-semibold uppercase tracking-wide text-slate-200">Histórico de avistamentos</h4>
          <span className="text-xs text-slate-500">{historico.length} registros</span>
        </div>
        <div className="max-h-80 overflow-y-auto">
          <table className="min-w-full text-sm">
            <thead className="bg-night-800/80 text-xs uppercase tracking-wide text-slate-300">
              <tr>
                <th className="px-4 py-3 text-left">Data</th>
                <th className="px-4 py-3 text-left">Drone</th>
                <th className="px-4 py-3 text-left">Estado</th>
                <th className="px-4 py-3 text-left">Precisão</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-800/80">
              {historico.length === 0 ? (
                <tr>
                  <td colSpan={4} className="px-4 py-6 text-center text-slate-400">
                    Nenhum histórico disponível.
                  </td>
                </tr>
              ) : (
                historico.map((evento) => (
                  <tr key={evento.id} className="hover:bg-night-800/60">
                    <td className="px-4 py-3 text-slate-200">{new Date(evento.criadoEm).toLocaleString('pt-BR')}</td>
                    <td className="px-4 py-3 text-slate-300">{evento.droneNumeroSerie}</td>
                    <td className="px-4 py-3 capitalize text-slate-200">{evento.estadoPato}</td>
                    <td className="px-4 py-3 text-slate-200">{evento.precisaoM.toFixed(2)} m</td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}

function DetailCard({ label, value, emphasis }: { label: string; value: string; emphasis?: boolean }) {
  return (
    <div className="rounded-lg border border-slate-700/60 bg-night-800/60 px-4 py-3">
      <p className="text-xs uppercase tracking-wide text-slate-400">{label}</p>
      <p className={`mt-2 text-sm ${emphasis ? 'text-emerald-200 font-semibold' : 'text-slate-100'}`}>{value}</p>
    </div>
  )
}

function capitalize(text: string) {
  return text ? text.charAt(0).toUpperCase() + text.slice(1) : text
}
