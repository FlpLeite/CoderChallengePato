using System;
using PatoPrimordialAPI.Domain.Entities;

namespace PatoPrimordialAPI.Services.Ingestao;

public record ObservacaoNormalizada(
    decimal AlturaCm,
    decimal PesoG,
    decimal PrecisaoM,
    double Confianca,
    string Cidade,
    string Pais,
    double Latitude,
    double Longitude,
    string Estado,
    int? Bpm,
    int? MutacoesQtde,
    string? PoderNome,
    string? PoderDescricao,
    string? PoderTagsCsv,
    DateTime ObservadoEm,
    PontoReferencia? PontoReferencia
);
