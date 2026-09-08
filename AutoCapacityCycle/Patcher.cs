using System.Reflection;
using HarmonyLib;

namespace AutoCapacityCycle
{
    // CitiesHarmony 官方推荐模式：先经 HarmonyHelper 确认运行时就绪，
    // 再由本类集中执行 PatchAll / UnpatchAll。
    public static class Patcher
    {
        public const string HarmonyId = "com.autocapacitycycle.autoemptylandfill";

        private static bool s_patched;

        public static bool Patched
        {
            get { return s_patched; }
        }

        public static void PatchAll()
        {
            if (s_patched)
            {
                return;
            }
            s_patched = true;

            Harmony harmony = new Harmony(HarmonyId);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static void UnpatchAll()
        {
            if (!s_patched)
            {
                return;
            }

            Harmony harmony = new Harmony(HarmonyId);
            harmony.UnpatchAll(HarmonyId);
            s_patched = false;
        }
    }
}
