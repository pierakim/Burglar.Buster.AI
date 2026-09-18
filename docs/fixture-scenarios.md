# Fixture scenarios

Internal reference for manual testing and evaluation-scenario authoring. **These IDs
are never surfaced to the model's prompt** — the agent should find its way to these
records purely through search, the same as it would with any of the ~1,000 seeded
people (see `BURGLAR_BUSTER_PROJECT_SPEC.md` §8, §14).

Fixtures are hand-authored in
[`src/BurglarBuster.Infrastructure/Seeding/FixtureScenarios.cs`](../src/BurglarBuster.Infrastructure/Seeding/FixtureScenarios.cs)
and always seeded first, at IDs `BB-0001`–`BB-0015`. The random bulk generator starts
at `BB-0100` and never touches this range, so these IDs and their data are stable
across reseeds as long as `FixtureScenarios.cs` itself doesn't change.

| ID | Scenario | Notes |
| --- | --- | --- |
| `BB-0001` | Unique exact match | Daniel Miller, DOB 1989-04-12. Matches the spec's own §13 HTTP example ("~35, ~180cm, brown eyes, dark hair, previously lived near Camden"). Has a previous address in Camden Reach and a current one in Silverbrook, plus a case reference. |
| `BB-0002`, `BB-0003` | Shared surname | Robert Miller and Emily Miller — same family name as BB-0001, unrelated people, different DOB/address. |
| `BB-0004` | Similar/misspelled name | Daniel **Millar** — same given name as BB-0001, family name off by one letter, distinct person. |
| `BB-0005` | Alias | Robert Nguyen, recorded alias "Bobby Nguyen". |
| `BB-0006` | Previous address | Sarah Thompson — one previous address (Elmsworth, closed-ended) and one current (Stonebridge). |
| `BB-0007` | Incomplete date of birth | Michael Brooks — `DateOfBirth` is null. |
| `BB-0008`, `BB-0009` | Similar physical descriptions | Olivia Carter and Chloe Carter — identical height/eye/hair, otherwise unrelated (different name, DOB, address). Demonstrates appearance alone must not produce a strong match. |
| `BB-0010`, `BB-0011` | Ambiguous candidates | James Ellison and Jason Ellison — same surname, same birth year, same locality (Stonebridge), similar given name. A vague description should leave both plausible. |
| `BB-0012` | Conflicting information | William Foster — two physical observations 4 years apart with a 20cm height discrepancy. |
| `BB-0013`, `BB-0014` | Potential duplicate pair | Sophia Grant / Sofia Grant — same DOB, same address, near-identical name spelling. Candidate for the `potential_duplicate` outcome. |
| `BB-0015` | Minimal/incomplete record | Thomas Harrington — no DOB, no address, no physical description, no alias. |

## No-match scenario

Not a seeded record — a canonical description that should **not** match anyone, for
exercising the `no_match` outcome:

> "A woman named Persephone Winterbourne, previously living near a place called
> Hollowmere Ridge."

`Persephone`/`Winterbourne`/`Hollowmere Ridge` are deliberately long, invented words
chosen so this is **collision-proof by construction**, not just unlikely to collide:
Levenshtein edit distance is always at least the length difference between two
strings, and every name/locality pool entry is short enough (≤12 characters) that no
pool entry can be within the matching engine's similarity tolerance
(`MatchingPolicy.NameSimilarityMaxEditDistance = 2`) of any of these three words,
regardless of what the random bulk generator happens to produce for a given seed.

An earlier version of this description ("Sarah Blackwood-Chen... Nowhere Creek")
looked safe but wasn't: `Sarah` is a real pool entry (used by `BB-0006` and
occasionally generated in bulk data), so a bulk-generated "Sarah" who also happened to
land in the stated approximate-age range had a non-trivial chance of producing a weak
plausible match — undermining a "definite no-match" fixture. Prefer names/localities
with no partial overlap with `SyntheticDataCatalog.cs`'s pools when adding more
no-match scenarios, rather than ones that are merely statistically unlikely to collide.

## Automated proof

`tests/BurglarBuster.Tests/Matching/MatchingEngineFixtureTests.cs` exercises the real
pipeline (`PersonCandidateRepository` + `MatchingEngine`) against these fixtures and
asserts each of the six outcomes above plus reproducibility (identical criteria always
produce identical results).
