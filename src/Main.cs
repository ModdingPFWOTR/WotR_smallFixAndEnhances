using System;
using HarmonyLib;
using UnityModManagerNet;

namespace smallFixAndEnhances
{
    public static class Main
    {
        public static UnityModManager.ModEntry ModEntry;
        public static Harmony HarmonyInstance;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            ModEntry = modEntry;
            HarmonyInstance = new Harmony(modEntry.Info.Id);
            modEntry.OnGUI = OnGUI;

            try
            {
                HarmonyInstance.PatchAll();
                Log("smallFixAndEnhances loaded.");
            }
            catch (Exception ex)
            {
                Log("Harmony patch failed: " + ex);
                return false;
            }

            return true;
        }

        public static void Log(string message)
        {
            ModEntry?.Logger.Log(message);
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            UnityEngine.GUILayout.Label("Divine Weapon Bond and Divine Hunter Bond skip the weapon-enchant animation while the caster is mounted.");
            UnityEngine.GUILayout.Label("On foot, the animation still plays.");
            UnityEngine.GUILayout.Label("Cackle and Chant grant Protective Luck to allies within 30 feet when the caster knows that hex.");
            UnityEngine.GUILayout.Label("It lasts the hex's normal duration (1 round, +1 at class level 8 and 16) and is refreshed instead of stacked.");
        }
    }
}
