using RimWorld;
using Verse;
using UnityEngine;

namespace CorvusProductionLogistics;

public class PlaceWorker_LogisticsConduit : PlaceWorker_Conduit
{
    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        base.DrawGhost(def, center, rot, ghostCol, thing);
        if (def != CorvusDefOf.Corvus_HiddenLogisticsConduit)
        {
            return;
        }

        LogisticsOverlayDrawer.DrawAllHiddenConduits(Find.CurrentMap, center, includeExtraCell: true);
    }
}
