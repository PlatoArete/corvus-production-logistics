using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CorvusProductionLogistics.Patches;

[HarmonyPatch(typeof(HaulAIUtility), nameof(HaulAIUtility.HaulToStorageJob))]
[HarmonyPriority(Priority.Last)]
public static class InputHopperHaulToStorageJobPatch
{
    [HarmonyPostfix]
    public static void HaulToStorageJobPostfix(Pawn p, Thing t, ref Job __result)
    {
        bool blocked = IsInvalidInputHopperHaulJob(p, t, __result);
        InputHopperDebug.LogHaulJob("HaulToStorageJob", p, t, __result, blocked);
        InputHopperDebug.LogStorageToStorageHaulJob("HaulToStorageJob", p, t, __result);
        bool blockedSameStorageReposition = IsInvalidSameStorageRepositionJob(p, t, __result);
        InputHopperDebug.LogSameStorageReposition("HaulToStorageJob", p, t, __result, blockedSameStorageReposition);
        if (blocked || blockedSameStorageReposition)
        {
            __result = null;
        }
    }

    internal static bool IsInvalidInputHopperHaulJob(Pawn pawn, Thing thing, Job job)
    {
        if (pawn?.CurJob?.def == JobDefOf.DoBill || thing == null || job == null || job.def != JobDefOf.HaulToCell)
        {
            return false;
        }

        Map map = pawn?.Map ?? thing.Map;
        if (map == null)
        {
            return false;
        }

        SlotGroup slotGroup = job.targetB.Cell.GetSlotGroup(map);
        if (slotGroup?.parent is not Building_InputLogisticsHopper hopper)
        {
            return false;
        }

        return InputHopperDebug.ShouldBlockAsStorageToInputHopper(hopper, thing);
    }

    internal static bool IsInvalidSameStorageRepositionJob(Pawn pawn, Thing thing, Job job)
    {
        if (thing == null || job == null || job.def != JobDefOf.HaulToCell)
        {
            return false;
        }

        Map map = pawn?.Map ?? thing.Map;
        return InputHopperDebug.ShouldBlockAsSameStorageReposition(job.targetB.Cell, map, thing, out _);
    }
}

[HarmonyPatch(typeof(HaulAIUtility), nameof(HaulAIUtility.HaulToCellStorageJob))]
[HarmonyPriority(Priority.Last)]
public static class InputHopperHaulToCellStorageJobPatch
{
    [HarmonyPostfix]
    public static void HaulToCellStorageJobPostfix(Pawn p, Thing t, ref Job __result)
    {
        bool blocked = InputHopperHaulToStorageJobPatch.IsInvalidInputHopperHaulJob(p, t, __result);
        InputHopperDebug.LogHaulJob("HaulToCellStorageJob", p, t, __result, blocked);
        InputHopperDebug.LogStorageToStorageHaulJob("HaulToCellStorageJob", p, t, __result);
        bool blockedSameStorageReposition = InputHopperHaulToStorageJobPatch.IsInvalidSameStorageRepositionJob(p, t, __result);
        InputHopperDebug.LogSameStorageReposition("HaulToCellStorageJob", p, t, __result, blockedSameStorageReposition);
        if (blocked || blockedSameStorageReposition)
        {
            __result = null;
        }
    }
}

[HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob))]
[HarmonyPriority(Priority.Last)]
public static class InputHopperStartJobDebugPatch
{
    private static readonly AccessTools.FieldRef<Pawn_JobTracker, Pawn> PawnField =
        AccessTools.FieldRefAccess<Pawn_JobTracker, Pawn>("pawn");

    [HarmonyPrefix]
    public static bool StartJobPrefix(Pawn_JobTracker __instance, Job newJob)
    {
        Pawn pawn = PawnField(__instance);
        Thing thing = newJob?.targetA.Thing;
        InputHopperDebug.LogRelevantJobStart("Pawn_JobTracker.StartJob", pawn, newJob);

        bool blockedInputHopper = InputHopperHaulToStorageJobPatch.IsInvalidInputHopperHaulJob(pawn, thing, newJob);
        InputHopperDebug.LogHaulJob("Pawn_JobTracker.StartJob", pawn, thing, newJob, blockedInputHopper);
        if (blockedInputHopper)
        {
            return false;
        }

        InputHopperDebug.LogStorageToStorageHaulJob("Pawn_JobTracker.StartJob", pawn, thing, newJob);
        Map map = pawn?.Map ?? thing?.Map;
        if (newJob?.def == JobDefOf.HaulToCell
            && InputHopperDebug.ShouldBlockAsConnectedStorageRebalance(newJob.targetB.Cell, map, thing, out _, out _))
        {
            return false;
        }

        bool blockedSameStorageReposition = InputHopperHaulToStorageJobPatch.IsInvalidSameStorageRepositionJob(pawn, thing, newJob);
        InputHopperDebug.LogSameStorageReposition("Pawn_JobTracker.StartJob", pawn, thing, newJob, blockedSameStorageReposition);
        if (blockedSameStorageReposition)
        {
            return false;
        }

        return true;
    }
}
