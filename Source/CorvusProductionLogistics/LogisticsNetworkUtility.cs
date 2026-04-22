using System.Collections.Generic;
using System.Linq;
using System;
using RimWorld;
using Verse;

namespace CorvusProductionLogistics;

public static class LogisticsNetworkUtility
{
    private static readonly IntVec3[] CardinalDirections =
    {
        IntVec3.North,
        IntVec3.East,
        IntVec3.South,
        IntVec3.West
    };

    public static LogisticsNetworkSnapshot BuildSnapshot(Building_LogisticsConduit root)
    {
        if (root == null || root.Map == null)
        {
            return default;
        }

        HashSet<IntVec3> conduitCells = CollectConnectedConduitCells(root);
        ResolveEndpoints(root.Map, conduitCells, out HashSet<Building_Storage> storages, out HashSet<Building_WorkTable> workTables);

        return new LogisticsNetworkSnapshot(conduitCells.Count, storages.Count, workTables.Count);
    }

    public static bool TryGetConnectedStorageBuildings(Building building, out HashSet<Building_Storage> storages)
    {
        storages = null;
        if (building == null || building.Map == null)
        {
            return false;
        }

        if (!TryGetConnectedConduitCells(building, out HashSet<IntVec3> conduitCells))
        {
            return false;
        }

        ResolveEndpoints(building.Map, conduitCells, out HashSet<Building_Storage> resolvedStorages, out _);
        if (resolvedStorages.Count == 0)
        {
            return false;
        }

        storages = resolvedStorages;
        return true;
    }

    public static bool IsItemInConnectedStorage(Building building, Thing item)
    {
        if (building == null || item == null || item.Map != building.Map)
        {
            return false;
        }

        Building_Storage storage = GetStorageBuildingForItem(item);
        if (storage == null)
        {
            return false;
        }

        return TryGetConnectedStorageBuildings(building, out HashSet<Building_Storage> storages) && storages.Contains(storage);
    }

    public static Building_Storage GetStorageBuildingForItem(Thing item)
    {
        if (item == null || item.Map == null || !item.Spawned)
        {
            return null;
        }

        SlotGroup slotGroup = item.Position.GetSlotGroup(item.Map);
        return slotGroup?.parent as Building_Storage;
    }

    public static bool AreStoragesOnSameLogisticsNetwork(Building_Storage first, Building_Storage second)
    {
        if (first == null || second == null || first.Map == null || first.Map != second.Map)
        {
            return false;
        }

        if (first == second)
        {
            return true;
        }

        return TryGetConnectedStorageBuildings(first, out HashSet<Building_Storage> storages) && storages.Contains(second);
    }

    public static IEnumerable<Thing> GetNetworkedItemsForBillGiver(Building building)
    {
        if (!TryGetConnectedStorageBuildings(building, out HashSet<Building_Storage> storages))
        {
            return Enumerable.Empty<Thing>();
        }

        List<Thing> items = new List<Thing>();
        foreach (Building_Storage storage in storages)
        {
            foreach (IntVec3 cell in storage.OccupiedRect())
            {
                List<Thing> thingList = cell.GetThingList(storage.Map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    Thing thing = thingList[i];
                    if (thing == storage || !thing.Spawned || thing.def.category != ThingCategory.Item)
                    {
                        continue;
                    }

                    items.Add(thing);
                }
            }
        }

        return items;
    }

    public static bool TryGetConnectedConduitCells(Building building, out HashSet<IntVec3> conduitCells)
    {
        conduitCells = null;
        if (building == null || building.Map == null)
        {
            return false;
        }

        foreach (IntVec3 cell in EnumerateEndpointTouchCells(building))
        {
            if (!ContainsLogisticsNetworkCell(building.Map, cell))
            {
                continue;
            }

            conduitCells = CollectConnectedConduitCells(building.Map, cell);
            return conduitCells.Count > 0;
        }

        return false;
    }

    public static HashSet<IntVec3> CollectConnectedConduitCells(Building_LogisticsConduit root)
    {
        return CollectConnectedConduitCells(root.Map, root.Position);
    }

    public static HashSet<IntVec3> CollectConnectedConduitCells(Map map, IntVec3 rootCell)
    {
        HashSet<IntVec3> visited = new HashSet<IntVec3>();
        Queue<IntVec3> pending = new Queue<IntVec3>();

        visited.Add(rootCell);
        pending.Enqueue(rootCell);

        while (pending.Count > 0)
        {
            IntVec3 current = pending.Dequeue();

            for (int i = 0; i < CardinalDirections.Length; i++)
            {
                IntVec3 neighbor = current + CardinalDirections[i];
                if (!neighbor.InBounds(map) || visited.Contains(neighbor))
                {
                    continue;
                }

                if (!ContainsLogisticsNetworkCell(map, neighbor))
                {
                    continue;
                }

                visited.Add(neighbor);
                pending.Enqueue(neighbor);
            }
        }

        return visited;
    }

    public static bool ContainsLogisticsConduit(Map map, IntVec3 cell)
    {
        List<Thing> thingList = cell.GetThingList(map);
        for (int i = 0; i < thingList.Count; i++)
        {
            if (thingList[i] is Building_LogisticsConduit)
            {
                return true;
            }
        }

        return false;
    }

    public static bool ContainsLogisticsNetworkCell(Map map, IntVec3 cell)
    {
        if (ContainsLogisticsConduit(map, cell))
        {
            return true;
        }

        return ContainsLogisticsStorageFootprint(map, cell);
    }

    private static bool ContainsLogisticsStorageFootprint(Map map, IntVec3 cell)
    {
        List<Thing> thingList = cell.GetThingList(map);
        for (int i = 0; i < thingList.Count; i++)
        {
            if (thingList[i] is Building_Storage and not Building_LogisticsHopperBase)
            {
                return true;
            }
        }

        return false;
    }

    public static bool TryFindBestConnectedStorageCellFor(
        Thing thing,
        Pawn carrier,
        Building building,
        out IntVec3 foundCell)
    {
        return TryFindBestConnectedStorageCellFor(thing, carrier, building, (Predicate<Building_Storage>)null, out foundCell);
    }

    public static bool TryFindBestConnectedStorageCellFor(
        Thing thing,
        Pawn carrier,
        Building building,
        Building_Storage excludedStorage,
        out IntVec3 foundCell)
    {
        return TryFindBestConnectedStorageCellFor(
            thing,
            carrier,
            building,
            storage => storage != excludedStorage,
            out foundCell);
    }

    public static bool TryFindBestConnectedStorageCellFor(
        Thing thing,
        Pawn carrier,
        Building building,
        Predicate<Building_Storage> storageValidator,
        out IntVec3 foundCell)
    {
        foundCell = IntVec3.Invalid;
        if (thing == null || building == null || building.Map == null || (carrier != null && carrier.Map != building.Map))
        {
            return false;
        }

        Map map = building.Map;
        Faction faction = carrier?.Faction ?? building.Faction ?? Faction.OfPlayer;

        if (!TryGetConnectedStorageBuildings(building, out HashSet<Building_Storage> storages))
        {
            return false;
        }

        foreach (Building_Storage storage in storages)
        {
            if (storageValidator != null && !storageValidator(storage))
            {
                continue;
            }

            ISlotGroup slotGroup = storage.GetSlotGroup();
            if (slotGroup == null)
            {
                continue;
            }

            if (StoreUtility.TryFindBestBetterStoreCellForIn(
                    thing,
                    carrier,
                    map,
                    StoragePriority.Unstored,
                    faction,
                    slotGroup,
                    out foundCell))
            {
                return true;
            }
        }

        foundCell = IntVec3.Invalid;
        return false;
    }

    public static IntVec3 FindBestStagingCell(Thing billGiver)
    {
        if (billGiver == null || billGiver.Map == null)
        {
            return IntVec3.Invalid;
        }

        if (billGiver is Building building && building.def.hasInteractionCell)
        {
            IntVec3 interactionCell = building.InteractionCell;
            if (interactionCell.InBounds(building.Map) && interactionCell.Walkable(building.Map))
            {
                return interactionCell;
            }
        }

        if (billGiver.Position.InBounds(billGiver.Map) && billGiver.Position.Walkable(billGiver.Map))
        {
            return billGiver.Position;
        }

        return CellFinder.StandableCellNear(billGiver.Position, billGiver.Map, 2f);
    }

    private static void ResolveEndpoints(
        Map map,
        HashSet<IntVec3> conduitCells,
        out HashSet<Building_Storage> storages,
        out HashSet<Building_WorkTable> workTables)
    {
        storages = new HashSet<Building_Storage>();
        workTables = new HashSet<Building_WorkTable>();

        foreach (IntVec3 cell in conduitCells)
        {
            AddEndpointsAtCell(map, cell, storages, workTables);

            for (int i = 0; i < CardinalDirections.Length; i++)
            {
                IntVec3 neighbor = cell + CardinalDirections[i];
                if (!neighbor.InBounds(map))
                {
                    continue;
                }

                AddEndpointsAtCell(map, neighbor, storages, workTables);
            }
        }
    }

    private static void AddEndpointsAtCell(
        Map map,
        IntVec3 cell,
        HashSet<Building_Storage> storages,
        HashSet<Building_WorkTable> workTables)
    {
        List<Thing> thingList = cell.GetThingList(map);
        for (int i = 0; i < thingList.Count; i++)
        {
            Thing thing = thingList[i];
            if (thing is Building_Storage storage)
            {
                if (storage is not Building_LogisticsHopperBase)
                {
                    storages.Add(storage);
                }
            }

            if (thing is Building_WorkTable workTable)
            {
                workTables.Add(workTable);
            }
        }
    }

    private static IEnumerable<IntVec3> EnumerateEndpointTouchCells(Building building)
    {
        CellRect rect = building.OccupiedRect();
        foreach (IntVec3 cell in rect)
        {
            yield return cell;

            for (int i = 0; i < CardinalDirections.Length; i++)
            {
                IntVec3 neighbor = cell + CardinalDirections[i];
                if (neighbor.InBounds(building.Map))
                {
                    yield return neighbor;
                }
            }
        }
    }
}
