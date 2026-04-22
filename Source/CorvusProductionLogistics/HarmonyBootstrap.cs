using HarmonyLib;
using Verse;

namespace CorvusProductionLogistics;

[StaticConstructorOnStartup]
public static class HarmonyBootstrap
{
    static HarmonyBootstrap()
    {
        Harmony harmony = new Harmony("andre.corvusproductionlogistics");
        harmony.PatchAll();
    }
}

