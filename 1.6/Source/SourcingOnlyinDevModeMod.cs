using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
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

    [StaticConstructorOnStartup]
    public static class Page_ChooseStartingGravship_DrawStartingGravshipItem_Patch
    {
        static Page_ChooseStartingGravship_DrawStartingGravshipItem_Patch()
        {
            if (Prepare())
            {
                var target = TargetMethod();
                if (target is null)
                {
                    Log.Error("Couldn't find VanillaGravshipExpanded.Page_ChooseStartingGravship:DrawStartingGravshipItem to patch");
                    return;
                }
                SourcingOnlyinDevModeMod.harmony.Patch(target, transpiler: new HarmonyMethod(AccessTools.Method(typeof(Page_ChooseStartingGravship_DrawStartingGravshipItem_Patch), nameof(Transpiler))));

                var contentsTarget = AccessTools.Method("VanillaGravshipExpanded.Page_ChooseStartingGravship:DoWindowContents");
                if (contentsTarget is null)
                {
                    Log.Error("Couldn't find VanillaGravshipExpanded.Page_ChooseStartingGravship:DoWindowContents to patch");
                    return;
                }
                SourcingOnlyinDevModeMod.harmony.Patch(contentsTarget, transpiler: new HarmonyMethod(AccessTools.Method(typeof(Page_ChooseStartingGravship_DrawStartingGravshipItem_Patch), nameof(DoWindowContentsTranspiler))));
            }
        }

        public static IEnumerable<CodeInstruction> DoWindowContentsTranspiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            var itemHeightInfo = AccessTools.Method(typeof(Page_ChooseStartingGravship_DrawStartingGravshipItem_Patch), nameof(ItemHeight));
            var rowStrideInfo = AccessTools.Method(typeof(Page_ChooseStartingGravship_DrawStartingGravshipItem_Patch), nameof(RowStride));
            var patchedHeights = 0;
            var patchedStrides = 0;
            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if (IsLoadingConstant(codes, i, 270f))
                {
                    patchedHeights++;
                    yield return new CodeInstruction(OpCodes.Call, itemHeightInfo).MoveLabelsFrom(code).MoveBlocksFrom(code);
                    continue;
                }
                if (IsLoadingConstant(codes, i, 278f))
                {
                    patchedStrides++;
                    yield return new CodeInstruction(OpCodes.Call, rowStrideInfo).MoveLabelsFrom(code).MoveBlocksFrom(code);
                    continue;
                }
                yield return code;
            }
            if (patchedHeights == 0 || patchedStrides == 0)
            {
                Log.Error("Couldn't find the item height in VanillaGravshipExpanded.Page_ChooseStartingGravship:DoWindowContents");
            }
        }

        public static float ItemHeight()
        {
            return 270f - 20f + SourceHeight();
        }

        public static float RowStride()
        {
            return ItemHeight() + 8f;
        }

        public static bool Prepare()
        {
            return ModsConfig.IsActive("vanillaexpanded.gravship");
        }

        public static MethodBase TargetMethod()
        {
            return AccessTools.Method("VanillaGravshipExpanded.Page_ChooseStartingGravship:DrawStartingGravshipItem");
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            var labelInfo = AccessTools.Method(typeof(Widgets), nameof(Widgets.Label), new[] { typeof(Rect), typeof(TaggedString) });
            var sourceHeightInfo = AccessTools.Method(typeof(Page_ChooseStartingGravship_DrawStartingGravshipItem_Patch), nameof(SourceHeight));
            var found = false;
            var patchedLabel = false;
            var patchedHeight = false;
            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if (code.opcode == OpCodes.Ldstr && code.operand as string == "VGE_SourceMod")
                {
                    found = true;
                }
                if (found && patchedLabel is false && code.Calls(labelInfo))
                {
                    patchedLabel = true;
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Page_ChooseStartingGravship_DrawStartingGravshipItem_Patch), nameof(LabelSource)));
                    continue;
                }
                if (patchedHeight is false && IsLoadingConstant(codes, i, 20f) && i + 3 < codes.Count
                    && codes[i + 1].opcode == OpCodes.Sub && IsLoadingConstant(codes, i + 2, 8f) && codes[i + 3].opcode == OpCodes.Sub)
                {
                    patchedHeight = true;
                    yield return new CodeInstruction(OpCodes.Call, sourceHeightInfo).MoveLabelsFrom(code).MoveBlocksFrom(code);
                    continue;
                }
                yield return code;
            }
            if (patchedLabel is false)
            {
                Log.Error("Couldn't find the source label in VanillaGravshipExpanded.Page_ChooseStartingGravship:DrawStartingGravshipItem");
            }
            if (patchedHeight is false)
            {
                Log.Error("Couldn't find the description height in VanillaGravshipExpanded.Page_ChooseStartingGravship:DrawStartingGravshipItem");
            }
        }

        private static bool IsLoadingConstant(List<CodeInstruction> codes, int index, float value)
        {
            return codes[index].opcode == OpCodes.Ldc_R4 && codes[index].operand is float f && f == value;
        }

        public static float SourceHeight()
        {
            return Prefs.DevMode ? 20f : 0f;
        }

        public static void LabelSource(Rect rect, TaggedString label)
        {
            if (Prefs.DevMode)
            {
                Widgets.Label(rect, label);
            }
        }
    }
}
