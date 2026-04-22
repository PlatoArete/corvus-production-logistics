using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace CorvusProductionLogistics.Patches;

[HarmonyPatch]
public static class AgriculturalOutputPatches
{
    [ThreadStatic]
    private static bool isRoutingHarvestOutput;

    public static IEnumerable<MethodBase> TargetMethods()
    {
        foreach (MethodInfo method in AccessTools.GetDeclaredMethods(typeof(GenPlace)))
        {
            ParameterInfo[] parameters = method.GetParameters();
            if (method.Name == nameof(GenPlace.TryPlaceThing)
                && parameters.Length == 8
                && !parameters[4].ParameterType.IsByRef)
            {
                yield return method;
            }
        }
    }

    [HarmonyPrefix]
    public static bool TryPlaceThingPrefix(
        Thing thing,
        IntVec3 center,
        Map map,
        ThingPlaceMode mode,
        ref bool __result)
    {
        if (isRoutingHarvestOutput || mode != ThingPlaceMode.Near)
        {
            return true;
        }

        if (!AgriculturalOutputRouting.TryFindConnectedStorageCellForHarvestProduct(thing, center, map, out IntVec3 storageCell))
        {
            return true;
        }

        try
        {
            isRoutingHarvestOutput = true;
            if (GenPlace.TryPlaceThing(thing, storageCell, map, ThingPlaceMode.Direct))
            {
                __result = true;
                return false;
            }

            return true;
        }
        finally
        {
            isRoutingHarvestOutput = false;
        }
    }
}
