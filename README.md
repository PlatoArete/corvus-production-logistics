# Corvus Production Logistics

Corvus Production Logistics is a RimWorld mod focused on logistics-assisted production.

The MVP goal for Tier 1 is:

- a buildable logistics conduit
- automatic network discovery
- detection of linked storage buildings and worktables on the same conduit run
- linked-storage ingredient availability for vanilla bill work
- output routing back into valid connected storage for non-`Drop on floor` bills
- logistics input/output hoppers for machines that interact through storage cells

## Repository Layout

- `About/`: RimWorld mod metadata
- `1.6/`: game content for RimWorld 1.6
- `Source/`: C# source and project files
- `docs/TEST_PLAN.md`: Tier 1 compatibility test checklist
- `PROJECT_META.md`: project scope and design reference

## Current MVP Status

Implemented:

- standard Git repo scaffold
- RimWorld mod layout
- buildable logistics conduit def
- logistics research project
- C# assembly/project setup
- in-game conduit network discovery for adjacent storage buildings and worktables
- linked-storage ingredients count for vanilla bills
- linked ingredients are consumed directly from connected storage
- completed products route into valid connected storage for stockpile output modes
- explicit `Drop on floor` output remains vanilla behavior
- harvested crops from connected plant growers/hydroponics basins route into valid connected storage
- 1x1 input and output logistics hoppers for compatibility with production buildings that do not use vanilla bills
- storage footprints participate in logistics networks, so storage buildings connect naturally without conduit around every occupied tile
- noisy logistics debug logging is disabled by default while the debug helpers remain in source for targeted testing

Still to validate:

- Fiann's Hauling compatibility
- broader special/custom production systems

## Build

The C# project outputs the mod assembly to `1.6/Assemblies/CorvusProductionLogistics.dll`.

Open or build:

- `Source/CorvusProductionLogistics/CorvusProductionLogistics.csproj`

Command-line build:

```powershell
dotnet build 'Source\CorvusProductionLogistics\CorvusProductionLogistics.csproj' --no-incremental
```

## RimWorld References

The project references the local RimWorld install at:

- `D:\Program Files\Steam\steamapps\common\RimWorld\RimWorldWin64_Data\Managed`

If your RimWorld install lives elsewhere, update the hint paths in the project file.
