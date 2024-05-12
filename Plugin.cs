using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using Rewired;

namespace HoldtoSprint;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private static readonly Harmony Harmony = new(MyPluginInfo.PLUGIN_GUID);

    private void Awake()
    {
        Harmony.Patch(
            AccessTools.Method(typeof(LocalizedText), nameof(LocalizedText.LoadMainTable)), // Target Method to Patch (ClassName, MethodName, Argument Types)
            postfix: new HarmonyMethod(typeof(Plugin), nameof(LocalizedText_LoadMainTable_Patch)) // Patch Method Prefix/Postfix/Transpiler (ClassName, MethodName)
        );
        Harmony.Patch(
            AccessTools.Method(typeof(OptionsMenu), nameof(OptionsMenu.OpenWindow)), // Target Method to Patch (ClassName, MethodName, Argument Types)
            postfix: new HarmonyMethod(typeof(Plugin), nameof(OptionsMenu_OpenWindow_Patch)) // Patch Method Prefix/Postfix/Transpiler (ClassName, MethodName)
        );
    }

    // Enable Sprinting when Sprinting key is Held Down
    public void Update()
    {
        var player = Player.singlePlayer;
        if (player == null) return;

        if (player.canSprint)
            player._sprinting = Input.GetKey(player.input.GetKeyCode("SprintTap"));

    }

    // Patch Localization to add new Locale for Hold to Sprint
    static void LocalizedText_LoadMainTable_Patch()
    {
        List<string> values_list = [
            "Hold Sprint",
            "Appuyer pour Sprinter",
            "Premi: Tieni premuto",
            "Halte drücken zum Sprinten",
            "Mantén Correr",
            "Mantenha para correr",
            "Удерживайте кнопку «Бег»",
            "按住衝刺",
            "按住冲刺",
            "ダッシュをホールド",
            "달리기 누르고 있기"];

        LocalizedText.MAIN_TABLE.Add("OPTIONS_SPRINTMODE_HOLD", values_list);
    }

    // Patch Settings Menu to add new Option for Hold to Sprint
    static void OptionsMenu_OpenWindow_Patch(OptionsMenu? __instance)
    {
        // Add New Option for Hold to Sprint
        var sprintOption = __instance?.GetOption("Sprint Mode");
        if (sprintOption != null) sprintOption.enumOptions = [sprintOption.enumOptions[0], sprintOption.enumOptions[1], "OPTIONS_SPRINTMODE_HOLD"];
    }
}
public static class Extensions
{
    public static OptionsMenuOption? GetOption(this OptionsMenu optionsMenu, string optionName) =>
        optionsMenu.optionsParent.GetComponentsInChildren<OptionsMenuOption>(true).FirstOrDefault(option => option.name == optionName);

    public static KeyCode GetKeyCode(this PlayerInputRewired input, string actionName)
    {
        if (input.controller.controllers.maps == null) throw new KeyNotFoundException($"Key: {actionName} Not found in Rewired Mappings");

        var actionElementMap = input.controller.controllers.maps.GetFirstButtonMapWithAction(ControllerType.Keyboard, 0, actionName, false);

        return actionElementMap == null ? throw new KeyNotFoundException($"Key: {actionName} Not found in Rewired Mappings") : actionElementMap.keyCode;
    }
}