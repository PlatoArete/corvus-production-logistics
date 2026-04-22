using System.Text;
using RimWorld;
using Verse;

namespace CorvusProductionLogistics;

public class Building_LogisticsConduit : Building
{
    public override string GetInspectString()
    {
        StringBuilder sb = new StringBuilder();
        string baseString = base.GetInspectString();
        if (!baseString.NullOrEmpty())
        {
            sb.AppendLine(baseString);
        }

        if (Map != null)
        {
            LogisticsNetworkSnapshot snapshot = LogisticsNetworkUtility.BuildSnapshot(this);
            sb.AppendLine($"Network size: {snapshot.ConduitCount} conduits");
            sb.AppendLine($"Linked storages: {snapshot.StorageCount}");
            sb.AppendLine($"Linked worktables: {snapshot.WorkTableCount}");
        }

        return sb.ToString().TrimEndNewlines();
    }

    public override void DrawExtraSelectionOverlays()
    {
        base.DrawExtraSelectionOverlays();
        if (def != CorvusDefOf.Corvus_HiddenLogisticsConduit || Map == null)
        {
            return;
        }

        LogisticsOverlayDrawer.DrawNetwork(Map, LogisticsNetworkUtility.CollectConnectedConduitCells(this));
    }
}
