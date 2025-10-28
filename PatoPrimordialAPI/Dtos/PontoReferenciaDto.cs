using System;

namespace PatoPrimordialAPI.Dtos;

public record PontoReferenciaDto
{
    public long Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public double RaioMetros { get; init; }
    public double? DistanciaMetros { get; init; }
}
