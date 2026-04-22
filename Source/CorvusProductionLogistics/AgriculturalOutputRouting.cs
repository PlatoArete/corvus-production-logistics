using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace CorvusProductionLogistics;

public static class AgriculturalOutputRouting
{
    public static bool TryFindConnectedStorageCellForHarvestProduct(
        Thing product,
        IntVec3 originalDropCell,
        Map map,
        out IntVec3 storageCell)
    {
        storageCell = IntVec3.Invalid;
        if (product == null || map == null || product.def.category != ThingCategory.Item)
        {
            return false;
        }

        Pawn harvester = FindHarvesterAt(originalDropCell, map);
        if (harvester == null)
        {
            return false;
        }

        Plant plant = harvester.CurJob.GetTarget(TargetIndex.A).Thing as Plant;
        Building_PlantGrower grower = FindPlantGrowerFor(plant);
        return grower != null && LogisticsNetworkUtility.TryFindBestConnectedStorageCellFor(product, harvester, grower, out storageCell);
    }

    private static Pawn FindHarvesterAt(IntVec3 cell, Map map)
    {
        IReadOnlyList<Pawn> pawns = map.mapPawns.AllPawnsSpawned;
        for (int i = 0; i < pawns.Count; i++)
        {
            Pawn pawn = pawns[i];
            if (pawn.Position != cell || pawn.CurJob == null)
            {
                continue;
            }

            if (pawn.CurJob.def == JobDefOf.Harvest || pawn.CurJob.def == JobDefOf.HarvestDesignated)
            {
                return pawn;
            }
        }

        return null;
    }

    private static Building_PlantGrower FindPlantGrowerFor(Plant plant)
    {
        if (plant == null || plant.Map == null || !plant.Spawned)
        {
            return null;
        }

        List<Thing> thingList = plant.Position.GetThingList(plant.Map);
        for (int i = 0; i < thingList.Count; i++)
        {
            if (thingList[i] is Building_PlantGrower grower)
            {
                return grower;
            }
        }

        return null;
    }
}
