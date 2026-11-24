using System.Reflection;
using HarmonyLib;
using MelonLoader;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(AnimatedJacketMod), "Animated Jacket", "1.0.0", "Mayu", null)]
[assembly: MelonGame("sega-interactive", "Sinmai")]

public class AnimatedJacketMod : MelonMod
{
}

[HarmonyPatch(typeof(AssetManager), "GetJacketTexture2D", typeof(string))]
static class AnimatedJacketFullPatch
{
    static void Prefix(string filename)
    {
        AnimatedJacket.Note(filename);
    }

    static void Postfix(UnityEngine.Texture2D __result, string filename)
    {
        AnimatedJacket.Remember(__result, filename);
    }
}

[HarmonyPatch(typeof(AssetManager), "GetJacketThumbTexture2D", typeof(string))]
static class AnimatedJacketThumbPatch
{
    static void Prefix(string filename)
    {
        AnimatedJacket.Note(filename);
    }

    static void Postfix(UnityEngine.Texture2D __result, string filename)
    {
        AnimatedJacket.Remember(__result, filename);
    }
}

[HarmonyPatch(typeof(Monitor.MusicSelect.ChainList.MusicChainCardObejct), "SetMusicData")]
static class AnimatedJacketCardPatch
{
    static readonly FieldInfo JacketField = AccessTools.Field(typeof(Monitor.MusicSelect.ChainList.MusicChainCardObejct), "_jacketImage");

    static void Postfix(Monitor.MusicSelect.ChainList.MusicChainCardObejct __instance)
    {
        RawImage image = JacketField.GetValue(__instance) as RawImage;
        AnimatedJacket.OnJacketSet(__instance, image);
    }
}

[HarmonyPatch(typeof(Monitor.MusicSelect.ChainList.MusicChainCardObejct), "ChangeSize")]
static class AnimatedJacketCenterPatch
{
    static readonly FieldInfo JacketField = AccessTools.Field(typeof(Monitor.MusicSelect.ChainList.MusicChainCardObejct), "_jacketImage");

    static void Postfix(Monitor.MusicSelect.ChainList.MusicChainCardObejct __instance, bool isMainActive)
    {
        RawImage image = JacketField.GetValue(__instance) as RawImage;
        AnimatedJacket.OnCenterChanged(__instance, image, isMainActive);
    }
}

[HarmonyPatch(typeof(MusicInfomationController), "SetMusicData")]
static class AnimatedJacketInfoPatch
{
    static readonly FieldInfo JacketField = AccessTools.Field(typeof(MusicInfomationController), "musicJacket");

    static void Postfix(MusicInfomationController __instance, MessageMusicData data)
    {
        if (data == null)
            return;
        RawImage image = JacketField.GetValue(__instance) as RawImage;
        AnimatedJacket.OnInfo(image, data.MusicId);
    }
}
