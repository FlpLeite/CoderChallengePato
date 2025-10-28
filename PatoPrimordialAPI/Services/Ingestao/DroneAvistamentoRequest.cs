using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PatoPrimordialAPI.Services.Ingestao;

public record DroneAvistamentoRequest
{
    [JsonPropertyName("drone_id")]
    public long DroneId { get; init; }

    [JsonPropertyName("numero_serie")]
    public string NumeroSerie { get; init; } = string.Empty;

    [JsonPropertyName("fabricante")]
    public string? FabricanteNome { get; init; }

    [JsonPropertyName("marca")]
    public string? Marca { get; init; }

    [JsonPropertyName("pais_origem")]
    public string? PaisOrigem { get; init; }

    [JsonPropertyName("altura_valor")]
    public decimal AlturaValor { get; init; }

    [JsonPropertyName("altura_unidade")]
    public string AlturaUnidade { get; init; } = string.Empty;

    [JsonPropertyName("peso_valor")]
    public decimal PesoValor { get; init; }

    [JsonPropertyName("peso_unidade")]
    public string PesoUnidade { get; init; } = string.Empty;

    [JsonPropertyName("lat")]
    public double Latitude { get; init; }

    [JsonPropertyName("lon")]
    public double Longitude { get; init; }

    [JsonPropertyName("precisao_valor")]
    public decimal PrecisaoValor { get; init; }

    [JsonPropertyName("precisao_unidade")]
    public string PrecisaoUnidade { get; init; } = string.Empty;

    [JsonPropertyName("cidade")]
    public string Cidade { get; init; } = string.Empty;

    [JsonPropertyName("pais")]
    public string Pais { get; init; } = string.Empty;

    [JsonPropertyName("estado_pato")]
    public string EstadoPato { get; init; } = string.Empty;

    [JsonPropertyName("bpm")]
    public int? Bpm { get; init; }

    [JsonPropertyName("mutacoes_qtde")]
    public int? MutacoesQtde { get; init; }

    [JsonPropertyName("poder")]
    public PoderPayload? Poder { get; init; }

    public record PoderPayload
    {
        [JsonPropertyName("nome")]
        public string? Nome { get; init; }

        [JsonPropertyName("descricao")]
        public string? Descricao { get; init; }

        [JsonPropertyName("tags")]
        public IReadOnlyCollection<string>? Tags { get; init; }
    }
}
