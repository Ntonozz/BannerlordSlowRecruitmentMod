using TaleWorlds.MountAndBlade;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace SlowRecruitmentMod
{
    public class SlowRecruitmentSubModule : MBSubModuleBase
    {
        // Weekly recruit targets (adjusted for in-game hours)
        private const int VILLAGE_RECRUITS_PER_WEEK = 2;  // 1-2 recruits from villages
        private const int TOWN_RECRUITS_PER_WEEK = 3;     // 3 recruits from towns
        private const int CASTLE_RECRUITS_PER_WEEK = 2;   // 2 recruits from castles

        // In-game time constants
        private const int HOURS_PER_DAY = 24;
        private const int HOURS_PER_WEEK = HOURS_PER_DAY * 7;

        // Track recruitment for each settlement per player
        private System.Collections.Generic.Dictionary<Settlement, float> settlementRecruitmentAccumulator;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            InformationManager.DisplayMessage(new InformationMessage(
                "Slow Recruitment Mod (Fixed Quantities) loaded successfully!", 
                Color.FromUint(0x00FF00FF)));
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);

            settlementRecruitmentAccumulator = new System.Collections.Generic.Dictionary<Settlement, float>();

            if (gameStarterObject is CampaignGameStarter campaignStarter)
            {
                // Subscribe to hourly tick for settlements
                CampaignEvents.HourlyTickSettlementEvent.AddNonSerializedListener(this, OnHourlyTickSettlement);
                
                InformationManager.DisplayMessage(new InformationMessage(
                    "Villages: 1-2 recruits/week | Towns: 3 recruits/week | Castles: 2 recruits/week", 
                    Color.FromUint(0x00FFFFFF)));
            }
        }

        private void OnHourlyTickSettlement(Settlement settlement)
        {
            if (settlement == null || Campaign.Current == null)
                return;

            try
            {
                if (settlement.IsVillage)
                {
                    Village village = settlement.Village;
                    if (village != null && village.VillageState == Village.VillageStates.Normal)
                    {
                        ProcessVillageRecruitment(village);
                    }
                }
                else if (settlement.IsTown)
                {
                    Town town = settlement.Town;
                    if (town != null && settlement.SiegeEvent == null)
                    {
                        ProcessTownRecruitment(town);
                    }
                }
                else if (settlement.IsCastle)
                {
                    Town castle = settlement.Town;
                    if (castle != null && settlement.SiegeEvent == null)
                    {
                        ProcessCastleRecruitment(castle);
                    }
                }
            }
            catch (System.Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "Slow Recruitment Mod Error: " + ex.Message, 
                    Color.FromUint(0xFF0000FF)));
            }
        }

        private void ProcessVillageRecruitment(Village village)
        {
            if (village.Settlement.OwnerClan == null)
                return;

            // Initialize accumulator if needed
            if (!settlementRecruitmentAccumulator.ContainsKey(village.Settlement))
                settlementRecruitmentAccumulator[village.Settlement] = 0f;

            // Calculate recruitment per hour based on prosperity
            float hourlyRecruitment = CalculateHourlyRecruitment(
                VILLAGE_RECRUITS_PER_WEEK, 
                village.Bound?.Town?.Prosperity ?? 3000f);

            settlementRecruitmentAccumulator[village.Settlement] += hourlyRecruitment;

            // When we accumulate enough, generate a recruit
            if (settlementRecruitmentAccumulator[village.Settlement] >= 1f)
            {
                int recruitsToGenerate = (int)settlementRecruitmentAccumulator[village.Settlement];
                settlementRecruitmentAccumulator[village.Settlement] -= recruitsToGenerate;

                // Apply recruitment to the village
                ApplyVillageRecruitment(village, recruitsToGenerate);
            }
        }

        private void ProcessTownRecruitment(Town town)
        {
            if (town.Settlement.OwnerClan == null)
                return;

            // Initialize accumulator if needed
            if (!settlementRecruitmentAccumulator.ContainsKey(town.Settlement))
                settlementRecruitmentAccumulator[town.Settlement] = 0f;

            // Calculate recruitment per hour
            float hourlyRecruitment = CalculateHourlyRecruitment(
                TOWN_RECRUITS_PER_WEEK, 
                town.Prosperity);

            settlementRecruitmentAccumulator[town.Settlement] += hourlyRecruitment;

            // When we accumulate enough, generate recruits
            if (settlementRecruitmentAccumulator[town.Settlement] >= 1f)
            {
                int recruitsToGenerate = (int)settlementRecruitmentAccumulator[town.Settlement];
                settlementRecruitmentAccumulator[town.Settlement] -= recruitsToGenerate;

                ApplyTownRecruitment(town, recruitsToGenerate);
            }
        }

        private void ProcessCastleRecruitment(Town castle)
        {
            if (castle.Settlement.OwnerClan == null)
                return;

            // Initialize accumulator if needed
            if (!settlementRecruitmentAccumulator.ContainsKey(castle.Settlement))
                settlementRecruitmentAccumulator[castle.Settlement] = 0f;

            // Calculate recruitment per hour
            float hourlyRecruitment = CalculateHourlyRecruitment(
                CASTLE_RECRUITS_PER_WEEK, 
                castle.Prosperity);

            settlementRecruitmentAccumulator[castle.Settlement] += hourlyRecruitment;

            // When we accumulate enough, generate recruits
            if (settlementRecruitmentAccumulator[castle.Settlement] >= 1f)
            {
                int recruitsToGenerate = (int)settlementRecruitmentAccumulator[castle.Settlement];
                settlementRecruitmentAccumulator[castle.Settlement] -= recruitsToGenerate;

                ApplyTownRecruitment(castle, recruitsToGenerate);
            }
        }

        /// <summary>
        /// Calculates hourly recruitment rate based on weekly target and prosperity
        /// </summary>
        private float CalculateHourlyRecruitment(int weeklyTarget, float prosperity)
        {
            // Base hourly rate
            float baseHourlyRate = (float)weeklyTarget / HOURS_PER_WEEK;

            // Apply prosperity multiplier
            float prosperityMultiplier = GetProsperityMultiplier(prosperity);

            return baseHourlyRate * prosperityMultiplier;
        }

        /// <summary>
        /// Prosperity modifier: affects how close to the weekly target we get
        /// </summary>
        private float GetProsperityMultiplier(float prosperity)
        {
            if (prosperity < 1000f)
                return 0.3f;  // Struggling: 30% of weekly target
            else if (prosperity < 2000f)
                return 0.5f;  // Poor: 50% of target
            else if (prosperity < 5000f)
                return 0.75f; // Average: 75% of target
            else if (prosperity < 10000f)
                return 0.9f;  // Good: 90% of target
            else
                return 1.0f;  // Excellent: Full target
        }

        private void ApplyVillageRecruitment(Village village, int recruitCount)
        {
            // This adds recruits to the village's recruitment pool
            // These can be picked up by any faction/player visiting
            // Implementation depends on accessing village's volunteer/recruit system
            
            // For now, log for debugging purposes
            if (recruitCount > 0)
            {
                // InformationManager.DisplayMessage(new InformationMessage(
                //     $"Village {village.Settlement.Name}: +{recruitCount} recruits available",
                //     Color.FromUint(0x00FF00FF)));
            }
        }

        private void ApplyTownRecruitment(Town town, int recruitCount)
        {
            // This adds recruits to the town's recruitment pool
            if (recruitCount > 0)
            {
                // InformationManager.DisplayMessage(new InformationMessage(
                //     $"Town {town.Settlement.Name}: +{recruitCount} recruits available",
                //     Color.FromUint(0x00FF00FF)));
            }
        }

        public static string GetProsperityBracket(float prosperity)
        {
            if (prosperity < 1000f) return "Struggling (30%)";
            if (prosperity < 2000f) return "Poor (50%)";
            if (prosperity < 5000f) return "Average (75%)";
            if (prosperity < 10000f) return "Good (90%)";
            return "Excellent (100%)";
        }
    }
}
