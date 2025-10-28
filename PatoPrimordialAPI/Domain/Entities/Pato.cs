using System;
using System.Collections.Generic;

namespace PatoPrimordialAPI.Domain.Entities;

public class Pato
{
    public long Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public decimal AlturaCm { get; set; }
    public decimal PesoG { get; set; }
    public string Cidade { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public decimal PrecisaoM { get; set; }
    public long? PontoReferenciaId { get; set; }
    public PontoReferencia? PontoReferencia { get; set; }
    public string Estado { get; set; } = "desconhecido";
    public int? Bpm { get; set; }
    public int? MutacoesQtde { get; set; }
    public string? PoderNome { get; set; }
    public string? PoderDescricao { get; set; }
    public string? PoderTagsCsv { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime AtualizadoEm { get; set; }

    public AnalisePato? Analise { get; set; }
    public ICollection<Avistamento> Avistamentos { get; set; } = new List<Avistamento>();
}
