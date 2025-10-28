using System;
using PatoPrimordialAPI.Domain.Entities;
using PatoPrimordialAPI.Utils;

namespace PatoPrimordialAPI.Services.Ingestao;

public class PatoObservationProcessor
{
    public Pato CriarNovo(string codigo, ObservacaoNormalizada observacao)
    {
        var agora = observacao.ObservadoEm;
        var pato = new Pato
        {
            Codigo = codigo,
            CriadoEm = agora,
            AtualizadoEm = agora
        };

        Aplicar(pato, observacao, true);
        return pato;
    }

    public void Aplicar(Pato pato, ObservacaoNormalizada observacao, bool sobrescreverTudo = false)
    {
        var confiancaAtual = pato.PrecisaoM > 0 ? 1d / (double)pato.PrecisaoM : 0d;
        var deveAtualizarPorConfianca = sobrescreverTudo || observacao.Confianca >= confiancaAtual;

        if (deveAtualizarPorConfianca)
        {
            pato.AlturaCm = observacao.AlturaCm;
            pato.PesoG = observacao.PesoG;
            pato.PrecisaoM = observacao.PrecisaoM;
            pato.Cidade = observacao.Cidade;
            pato.Pais = observacao.Pais;
            pato.Latitude = observacao.Latitude;
            pato.Longitude = observacao.Longitude;
            pato.PontoReferencia = observacao.PontoReferencia;
            pato.PontoReferenciaId = observacao.PontoReferencia?.Id;
            pato.PoderNome = observacao.PoderNome ?? pato.PoderNome;
            pato.PoderDescricao = observacao.PoderDescricao ?? pato.PoderDescricao;
            pato.PoderTagsCsv = observacao.PoderTagsCsv ?? pato.PoderTagsCsv;
        }
        else
        {
            pato.PoderNome ??= observacao.PoderNome;
            pato.PoderDescricao ??= observacao.PoderDescricao;
            pato.PoderTagsCsv ??= observacao.PoderTagsCsv;
        }

        if (EstadoPatoHierarchy.DeveAtualizar(pato.Estado, observacao.Estado))
        {
            pato.Estado = observacao.Estado;
        }

        pato.Bpm = observacao.Bpm;

        if (observacao.MutacoesQtde.HasValue)
        {
            pato.MutacoesQtde = Math.Max(pato.MutacoesQtde ?? 0, observacao.MutacoesQtde.Value);
        }

        pato.AtualizadoEm = observacao.ObservadoEm;
    }
}
