using Verse;
using Verse.AI;

namespace CorvusProductionLogistics;

public readonly struct LogisticsOutputRoute
{
    public LogisticsOutputRoute(Pawn pawn, Thing thing, IntVec3 cell)
    {
        Pawn = pawn;
        Thing = thing;
        Cell = cell;
    }

    public Pawn Pawn { get; }

    public Thing Thing { get; }

    public IntVec3 Cell { get; }
}
