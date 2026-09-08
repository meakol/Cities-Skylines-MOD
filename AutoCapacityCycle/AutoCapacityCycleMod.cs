using ICities;
using CitiesHarmony.API;

// 自动清空垃圾填埋场 MOD —— IUserMod 入口。
// 注意：IUserMod 实现中不允许引用任何 HarmonyLib 类型（CitiesHarmony 硬性要求），
// 否则在未安装/未启用 CitiesHarmony 时本 MOD 将无法加载；
// 所有 HarmonyLib 相关代码集中在 Patcher.cs，通过 HarmonyHelper 确认就绪后再挂载。
namespace AutoCapacityCycle
{
    public class AutoCapacityCycleMod : IUserMod
    {
        public string Name
        {
            get { return "Auto Capacity Cycle"; }
        }

        public string Description
        {
            get
            {
                return "垃圾填埋场、垃圾转运设施与公墓满仓时自动触发原版清空流程，"
                     + "清空完成后自动恢复运营。可在 Options > Mods 中开关。需要启用 CitiesHarmony。";
            }
        }

        public void OnEnabled()
        {
            ModSettings.EnsureLoaded();
            HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());
        }

        public void OnDisabled()
        {
            if (HarmonyHelper.IsHarmonyInstalled)
            {
                Patcher.UnpatchAll();
            }
        }

        // 游戏 Options > Mods 面板：两个独立开关，勾选后即时生效，状态持久化到本地 XML。
        public void OnSettingsUI(UIHelperBase helper)
        {
            ModSettings.EnsureLoaded();

            UIHelperBase group = helper.AddGroup("满仓自动清空");
            group.AddCheckbox(
                "垃圾填埋场 / 垃圾转运设施：满仓时自动清空，清空完成后自动恢复运营",
                ModSettings.AutoEmptyLandfills,
                delegate(bool isChecked)
                {
                    ModSettings.SetAutoEmptyLandfills(isChecked);
                });
            group.AddCheckbox(
                "公墓：满仓时自动清空，清空完成后自动恢复运营（火葬场不受影响）",
                ModSettings.AutoEmptyCemeteries,
                delegate(bool isChecked)
                {
                    ModSettings.SetAutoEmptyCemeteries(isChecked);
                });
        }
    }
}
