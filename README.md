# Slow Recruitment Mod - Bannerlord

A Mount & Blade II: Bannerlord mod that slows settlement recruitment rates based on prosperity levels. Make battles extremely more straining on factions and the player!

## Features

- ✅ **Fixed Weekly Recruitment Rates**:
  - Villages: 1-2 recruits per week
  - Towns: 3 recruits per week
  - Castles: 2 recruits per week

- ✅ **Prosperity-Based Scaling**: Recruitment rates scale with settlement prosperity
  - Struggling settlements (< 1000 prosperity): 30% of target
  - Poor settlements (1000-2000): 50% of target
  - Average settlements (2000-5000): 75% of target
  - Good settlements (5000-10000): 90% of target
  - Excellent settlements (> 10000): 100% of target

- ✅ **War Sails DLC Compatible**: Full support for War Sails expansion
- ✅ **Siege Exemption**: Settlements under siege are not affected
- ✅ **Applies to All Factions**: Affects both player and NPC recruitment

## Installation

### Requirements
- Mount & Blade II: Bannerlord (v1.3.4 or higher)
- War Sails DLC (optional, but recommended for full compatibility)
- .NET Framework 4.7.2 or higher

### Step 1: Download
Download the latest release from the [Releases](https://github.com/Ntonozz/BannerlordSlowRecruitmentMod/releases) page.

### Step 2: Extract
Extract the mod folder to your Bannerlord Modules directory:
```
C:\Users\[YourUsername]\Documents\Mount and Blade II Bannerlord\Modules\
```

The structure should look like:
```
Modules/
├── SlowRecruitmentMod/
│   ├── SubModule.xml
│   ├── ModConfig.xml
│   ├── bin/
│   │   └── Win64_Shipping_Client/
│   │       └── SlowRecruitmentMod.dll
│   └── ...
```

### Step 3: Enable the Mod
1. Launch Bannerlord
2. Click "Mods" at the main menu
3. Find "Slow Recruitment Mod - Prosperity Edition" in the list
4. Check the box to enable it
5. Make sure "War Sails" is also enabled if you have the DLC
6. Click "Play"

## Configuration

You can customize the mod by editing `ModConfig.xml`:

```xml
<VillageRecruitsPerWeek value="2"/>    <!-- Change recruitment rates -->
<TownRecruitsPerWeek value="3"/>
<CastleRecruitsPerWeek value="2"/>

<!-- Adjust prosperity multipliers (0.0 to 1.0) -->
<VeryLowProsperity value="0.3"/>
<LowProsperity value="0.5"/>
<MediumProsperity value="0.75"/>
<HighProsperity value="0.9"/>
<VeryHighProsperity value="1.0"/>
```

## Gameplay Impact

### Strategic Changes
- Armies need to be managed more carefully
- Battles have permanent consequences (harder to replace losses)
- Settlement prosperity becomes critical for military power
- Factions must balance military campaigns with economic development
- Small skirmishes are more costly and impactful

### Example Scenarios

**Scenario 1: Average Prosperity Town (3000)**
- Base: 3 recruits/week
- Actual: 2.25 recruits/week (75% multiplier)
- Result: Takes ~3 days per recruit

**Scenario 2: Struggling Village (800 prosperity)**
- Base: 2 recruits/week
- Actual: 0.6 recruits/week (30% multiplier)
- Result: Takes ~12 days per recruit

## Compatibility

- ✅ War Sails DLC
- ✅ Vanilla Bannerlord content
- ✅ Most other mods (use with discretion)
- ❌ Mods that heavily modify recruitment systems (may conflict)

## Building from Source

### Requirements
- Visual Studio 2019 or later
- .NET Framework 4.7.2
- Bannerlord Modding Kit

### Build Steps
1. Clone the repository
2. Open `SlowRecruitmentMod.sln`
3. Add references to Bannerlord DLLs from your Bannerlord installation
4. Build the solution (Release configuration)
5. Copy `SlowRecruitmentMod.dll` to `bin/Win64_Shipping_Client/`

## Troubleshooting

### Mod Not Loading
- Verify the mod folder is in the correct Modules directory
- Check that SubModule.xml is properly formatted
- Ensure War Sails is enabled if you have the DLC
- Check the launcher log for errors

### Recruitment Still Too Fast
- Verify the mod is checked in the launcher
- Try lowering the `VillageRecruitsPerWeek` and `TownRecruitsPerWeek` values
- Ensure no other recruitment mods are enabled

### Game Crashes
- Update to the latest Bannerlord patch
- Verify the DLL was compiled for the correct .NET version
- Try disabling other mods to identify conflicts

## Support

For issues, suggestions, or contributions, please open an [Issue](https://github.com/Ntonozz/BannerlordSlowRecruitmentMod/issues) or [Pull Request](https://github.com/Ntonozz/BannerlordSlowRecruitmentMod/pulls).

## License

This mod is provided as-is for personal use. Feel free to modify and distribute with credit.

## Credits

Created with the Bannerlord Modding Kit by TaleWorlds Entertainment.
