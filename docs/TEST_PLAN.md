# Corvus Production Logistics Test Plan

Use this checklist for Tier 1 compatibility testing after code changes.

## Core Vanilla Flow

- Build one visible logistics conduit between one storage building and one vanilla workbench.
- Confirm the conduit inspect string reports the expected network, storage, and worktable counts.
- Put all bill ingredients in connected storage and none near the bench.
- Confirm a pawn can start the bill without hauling ingredients from storage to the bench first.
- Confirm the finished product routes to connected storage when the bill output mode is stockpile.
- Confirm the finished product drops on the floor when the bill output mode is `Drop on floor`.

## Storage Selection

- Connect two storage buildings with different filters and confirm output only enters valid storage.
- Connect two valid storage buildings with different priorities and confirm vanilla priority is respected.
- Fill the valid connected storage and confirm the product falls back safely instead of crashing.
- Test `Take to best stockpile` and `Take to specific stockpile` bill modes.
- Build a logistics hopper and confirm it counts as connected storage when linked to conduit.
- Confirm hopper filters and priorities affect routing like other storage buildings.

## Recipe Coverage

- Test a single-output recipe with a normal item product.
- Test a multi-output recipe and confirm the primary product is routed and extra products behave acceptably.
- Test a stuff-based recipe such as apparel or art.
- Test a recipe using partial stacks from connected storage.
- Test a recipe that consumes all of an ingredient stack.

## Mod Compatibility Smoke Tests

- Test a popular modded workbench that subclasses or behaves like `Building_WorkTable`.
- Test a modded storage building that subclasses or behaves like `Building_Storage`.
- Test with Pick Up And Haul enabled.
- Test with Craft With Color enabled.
- Test with Nice Bill Tab / Nice Inventory Tab enabled.

## Agricultural Output Routing

- Connect a hydroponics basin to valid connected storage with logistics conduit.
- Grow and harvest a crop in the connected hydroponics basin.
- Confirm harvested crops route into connected storage instead of being placed at the pawn.
- Confirm storage filters are respected by allowing the crop in one connected storage building and disallowing it in another.
- Confirm harvest output falls back to vanilla floor placement if no connected storage accepts the crop.
- Confirm ordinary field crops not grown on a connected plant grower still behave as vanilla.
- Confirm additional harvest yields, if any active mod adds them, do not crash and route where valid.

## Regression Checks

- Confirm disconnected storage does not contribute ingredients.
- Confirm disconnected benches do not pull from logistics storage.
- Confirm hidden conduits connect networks the same way visible conduits do.
- Confirm saving and reloading a map with active networks does not create load errors.
- Confirm no routine `[Corvus Production Logistics]` log messages appear during normal operation.
