using System.Collections.Concurrent;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatoPrimordialAPI.Domain.Entities;
using PatoPrimordialAPI.Domain.Missoes;
using PatoPrimordialAPI.Dtos.Missoes;
using PatoPrimordialAPI.Infrastructure.Data;

namespace PatoPrimordialAPI.Services.Missoes;

public class MissaoService : IMissaoService
{
    private const double TickIntervalMs = 200;
    private const double MinTelemetria = 5.0;
    private const int TimelineMax = 2000;

    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly IMissaoCatalogoService _catalogo;
    private readonly ILogger<MissaoService> _logger;
    private readonly ConcurrentDictionary<long, MissaoRuntime> _missoes = new();
    private readonly JsonSerializerOptions _jsonOptions = MissaoJson.Options;
    private readonly object _randomLock = new();
    private readonly Random _random = new();
    private long _idSequence;

    public MissaoService(
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        IMissaoCatalogoService catalogo,
        ILogger<MissaoService> logger)
    {
        _dbContextFactory = dbContextFactory;
        _catalogo = catalogo;
        _logger = logger;
    }

    private long NextId() => Interlocked.Increment(ref _idSequence);

    public MissaoDto CriarMissao(long patoId)
    {
        using var db = _dbContextFactory.CreateDbContext();
        var pato = db.Patos.AsNoTracking().FirstOrDefault(p => p.Id == patoId)
                   ?? throw new InvalidOperationException($"Pato {patoId} não encontrado");

        var analise = db.AnalisesPatos.AsNoTracking().FirstOrDefault(a => a.PatoId == patoId);

        var contexto = ConstruirContexto(pato, analise);
        var fraquezas = CalcularFraquezas(contexto);
        var taticas = SelecionarTaticas(contexto, fraquezas);
        var defesas = SelecionarDefesas(contexto);

        var missao = new Missao
        {
            Id = NextId(),
            PatoId = patoId,
            Status = MissaoStatus.Planejada,
            PoderioAlocado = contexto.Poderio,
            CriadoEm = DateTime.UtcNow,
            RiscoTotal = contexto.RiscoTotal,
            EstrategiaJson = JsonSerializer.Serialize(CriarPlanoEstrategiaDto(contexto, taticas), _jsonOptions),
            DefesaJson = JsonSerializer.Serialize(CriarPlanoDefesaDto(defesas), _jsonOptions)
        };

        var runtime = new MissaoRuntime(missao, contexto, fraquezas, taticas, defesas);
        _missoes[missao.Id] = runtime;

        _logger.LogInformation("Missão {MissaoId} criada para pato {PatoId}", missao.Id, patoId);

        return CriarMissaoDto(runtime);
    }

    public async Task<MissaoDto> IniciarAsync(long missaoId, CancellationToken cancellationToken)
    {
        if (!_missoes.TryGetValue(missaoId, out var runtime))
        {
            throw new InvalidOperationException("Missão não encontrada");
        }

        lock (runtime.SyncRoot)
        {
            if (runtime.Missao.Status == MissaoStatus.EmExecucao)
            {
                return CriarMissaoDto(runtime);
            }

            if (runtime.Missao.Status != MissaoStatus.Planejada)
            {
                return CriarMissaoDto(runtime);
            }

            runtime.Missao.Status = MissaoStatus.EmExecucao;
            runtime.Missao.IniciadoEm = DateTime.UtcNow;
            runtime.FaseAtual = Fase.Takeoff;
            runtime.ProximaFase = Fase.Cruise;
            RegistrarTick(runtime, "missao_iniciada", new { mensagem = "Missão iniciada" });
        }

        _ = Task.Run(() => ExecutarMissaoAsync(runtime), cancellationToken);

        return CriarMissaoDto(runtime);
    }

    public MissaoDto Abortar(long missaoId)
    {
        if (!_missoes.TryGetValue(missaoId, out var runtime))
        {
            throw new InvalidOperationException("Missão não encontrada");
        }

        lock (runtime.SyncRoot)
        {
            if (runtime.Concluida)
            {
                return CriarMissaoDto(runtime);
            }

            runtime.AbortRequested = true;
            if (runtime.Missao.Status == MissaoStatus.Planejada)
            {
                runtime.Missao.Status = MissaoStatus.Abortada;
                runtime.Missao.FinalizadoEm = DateTime.UtcNow;
                runtime.Concluida = true;
                runtime.Missao.Resultado = "Missão abortada antes da decolagem.";
                RegistrarTick(runtime, "missao_abortada", new { motivo = "abortada antes de iniciar" });
            }
            else if (runtime.Missao.Status == MissaoStatus.EmExecucao)
            {
                RegistrarTick(runtime, "missao_abortada", new { motivo = "abortada pelo operador" });
            }
        }

        return CriarMissaoDto(runtime);
    }

    public MissaoDto? Obter(long missaoId)
    {
        if (!_missoes.TryGetValue(missaoId, out var runtime))
        {
            return null;
        }

        lock (runtime.SyncRoot)
        {
            return CriarMissaoDto(runtime);
        }
    }

    public IReadOnlyCollection<MissaoListItemDto> Listar()
    {
        return _missoes.Values
            .OrderByDescending(m => m.Missao.CriadoEm)
            .Select(runtime =>
            {
                lock (runtime.SyncRoot)
                {
                    var missao = runtime.Missao;
                    return new MissaoListItemDto
                    {
                        Id = missao.Id,
                        PatoId = missao.PatoId,
                        Status = ConverterStatus(missao.Status),
                        PoderioAlocado = missao.PoderioAlocado,
                        CriadoEm = missao.CriadoEm,
                        IniciadoEm = missao.IniciadoEm,
                        FinalizadoEm = missao.FinalizadoEm,
                        Resultado = missao.Resultado
                    };
                }
            })
            .ToList();
    }

    public MissaoTimelineDto Timeline(long missaoId, int page, int size)
    {
        if (!_missoes.TryGetValue(missaoId, out var runtime))
        {
            throw new InvalidOperationException("Missão não encontrada");
        }

        if (page <= 0)
        {
            page = 1;
        }

        if (size <= 0)
        {
            size = 50;
        }

        lock (runtime.SyncRoot)
        {
            var total = runtime.Ticks.Count;
            var skip = (page - 1) * size;
            var items = runtime.Ticks
                .Skip(skip)
                .Take(size)
                .Select(tick => new MissaoTickDto
                {
                    Id = tick.Id,
                    Tick = tick.Tick,
                    Fase = ConverterFase(tick.Fase),
                    Evento = tick.Evento,
                    Detalhe = ParseDetalhe(tick.DetalheJson),
                    BateriaPct = tick.BateriaPct,
                    CombustivelPct = tick.CombustivelPct,
                    IntegridadePct = tick.IntegridadePct,
                    DistanciaM = tick.DistanciaM,
                    Sucesso = tick.Sucesso
                })
                .ToList();

            return new MissaoTimelineDto
            {
                Page = page,
                PageSize = size,
                TotalCount = total,
                Items = items
            };
        }
    }

    private async Task ExecutarMissaoAsync(MissaoRuntime runtime)
    {
        try
        {
            await SimularAsync(runtime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao simular missão {MissaoId}", runtime.Missao.Id);
            lock (runtime.SyncRoot)
            {
                runtime.Missao.Status = MissaoStatus.Abortada;
                runtime.Missao.FinalizadoEm = DateTime.UtcNow;
                runtime.Missao.Resultado = "Falha na simulação";
                runtime.Concluida = true;
                RegistrarTick(runtime, "erro_simulacao", new { mensagem = ex.Message });
            }
        }
    }

    private async Task SimularAsync(MissaoRuntime runtime)
    {
        var sequencia = new[] { Fase.Takeoff, Fase.Cruise, Fase.Approach, Fase.Engage, Fase.Egress, Fase.Land };
        var duracoes = new Dictionary<Fase, int>
        {
            [Fase.Takeoff] = 25,
            [Fase.Cruise] = 80,
            [Fase.Approach] = 50,
            [Fase.Engage] = 120,
            [Fase.Egress] = 45,
            [Fase.Land] = 30
        };

        int faseIndex = 0;
        while (faseIndex < sequencia.Length)
        {
            if (runtime.AbortRequested && sequencia[faseIndex] != Fase.Egress && sequencia[faseIndex] != Fase.Land)
            {
                faseIndex = Array.IndexOf(sequencia, Fase.Egress);
            }

            var fase = sequencia[faseIndex];
            int ticksFase = duracoes[fase];

            lock (runtime.SyncRoot)
            {
                runtime.FaseAtual = fase;
                runtime.ProximaFase = faseIndex + 1 < sequencia.Length ? sequencia[faseIndex + 1] : null;
                RegistrarTick(runtime, "fase_inicio", new { fase = ConverterFase(fase) });
            }

            var encerrarFase = false;
            for (int i = 0; i < ticksFase; i++)
            {
                lock (runtime.SyncRoot)
                {
                    if (runtime.Concluida)
                    {
                        return;
                    }
                }

                AtualizarTelemetria(runtime, fase);
                var resultado = VerificarEncerramento(runtime);
                if (resultado is not null)
                {
                    FinalizarMissao(runtime, resultado.Value.sucesso, resultado.Value.motivo);
                    return;
                }

                if (fase == Fase.Engage)
                {
                    ExecutarEngajamento(runtime);
                    var posResultado = VerificarEncerramento(runtime);
                    if (posResultado is not null)
                    {
                        FinalizarMissao(runtime, posResultado.Value.sucesso, posResultado.Value.motivo);
                        return;
                    }

                    lock (runtime.SyncRoot)
                    {
                        if (runtime.CapturaEfetiva)
                        {
                            encerrarFase = true;
                        }
                    }
                }

                if (encerrarFase)
                {
                    break;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(20));
            }

            lock (runtime.SyncRoot)
            {
                RegistrarTick(runtime, "fase_fim", new { fase = ConverterFase(fase) });
            }

            faseIndex++;
        }

        FinalizarMissao(runtime, false, "Timeout operacional");
    }

    private void AtualizarTelemetria(MissaoRuntime runtime, Fase fase)
    {
        lock (runtime.SyncRoot)
        {
            var telemetria = runtime.Telemetria;
            var consumo = ObterConsumoPorTick(fase);
            telemetria.Bateria = Math.Max(0, telemetria.Bateria - consumo);
            telemetria.Combustivel = Math.Max(0, telemetria.Combustivel - consumo);

            switch (fase)
            {
                case Fase.Takeoff:
                    telemetria.DistanciaM = Math.Max(1200, telemetria.DistanciaM - 15);
                    break;
                case Fase.Cruise:
                    telemetria.DistanciaM = Math.Max(350, telemetria.DistanciaM - 14);
                    break;
                case Fase.Approach:
                    telemetria.DistanciaM = Math.Max(40, telemetria.DistanciaM - 10);
                    break;
                case Fase.Engage:
                    telemetria.DistanciaM = Math.Max(15, telemetria.DistanciaM - 2);
                    break;
                case Fase.Egress:
                    telemetria.DistanciaM = Math.Min(1800, telemetria.DistanciaM + 18);
                    break;
                case Fase.Land:
                    telemetria.DistanciaM = Math.Max(0, telemetria.DistanciaM - 25);
                    break;
            }

            AplicarDano(runtime, fase);
            RegistrarTick(runtime, "telemetria", new
            {
                fase = ConverterFase(fase),
                telemetria = runtime.Telemetria,
                taticaAtual = runtime.Taticas.ElementAtOrDefault(runtime.TaticaIndex)?.Regra.Nome,
                abortada = runtime.AbortRequested
            });
        }
    }

    private void AplicarDano(MissaoRuntime runtime, Fase fase)
    {
        if (fase == Fase.Land || fase == Fase.Takeoff)
        {
            return;
        }

        var probBase = runtime.Contexto.RiscoTotal / 750d;
        var mitigacaoPrimaria = runtime.Defesas.Primaria?.Defesa.Mitigacao ?? 0;
        var mitigacaoFallback = runtime.Defesas.Fallback?.Defesa.Mitigacao ?? 0;
        var probDano = Math.Clamp(probBase * (1 - mitigacaoPrimaria) * (1 - (mitigacaoFallback * 0.5)), 0, 0.75);

        if (RandomDouble() <= probDano)
        {
            var dano = 2.4 + RandomDouble() * 6.5;
            runtime.Telemetria.Integridade = Math.Max(0, runtime.Telemetria.Integridade - dano);
            RegistrarTick(runtime, "dano_recebido", new { dano, probabilidade = probDano });
        }
    }

    private (bool sucesso, string motivo)? VerificarEncerramento(MissaoRuntime runtime)
    {
        lock (runtime.SyncRoot)
        {
            if (runtime.Telemetria.Bateria <= MinTelemetria)
            {
                return (false, "Bateria insuficiente");
            }

            if (runtime.Telemetria.Combustivel <= MinTelemetria)
            {
                return (false, "Combustível insuficiente");
            }

            if (runtime.Telemetria.Integridade <= 0)
            {
                return (false, "Integridade do drone comprometida");
            }

            if (runtime.CapturaEfetiva && runtime.FaseAtual == Fase.Land)
            {
                return (true, "Alvo contido com sucesso");
            }

            if (runtime.AbortRequested && runtime.FaseAtual == Fase.Land)
            {
                return (false, "Missão abortada");
            }
        }

        return null;
    }

    private void ExecutarEngajamento(MissaoRuntime runtime)
    {
        lock (runtime.SyncRoot)
        {
            if (runtime.CapturaEfetiva)
            {
                return;
            }

            runtime.TicksSemAcao++;
            if (runtime.TicksSemAcao < 4)
            {
                return;
            }

            runtime.TicksSemAcao = 0;
            var tatica = runtime.Taticas.ElementAtOrDefault(runtime.TaticaIndex);
            if (tatica is null)
            {
                runtime.TaticaIndex = 0;
                tatica = runtime.Taticas.FirstOrDefault();
                if (tatica is null)
                {
                    return;
                }
            }

            var chanceBase = 0.25;
            var chance = Math.Clamp(chanceBase + tatica.BonusFraqueza + runtime.Fraquezas.Sum(f => f.Bonus), 0, 0.95);
            var resultado = RandomDouble();
            var sucesso = resultado <= chance;
            if (sucesso)
            {
                runtime.IntegridadeAlvo = Math.Max(0, runtime.IntegridadeAlvo - (35 + RandomDouble() * 20));
                if (runtime.IntegridadeAlvo <= 0)
                {
                    runtime.CapturaEfetiva = true;
                }
            }
            else
            {
                runtime.TaticaIndex = (runtime.TaticaIndex + 1) % runtime.Taticas.Count;
            }

            RegistrarTick(runtime, "tatica_execucao", new
            {
                tatica = tatica.Regra.Nome,
                chance,
                sucesso,
                integridadeAlvo = runtime.IntegridadeAlvo
            }, sucesso);
        }
    }

    private void FinalizarMissao(MissaoRuntime runtime, bool sucesso, string motivo)
    {
        lock (runtime.SyncRoot)
        {
            if (runtime.Concluida)
            {
                return;
            }

            runtime.Concluida = true;
            runtime.Missao.FinalizadoEm = DateTime.UtcNow;
            runtime.Missao.Status = sucesso ? MissaoStatus.Concluida : (runtime.AbortRequested ? MissaoStatus.Abortada : MissaoStatus.Concluida);
            if (!sucesso && runtime.AbortRequested)
            {
                runtime.Missao.Status = MissaoStatus.Abortada;
            }
            else if (!sucesso)
            {
                runtime.Missao.Status = MissaoStatus.Concluida;
            }

            runtime.Missao.Resultado = sucesso ? motivo : $"Falha: {motivo}";
            RegistrarTick(runtime, "missao_finalizada", new { sucesso, motivo }, sucesso);
        }
    }

    private double ObterConsumoPorTick(Fase fase)
    {
        return fase switch
        {
            Fase.Takeoff => ConverterConsumoPorTick(5),
            Fase.Cruise => ConverterConsumoPorTick(2),
            Fase.Approach => ConverterConsumoPorTick(3),
            Fase.Engage => ConverterConsumoPorTick(6),
            Fase.Egress => ConverterConsumoPorTick(3),
            Fase.Land => ConverterConsumoPorTick(2),
            _ => ConverterConsumoPorTick(2)
        };
    }

    private static double ConverterConsumoPorTick(double consumoPorMinuto)
    {
        return consumoPorMinuto / 300d;
    }

    private double RandomDouble()
    {
        lock (_randomLock)
        {
            return _random.NextDouble();
        }
    }

    private static JsonElement? ParseDetalhe(string? detalheJson)
    {
        if (detalheJson is null)
        {
            return null;
        }

        using var doc = JsonDocument.Parse(detalheJson);
        return doc.RootElement.Clone();
    }

    private void RegistrarTick(MissaoRuntime runtime, string evento, object? detalhes, bool sucesso = false)
    {
        var tick = new MissaoTick
        {
            Id = NextId(),
            MissaoId = runtime.Missao.Id,
            Tick = ++runtime.TickSequencial,
            Fase = runtime.FaseAtual,
            Evento = evento,
            DetalheJson = detalhes is null ? null : JsonSerializer.Serialize(detalhes, _jsonOptions),
            BateriaPct = runtime.Telemetria.Bateria,
            CombustivelPct = runtime.Telemetria.Combustivel,
            IntegridadePct = runtime.Telemetria.Integridade,
            DistanciaM = runtime.Telemetria.DistanciaM,
            Sucesso = sucesso
        };

        runtime.Ticks.Add(tick);
        if (runtime.Ticks.Count > TimelineMax)
        {
            runtime.Ticks.RemoveRange(0, runtime.Ticks.Count - TimelineMax);
        }
    }

    private MissaoDto CriarMissaoDto(MissaoRuntime runtime)
    {
        var missao = runtime.Missao;
        var telemetria = runtime.Telemetria;
        var plano = CriarPlanoEstrategiaDto(runtime.Contexto, runtime.Taticas);
        var defesas = CriarPlanoDefesaDto(runtime.Defesas);
        var fraquezas = runtime.Fraquezas
            .Select(f => new FraquezaAplicadaDto
            {
                Id = f.Fraqueza.Id,
                Nome = f.Fraqueza.Nome,
                Descricao = f.Fraqueza.Efeito.Descricao,
                BonusSucesso = Math.Round(f.Bonus, 2)
            })
            .ToList();

        return new MissaoDto
        {
            Id = missao.Id,
            PatoId = missao.PatoId,
            Status = ConverterStatus(missao.Status),
            PoderioAlocado = missao.PoderioAlocado,
            CriadoEm = missao.CriadoEm,
            IniciadoEm = missao.IniciadoEm,
            FinalizadoEm = missao.FinalizadoEm,
            Telemetria = new TelemetriaDto
            {
                Bateria = Math.Round(telemetria.Bateria, 2),
                Combustivel = Math.Round(telemetria.Combustivel, 2),
                Integridade = Math.Round(telemetria.Integridade, 2),
                DistanciaM = Math.Round(telemetria.DistanciaM, 2)
            },
            FaseAtual = ConverterFase(runtime.FaseAtual),
            ProximaFase = runtime.ProximaFase is null ? null : ConverterFase(runtime.ProximaFase.Value),
            Estrategia = plano,
            Defesas = defesas,
            Fraquezas = fraquezas,
            Resultado = missao.Resultado
        };
    }

    private static PlanoEstrategiaDto CriarPlanoEstrategiaDto(MissaoContexto contexto, IReadOnlyList<TaticaSelecionada> taticas)
    {
        return new PlanoEstrategiaDto
        {
            Porte = contexto.Porte,
            ClasseRisco = contexto.ClasseRisco,
            Taticas = taticas
                .Select(t => new TaticaPlanoDto
                {
                    Nome = t.Regra.Nome,
                    Descricao = t.Regra.Descricao,
                    Prioridade = t.Regra.Prioridade,
                    BonusFraqueza = Math.Round(t.BonusFraqueza, 2),
                    ExplorandoFraqueza = t.BonusFraqueza > 0
                })
                .ToList()
        };
    }

    private static PlanoDefesaDto CriarPlanoDefesaDto(PlanoDefesaSelecionada defesas)
    {
        DefesaSelecionadaDto? Mapear(DefesaSelecionada? selecionada)
        {
            if (selecionada is null)
            {
                return null;
            }

            return new DefesaSelecionadaDto
            {
                Nome = selecionada.Defesa.Nome,
                Contramedida = selecionada.Defesa.Contramedida,
                TagsAmeaca = selecionada.Defesa.TagsAmeaca.ToArray(),
                Rareza = selecionada.Defesa.Rareza,
                Mitigacao = Math.Round(selecionada.Defesa.Mitigacao, 2)
            };
        }

        return new PlanoDefesaDto
        {
            Primaria = Mapear(defesas.Primaria),
            Fallback = Mapear(defesas.Fallback)
        };
    }

    private static string ConverterStatus(MissaoStatus status)
    {
        return status switch
        {
            MissaoStatus.Planejada => "planejada",
            MissaoStatus.EmExecucao => "em_execucao",
            MissaoStatus.Abortada => "abortada",
            MissaoStatus.Concluida => "concluida",
            _ => "planejada"
        };
    }
    private static string ConverterFase(Fase fase)
    {
        return fase switch
        {
            Fase.Idle => "Ociosa",
            Fase.Takeoff => "Decolagem",
            Fase.Cruise => "Cruzeiro",
            Fase.Approach => "Aproximação",
            Fase.Engage => "Engajamento",
            Fase.Egress => "Retirada",
            Fase.Land => "Pouso",
            _ => fase.ToString()
        };
    }

    private MissaoContexto ConstruirContexto(Pato pato, AnalisePato? analise)
    {
        var tags = (pato.PoderTagsCsv ?? string.Empty)
            .Split(';', ',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.ToLowerInvariant())
            .Distinct()
            .ToArray();

        var mutacoes = pato.MutacoesQtde ?? 0;
        var porte = ClassificarPorte(pato.PesoG);
        var classeRisco = analise?.ClasseRisco?.ToLowerInvariant() ?? InferirClasseRisco(analise?.RiscoTotal ?? 40);
        var riscoTotal = analise?.RiscoTotal ?? 40;
        var poderio = analise?.PoderioNecessario ?? Math.Clamp(riscoTotal * 0.8, 20, 120);

        return new MissaoContexto
        {
            PatoId = pato.Id,
            Estado = pato.Estado?.ToLowerInvariant() ?? "desconhecido",
            PoderTags = tags,
            AlturaCm = (double?)pato.AlturaCm ?? 0,
            PesoG = (double?)pato.PesoG ?? 0,
            Bpm = pato.Bpm ?? 0,
            Mutacoes = mutacoes,
            Porte = porte,
            ClasseRisco = classeRisco,
            RiscoTotal = riscoTotal,
            Poderio = poderio,
            DistanciaMetros = (analise?.DistKm ?? 1) * 1000d
        };
    }

    private IReadOnlyList<FraquezaAplicadaInterna> CalcularFraquezas(MissaoContexto contexto)
    {
        var fraquezas = new List<FraquezaAplicadaInterna>();
        foreach (var fraqueza in _catalogo.ObterFraquezas())
        {
            if (AvaliarFraqueza(fraqueza, contexto))
            {
                fraquezas.Add(new FraquezaAplicadaInterna(fraqueza, fraqueza.Efeito.BonusSucesso));
            }
        }

        return fraquezas;
    }

    private IReadOnlyList<TaticaSelecionada> SelecionarTaticas(MissaoContexto contexto, IReadOnlyList<FraquezaAplicadaInterna> fraquezas)
    {
        var aplicaveis = new List<TaticaSelecionada>();
        foreach (var regra in _catalogo.ObterTaticas())
        {
            if (AvaliarRegra(regra, contexto))
            {
                var bonus = CalcularBonusFraqueza(regra, contexto, fraquezas);
                aplicaveis.Add(new TaticaSelecionada(regra, bonus));
            }
        }

        if (aplicaveis.Count == 0)
        {
            aplicaveis.Add(new TaticaSelecionada(new RegraTatica
            {
                Id = NextId(),
                Nome = "Protocolo padrão",
                Descricao = "Procedimento genérico de contenção em baixa altitude.",
                Acao = new RegraTaticaAcao { Tipo = "padrao", Tatica = "procedimento_padrao" },
                Condicao = new RegraTaticaCondicao(),
                Prioridade = 50
            }, 0));
        }

        return aplicaveis
            .OrderByDescending(t => t.Regra.Prioridade)
            .ThenByDescending(t => t.BonusFraqueza)
            .ToList();
    }

    private PlanoDefesaSelecionada SelecionarDefesas(MissaoContexto contexto)
    {
        var defesas = _catalogo.ObterDefesas();
        var tags = contexto.PoderTags;
        DefesaSelecionada? primaria = null;
        DefesaSelecionada? fallback = null;

        foreach (var defesa in defesas.Where(d => !d.Fallback))
        {
            if (defesa.TagsAmeaca.Count == 0)
            {
                continue;
            }

            if (defesa.TagsAmeaca.Any(tag => tags.Contains(tag)))
            {
                var peso = defesa.Rareza + (defesa.TagsAmeaca.Intersect(tags).Count() * 10);
                var randomPeso = RandomDouble();
                if (primaria is null || randomPeso * peso > primaria.Peso)
                {
                    primaria = new DefesaSelecionada(defesa, peso * randomPeso);
                }
            }
        }

        if (primaria is null)
        {
            var generica = defesas.FirstOrDefault(d => d.Fallback == false);
            if (generica is not null)
            {
                primaria = new DefesaSelecionada(generica, generica.Rareza);
            }
        }

        fallback = defesas
            .Where(d => d.Fallback)
            .Select(d => new DefesaSelecionada(d, d.Rareza))
            .OrderByDescending(d => d.Peso)
            .FirstOrDefault();

        return new PlanoDefesaSelecionada(primaria, fallback);
    }

    private static bool AvaliarFraqueza(FraquezaCatalogo fraqueza, MissaoContexto contexto)
    {
        if (fraqueza.Condicao.Estado is not null && !string.Equals(fraqueza.Condicao.Estado, contexto.Estado, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (fraqueza.Condicao.Porte is not null && !string.Equals(fraqueza.Condicao.Porte, contexto.Porte, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (fraqueza.Condicao.PoderTag is not null && !contexto.PoderTags.Contains(fraqueza.Condicao.PoderTag))
        {
            return false;
        }

        if (fraqueza.Condicao.MutacoesMin.HasValue && contexto.Mutacoes < fraqueza.Condicao.MutacoesMin.Value)
        {
            return false;
        }

        if (fraqueza.Condicao.MutacoesMax.HasValue && contexto.Mutacoes > fraqueza.Condicao.MutacoesMax.Value)
        {
            return false;
        }

        return true;
    }

    private static bool AvaliarRegra(RegraTatica regra, MissaoContexto contexto)
    {
        var condicao = regra.Condicao;

        if (condicao.Porte is not null && !string.Equals(condicao.Porte, contexto.Porte, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (condicao.ClasseRisco is not null && !string.Equals(condicao.ClasseRisco, contexto.ClasseRisco, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (condicao.Estado is not null && !string.Equals(condicao.Estado, contexto.Estado, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (condicao.PoderTag is not null && !contexto.PoderTags.Contains(condicao.PoderTag))
        {
            return false;
        }

        if (condicao.AlturaMinCm.HasValue && contexto.AlturaCm < condicao.AlturaMinCm.Value)
        {
            return false;
        }

        if (condicao.AlturaMaxCm.HasValue && contexto.AlturaCm > condicao.AlturaMaxCm.Value)
        {
            return false;
        }

        if (condicao.MutacoesMin.HasValue && contexto.Mutacoes < condicao.MutacoesMin.Value)
        {
            return false;
        }

        if (condicao.MutacoesMax.HasValue && contexto.Mutacoes > condicao.MutacoesMax.Value)
        {
            return false;
        }

        if (condicao.BpmMin.HasValue && contexto.Bpm < condicao.BpmMin.Value)
        {
            return false;
        }

        return true;
    }

    private static double CalcularBonusFraqueza(RegraTatica regra, MissaoContexto contexto, IReadOnlyList<FraquezaAplicadaInterna> fraquezas)
    {
        double bonus = 0;
        foreach (var fraqueza in fraquezas)
        {
            if (fraqueza.Fraqueza.Condicao.PoderTag is not null && regra.Condicao.PoderTag == fraqueza.Fraqueza.Condicao.PoderTag)
            {
                bonus += fraqueza.Bonus * 0.6;
            }

            if (fraqueza.Fraqueza.Condicao.Estado is not null && regra.Condicao.Estado == fraqueza.Fraqueza.Condicao.Estado)
            {
                bonus += fraqueza.Bonus * 0.5;
            }

            if (fraqueza.Fraqueza.Condicao.Porte is not null && regra.Condicao.Porte == fraqueza.Fraqueza.Condicao.Porte)
            {
                bonus += fraqueza.Bonus * 0.4;
            }
        }

        return Math.Min(0.4, bonus);
    }

    private static string ClassificarPorte(decimal? pesoG)
    {
        var peso = (double? )pesoG ?? 0;
        if (peso <= 2500)
        {
            return "leve";
        }

        if (peso <= 6000)
        {
            return "medio";
        }

        return "pesado";
    }

    private static string InferirClasseRisco(double riscoTotal)
    {
        if (riscoTotal >= 70)
        {
            return "alto";
        }

        if (riscoTotal >= 40)
        {
            return "medio";
        }

        return "baixo";
    }

    private sealed class MissaoRuntime
    {
        public MissaoRuntime(
            Missao missao,
            MissaoContexto contexto,
            IReadOnlyList<FraquezaAplicadaInterna> fraquezas,
            IReadOnlyList<TaticaSelecionada> taticas,
            PlanoDefesaSelecionada defesas)
        {
            Missao = missao;
            Contexto = contexto;
            Fraquezas = fraquezas;
            Taticas = taticas;
            Defesas = defesas;
        }

        public Missao Missao { get; }
        public MissaoContexto Contexto { get; }
        public IReadOnlyList<FraquezaAplicadaInterna> Fraquezas { get; }
        public IReadOnlyList<TaticaSelecionada> Taticas { get; }
        public PlanoDefesaSelecionada Defesas { get; }
        public Telemetria Telemetria { get; } = new();
        public List<MissaoTick> Ticks { get; } = new();
        public Fase FaseAtual { get; set; } = Fase.Idle;
        public Fase? ProximaFase { get; set; }
        public bool AbortRequested { get; set; }
        public bool Concluida { get; set; }
        public bool CapturaEfetiva { get; set; }
        public double IntegridadeAlvo { get; set; } = 100;
        public int TaticaIndex { get; set; }
        public int TicksSemAcao { get; set; }
        public int TickSequencial { get; set; }
        public object SyncRoot { get; } = new();
    }

    private sealed record MissaoContexto
    {
        public long PatoId { get; init; }
        public string Estado { get; init; } = string.Empty;
        public IReadOnlyCollection<string> PoderTags { get; init; } = Array.Empty<string>();
        public double AlturaCm { get; init; }
        public double PesoG { get; init; }
        public double Bpm { get; init; }
        public int Mutacoes { get; init; }
        public string Porte { get; init; } = string.Empty;
        public string ClasseRisco { get; init; } = string.Empty;
        public double RiscoTotal { get; init; }
        public double Poderio { get; init; }
        public double DistanciaMetros { get; init; }
    }

    private sealed record FraquezaAplicadaInterna(FraquezaCatalogo Fraqueza, double Bonus)
    {
        public double BonusSucesso => Bonus;
    }

    private sealed record TaticaSelecionada(RegraTatica Regra, double BonusFraqueza);

    private sealed record DefesaSelecionada(RegraDefesa Defesa, double Peso);

    private sealed record PlanoDefesaSelecionada(DefesaSelecionada? Primaria, DefesaSelecionada? Fallback);
}
