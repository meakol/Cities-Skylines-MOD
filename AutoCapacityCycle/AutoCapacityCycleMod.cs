using ICities;
using CitiesHarmony.API;

// 自动清空垃圾填埋场 MOD —— IUserMod 入口。
// 注意：IUserMod 实现中不允许引用任何 HarmonyLib 类型（CitiesHarmony 硬性要求），
// 否则在未安装/未启用 CitiesHarmony 时本 MOD 将无法加载；
// 所有 HarmonyLib 相关代码集中在 Patcher.cs，通过 HarmonyHelper 确认就绪后再挂载。
// 界面文案统一走 ModLocalization（中/英双语，可在 Options 里选择语言）。
namespace AutoCapacityCycle
{
    public class AutoCapacityCycleMod : IUserMod
    {
        public string Name
        {
            get { return ModLocalization.ModName; }
        }

        public string Description
        {
            get { return ModLocalization.Description; }
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

        // 游戏 Options > Mods 面板：语言下拉 + 两个独立开关。
        // 开关勾选即时生效；语言更改后需关闭并重新打开本面板刷新文案。
        public void OnSettingsUI(UIHelperBase helper)
        {
            ModSettings.EnsureLoaded();

            helper.AddDropdown(
                ModLocalization.LanguageLabel,
                ModLocalization.LanguageOptions,
                ModSettings.LanguageMode,
                delegate(int selected)
                {
                    ModSettings.SetLanguageMode(selected);
                });

            UIHelperBase group = helper.AddGroup(ModLocalization.GroupTitle);
            group.AddCheckbox(
                ModLocalization.LandfillOption,
                ModSettings.AutoEmptyLandfills,
                delegate(bool isChecked)
                {
                    ModSettings.SetAutoEmptyLandfills(isChecked);
                });
            group.AddCheckbox(
                ModLocalization.CemeteryOption,
                ModSettings.AutoEmptyCemeteries,
                delegate(bool isChecked)
                {
                    ModSettings.SetAutoEmptyCemeteries(isChecked);
                });
        }
    }
}
