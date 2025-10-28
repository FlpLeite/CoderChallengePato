using System;
using System.Collections.Generic;

namespace PatoPrimordialAPI.Domain.Entities;

public class PontoReferencia
{
    public long Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double RaioMetros { get; set; }

    public ICollection<Pato> Patos { get; set; } = new List<Pato>();
}
