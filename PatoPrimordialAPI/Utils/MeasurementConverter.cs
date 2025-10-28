using System;

namespace PatoPrimordialAPI.Utils;

public static class MeasurementConverter
{
    public static decimal ToCentimeters(decimal value, string unidade)
    {
        return Normalizar(unidade) switch
        {
            "cm" => value,
            "m" => value * 100m,
            "ft" => value * 30.48m,
            _ => throw new ArgumentException($"Unidade de altura desconhecida: {unidade}", nameof(unidade))
        };
    }

    public static decimal ToGrams(decimal value, string unidade)
    {
        return Normalizar(unidade) switch
        {
            "g" => value,
            "kg" => value * 1000m,
            "lb" => value * 453.59237m,
            _ => throw new ArgumentException($"Unidade de peso desconhecida: {unidade}", nameof(unidade))
        };
    }

    public static decimal ToMeters(decimal value, string unidade)
    {
        return Normalizar(unidade) switch
        {
            "m" => value,
            "cm" => value / 100m,
            "yd" => value * 0.9144m,
            _ => throw new ArgumentException($"Unidade de precisão desconhecida: {unidade}", nameof(unidade))
        };
    }
    public static decimal FromMeters(decimal value, string unidade)
    {
        return Normalizar(unidade) switch
        {
            "m" => value,
            "cm" => value * 100m,
            "yd" => value / 0.9144m,
            _ => throw new ArgumentException($"Unidade de precisão desconhecida: {unidade}", nameof(unidade))
        };
    }

    private static string Normalizar(string unidade) => unidade.Trim().ToLowerInvariant();
}
