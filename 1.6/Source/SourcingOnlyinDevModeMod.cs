using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SourcingOnlyinDevMode
{
    public class SourcingOnlyinDevModeMod : Mod
    {
        public static Harmony harmony;
        public SourcingOnlyinDevModeMod(ModContentPack pack) : base(pack)
        {
            harmony = new Harmony("SourcingOnlyinDevModeMod");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(MainTabWindow_Research), "DrawContentSource")]
    public static class MainTabWindow_Research_DrawContentSource_Patch
    {
        public static bool Prefix()
        {
            if (Prefs.DevMode is false)
            {
                return false;
            }
            return true;
        }
    }

    [StaticConstructorOnStartup]
    public static class MainTabWindow_NextResearch_DrawContentSource_Patch
    {
        static MainTabWindow_NextResearch_DrawContentSource_Patch()
        {
            if (Prepare())
            {
                var target = TargetMethod();
                if (target is null)
                {
                    Log.Error("Couldn't find CM_Semi_Random_Research.MainTabWindow_NextResearch:DrawContentSource to patch");
                    return;
                }
                SourcingOnlyinDevModeMod.harmony.Patch(target, prefix: new HarmonyMethod(AccessTools.Method(typeof(MainTabWindow_NextResearch_DrawContentSource_Patch), nameof(Prefix))));
            }
        }
        public static bool Prepare()
        {
            return ModsConfig.IsActive("arodoid.semirandomprogression");
        }

        public static MethodBase TargetMethod()
        {
            return AccessTools.Method("CM_Semi_Random_Research.MainTabWindow_NextResearch:DrawContentSource");
        }

        public static bool Prefix()
        {
            if (Prefs.DevMode is false)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(TransferableUIUtility), "ContentSourceDescription")]
    public static class TransferableUIUtility_ContentSourceDescription_Patch
    {
        public static bool Prefix()
        {
            if (Prefs.DevMode is false)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Def), "SpecialDisplayStats")]
    public static class Def_SpecialDisplayStats_Patch
    {
        public static IEnumerable<StatDrawEntry> Postfix(IEnumerable<StatDrawEntry> __result)
        {
            foreach (var entry in __result)
            {
                if (entry.category == StatCategoryDefOf.Source && Prefs.DevMode is false)
                {
                    continue;
                }
                else
                {
                    yield return entry;
                }
            }
        }
    }

    [HarmonyPatch(typeof(ResearchProjectDef), "GetTip")]
    public static class ResearchProjectDef_GetTip_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            var get_IsCoreModInfo = AccessTools.Method(typeof(ModContentPack), "get_IsCoreMod");
            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                yield return code;
                if (codes[i].opcode == OpCodes.Brtrue_S && codes[i - 1].Calls(get_IsCoreModInfo))
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ResearchProjectDef_GetTip_Patch), nameof(ShouldShow)));
                    yield return new CodeInstruction(OpCodes.Brfalse_S, codes[i].operand);
                }
            }
        }

        public static bool ShouldShow()
        {
            return Prefs.DevMode;
        }
    }

    [HarmonyPatch(typeof(BackstoryDef), "FullDescriptionFor")]
    public static class BackstoryDef_FullDescriptionFor_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            var get_IsCoreModInfo = AccessTools.Method(typeof(ModContentPack), "get_IsOfficialMod");
            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                yield return code;
                if (codes[i].opcode == OpCodes.Brtrue_S && codes[i - 1].Calls(get_IsCoreModInfo))
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BackstoryDef_FullDescriptionFor_Patch), nameof(ShouldShow)));
                    yield return new CodeInstruction(OpCodes.Brfalse_S, codes[i].operand);
                }
            }
        }

        public static bool ShouldShow()
        {
            return Prefs.DevMode;
        }
    }
}
