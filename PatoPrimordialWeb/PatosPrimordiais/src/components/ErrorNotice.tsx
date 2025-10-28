interface ErrorNoticeProps {
  error: string
  onRetry?: () => void
}

export function ErrorNotice({ error, onRetry }: ErrorNoticeProps) {
  return (
    <div className="flex items-start justify-between gap-4 rounded border border-red-500/30 bg-red-500/10 px-4 py-3 text-sm text-red-200">
      <span>{error}</span>
      {onRetry && (
        <button
          type="button"
          onClick={onRetry}
          className="rounded border border-red-400/50 px-3 py-1 text-xs font-semibold uppercase tracking-wide text-red-100 transition hover:border-red-200"
        >
          Tentar novamente
        </button>
      )}
    </div>
  )
}
