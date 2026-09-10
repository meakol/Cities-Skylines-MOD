using System;
using ColossalFramework.Globalization;
using UnityEngine;

namespace AutoCapacityCycle
{
    // 界面语言：0=自动（跟随游戏语言），1=简体中文，2=English
    // （索引与 ModSettings.LanguageMode 一一对应）
    public static class ModLocalization
    {
        public const int LanguageAuto = 0;
        public const int LanguageChinese = 1;
        public const int LanguageEnglish = 2;

        // 当前是否显示中文
        public static bool IsChinese
        {
            get
            {
                switch (ModSettings.LanguageMode)
                {
                    case LanguageChinese:
                        return true;
                    case LanguageEnglish:
                        return false;
                    default:
                        return GameLanguageIsChinese();
                }
            }
        }

        // 自动模式：读取游戏当前语言（以 "zh" 开头视为中文），失败则退回系统语言
        private static bool GameLanguageIsChinese()
        {
            try
            {
                string code = LocaleInfo.currentLanguage;
                if (!string.IsNullOrEmpty(code))
                {
                    return code.ToLowerInvariant().Contains("zh");
                }
            }
            catch
            {
            }

            try
            {
                return Application.systemLanguage.ToString().IndexOf("Chinese", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch
            {
                return false;
            }
        }

        // MOD 名称（内容管理器显示，两种语言一致）
        public static string ModName
        {
            get { return "Auto Capacity Cycle"; }
        }

        public static string Description
        {
            get
            {
                return IsChinese
                    ? "垃圾填埋场、垃圾转运设施与公墓满仓时自动触发原版清空流程，清空完成后自动恢复运营。可在 Options > Mods 中开关。需要启用 CitiesHarmony。"
                    : "Automatically starts the vanilla emptying process when Landfills, Waste Transfer Facilities or Cemeteries reach capacity, and restores normal operation once emptied. Toggle in Options > Mods. Requires CitiesHarmony.";
            }
        }

        public static string GroupTitle
        {
            get { return IsChinese ? "满仓自动清空" : "Auto Empty When Full"; }
        }

        public static string LandfillOption
        {
            get
            {
                return IsChinese
                    ? "垃圾填埋场 / 垃圾转运设施：满仓时自动清空，清空完成后自动恢复运营"
                    : "Landfills / Waste Transfer Facilities: auto empty when full, resume normally afterwards";
            }
        }

        public static string CemeteryOption
        {
            get
            {
                return IsChinese
                    ? "公墓：满仓时自动清空，清空完成后自动恢复运营（火葬场不受影响）"
                    : "Cemeteries: auto empty when full, resume normally afterwards (Crematoriums unaffected)";
            }
        }

        public static string LanguageLabel
        {
            get
            {
                return IsChinese
                    ? "界面语言（更改后请关闭并重新打开本面板生效）"
                    : "UI language (close and reopen this panel after changing)";
            }
        }

        // 语言下拉选项：自动 / 中文 / English
        public static string[] LanguageOptions
        {
            get
            {
                return new string[]
                {
                    IsChinese ? "自动（跟随游戏语言）" : "Auto (follow game language)",
                    "中文",
                    "English"
                };
            }
        }
    }
}
