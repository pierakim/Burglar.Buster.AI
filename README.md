# Burglar Buster

Burglar Buster is a **fictional, educational** identity-resolution and record-triage
application. It exists to teach LLM tool/function calling, a manually implemented
agent loop, and the separation between probabilistic reasoning and deterministic
business logic over structured data.

> **This is not a real law-enforcement system.** All people, addresses, aliases and
> case references are synthetic and generated. The application never performs facial
> recognition or biometric matching, never infers guilt/criminality/dangerousness, and
> never uses protected traits (race, ethnicity, religion, health, disability, sexuality)
> for matching or ranking. A result is a candidate for **human verification only** —
> never a confirmed identity produced autonomously by the model. `no_match` means only
> that no likely record was found; it is never proof that a person is unknown, innocent
> or cleared. See [`CLAUDE.md`](CLAUDE.md) and [`PROJECT.md`](PROJECT.md) for the full
> list of safety and domain boundaries, which are mandatory for every milestone.

This is a learning project driven by [`BURGLAR_BUSTER_PROJECT_SPEC.md`](BURGLAR_BUSTER_PROJECT_SPEC.md)
and implemented incrementally, one milestone at a time. See
[`USING_CLAUDE.md`](USING_CLAUDE.md) for how sessions are structured and
[`CURRENT_STEP.md`](CURRENT_STEP.md) for what is actively being worked on.

## Status

Milestone 0 (repository and learning-workflow initialization) is in progress. No
Function app, database or model integration exists yet — see
[`IMPLEMENTATION_PLAN.md`](IMPLEMENTATION_PLAN.md).

## Prerequisites

- [.NET SDK 10](https://dotnet.microsoft.com/) (developed against `10.0.302`; isolated-worker
  Azure Functions on .NET 10 has been GA since February 2026)
- [Azure Functions Core Tools v4](https://learn.microsoft.com/azure/azure-functions/functions-run-local)
  (developed against `4.13.0`)
- An existing Azure AI Foundry resource with a **tool-capable chat model deployment**
  (endpoint + deployment name; API key or Entra ID credential). This project does not
  provision Foundry resources — you must already have one available for Milestone 5
  onward.
- git

## Project layout

```text
src/
  BurglarBuster.Functions/       HTTP functions, configuration, composition root
  BurglarBuster.Core/            domain models, matching, outcome policy, agent loop contracts
  BurglarBuster.Infrastructure/  EF Core, repositories/tool implementations, Foundry client
tests/
  BurglarBuster.Tests/            unit and component tests
  BurglarBuster.IntegrationTests/ optional real-model integration tests
samples/
  evaluation-scenarios.json       (added in Milestone 8)
scripts/
  evaluate.*                      (added in Milestone 8)
```

Project files under `src/`/`tests/` are created in Milestone 1 onward; the folders
exist now only as placeholders for the agreed structure.

## Local secrets

Copy `local.settings.example.json` to `local.settings.json` (git-ignored) and fill in
your own Foundry endpoint/deployment/key once Milestone 5 is reached. Never commit
`local.settings.json` or any real endpoint/key.

## Documentation map

- [`CLAUDE.md`](CLAUDE.md) — permanent operating rules and safety boundaries for every session
- [`PROJECT.md`](PROJECT.md) — architecture, domain model, contracts, definition of done
- [`IMPLEMENTATION_PLAN.md`](IMPLEMENTATION_PLAN.md) — milestones and acceptance criteria
- [`CURRENT_STEP.md`](CURRENT_STEP.md) — what is active right now
- [`DECISIONS.md`](DECISIONS.md) — architecture decision log
- [`NOTES.md`](NOTES.md) — working notebook (learnings, commands, questions)
- [`USING_CLAUDE.md`](USING_CLAUDE.md) — how to run a session with Claude on this project
