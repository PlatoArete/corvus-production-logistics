using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CorvusProductionLogistics.Patches;

[HarmonyPatch]
public static class WorkGiverDoBillPatches
{
    private static readonly MethodInfo TryFindBestBillIngredientsInSetMethod =
        AccessTools.Method(typeof(WorkGiver_DoBill), "TryFindBestBillIngredientsInSet");

    [HarmonyPatch(typeof(WorkGiver_DoBill), "TryFindBestBillIngredients")]
    [HarmonyPostfix]
    public static void TryFindBestBillIngredientsPostfix(
        Bill bill,
        Pawn pawn,
        Thing billGiver,
        List<ThingCount> chosen,
        List<IngredientCount> missingIngredients,
        ref bool __result)
    {
        if (__result || billGiver is not Building_WorkTable workTable)
        {
            return;
        }

        List<Thing> networkedItems = new List<Thing>(LogisticsNetworkUtility.GetNetworkedItemsForBillGiver(workTable));
        if (networkedItems.Count == 0)
        {
            return;
        }

        chosen.Clear();
        if (missingIngredients != null)
        {
            missingIngredients.Clear();
        }

        IntVec3 rootCell = workTable.def.hasInteractionCell ? workTable.InteractionCell : workTable.Position;
        object[] args =
        {
            networkedItems,
            bill,
            chosen,
            rootCell,
            false,
            missingIngredients
        };

        __result = (bool)TryFindBestBillIngredientsInSetMethod.Invoke(null, args);
        if (!__result)
        {
            chosen.Clear();
        }
    }

    [HarmonyPatch(typeof(WorkGiver_DoBill), nameof(WorkGiver_DoBill.TryStartNewDoBillJob))]
    [HarmonyPostfix]
    public static void TryStartNewDoBillJobPostfix(
        Pawn pawn,
        Bill bill,
        IBillGiver giver,
        List<ThingCount> chosenIngThings,
        ref Job __result)
    {
        if (__result == null || __result.def != JobDefOf.DoBill || giver is not Building_WorkTable workTable)
        {
            return;
        }

        if (__result.targetQueueB == null || __result.targetQueueB.Count == 0)
        {
            return;
        }

        List<int> indicesToRemove = new List<int>();
        __result.placedThings ??= new List<ThingCountClass>();

        for (int i = 0; i < chosenIngThings.Count; i++)
        {
            Thing sourceThing = chosenIngThings[i].Thing;
            if (!LogisticsNetworkUtility.IsItemInConnectedStorage(workTable, sourceThing))
            {
                continue;
            }

            int queueIndex = FindTargetQueueIndex(__result, sourceThing);
            if (queueIndex < 0)
            {
                continue;
            }

            int requiredCount = __result.countQueue[queueIndex];
            if (requiredCount <= 0)
            {
                continue;
            }

            if (!pawn.Reserve(sourceThing, __result, 1, -1, null, errorOnFailed: false))
            {
                Log.Warning($"Corvus Production Logistics could not reserve linked ingredient {sourceThing} for {pawn}.");
                continue;
            }

            __result.placedThings.Add(new ThingCountClass(sourceThing, requiredCount));
            indicesToRemove.Add(queueIndex);
        }

        for (int i = indicesToRemove.Count - 1; i >= 0; i--)
        {
            int index = indicesToRemove[i];
            __result.targetQueueB.RemoveAt(index);
            __result.countQueue.RemoveAt(index);
        }
    }

    private static int FindTargetQueueIndex(Job job, Thing thing)
    {
        int count = job.targetQueueB.Count < job.countQueue.Count ? job.targetQueueB.Count : job.countQueue.Count;
        for (int i = 0; i < count; i++)
        {
            if (job.targetQueueB[i].Thing == thing)
            {
                return i;
            }
        }

        return -1;
    }
}
