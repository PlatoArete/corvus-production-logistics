# [COG] Corvus Production Logistics

## Overview

A lightweight production logistics mod for RimWorld that allows worktables and storage buildings to be connected through a buildable logistics conduit network.

Instead of relying entirely on pawns to manually haul ingredients to benches and finished products back into storage, connected production stations can access valid ingredients from linked storage and route completed outputs back into the network.

No belts. No drones. No pawn with twelve potatoes in their arms having an existential crisis halfway across the workshop.

Just clean, corporate-approved material flow.

## Features

### Buildable Logistics Conduit

Adds a new logistics conduit that can be placed to form production networks between storage buildings and worktables.

Use conduits to connect:

* Workbenches
* Storage buildings
* Production areas
* Dedicated material stockpiles
* Finished goods storage

The system is designed to fit naturally into workshop layouts without requiring huge rebuilds or strange production shrines.

### Automatic Network Discovery

Production Logistics automatically detects connected conduit runs and identifies which buildings belong to the same logistics network.

No manual linking.  
No configuration window labyrinth.  
No “assign this bench to Storage Node 14-B unless the moon is angry.”

Place the conduits, connect the buildings, and the network is discovered automatically.

### Linked Storage Detection

The mod detects valid storage buildings connected to the same conduit network as a worktable.

Connected storage can be used as part of the production workflow, allowing benches to see ingredients stored nearby through the logistics network rather than only relying on direct pawn delivery.

### Ingredient Availability for Bills

Vanilla bill work can use ingredient availability from linked storage buildings on the same conduit run.

This means a worktable connected to a logistics network can recognise materials held in connected storage when checking whether a bill can be performed.

Useful for:

* Centralised material storage
* Cleaner workshop layouts
* Dedicated production rooms
* Reducing unnecessary hauling traffic
* Keeping benches supplied without micromanaging local stockpiles

### Output Routing

For bills that are not set to `Drop on floor`, completed products can be routed back into valid connected storage.

When a finished item is produced, the system looks for appropriate storage linked to the same logistics network and routes the output there where possible.

This helps keep workshops clean and reduces the classic RimWorld production-room floor mosaic of meals, weapons, textiles, drugs, hats, and questionable decisions.

## Notes

Production Logistics is intended to enhance vanilla-style production rather than replace it with an entirely new factory system.

The goal is simple:

```text
Storage ↔ Conduit Network ↔ Worktable
```

Ingredients flow in.  
Products flow out.  
Colonists remain slightly less burdened by the endless tyranny of carrying things.

## Compatibility

Designed to work with vanilla bill-based production.

Mods that add normal worktables, recipes, and storage buildings should generally benefit from the system, provided they use standard RimWorld production and storage behaviour.

Further compatibility details will depend on how heavily other mods alter bill logic, storage logic, or item placement behaviour.

Tested well so far with:

* Vanilla RimWorld
* Adaptive Storage Framework
* Better Architect Menu
* Pick Up And Haul
* Project RimFactory
* Omni Bot

Still worth broader testing with:

* Fiann's Hauling
* Other heavily customised production systems

---

# COLONIAL PRODUCTION SOLUTIONS(TM)

## Logistics Conduit Network v1.0

### Moving Materials So Your Colonists Don't Have To(TM)

At Colonial Production Solutions, we understand the greatest threat to colony productivity is not famine, mechanoids, toxic fallout, or spontaneous social collapse.

It is walking.

Every second your colonists spend carrying steel, cloth, components, herbal medicine, or one regrettably important potato is a second they are not crafting, researching, mining, cooking, sewing, operating machinery, or contributing to shareholder value.

That is why we are proud to introduce the **Logistics Conduit Network**, a revolutionary production-support system that allows connected worktables and storage buildings to participate in a unified material flow architecture.

Through our proprietary conduit discovery technology, your workshops can now identify linked storage, access available ingredients, and route finished products back into valid connected storage with minimal colonist involvement.

### Revolutionary Features

**Conduit-Based Material Networks**  
Create clean, scalable workshop logistics using buildable conduit infrastructure.

**Automatic Network Discovery**  
Our system identifies connected production and storage assets without tedious manual assignment.

**Linked-Storage Ingredient Access**  
Worktables can recognise ingredients stored in valid connected storage buildings on the same conduit network.

**Output Routing Technology**  
Finished products can be routed back into suitable connected storage for bills that are not configured to drop items on the floor.

**Reduced Pawn Hauling Dependency**  
Because skilled labour should not be wasted carrying thirty units of steel from one side of the room to the other.

> "Before installing the Logistics Conduit Network, my best crafter spent half the day walking to storage. Now she spends all day making rifles. Morale has not improved, but output is excellent."
>
> - Production Overseer J████, Workshop Complex 12-C

**Colonial Production Solutions: Your Success Is Our Throughput(TM)**

---

Results may vary. Colonial Production Solutions is not responsible for conduit misplacement, storage misclassification, production bottlenecks, unexpected item routing, colonist dependency on automated logistics, or philosophical debates regarding whether hauling was ever meaningful labour. Do not insert hands, pets, prisoners, organs, ideology relics, or royal tribute into active conduit networks. Logistics Conduit Network is a Corvus Operations Group subsidiary technology.

**A Corvus Operations Group Subsidiary | Building Tomorrow's Rim, Today(TM)**

## Further Info

Further info can be found on GitHub:

[https://github.com/PlatoArete/corvus-production-logistics](https://github.com/PlatoArete/corvus-production-logistics)

## Repository Layout

* `About/`: RimWorld mod metadata
* `1.6/`: game content for RimWorld 1.6
* `Source/`: C# source and project files
* `docs/TEST_PLAN.md`: Tier 1 compatibility test checklist
* `PROJECT_META.md`: project scope and design reference

## Build

The C# project outputs the mod assembly to `1.6/Assemblies/CorvusProductionLogistics.dll`.

Open or build:

* `Source/CorvusProductionLogistics/CorvusProductionLogistics.csproj`

Command-line build:

```powershell
dotnet build 'Source\CorvusProductionLogistics\CorvusProductionLogistics.csproj' --no-incremental
```

## RimWorld References

The project references the local RimWorld install at:

* `D:\Program Files\Steam\steamapps\common\RimWorld\RimWorldWin64_Data\Managed`

If your RimWorld install lives elsewhere, update the hint paths in the project file.
