import { useEffect, useState } from 'react'
import { fetchAvistamentosRecentes, fetchPatosKpis } from '../api/client'
import type { AvistamentoDto, PatosKpiDto } from '../api/types'
import { Dashboard } from '../components/Dashboard'
import { ErrorNotice } from '../components/ErrorNotice'
import { LoadingState } from '../components/LoadingState'

export function DashboardPage() {
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [kpis, setKpis] = useState<PatosKpiDto | undefined>()
  const [recentes, setRecentes] = useState<AvistamentoDto[]>([])

  useEffect(() => {
    carregar()
  }, [])

  async function carregar() {
    try {
      setLoading(true)
      setError(null)
      const [kpiData, recentesData] = await Promise.all([fetchPatosKpis(), fetchAvistamentosRecentes()])
      setKpis(kpiData)
      setRecentes(recentesData)
    } catch (err) {
      setError((err as Error).message)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="space-y-6">
      {error && <ErrorNotice error={error} onRetry={carregar} />}
      {loading ? <LoadingState message="Carregando indicadores..." /> : <Dashboard kpis={kpis} avistamentos={recentes} />}
    </div>
  )
}
