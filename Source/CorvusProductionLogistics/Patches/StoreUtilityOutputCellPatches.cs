using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CorvusProductionLogistics.Patches;

[HarmonyPatch(typeof(StoreUtility), nameof(StoreUtility.TryFindBestBetterStoreCellFor))]
[HarmonyPriority(Priority.Last)]
public static class StoreUtilityOutputCellPatches
{
    [HarmonyPostfix]
    public static void TryFindBestBetterStoreCellForPostfix(
        Thing t,
        Pawn carrier,
        Map map,
        StoragePriority currentPriority,
        Faction faction,
        ref IntVec3 foundCell,
        bool needAccurateResult,
        ref bool __result)
    {
        TryRouteToConnectedStorage(t, carrier, map, ref foundCell, ref __result);
    }

    internal static void TryRouteToConnectedStorage(
        Thing thing,
        Pawn carrier,
        Map map,
        ref IntVec3 foundCell,
        ref bool result)
    {
        PreventInputHopperStorageLoop(thing, map, ref foundCell, ref result);
        PreventConnectedStorageRebalance("TryFindBestBetterStoreCellFor", thing, carrier, map, ref foundCell, ref result);
        PreventSameStorageReposition("TryFindBestBetterStoreCellFor", thing, carrier, map, ref foundCell, ref result);

        if (LogisticsOutputRouting.IsRoutingConnectedStorage || carrier?.CurJob?.def != JobDefOf.DoBill)
        {
            return;
        }

        Thing billGiver = carrier.CurJob.GetTarget(TargetIndex.A).Thing;
        if (billGiver is not Building_WorkTable workTable || workTable.Map != map)
        {
            return;
        }

        if (!LogisticsOutputRouting.TryFindConnectedStorageCell(thing, carrier, workTable, out IntVec3 connectedCell))
        {
            return;
        }

        foundCell = connectedCell;
        result = true;
    }

    private static void PreventInputHopperStorageLoop(
        Thing thing,
        Map map,
        ref IntVec3 foundCell,
        ref bool result)
    {
        if (!result || thing == null || map == null || !foundCell.IsValid)
        {
            return;
        }

        SlotGroup slotGroup = foundCell.GetSlotGroup(map);
        if (slotGroup?.parent is not Building_InputLogisticsHopper hopper)
        {
            return;
        }

        if (!InputHopperDebug.ShouldBlockAsStorageToInputHopper(hopper, thing))
        {
            InputHopperDebug.LogStoreCellCheck("TryFindBestBetterStoreCellFor", foundCell, map, thing, pawn: null, result, blocked: false);
            return;
        }

        InputHopperDebug.LogStoreCellCheck("TryFindBestBetterStoreCellFor", foundCell, map, thing, pawn: null, result, blocked: true);
        foundCell = IntVec3.Invalid;
        result = false;
    }

    internal static void PreventConnectedStorageRebalance(
        string source,
        Thing thing,
        Pawn pawn,
        Map map,
        ref IntVec3 foundCell,
        ref bool result)
    {
        if (!result || thing == null || map == null || !foundCell.IsValid)
        {
            return;
        }

        bool blocked = InputHopperDebug.ShouldBlockAsConnectedStorageRebalance(
            foundCell,
            map,
            thing,
            out _,
            out _);
        InputHopperDebug.LogConnectedStorageRebalanceCheck(source, foundCell, map, thing, pawn, resultBefore: result, blocked);
        if (!blocked)
        {
            return;
        }

        foundCell = IntVec3.Invalid;
        result = false;
    }

    internal static void PreventSameStorageReposition(
        string source,
        Thing thing,
        Pawn pawn,
        Map map,
        ref IntVec3 foundCell,
        ref bool result)
    {
        if (!result || thing == null || map == null || !foundCell.IsValid)
        {
            return;
        }

        bool blocked = InputHopperDebug.ShouldBlockAsSameStorageReposition(foundCell, map, thing, out _);
        InputHopperDebug.LogSameStorageRepositionCheck(source, foundCell, map, thing, pawn, resultBefore: result, blocked);
        if (!blocked)
        {
            return;
        }

        foundCell = IntVec3.Invalid;
        result = false;
    }
}

[HarmonyPatch(typeof(StoreUtility), nameof(StoreUtility.TryFindBestBetterStoreCellForIn))]
[HarmonyPriority(Priority.Last)]
public static class StoreUtilityOutputCellInPatches
{
    [HarmonyPostfix]
    public static void TryFindBestBetterStoreCellForInPostfix(
        Thing t,
        Pawn carrier,
        Map map,
        StoragePriority currentPriority,
        Faction faction,
        ISlotGroup slotGroup,
        ref IntVec3 foundCell,
        bool needAccurateResult,
        ref bool __result)
    {
        StoreUtilityOutputCellPatches.TryRouteToConnectedStorage(t, carrier, map, ref foundCell, ref __result);
    }
}

[HarmonyPatch(typeof(StoreUtility), nameof(StoreUtility.IsValidStorageFor))]
[HarmonyPriority(Priority.Last)]
public static class StoreUtilityInputHopperValidityPatches
{
    [HarmonyPostfix]
    public static void IsValidStorageForPostfix(IntVec3 c, Map map, Thing storable, ref bool __result)
    {
        bool resultBefore = __result;
        if (storable == null || map == null)
        {
            return;
        }

        SlotGroup slotGroup = c.GetSlotGroup(map);
        if (slotGroup?.parent is Building_InputLogisticsHopper hopper)
        {
            bool blocked = __result && InputHopperDebug.ShouldBlockAsStorageToInputHopper(hopper, storable);
            InputHopperDebug.LogStoreCellCheck("IsValidStorageFor", c, map, storable, pawn: null, resultBefore, blocked);
            if (blocked)
            {
                __result = false;
                return;
            }
        }

        if (__result && InputHopperDebug.ShouldBlockAsConnectedStorageRebalance(c, map, storable, out _, out _))
        {
            InputHopperDebug.LogConnectedStorageRebalanceCheck("IsValidStorageFor", c, map, storable, pawn: null, resultBefore, blocked: true);
            __result = false;
            return;
        }

        if (__result && InputHopperDebug.ShouldBlockAsSameStorageReposition(c, map, storable, out _))
        {
            InputHopperDebug.LogSameStorageRepositionCheck("IsValidStorageFor", c, map, storable, pawn: null, resultBefore, blocked: true);
            __result = false;
        }
    }
}

[HarmonyPatch(typeof(StoreUtility), nameof(StoreUtility.IsGoodStoreCell))]
[HarmonyPriority(Priority.Last)]
public static class StoreUtilityInputHopperGoodCellPatches
{
    [HarmonyPostfix]
    public static void IsGoodStoreCellPostfix(IntVec3 c, Map map, Thing t, Pawn carrier, Faction faction, ref bool __result)
    {
        bool resultBefore = __result;
        if (t == null || map == null)
        {
            return;
        }

        SlotGroup slotGroup = c.GetSlotGroup(map);
        if (slotGroup?.parent is Building_InputLogisticsHopper hopper)
        {
            bool blocked = __result && InputHopperDebug.ShouldBlockAsStorageToInputHopper(hopper, t);
            InputHopperDebug.LogStoreCellCheck("IsGoodStoreCell", c, map, t, carrier, resultBefore, blocked);
            if (blocked)
            {
                __result = false;
                return;
            }
        }

        if (__result && InputHopperDebug.ShouldBlockAsConnectedStorageRebalance(c, map, t, out _, out _))
        {
            InputHopperDebug.LogConnectedStorageRebalanceCheck("IsGoodStoreCell", c, map, t, carrier, resultBefore, blocked: true);
            __result = false;
            return;
        }

        if (__result && InputHopperDebug.ShouldBlockAsSameStorageReposition(c, map, t, out _))
        {
            InputHopperDebug.LogSameStorageRepositionCheck("IsGoodStoreCell", c, map, t, carrier, resultBefore, blocked: true);
            __result = false;
        }
    }
}
