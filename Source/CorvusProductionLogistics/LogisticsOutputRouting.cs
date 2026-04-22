using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace CorvusProductionLogistics;

public static class LogisticsOutputRouting
{
    [ThreadStatic]
    private static bool isRoutingConnectedStorage;

    private static readonly Dictionary<Job, LogisticsOutputRoute> PendingOutputRoutes = new Dictionary<Job, LogisticsOutputRoute>();

    public static bool IsRoutingConnectedStorage => isRoutingConnectedStorage;

    public static bool TryFindConnectedStorageCell(
        Thing thing,
        Pawn carrier,
        Building_WorkTable workTable,
        out IntVec3 foundCell)
    {
        foundCell = IntVec3.Invalid;
        if (isRoutingConnectedStorage)
        {
            return false;
        }

        try
        {
            isRoutingConnectedStorage = true;
            return LogisticsNetworkUtility.TryFindBestConnectedStorageCellFor(thing, carrier, workTable, out foundCell);
        }
        finally
        {
            isRoutingConnectedStorage = false;
        }
    }

    public static void RegisterOutputRoute(Job job, Pawn pawn, Thing thing, IntVec3 cell)
    {
        if (job == null || pawn == null || thing == null || !cell.IsValid)
        {
            return;
        }

        PendingOutputRoutes[job] = new LogisticsOutputRoute(pawn, thing, cell);
    }

    public static bool TryConsumeOutputRoute(Job job, Pawn pawn, out LogisticsOutputRoute route)
    {
        route = default;
        if (job == null || pawn == null || !PendingOutputRoutes.TryGetValue(job, out LogisticsOutputRoute pending))
        {
            return false;
        }

        PendingOutputRoutes.Remove(job);
        if (pending.Pawn != pawn)
        {
            return false;
        }

        route = pending;
        return true;
    }
}
