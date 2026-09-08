using HarmonyLib;

namespace AutoCapacityCycle
{
    // Patch 1 —— Prefix 拦截 LandfillSiteAI.CheckCapacity：
    // 覆盖纯垃圾填埋场 与 垃圾转运设施（Level3）——条件与原版 CheckCapacity 自身的守卫
    // （不发电、不产材料）保持一致，即“原版自己会做装满检查”的储运类垃圾设施。
    // 垃圾量达到容量上限时自动替玩家设置 Building.Flags.Downgrading，触发原版“清空转移”
    // （ProduceGoods 检测到 Downgrading 后派出 GarbageMove 卡车把存垃圾运往其它有空间的设施），
    // 并 return false 阻止原版 CheckCapacity 设置 CapacityFull / LandfillFull / WasteTransferFacilityFull 红警。
    [HarmonyPatch(typeof(LandfillSiteAI), "CheckCapacity")]
    public static class LandfillAutoEmpty_CheckCapacity
    {
        public static bool Prefix(ref Building buildingData, LandfillSiteAI __instance)
        {
            // 填埋场总开关关闭时，完全放行原版逻辑（不干预）
            if (!ModSettings.AutoEmptyLandfills)
            {
                return true;
            }

            // 与原版 CheckCapacity 的守卫一致（electricity/material 双零）：纯填埋场与垃圾转运设施。
            // 发电/回收等处理型设施（electricity 或 material != 0）原版不会走“装满”逻辑，这里同样不触发。
            if (__instance.m_electricityProduction == 0 &&
                __instance.m_materialProduction == 0)
            {
                int garbageAmount = buildingData.m_customBuffer1 * 1000 + buildingData.m_garbageBuffer;
                if (garbageAmount >= __instance.m_garbageCapacity)
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

    // Patch 2 —— Postfix 监听 LandfillSiteAI.SimulationStepActive：
    // 原版在 Downgrading 清空过程中，若垃圾已全部转移完会设置 EmptyingFinished；
    // 此时自动清除 Downgrading，使填埋场退出“清空中”状态，下一帧恢复正常收垃圾。
    [HarmonyPatch(typeof(LandfillSiteAI), "SimulationStepActive")]
    public static class LandfillAutoEmpty_SimulationStepActive
    {
        public static void Postfix(ref Building buildingData)
        {
            // 填埋场总开关关闭时不干预（不影响玩家手动清空流程）
            if (!ModSettings.AutoEmptyLandfills)
            {
                return;
            }

            if ((buildingData.m_flags & Building.Flags.Downgrading) != Building.Flags.None &&
                (buildingData.m_problems.m_Problems1 & Notification.Problem1.EmptyingFinished) != 0)
            {
                buildingData.m_flags &= ~Building.Flags.Downgrading;
            }
        }
    }
}
