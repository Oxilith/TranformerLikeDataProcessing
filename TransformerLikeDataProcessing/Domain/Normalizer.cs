namespace TransformerLikeDataProcessing.Domain;

public static class Normalizer
{
    // Auto-scale numeric values by order of magnitude.
    public static double Normalize(double value)
    {
        if (value == 0) return 0;
        var digits = (int)Math.Floor(Math.Log10(Math.Abs(value))) + 1;
        var maxPossible = Math.Pow(10, digits);
        return value / maxPossible;
    }
}