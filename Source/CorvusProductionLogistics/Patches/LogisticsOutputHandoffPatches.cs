using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CorvusProductionLogistics.Patches;

[HarmonyPatch(typeof(HaulAIUtility), nameof(HaulAIUtility.HaulToCellStorageJob))]
public static class LogisticsOutputHaulJobPatch
{
    [HarmonyPostfix]
    public static void HaulToCellStorageJobPostfix(Pawn p, Thing t, IntVec3 storeCell, Job __result)
    {
        if (__result == null || p?.CurJob?.def != JobDefOf.DoBill)
        {
            return;
        }

        Thing billGiver = p.CurJob.GetTarget(TargetIndex.A).Thing;
        if (billGiver is not Building_WorkTable workTable)
        {
            return;
        }

        if (!LogisticsNetworkUtility.TryFindBestConnectedStorageCellFor(t, p, workTable, out IntVec3 connectedCell))
        {
            return;
        }

        if (connectedCell == storeCell)
        {
            LogisticsOutputRouting.RegisterOutputRoute(__result, p, t, storeCell);
        }
    }
}

[HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob))]
public static class LogisticsOutputJobStartPatch
{
    private static readonly AccessTools.FieldRef<Pawn_JobTracker, Pawn> PawnField =
        AccessTools.FieldRefAccess<Pawn_JobTracker, Pawn>("pawn");

    [HarmonyPrefix]
    public static bool StartJobPrefix(Pawn_JobTracker __instance, Job newJob)
    {
        Pawn pawn = PawnField(__instance);
        if (!LogisticsOutputRouting.TryConsumeOutputRoute(newJob, pawn, out LogisticsOutputRoute route))
        {
            return true;
        }

        // Vanilla starts a separate haul job after bill completion. For logistics output,
        // replace that handoff with an immediate insert into the connected storage cell.
        if (pawn.carryTracker.CarriedThing == route.Thing)
        {
            if (!pawn.carryTracker.TryDropCarriedThing(route.Cell, ThingPlaceMode.Direct, out Thing _))
            {
                pawn.carryTracker.TryDropCarriedThing(route.Cell, ThingPlaceMode.Near, out Thing _);
            }
        }
        else if (!route.Thing.Spawned)
        {
            GenPlace.TryPlaceThing(route.Thing, route.Cell, pawn.Map, ThingPlaceMode.Direct);
        }

        pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
        return false;
    }
}
