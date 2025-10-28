using PatoPrimordialAPI.Domain.Missoes;

namespace PatoPrimordialAPI.Services.Missoes;

public class MissaoCatalogoService : IMissaoCatalogoService
{
    private readonly IReadOnlyCollection<RegraTatica> _taticas;
    private readonly IReadOnlyCollection<RegraDefesa> _defesas;
    private readonly IReadOnlyCollection<FraquezaCatalogo> _fraquezas;

    public MissaoCatalogoService()
    {
        _taticas = CarregarTaticas();
        _defesas = CarregarDefesas();
        _fraquezas = CarregarFraquezas();
    }

    public IReadOnlyCollection<RegraTatica> ObterTaticas() => _taticas;
    public IReadOnlyCollection<RegraDefesa> ObterDefesas() => _defesas;
    public IReadOnlyCollection<FraquezaCatalogo> ObterFraquezas() => _fraquezas;

    private static IReadOnlyCollection<RegraTatica> CarregarTaticas()
    {
        return new List<RegraTatica>
        {
            new()
            {
                Id = 1001L,
                Nome = "Ataque Superior",
                Descricao = "Executa contenção vertical lançando peso amortecido sobre a zona alvo.",
                Condicao = new RegraTaticaCondicao { AlturaMinCm = 100 },
                Acao = new RegraTaticaAcao { Tipo = "supressao", Tatica = "queda_controlada", Observacao = "Aplicar desaceleração final antes do impacto." },
                Prioridade = 80
            },
            new()
            {
                Id = 1002L,
                Nome = "Isolamento dielétrico",
                Descricao = "Drones auxiliares criam capa isolante para poderes elétricos.",
                Condicao = new RegraTaticaCondicao { PoderTag = "eletrico" },
                Acao = new RegraTaticaAcao { Tipo = "isolamento", Tatica = "ataque_lateral", Observacao = "Evitar linha direta com descargas." },
                Prioridade = 90
            },
            new()
            {
                Id = 1003L,
                Nome = "Rede de contenção pré-estímulo",
                Descricao = "Lançamento de rede com amortecimento acústico antes de despertar patos em hibernação.",
                Condicao = new RegraTaticaCondicao { Estado = "hibernacao" },
                Acao = new RegraTaticaAcao { Tipo = "contenção", Tatica = "rede_pre_est" },
                Prioridade = 85
            },
            new()
            {
                Id = 1004L,
                Nome = "Ressonância calmante",
                Descricao = "Emite ruído branco antes da aproximação para patos em transe acelerado.",
                Condicao = new RegraTaticaCondicao { Estado = "transe", BpmMin = 110 },
                Acao = new RegraTaticaAcao { Tipo = "suporte", Tatica = "ruido_calmante" },
                Prioridade = 70
            },
            new()
            {
                Id = 1005L,
                Nome = "Cinturão de microdrones",
                Descricao = "Forma barreira cinética contra mutações agressivas.",
                Condicao = new RegraTaticaCondicao { MutacoesMin = 3 },
                Acao = new RegraTaticaAcao { Tipo = "supressao", Tatica = "anel_microdrones" },
                Prioridade = 75
            },
            new()
            {
                Id = 1006L,
                Nome = "Aproximação furtiva",
                Descricao = "Rota baixa e lenta para patos de alto risco.",
                Condicao = new RegraTaticaCondicao { ClasseRisco = "alto" },
                Acao = new RegraTaticaAcao { Tipo = "aproximacao", Tatica = "furtiva" },
                Prioridade = 95
            },
            new()
            {
                Id = 1007L,
                Nome = "Cortina de espuma",
                Descricao = "Liberação de espuma isolante durante engajamento curto.",
                Condicao = new RegraTaticaCondicao { Porte = "leve" },
                Acao = new RegraTaticaAcao { Tipo = "contenção", Tatica = "espuma_isolante" },
                Prioridade = 60
            },
            new()
            {
                Id = 1008L,
                Nome = "Campo de gravidade inversa",
                Descricao = "Reorienta patos pesados para o solo antes da captura.",
                Condicao = new RegraTaticaCondicao { Porte = "pesado" },
                Acao = new RegraTaticaAcao { Tipo = "controle", Tatica = "gravidade_invertida" },
                Prioridade = 65
            }
        };
    }

    private static IReadOnlyCollection<RegraDefesa> CarregarDefesas()
    {
        return new List<RegraDefesa>
        {
            new()
            {
                Id = 2001L,
                Nome = "Fumigação de espuma isolante",
                TagsAmeaca = new [] { "eletrico", "belico" },
                Contramedida = "Nuvem dielétrica que reduz condução e impactos.",
                Rareza = 24,
                Mitigacao = 0.35,
                Fallback = false
            },
            new()
            {
                Id = 2002L,
                Nome = "Campo de microdrones isca",
                TagsAmeaca = new [] { "controle_mental", "sonico" },
                Contramedida = "Desvia ataques para iscas autônomas.",
                Rareza = 28,
                Mitigacao = 0.4,
                Fallback = false
            },
            new()
            {
                Id = 2003L,
                Nome = "Sprays de refletor óptico",
                TagsAmeaca = new [] { "elemental", "etereo" },
                Contramedida = "Refrata energia direcionada para longe do drone.",
                Rareza = 22,
                Mitigacao = 0.32,
                Fallback = false
            },
            new()
            {
                Id = 2004L,
                Nome = "Jatos de água salina",
                TagsAmeaca = new [] { "termico", "igneo" },
                Contramedida = "Cria película refrigerante e condutiva.",
                Rareza = 18,
                Mitigacao = 0.28,
                Fallback = false
            },
            new()
            {
                Id = 2005L,
                Nome = "Dispersor de brigadeiros",
                TagsAmeaca = new [] { "fraqueza_chocolate" },
                Contramedida = "Cria distração gustativa irresistível.",
                Rareza = 32,
                Mitigacao = 0.45,
                Fallback = false
            },
            new()
            {
                Id = 2006L,
                Nome = "Blindagem neutra",
                TagsAmeaca = Array.Empty<string>(),
                Contramedida = "Placas compostas padrão para proteção genérica.",
                Rareza = 12,
                Mitigacao = 0.18,
                Fallback = true
            },
            new()
            {
                Id = 2007L,
                Nome = "Rede anti-transe",
                TagsAmeaca = new [] { "transe" },
                Contramedida = "Rede com emissores de pulsos calmantes.",
                Rareza = 26,
                Mitigacao = 0.33,
                Fallback = false
            }
        };
    }

    private static IReadOnlyCollection<FraquezaCatalogo> CarregarFraquezas()
    {
        return new List<FraquezaCatalogo>
        {
            new()
            {
                Id = 3001L,
                Nome = "Letargia hibernal",
                Condicao = new FraquezaCondicao { Estado = "hibernacao" },
                Efeito = new FraquezaEfeito { BonusSucesso = 0.35, Descricao = "Respostas lentas permitem captura de rede pré-estímulo." }
            },
            new()
            {
                Id = 3002L,
                Nome = "Sobrecarga elétrica",
                Condicao = new FraquezaCondicao { PoderTag = "eletrico" },
                Efeito = new FraquezaEfeito { BonusSucesso = 0.28, Descricao = "Ataques dielétricos redirecionam descarga." }
            },
            new()
            {
                Id = 3003L,
                Nome = "Avidez por chocolate",
                Condicao = new FraquezaCondicao { PoderTag = "fraqueza_chocolate" },
                Efeito = new FraquezaEfeito { BonusSucesso = 0.4, Descricao = "Distração culinária reduz resistência." }
            },
            new()
            {
                Id = 3004L,
                Nome = "Fragilidade estrutural",
                Condicao = new FraquezaCondicao { Porte = "leve", MutacoesMax = 1 },
                Efeito = new FraquezaEfeito { BonusSucesso = 0.22, Descricao = "Estrutura óssea leve sucumbe a espuma compressiva." }
            },
            new()
            {
                Id = 3005L,
                Nome = "Instabilidade mutacional",
                Condicao = new FraquezaCondicao { MutacoesMin = 4 },
                Efeito = new FraquezaEfeito { BonusSucesso = 0.3, Descricao = "Mutação excessiva gera colapsos após supressão." }
            }
        };
    }
}
