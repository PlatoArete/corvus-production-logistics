using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CorvusProductionLogistics;

[StaticConstructorOnStartup]
public static class LogisticsOverlayDrawer
{
    private static readonly Material LineMaterial =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(1f, 1f, 1f, 0.78f), true);

    private const float LineWidth = 0.22f;

    private static readonly IntVec3[] CardinalDirections =
    {
        IntVec3.North,
        IntVec3.East,
        IntVec3.South,
        IntVec3.West
    };

    public static void DrawNetwork(Map map, HashSet<IntVec3> cells, IntVec3 extraCell = default, bool includeExtraCell = false)
    {
        if (map == null || cells == null)
        {
            return;
        }

        HashSet<IntVec3> drawCells = includeExtraCell ? new HashSet<IntVec3>(cells) : cells;
        if (includeExtraCell && extraCell.InBounds(map))
        {
            drawCells.Add(extraCell);
        }

        HashSet<IntVec3> drawnEdges = new HashSet<IntVec3>();
        foreach (IntVec3 cell in drawCells)
        {
            bool drewConnection = false;
            for (int i = 0; i < CardinalDirections.Length; i++)
            {
                IntVec3 neighbor = cell + CardinalDirections[i];
                if (!drawCells.Contains(neighbor))
                {
                    continue;
                }

                // Draw each undirected edge once.
                if (neighbor.x < cell.x || neighbor.z < cell.z)
                {
                    continue;
                }

                DrawSegment(cell, neighbor);
                drewConnection = true;
                drawnEdges.Add(cell);
                drawnEdges.Add(neighbor);
            }

            if (!drewConnection && !drawnEdges.Contains(cell))
            {
                DrawIsolatedCell(cell);
            }
        }
    }

    public static void DrawAllHiddenConduits(Map map, IntVec3 extraCell = default, bool includeExtraCell = false)
    {
        if (map == null)
        {
            return;
        }

        HashSet<IntVec3> cells = new HashSet<IntVec3>();
        List<Building> buildings = map.listerBuildings.allBuildingsColonist;
        for (int i = 0; i < buildings.Count; i++)
        {
            if (buildings[i] is Building_LogisticsConduit conduit && conduit.def == CorvusDefOf.Corvus_HiddenLogisticsConduit)
            {
                cells.Add(conduit.Position);
            }
        }

        DrawNetwork(map, cells, extraCell, includeExtraCell);
    }

    private static void DrawSegment(IntVec3 a, IntVec3 b)
    {
        GenDraw.DrawLineBetween(
            a.ToVector3Shifted(),
            b.ToVector3Shifted(),
            AltitudeLayer.MetaOverlays.AltitudeFor(),
            LineMaterial,
            LineWidth);
    }

    private static void DrawIsolatedCell(IntVec3 cell)
    {
        Vector3 center = cell.ToVector3Shifted();
        Vector3 horizontal = new Vector3(0.28f, 0f, 0f);
        Vector3 vertical = new Vector3(0f, 0f, 0.28f);
        GenDraw.DrawLineBetween(center - horizontal, center + horizontal, AltitudeLayer.MetaOverlays.AltitudeFor(), LineMaterial, LineWidth);
        GenDraw.DrawLineBetween(center - vertical, center + vertical, AltitudeLayer.MetaOverlays.AltitudeFor(), LineMaterial, LineWidth);
    }
}
