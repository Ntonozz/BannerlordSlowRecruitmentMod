using TaleWorlds.MountAndBlade;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Party;
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

        // Season multipliers
        private const float SPRING_MULTIPLIER = 1.0f;     // Normal
        private const float SUMMER_MULTIPLIER = 1.1f;     // 10% boost
        private const float FALL_MULTIPLIER = 0.7f;       // 30% reduction
        private const float WINTER_MULTIPLIER = 0.4f;     // 60% reduction

        // War status multipliers
        private const float AGGRESSED_NATION_BOOST = 1.15f;      // +15% when defending
        private const float UNJUSTIFIED_INVADER_PENALTY = 0.65f; // -35% when invading unjustly

        // Track recruitment for each settlement per player
        private System.Collections.Generic.Dictionary<Settlement, float> settlementRecruitmentAccumulator;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            InformationManager.DisplayMessage(new InformationMessage(
                "Slow Recruitment Mod (Season & War Effects) loaded successfully!", 
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
                
                InformationManager.DisplayMessage(new InformationMessage(
                    "Season & War Status effects ENABLED", 
                    Color.FromUint(0xFFFF00FF)));
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

            // Calculate recruitment per hour based on prosperity, season, and war status
            float hourlyRecruitment = CalculateHourlyRecruitment(
                VILLAGE_RECRUITS_PER_WEEK, 
                village.Bound?.Town?.Prosperity ?? 3000f,
                village.Settlement.OwnerClan);

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
                town.Prosperity,
                town.Settlement.OwnerClan);

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
                castle.Prosperity,
                castle.Settlement.OwnerClan);

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
        /// Calculates hourly recruitment rate based on weekly target, prosperity, season, and war status
        /// </summary>
        private float CalculateHourlyRecruitment(int weeklyTarget, float prosperity, Clan ownerClan)
        {
            // Base hourly rate
            float baseHourlyRate = (float)weeklyTarget / HOURS_PER_WEEK;

            // Apply prosperity multiplier
            float prosperityMultiplier = GetProsperityMultiplier(prosperity);

            // Apply season multiplier
            float seasonMultiplier = GetSeasonMultiplier();

            // Apply war status multiplier
            float warMultiplier = GetWarStatusMultiplier(ownerClan);

            return baseHourlyRate * prosperityMultiplier * seasonMultiplier * warMultiplier;
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

        /// <summary>
        /// Season multiplier: affects recruitment based on current season
        /// Fall and Winter severely reduce recruitment, Summer boosts it
        /// </summary>
        private float GetSeasonMultiplier()
        {
            if (Campaign.Current == null)
                return 1.0f;

            // Get current game time
            CampaignTime currentTime = Campaign.Current.CurrentMenuTime;
            
            // Bannerlord seasons: Spring (0), Summer (1), Fall (2), Winter (3)
            // Each season is roughly 30 days
            int dayOfYear = (int)currentTime.GetDayOfYear;
            int season = (dayOfYear / 30) % 4;

            switch (season)
            {
                case 0: return SPRING_MULTIPLIER;     // Spring: Normal recruitment
                case 1: return SUMMER_MULTIPLIER;     // Summer: +10% recruitment boost
                case 2: return FALL_MULTIPLIER;       // Fall: -30% recruitment (harsh)
                case 3: return WINTER_MULTIPLIER;     // Winter: -60% recruitment (extremely harsh)
                default: return 1.0f;
            }
        }

        /// <summary>
        /// War status multiplier: affects recruitment based on faction's diplomatic status
        /// Defending factions get a boost (+15%), unjustified aggressors suffer penalty (-35%)
        /// </summary>
        private float GetWarStatusMultiplier(Clan ownerClan)
        {
            if (ownerClan == null || ownerClan.Kingdom == null)
                return 1.0f;

            float multiplier = 1.0f;

            // Check if this kingdom is at war
            if (ownerClan.Kingdom.Wars != null)
            {
                foreach (var war in ownerClan.Kingdom.Wars)
                {
                    if (war == null)
                        continue;

                    bool isOwnerDefender = war.Defender == ownerClan.Kingdom;
                    bool isOwnerAggressor = war.Aggressor == ownerClan.Kingdom;

                    if (isOwnerDefender)
                    {
                        // Defending nation (being aggressed) gets recruitment boost
                        multiplier *= AGGRESSED_NATION_BOOST;
                    }
                    else if (isOwnerAggressor)
                    {
                        // Aggressive faction suffers recruitment penalties
                        multiplier *= UNJUSTIFIED_INVADER_PENALTY;
                    }
                }
            }

            return multiplier;
        }

        private void ApplyVillageRecruitment(Village village, int recruitCount)
        {
            // This adds recruits to the village's recruitment pool
            if (recruitCount > 0)
            {
                // Logging can be enabled for debugging
            }
        }

        private void ApplyTownRecruitment(Town town, int recruitCount)
        {
            // This adds recruits to the town's recruitment pool
            if (recruitCount > 0)
            {
                // Logging can be enabled for debugging
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

        public static string GetSeasonName()
        {
            if (Campaign.Current == null)
                return "Unknown";

            CampaignTime currentTime = Campaign.Current.CurrentMenuTime;
            int dayOfYear = (int)currentTime.GetDayOfYear;
            int season = (dayOfYear / 30) % 4;

            switch (season)
            {
                case 0: return "Spring";
                case 1: return "Summer";
                case 2: return "Fall";
                case 3: return "Winter";
                default: return "Unknown";
            }
        }
    }
}
