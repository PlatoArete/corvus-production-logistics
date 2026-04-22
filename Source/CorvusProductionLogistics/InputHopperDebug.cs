using RimWorld;
using Verse;
using Verse.AI;

namespace CorvusProductionLogistics;

public static class InputHopperDebug
{
    public static bool Enabled = false;

    public static bool TryGetInputHopperAtCell(Map map, IntVec3 cell, out Building_InputLogisticsHopper hopper)
    {
        hopper = null;
        if (map == null || !cell.IsValid)
        {
            return false;
        }

        SlotGroup slotGroup = cell.GetSlotGroup(map);
        hopper = slotGroup?.parent as Building_InputLogisticsHopper;
        return hopper != null;
    }

    public static bool IsConnectedStorageLoop(Building_InputLogisticsHopper hopper, Thing thing)
    {
        return hopper != null && thing != null && LogisticsNetworkUtility.IsItemInConnectedStorage(hopper, thing);
    }

    public static bool ShouldBlockAsStorageToInputHopper(Building_InputLogisticsHopper hopper, Thing thing)
    {
        if (hopper == null || thing == null)
        {
            return false;
        }

        Building_Storage sourceStorage = LogisticsNetworkUtility.GetStorageBuildingForItem(thing);
        return sourceStorage != null && sourceStorage != hopper;
    }

    public static bool ShouldBlockAsConnectedStorageRebalance(
        IntVec3 destinationCell,
        Map map,
        Thing thing,
        out Building_Storage sourceStorage,
        out Building_Storage destinationStorage)
    {
        sourceStorage = null;
        destinationStorage = null;
        if (map == null || thing == null || !destinationCell.IsValid)
        {
            return false;
        }

        sourceStorage = LogisticsNetworkUtility.GetStorageBuildingForItem(thing);
        if (sourceStorage == null)
        {
            return false;
        }

        SlotGroup destinationSlotGroup = destinationCell.GetSlotGroup(map);
        destinationStorage = destinationSlotGroup?.parent as Building_Storage;
        if (destinationStorage == null || destinationStorage == sourceStorage)
        {
            return false;
        }

        return LogisticsNetworkUtility.AreStoragesOnSameLogisticsNetwork(sourceStorage, destinationStorage);
    }

    public static bool ShouldBlockAsSameStorageReposition(
        IntVec3 destinationCell,
        Map map,
        Thing thing,
        out Building_Storage storage)
    {
        storage = null;
        if (map == null || thing == null || !destinationCell.IsValid)
        {
            return false;
        }

        storage = LogisticsNetworkUtility.GetStorageBuildingForItem(thing);
        if (storage == null)
        {
            return false;
        }

        SlotGroup destinationSlotGroup = destinationCell.GetSlotGroup(map);
        if (destinationSlotGroup?.parent != storage)
        {
            return false;
        }

        return LogisticsNetworkUtility.TryGetConnectedStorageBuildings(storage, out _);
    }

    public static void LogStoreCellCheck(string source, IntVec3 cell, Map map, Thing thing, Pawn pawn, bool resultBefore, bool blocked)
    {
        if (!Enabled || !blocked || !TryGetInputHopperAtCell(map, cell, out Building_InputLogisticsHopper hopper))
        {
            return;
        }

        Building_Storage sourceStorage = LogisticsNetworkUtility.GetStorageBuildingForItem(thing);
        bool inConnectedStorage = IsConnectedStorageLoop(hopper, thing);
        Log.Message(
            $"[Corvus Production Logistics] Input hopper debug {source}: item={thing}, pawn={pawn}, cell={cell}, resultBefore={resultBefore}, blocked={blocked}, inConnectedStorage={inConnectedStorage}, sourceStorage={sourceStorage}, hopper={hopper}.");
    }

    public static void LogHaulJob(string source, Pawn pawn, Thing thing, Job job, bool blocked)
    {
        if (!Enabled || !blocked || job == null)
        {
            return;
        }

        Map map = pawn?.Map ?? thing?.Map;
        IntVec3 destination = job.targetB.Cell;
        if (!TryGetInputHopperAtCell(map, destination, out Building_InputLogisticsHopper hopper))
        {
            return;
        }

        Building_Storage sourceStorage = LogisticsNetworkUtility.GetStorageBuildingForItem(thing);
        bool inConnectedStorage = IsConnectedStorageLoop(hopper, thing);
        Log.Message(
            $"[Corvus Production Logistics] Input hopper debug {source}: job={job.def}, item={thing}, pawn={pawn}, dest={destination}, blocked={blocked}, inConnectedStorage={inConnectedStorage}, sourceStorage={sourceStorage}, hopper={hopper}, targetA={job.targetA}, targetB={job.targetB}.");
    }

    public static void LogConnectedStorageRebalanceCheck(
        string source,
        IntVec3 cell,
        Map map,
        Thing thing,
        Pawn pawn,
        bool resultBefore,
        bool blocked)
    {
        if (!Enabled || !ShouldBlockAsConnectedStorageRebalance(cell, map, thing, out Building_Storage sourceStorage, out Building_Storage destinationStorage))
        {
            return;
        }

        Log.Message(
            $"[Corvus Production Logistics] Storage rebalance debug {source}: item={thing}, pawn={pawn}, cell={cell}, resultBefore={resultBefore}, blocked={blocked}, sourceStorage={sourceStorage}, destinationStorage={destinationStorage}.");
    }

    public static void LogStorageToStorageHaulJob(string source, Pawn pawn, Thing thing, Job job)
    {
        if (!Enabled || job == null || thing == null)
        {
            return;
        }

        Map map = pawn?.Map ?? thing.Map;
        if (map == null || !job.targetB.Cell.IsValid)
        {
            return;
        }

        if (!ShouldBlockAsConnectedStorageRebalance(job.targetB.Cell, map, thing, out Building_Storage sourceStorage, out Building_Storage destinationStorage))
        {
            return;
        }

        Log.Message(
            $"[Corvus Production Logistics] Storage haul debug {source}: job={job.def}, item={thing}, pawn={pawn}, sourceStorage={sourceStorage}, destinationStorage={destinationStorage}, dest={job.targetB.Cell}, targetA={job.targetA}, targetB={job.targetB}.");
    }

    public static void LogSameStorageReposition(string source, Pawn pawn, Thing thing, Job job, bool blocked)
    {
        if (!Enabled || !blocked || job == null || thing == null)
        {
            return;
        }

        Map map = pawn?.Map ?? thing.Map;
        if (map == null || !job.targetB.Cell.IsValid)
        {
            return;
        }

        if (!ShouldBlockAsSameStorageReposition(job.targetB.Cell, map, thing, out Building_Storage storage))
        {
            return;
        }

        Log.Message(
            $"[Corvus Production Logistics] Same storage reposition debug {source}: job={job.def}, item={thing}, pawn={pawn}, storage={storage}, from={thing.Position}, dest={job.targetB.Cell}, targetA={job.targetA}, targetB={job.targetB}.");
    }

    public static void LogSameStorageRepositionCheck(
        string source,
        IntVec3 cell,
        Map map,
        Thing thing,
        Pawn pawn,
        bool resultBefore,
        bool blocked)
    {
        if (!Enabled || !blocked || !ShouldBlockAsSameStorageReposition(cell, map, thing, out Building_Storage storage))
        {
            return;
        }

        Log.Message(
            $"[Corvus Production Logistics] Same storage reposition debug {source}: item={thing}, pawn={pawn}, cell={cell}, resultBefore={resultBefore}, blocked={blocked}, storage={storage}, from={thing.Position}.");
    }

    public static void LogRelevantJobStart(string source, Pawn pawn, Job job)
    {
        if (!Enabled || job == null)
        {
            return;
        }

        Map map = pawn?.Map ?? job.targetA.Thing?.Map ?? job.targetB.Thing?.Map;
        Thing targetAThing = job.targetA.Thing;
        Thing targetBThing = job.targetB.Thing;
        Building_Storage sourceStorage = LogisticsNetworkUtility.GetStorageBuildingForItem(targetAThing);
        Building_Storage destinationStorage = null;
        if (map != null && job.targetB.Cell.IsValid)
        {
            destinationStorage = job.targetB.Cell.GetSlotGroup(map)?.parent as Building_Storage;
        }

        if (sourceStorage == null && destinationStorage == null && targetAThing?.def.category != ThingCategory.Item && targetBThing?.def.category != ThingCategory.Item)
        {
            return;
        }

        Log.Message(
            $"[Corvus Production Logistics] Job start debug {source}: job={job.def}, pawn={pawn}, targetA={job.targetA}, targetB={job.targetB}, targetC={job.targetC}, count={job.count}, sourceStorage={sourceStorage}, destinationStorage={destinationStorage}, targetAThing={targetAThing}, targetBThing={targetBThing}.");
    }

    public static void LogHopperTransfer(string source, Building_LogisticsHopperBase hopper, Thing item, Building_Storage storage, bool moved)
    {
        if (!Enabled)
        {
            return;
        }

        Log.Message(
            $"[Corvus Production Logistics] Hopper transfer debug {source}: hopper={hopper}, item={item}, storage={storage}, moved={moved}.");
    }
}
