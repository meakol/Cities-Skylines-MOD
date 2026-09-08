using HarmonyLib;

namespace AutoCapacityCycle
{
    // Patch 1 —— Prefix 拦截 CemeteryAI.CheckCapacity：
    // 仅针对“公墓”（m_graveCount > 0）。火葬场 m_graveCount == 0，原版对它的
    // CheckCapacity / IsFull / SetEmptying / CanBeEmptied 全部不生效，天然被排除。
    // 尸体量(buildingData.m_customBuffer1)达到 m_graveCount 上限时自动置 Downgrading，
    // 触发原版灵车 DeadMove 清空流程；return false 阻止原版设置 CapacityFull +
    // LandfillFull 红色告警（原版公墓满也复用它）。
    [HarmonyPatch(typeof(CemeteryAI), "CheckCapacity")]
    public static class CemeteryAutoEmpty_CheckCapacity
    {
        public static bool Prefix(ref Building buildingData, CemeteryAI __instance)
        {
            // 公墓开关关闭时不干预
            if (!ModSettings.AutoEmptyCemeteries)
            {
                return true;
            }

            if (__instance.m_graveCount != 0)
            {
                int corpses = buildingData.m_customBuffer1;
                if (corpses >= __instance.m_graveCount)
                {
                    if ((buildingData.m_flags & Building.Flags.Downgrading) == Building.Flags.None)
                    {
                        // 首次达到上限：替玩家“按下清空按钮”
                        buildingData.m_flags |= Building.Flags.Downgrading;
                    }
                    // 阻止原版 CheckCapacity 剩余逻辑（不设置 CapacityFull / LandfillFull）
                    return false;
                }
            }
            return true;
        }
    }

    // Patch 2 —— Postfix 监听 CemeteryAI.SimulationStepActive：
    // 原版在 Downgrading 清空过程中，当 customBuffer1 == 0（尸体全部转出）时会设置
    // EmptyingFinished；此时自动清除 Downgrading，使公墓退出“清空中”并恢复正常收尸。
    // （同样限定公墓 m_graveCount != 0，火葬场完全不受影响。）
    [HarmonyPatch(typeof(CemeteryAI), "SimulationStepActive")]
    public static class CemeteryAutoEmpty_SimulationStepActive
    {
        public static void Postfix(ref Building buildingData, CemeteryAI __instance)
        {
            if (!ModSettings.AutoEmptyCemeteries)
            {
                return;
            }

            if (__instance.m_graveCount != 0 &&
                (buildingData.m_flags & Building.Flags.Downgrading) != Building.Flags.None &&
                (buildingData.m_problems.m_Problems1 & Notification.Problem1.EmptyingFinished) != 0)
            {
                buildingData.m_flags &= ~Building.Flags.Downgrading;
            }
        }
    }
}
