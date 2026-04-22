using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CorvusProductionLogistics;

public abstract class Building_LogisticsHopperBase : Building_Storage
{
    protected bool TryPlaceItemInStorage(Thing item, Building_Storage storage)
    {
        if (item == null || storage == null || storage.Map != Map || storage is Building_LogisticsHopperBase)
        {
            return false;
        }

        if (!storage.GetStoreSettings().AllowedToAccept(item))
        {
            return false;
        }

        IntVec3 sourceCell = item.Position;
        item.DeSpawn();

        foreach (IntVec3 cell in storage.OccupiedRect())
        {
            if (GenPlace.TryPlaceThing(item, cell, Map, ThingPlaceMode.Direct))
            {
                return true;
            }
        }

        GenPlace.TryPlaceThing(item, sourceCell, Map, ThingPlaceMode.Direct);
        return false;
    }

    protected Thing FirstStoredItem()
    {
        foreach (IntVec3 cell in this.OccupiedRect())
        {
            List<Thing> thingList = cell.GetThingList(Map);
            for (int i = 0; i < thingList.Count; i++)
            {
                Thing thing = thingList[i];
                if (thing != this && thing.Spawned && thing.def.category == ThingCategory.Item)
                {
                    return thing;
                }
            }
        }

        return null;
    }

    protected bool HasStoredItem()
    {
        return FirstStoredItem() != null;
    }

    protected static bool IsTransferableItem(Thing thing)
    {
        return thing is { Spawned: true } && thing.def.category == ThingCategory.Item;
    }
}

public class Building_InputLogisticsHopper : Building_LogisticsHopperBase
{
    protected override void Tick()
    {
        base.Tick();
        if (!Spawned || !this.IsHashIntervalTick(30))
        {
            return;
        }

        TryFlushOneItemToNetwork();
    }

    private void TryFlushOneItemToNetwork()
    {
        Thing item = FirstStoredItem();
        if (item == null || !LogisticsNetworkUtility.TryGetConnectedStorageBuildings(this, out HashSet<Building_Storage> storages))
        {
            return;
        }

        foreach (Building_Storage storage in storages)
        {
            if (TryPlaceItemInStorage(item, storage))
            {
                InputHopperDebug.LogHopperTransfer("InputHopper.Flush", this, item, storage, moved: true);
                return;
            }

            InputHopperDebug.LogHopperTransfer("InputHopper.Flush", this, item, storage, moved: false);
        }
    }
}

public class Building_OutputLogisticsHopper : Building_LogisticsHopperBase
{
    protected override void Tick()
    {
        base.Tick();
        if (!Spawned || !this.IsHashIntervalTick(30))
        {
            return;
        }

        TryPullOneItemFromNetwork();
    }

    private void TryPullOneItemFromNetwork()
    {
        if (HasStoredItem() || !LogisticsNetworkUtility.TryGetConnectedStorageBuildings(this, out HashSet<Building_Storage> storages))
        {
            return;
        }

        foreach (Building_Storage storage in storages)
        {
            if (storage is Building_LogisticsHopperBase)
            {
                continue;
            }

            Thing item = FirstAcceptedStoredItem(storage);
            if (item == null)
            {
                continue;
            }

            IntVec3 sourceCell = item.Position;
            item.DeSpawn();
            if (GenPlace.TryPlaceThing(item, Position, Map, ThingPlaceMode.Direct))
            {
                InputHopperDebug.LogHopperTransfer("OutputHopper.Pull", this, item, storage, moved: true);
                return;
            }

            GenPlace.TryPlaceThing(item, sourceCell, Map, ThingPlaceMode.Direct);
            InputHopperDebug.LogHopperTransfer("OutputHopper.Pull", this, item, storage, moved: false);
            return;
        }
    }

    private Thing FirstAcceptedStoredItem(Building_Storage storage)
    {
        foreach (IntVec3 cell in storage.OccupiedRect())
        {
            List<Thing> thingList = cell.GetThingList(storage.Map);
            for (int i = 0; i < thingList.Count; i++)
            {
                Thing thing = thingList[i];
                if (!IsTransferableItem(thing) || !GetStoreSettings().AllowedToAccept(thing))
                {
                    continue;
                }

                return thing;
            }
        }

        return null;
    }
}
