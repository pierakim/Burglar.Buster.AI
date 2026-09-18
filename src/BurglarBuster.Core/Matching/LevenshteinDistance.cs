namespace BurglarBuster.Core.Matching;

/// <summary>Standard edit-distance calculation, used to detect likely misspellings.</summary>
/// If you are there you are curious: en.wikipedia.org/wiki/Levenshtein_distance
internal static class LevenshteinDistance
{
    public static int Compute(string a, string b)
    {
        if (a.Length == 0)
        {
            return b.Length;
        }

        if (b.Length == 0)
        {
            return a.Length;
        }

        var previousRow = new int[b.Length + 1];
        var currentRow = new int[b.Length + 1];

        for (var j = 0; j <= b.Length; j++)
        {
            previousRow[j] = j;
        }

        for (var i = 1; i <= a.Length; i++)
        {
            currentRow[0] = i;

            for (var j = 1; j <= b.Length; j++)
            {
                var deletionCost = previousRow[j] + 1;
                var insertionCost = currentRow[j - 1] + 1;
                var substitutionCost = previousRow[j - 1] + (a[i - 1] == b[j - 1] ? 0 : 1);

                currentRow[j] = Math.Min(Math.Min(deletionCost, insertionCost), substitutionCost);
            }

            (previousRow, currentRow) = (currentRow, previousRow);
        }

        return previousRow[b.Length];
    }
}
