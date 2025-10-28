using System;

namespace PatoPrimordialAPI.Utils;

public static class MeasurementValidator
{
    public const decimal AlturaMinCm = 10m;
    public const decimal AlturaMaxCm = 3000m;
    public const decimal PesoMinG = 100m;
    public const decimal PesoMaxG = 200_000_000m;
    public const decimal PrecisaoMinM = 0.04m;
    public const decimal PrecisaoMaxM = 30m;

    public static decimal LimitarPrecisao(decimal precisaoM)
    {
        if (precisaoM < PrecisaoMinM)
        {
            return PrecisaoMinM;
        }

        if (precisaoM > PrecisaoMaxM)
        {
            return PrecisaoMaxM;
        }

        return precisaoM;
    }

    public static decimal LimitarPrecisaoNominalMinCm(decimal precisaoCm)
    {
        var minimoCm = PrecisaoMinM * 100m;
        var maximoCm = PrecisaoMaxM * 100m;

        if (precisaoCm < minimoCm)
        {
            return minimoCm;
        }

        if (precisaoCm > maximoCm)
        {
            return maximoCm;
        }

        return precisaoCm;
    }

    public static void ValidarAltura(decimal alturaCm)
    {
        if (alturaCm is < AlturaMinCm or > AlturaMaxCm)
        {
            throw new ArgumentOutOfRangeException(nameof(alturaCm),
                $"Altura normalizada deve estar entre {AlturaMinCm}cm e {AlturaMaxCm}cm.");
        }
    }

    public static void ValidarPeso(decimal pesoG)
    {
        if (pesoG is < PesoMinG or > PesoMaxG)
        {
            throw new ArgumentOutOfRangeException(nameof(pesoG),
                $"Peso normalizado deve estar entre {PesoMinG}g e {PesoMaxG}g.");
        }
    }

    public static void ValidarPrecisao(decimal precisaoM)
    {
        if (precisaoM is < PrecisaoMinM or > PrecisaoMaxM)
        {
            throw new ArgumentOutOfRangeException(nameof(precisaoM),
                $"Precisão normalizada deve estar entre {PrecisaoMinM}m e {PrecisaoMaxM}m.");
        }
    }
}
