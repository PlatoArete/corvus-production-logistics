# Corvus Production Logistics

## Purpose

This project is a RimWorld mod focused on logistics-assisted production.

The initial goal is to remove unnecessary pawn hauling between storage and workbenches without replacing the vanilla bill system. The player should still think in terms of normal items, normal storage, and normal bills, but a connected logistics network should make linked ingredients effectively available at the bench and return finished products back to storage automatically.

This document is a project-level reference for scope, design intent, implementation direction, and compatibility expectations.

## Vision

The long-term vision is a two-tier production ecosystem:

- Tier 1: industrial logistics infrastructure
- Tier 2: high-tech autonomous replication

The project should start with Tier 1 only and avoid premature work on Tier 2 until the core logistics model is proven stable.

## Design Principles

- Preserve vanilla production flow where possible.
- Prefer compatibility through shared vanilla systems, not per-bench patches.
- Treat items as real `Thing`s, not abstract network fluid/resources.
- Keep the MVP understandable to players and maintainable in code.
- Build the logistics system first; add custom UI only when the vanilla UI becomes a real bottleneck.
- Avoid hard dependencies unless they solve a core problem cleanly.

## Non-Goals For MVP

- No Tier 2 replicator implementation.
- No requirement for Vanilla Expanded Framework.
- No requirement for Adaptive Storage Framework.
- No attempt to support every custom machine system automatically.
- No attempt to replace all hauling in the colony.
- No mixed simulation of physical tube packets unless needed later for visuals only.

## Tier 1 Product Goal

Tier 1 should feel like a pneumatic logistics layer built on top of vanilla crafting.

Expected player-facing behavior:

- The player builds conduit-like logistics lines.
- Storage endpoints and workbenches can be attached to the same network.
- If required ingredients exist in linked storage, they should count as available to eligible bills.
- Pawns should not need to manually haul those ingredients from stockpile to bench in the usual way.
- Finished products should be returned to linked storage automatically where possible.
- Vanilla bill settings should remain meaningful.

The player fantasy is not "a new factory minigame". The fantasy is "my colony has infrastructure now, so workbenches are fed automatically".

## Core Technical Thesis

The correct integration layer is the vanilla bill pipeline, not individual workbenches.

Most normal modded workbenches will work automatically if they continue to use:

- `Building_WorkTable`
- `BillStack`
- vanilla `WorkGiver_DoBill`
- vanilla `JobDriver_DoBill`
- normal `RecipeDef`s

This means Tier 1 should be implemented as an extension of vanilla ingredient availability, reservation, consumption, and output routing, rather than as custom support for specific benches.

## Why Existing Frameworks Are Not Core Dependencies

### Vanilla Expanded Framework

VEF's `PipeSystem` is not a good fit for the core mechanic. It appears to be a resource-network system with converters and processors, not a native item logistics extension for vanilla bills.

It may be useful for inspiration, but it should not be the foundation of the project.

### Adaptive Storage Framework

ASF is useful for advanced storage presentation, rendering, and storage UX, but it does not solve bill integration or ingredient routing. It should be treated as a compatibility target, not a required dependency.

## Compatibility Philosophy

Compatibility should be categorized explicitly.

### Automatic Support Target

These should work automatically or near-automatically if Tier 1 is implemented correctly:

- Vanilla `Building_WorkTable` crafting
- Most modded worktables that use vanilla bills
- Mods that add new recipes to standard worktables

### Likely With Minor Validation

These likely work, but should be tested deliberately:

- Vanilla autonomous bill users such as `Building_WorkTableAutonomous`
- `Building_MechGestator`

These use a special bill path, but still live close enough to vanilla bill systems that support is realistic.

### Explicit Compatibility Required

These are out of the automatic-support path:

- `Building_GeneAssembler`
- `[SYR] Processor Framework`
- Processor-style machines with their own work givers and job drivers
- Custom factory systems
- Mods that replace ingredient selection or hauling behavior

These systems should only be supported through dedicated compatibility modules if they are high-value enough to justify the maintenance cost.

## MVP Scope

The MVP should deliver only the minimum Tier 1 experience.

### MVP Features

- Conduit-like logistics building
- Storage endpoint building or comp
- Workbench endpoint building or comp
- Network/linking model between endpoints
- Linked storage ingredients count as available to vanilla bills
- Correct reservation and consumption behavior
- Correct product return to linked storage
- Basic player feedback for linked/unlinked state

### MVP Exclusions

- No Tier 2 replicator
- No feedstock system
- No custom bill browser
- No explicit support for Processor Framework
- No broad compatibility layer for all autonomous/custom machines
- No advanced art/animation requirements beyond readable conduit visuals

## Technical Risks

The core risks for Tier 1 are not UI risks. They are logic and compatibility risks.

### Ingredient Discovery

Linked storage must be included in ingredient search without allowing invalid jobs to start.

### Reservation Integrity

If multiple pawns or benches are drawing from the same linked storage, the system must prevent double-booking ingredients.

### Consumption Correctness

Items that count as available must also be consumed correctly when the bill completes or advances.

### Output Routing

Finished goods must be stored back into the linked network cleanly, without duplication, loss, or unexpected floor drops when avoidable.

### Storage Mod Interaction

The system should avoid assumptions that only vanilla stockpiles exist. It should work with normal storage group logic as much as possible.

### Special Bill Paths

Autonomous or custom bill systems may need validation even if they appear close to vanilla.

## Likely Patch Areas

The following areas are the most likely implementation points:

- `WorkGiver_DoBill`
- `JobDriver_DoBill`
- vanilla bill ingredient selection helpers
- product storage handoff after recipe completion

The objective is:

- include linked items during ingredient search
- ensure selected linked items are actually reserved and consumed
- allow output to return to linked storage

The project should avoid per-bench patches unless a specific compatibility module is being written.

## Conceptual Architecture

The exact classes may change, but the architecture should look roughly like this:

- A network/building layer representing pneumatic logistics lines
- A storage endpoint representation
- A workbench endpoint representation
- A network service that can answer:
  - what storage is linked to this bench
  - what items are available to this bench
  - what can be reserved
  - where outputs should go
- A reservation layer for in-flight logistics usage
- Bill integration patches that consult the network service

## Suggested Milestones

### Milestone 1: Foundation

- Create mod skeleton
- Create conduit-like logistics building defs
- Create endpoint/linking model
- Add minimal debug inspection output

### Milestone 2: Ingredient Availability

- Make linked storage visible to vanilla ingredient search
- Prove that bills recognize linked ingredients as available
- Add debug tools/logging for search results

### Milestone 3: Reservation And Consumption

- Ensure chosen ingredients can be reserved correctly
- Ensure ingredients are consumed correctly
- Prevent duplicate use under concurrent crafting

### Milestone 4: Output Return

- Route completed products to linked storage
- Respect sensible fallback behavior if no valid destination exists
- Preserve vanilla `Drop on floor` behavior when the player explicitly selects it

## Current Implementation Notes

- Visible and hidden logistics conduits connect storage buildings and worktables by adjacency.
- Linked storage is added to vanilla bill ingredient search for `Building_WorkTable` users.
- Linked ingredients are moved into `Job.placedThings` so vanilla recipe completion consumes them without staging haul jobs.
- Product output uses vanilla storage-cell selection, then replaces the generated haul handoff with direct insertion into the selected connected storage cell.
- Output routing respects storage filters and priorities because destination selection delegates back to vanilla `StoreUtility`.
- Hydroponics/agricultural output routing is implemented separately from bills by intercepting vanilla harvest product placement for plants grown on connected `Building_PlantGrower` buildings.
- A 1x1 logistics hopper acts as a vanilla storage endpoint for odd modded production buildings that need a physical input/output buffer.
- Compatibility testing should follow `docs/TEST_PLAN.md`.

### Milestone 5: Validation

- Test against multiple vanilla benches
- Test against mech gestator
- Test with at least one storage mod
- Verify failure modes are safe and understandable

### Milestone 6: UX Pass

- Improve inspect strings, overlays, and player feedback
- Add any small QoL controls needed for linking or diagnostics

## Tier 2 Direction

Tier 2 should remain a future system, not a design constraint on the MVP.

Current direction:

- A dedicated replicator building
- A custom recipe/bill browser
- Default recipe list limited to relevant crafted items
- Player-configurable allowlist/disallowlist
- Support for direct ingredients first
- Optional feedstock fallback later

### Tier 2 Recipe Relevance Guidelines

Likely relevant:

- Normal crafted items
- Apparel
- Weapons
- Components and similar manufactured goods

Likely not relevant:

- Surgery
- Corpse processing
- Pawn-targeted work
- Ritual/special-case systems
- Recipes with highly custom or unsafe worker logic

## Feedstock Direction

If feedstock is added later, it should probably be a fallback path rather than a fully mixed ingredient substitution model.

Preferred future behavior:

- use normal ingredients if available
- otherwise use feedstock if enough exists
- otherwise cannot start

This is much simpler to balance and implement than per-ingredient free substitution.

## Testing Priorities

The project should test behavior, not just compilation.

High-priority test cases:

- Single bench, single storage, simple recipe
- Multiple benches on one network
- Multiple pawns competing for the same ingredients
- Recipes with multiple ingredient types
- Recipes producing stackable outputs
- Recipes producing non-stackable outputs
- No valid output storage available
- Bench disconnected mid-process
- Storage emptied while a job is being planned
- Mech gestator ingredient gathering

## Open Questions

- Should storage endpoints be explicit buildings, or should normal storage become linkable directly?
- Should benches need a separate endpoint attachment, or should the conduit adjacent to a bench be enough?
- How should players visualize network membership?
- Should output routing be per-bill, per-bench, or purely network-driven?
- How much explicit compatibility is worth shipping during MVP?

## Recommended Near-Term Next Step

After this document, the next artifact should be a technical design document for Tier 1 that names:

- planned defs
- planned comps/services
- likely Harmony patch targets
- reservation strategy
- output routing strategy
- debugging/test plan

That document should be implementation-oriented and should stay narrower than this project meta document.
