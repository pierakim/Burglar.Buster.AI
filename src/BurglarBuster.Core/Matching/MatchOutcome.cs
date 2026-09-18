using System.Text.Json.Serialization;

namespace BurglarBuster.Core.Matching;

/// <summary>
/// The only allowed investigation outcomes — see spec §12. JSON names are pinned
/// explicitly to the spec's own snake_case values so they're stable regardless of
/// whatever naming policy a given serialization context uses elsewhere.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<MatchOutcome>))]
public enum MatchOutcome
{
    [JsonStringEnumMemberName("strong_candidate")]
    StrongCandidate,

    [JsonStringEnumMemberName("ambiguous")]
    Ambiguous,

    [JsonStringEnumMemberName("no_match")]
    NoMatch,

    [JsonStringEnumMemberName("insufficient_information")]
    InsufficientInformation,

    [JsonStringEnumMemberName("conflicting_information")]
    ConflictingInformation,

    [JsonStringEnumMemberName("potential_duplicate")]
    PotentialDuplicate,

    [JsonStringEnumMemberName("manual_review_required")]
    ManualReviewRequired,
}
