using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Party;

namespace SlowRecruitmentMod.Patches
{
    [HarmonyPatch(typeof(Village))]
    public class VillageRecruitmentQuantityPatch
    {
        // 1-2 recruits per week from villages
        private const int VILLAGE_RECRUITS_PER_WEEK = 2;
        private const int HOURS_PER_WEEK = 168; // 24 * 7

        private static float GetProsperityMultiplier(float prosperity)
        {
            if (prosperity < 1000f) return 0.3f;
            if (prosperity < 2000f) return 0.5f;
            if (prosperity < 5000f) return 0.75f;
            if (prosperity < 10000f) return 0.9f;
            return 1.0f;
        }

        /// <summary>
        /// Patch the volunteer production to match fixed weekly rates
        /// </summary>
        [HarmonyPatch("GetVolunteerProductionSum")]
        [HarmonyPostfix]
        public static void PatchVolunteerProduction(ref float __result, Village __instance)
        {
            if (__instance.Bound == null || __instance.Bound.Town == null)
                return;

            if (__instance.Settlement.SiegeEvent != null)
                return;

            // Calculate target hourly rate for 2 recruits per week
            float targetHourlyRate = (float)VILLAGE_RECRUITS_PER_WEEK / HOURS_PER_WEEK;
            
            // Apply prosperity multiplier
            float prosperity = __instance.Bound.Town.Prosperity;
            float multiplier = GetProsperityMultiplier(prosperity);

            __result = targetHourlyRate * multiplier;
        }

        [HarmonyPatch("GetRecruitsProductionSum")]
        [HarmonyPostfix]
        public static void PatchRecruitsProduction(ref float __result, Village __instance)
        {
            if (__instance.Bound == null || __instance.Bound.Town == null)
                return;

            if (__instance.Settlement.SiegeEvent != null)
                return;

            float targetHourlyRate = (float)VILLAGE_RECRUITS_PER_WEEK / HOURS_PER_WEEK;
            float prosperity = __instance.Bound.Town.Prosperity;
            float multiplier = GetProsperityMultiplier(prosperity);

            __result = targetHourlyRate * multiplier;
        }
    }

    [HarmonyPatch(typeof(Town))]
    public class TownRecruitmentQuantityPatch
    {
        // 3 recruits per week from towns
        private const int TOWN_RECRUITS_PER_WEEK = 3;
        private const int HOURS_PER_WEEK = 168;

        private static float GetProsperityMultiplier(float prosperity)
        {
            if (prosperity < 1000f) return 0.3f;
            if (prosperity < 2000f) return 0.5f;
            if (prosperity < 5000f) return 0.75f;
            if (prosperity < 10000f) return 0.9f;
            return 1.0f;
        }

        [HarmonyPatch("GetRecruitSlots")]
        [HarmonyPostfix]
        public static void PatchRecruitSlots(ref int __result, Town __instance)
        {
            if (__instance.Settlement.SiegeEvent != null)
                return;

            // For towns: 3 recruits per week
            float targetHourlyRate = (float)TOWN_RECRUITS_PER_WEEK / HOURS_PER_WEEK;
            float prosperity = __instance.Prosperity;
            float multiplier = GetProsperityMultiplier(prosperity);

            // Convert to slots per hour
            __result = (int)(targetHourlyRate * multiplier * 100); // Scale up for slot calculation
        }

        [HarmonyPatch("GetVolunteerProduction")]
        [HarmonyPostfix]
        public static void PatchVolunteerProduction(ref float __result, Town __instance)
        {
            if (__instance.Settlement.SiegeEvent != null)
                return;

            float targetHourlyRate = (float)TOWN_RECRUITS_PER_WEEK / HOURS_PER_WEEK;
            float prosperity = __instance.Prosperity;
            float multiplier = GetProsperityMultiplier(prosperity);

            __result = targetHourlyRate * multiplier;
        }
    }
}
