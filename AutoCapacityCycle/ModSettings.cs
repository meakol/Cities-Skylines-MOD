using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace AutoCapacityCycle
{
    // MOD 设置：只有一个“满仓自动清空”总开关。
    // 以 XML 形式保存在 MOD 目录（首选）或游戏用户数据目录（兜底）。
    // 补丁代码每一帧直接读取 AutoEmptyEnabled 静态属性，勾选后即时生效，无需重载 MOD。
    public static class ModSettings
    {
        private const string FileName = "AutoCapacityCycle_settings.xml";

        // 真正用于序列化的数据（公共字段，默认都开启）
        [XmlRoot("AutoCapacityCycleSettings")]
        public class Data
        {
            // 垃圾填埋场开关：XML 保留旧元素名 AutoEmptyEnabled，已保存过的设置不会丢
            [XmlElement("AutoEmptyEnabled")]
            public bool AutoEmptyLandfills = true;

            // 公墓开关（仅 m_graveCount > 0 的 CemeteryAI 即“公墓”，火葬场不受影响）
            [XmlElement("AutoEmptyCemeteries")]
            public bool AutoEmptyCemeteries = true;

            // 界面语言：0=自动（跟随游戏语言），1=简体中文，2=English
            [XmlElement("Language")]
            public int LanguageMode;
        }

        private static Data s_data;
        private static string s_path;

        // 运行时开关状态（未加载配置时按默认值=开启处理）
        public static bool AutoEmptyLandfills
        {
            get { return s_data == null || s_data.AutoEmptyLandfills; }
        }

        public static bool AutoEmptyCemeteries
        {
            get { return s_data == null || s_data.AutoEmptyCemeteries; }
        }

        // 界面语言：0=自动，1=中文，2=English（未加载配置时为 0=自动）
        public static int LanguageMode
        {
            get { return s_data == null ? 0 : s_data.LanguageMode; }
        }

        public static void EnsureLoaded()
        {
            if (s_data == null)
            {
                Load();
            }
        }

        public static void Load()
        {
            s_path = GetSettingsFilePath();
            Data loaded = null;
            if (!string.IsNullOrEmpty(s_path) && File.Exists(s_path))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Data));
                    using (StreamReader reader = new StreamReader(s_path))
                    {
                        loaded = (Data)serializer.Deserialize(reader);
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogWarning("[AutoCapacityCycle] 读取设置失败，使用默认值: " + e.Message);
                }
            }
            s_data = loaded ?? new Data();
        }

        public static void SetAutoEmptyLandfills(bool value)
        {
            EnsureLoaded();
            if (s_data.AutoEmptyLandfills == value)
            {
                return;
            }
            s_data.AutoEmptyLandfills = value;
            Save();
        }

        public static void SetAutoEmptyCemeteries(bool value)
        {
            EnsureLoaded();
            if (s_data.AutoEmptyCemeteries == value)
            {
                return;
            }
            s_data.AutoEmptyCemeteries = value;
            Save();
        }

        public static void SetLanguageMode(int value)
        {
            EnsureLoaded();
            if (value < 0)
            {
                value = 0;
            }
            else if (value > 2)
            {
                value = 2;
            }
            if (s_data.LanguageMode == value)
            {
                return;
            }
            s_data.LanguageMode = value;
            Save();
        }

        public static void Save()
        {
            if (s_data == null || string.IsNullOrEmpty(s_path))
            {
                return;
            }
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Data));
                using (StreamWriter writer = new StreamWriter(s_path))
                {
                    serializer.Serialize(writer, s_data);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning("[AutoCapacityCycle] 保存设置失败: " + e.Message);
            }
        }

        // 设置文件存放路径
        private static string GetSettingsFilePath()
        {
            // 1) 首选：与 MOD dll 同目录（本地 Addons/Mods 部署时可写）
            try
            {
                string location = Assembly.GetExecutingAssembly().Location;
                if (!string.IsNullOrEmpty(location))
                {
                    string dir = Path.GetDirectoryName(location);
                    if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                    {
                        return Path.Combine(dir, FileName);
                    }
                }
            }
            catch { }

            // 2) 兜底：游戏用户数据目录
            try
            {
                string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (!string.IsNullOrEmpty(local))
                {
                    // net35 的 Path.Combine 只有两个参数的重载
                    string dir = Path.Combine(Path.Combine(local, "Colossal Order"), "Cities_Skylines");
                    Directory.CreateDirectory(dir);
                    return Path.Combine(dir, FileName);
                }
            }
            catch { }

            return null;
        }
    }
}
