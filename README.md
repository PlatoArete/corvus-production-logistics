# Corvus Production Logistics

Corvus Production Logistics is a RimWorld mod focused on logistics-assisted production.

The MVP goal for Tier 1 is:

- a buildable logistics conduit
- automatic network discovery
- detection of linked storage buildings and worktables on the same conduit run
- linked-storage ingredient availability for vanilla bill work
- output routing back into valid connected storage for non-`Drop on floor` bills

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
- a 1x1 logistics hopper storage endpoint for compatibility with production buildings that do not use vanilla bills

Still to validate:

- broader vanilla bench coverage
- modded worktables that use vanilla bills
- modded storage buildings that use vanilla storage groups
- special/custom production systems

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
