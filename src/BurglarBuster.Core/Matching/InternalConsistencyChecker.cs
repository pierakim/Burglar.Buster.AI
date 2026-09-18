using BurglarBuster.Core.Entities;

namespace BurglarBuster.Core.Matching;

/// <summary>
/// A single Person row can have several PhysicalDescription child rows — separate
/// observations recorded on different dates, not separate people. This checks whether
/// those observations of the SAME person agree with each other closely enough to be
/// plausible (e.g. height shouldn't vary by 20cm between two adult observations).
/// It doesn't pick a "correct" value or merge them — it only flags disagreement.
/// Independent of any search criteria. Feeds the conflicting_information outcome
/// (spec §12); see BB-0012 in docs/fixture-scenarios.md for the fixture this exists
/// to catch.
/// </summary>
internal static class InternalConsistencyChecker
{
    public static IReadOnlyList<string> GetInternalConflicts(Person person)
    {
        var conflicts = new List<string>();

        var heights = person.PhysicalDescriptions
            .Where(d => d.ApproximateHeightCm.HasValue)
            .Select(d => d.ApproximateHeightCm!.Value)
            .ToList();

        if (heights.Count >= 2)
        {
            var spread = heights.Max() - heights.Min();
            if (spread > MatchingPolicy.ApproximateHeightToleranceCm * 2)
            {
                conflicts.Add($"recorded height varies by {spread} cm across observations");
            }
        }

        var eyeColours = person.PhysicalDescriptions
            .Select(d => d.EyeColour)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(NameNormalizer.Normalize)
            .Distinct()
            .ToList();

        if (eyeColours.Count > 1)
        {
            conflicts.Add("recorded eye colour is inconsistent across observations");
        }

        return conflicts;
    }
}
