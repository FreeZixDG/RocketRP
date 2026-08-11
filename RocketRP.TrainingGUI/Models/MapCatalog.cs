namespace RocketRP.TrainingGUI.Models
{
	/// <summary>
	/// Map keys known to the game, taken from the dumps shipped in GameValues/BoostPickupsDumps.
	/// Only these are proposed so that a typo can never reach the .Tem file: an unknown map key
	/// makes the game crash when it loads the pack. Positions of the rounds are not adapted to
	/// another map, so switching stays a manual and deliberate action.
	/// </summary>
	public static class MapCatalog
	{
		public static readonly IReadOnlyList<string> KnownMaps =
		[
			"ARC_Darc_P",
			"ARC_P",
			"arc_standard_p",
			"bb_p",
			"Beach_Night_GRS_P",
			"beach_night_p",
			"beach_P",
			"BG_CS",
			"CHN_Stadium_Day_P",
			"CHN_Stadium_P",
			"cs_day_p",
			"cs_hw_p",
			"cs_p",
			"EuroStadium_Dusk_P",
			"EuroStadium_Night_P",
			"EuroStadium_P",
			"EuroStadium_Rainy_P",
			"eurostadium_snownight_p",
			"Farm_GRS_P",
			"Farm_HW_P",
			"Farm_Night_P",
			"farm_p",
			"FF_Dusk_P",
			"FNI_Stadium_P",
			"Haunted_TrainStation_P",
			"HoopsStadium_P",
			"hoopsStreet_p",
			"Labs_4v4_Arena15_Blackout_P",
			"Labs_4v4_Arena15_EuroStadium_Night_P",
			"Labs_4v4_Arena15_Retro_P",
			"Labs_Basin_P",
			"Labs_CirclePillars_P",
			"Labs_Corridor_P",
			"Labs_Cosmic_P",
			"Labs_Cosmic_V4_P",
			"Labs_DoubleGoal_P",
			"Labs_DoubleGoal_V2_P",
			"Labs_Galleon_Mast_P",
			"Labs_Galleon_P",
			"Labs_Holyfield_P",
			"Labs_Holyfield_Space_P",
			"Labs_Octagon_02_P",
			"Labs_Octagon_B2B_02_P",
			"Labs_Octagon_P",
			"Labs_PillarGlass_P",
			"Labs_PillarHeat_P",
			"Labs_PillarWings_P",
			"Labs_Underpass_P",
			"Labs_Underpass_v0_p",
			"Labs_Utopia_P",
			"mall_day_p",
			"music_p",
			"NeoTokyo_Arcade_P",
			"NeoTokyo_Hax_P",
			"NeoTokyo_P",
			"NeoTokyo_Standard_P",
			"NeoTokyo_Toon_p",
			"Outlaw_Oasis_P",
			"outlaw_p",
			"Paname_Dusk_P",
			"park_bman_p",
			"Park_Night_P",
			"Park_P",
			"Park_Rainy_P",
			"Park_Snowy_P",
			"STADIUM_10A_P",
			"stadium_day_p",
			"Stadium_Foggy_P",
			"Stadium_P",
			"Stadium_Race_Day_p",
			"Stadium_Winter_P",
			"street_p",
			"swoosh_p",
			"throwbackhockey_p",
			"throwbackstadium_P",
			"TrainStation_Dawn_P",
			"TrainStation_Night_P",
			"TrainStation_P",
			"UF_Day_P",
			"UF_Night_P",
			"Underwater_GRS_P",
			"Underwater_P",
			"UtopiaStadium_Dusk_P",
			"UtopiaStadium_Lux_P",
			"UtopiaStadium_P",
			"UtopiaStadium_Snow_P",
			"Wasteland_GRS_P",
			"Wasteland_Night_P",
			"wasteland_Night_S_P",
			"Wasteland_P",
			"wasteland_s_p",
			"Woods_Night_P",
			"woods_p",
		];

		/// <summary>The known maps, plus <paramref name="currentMap"/> when the pack uses a key we don't list.</summary>
		public static IReadOnlyList<string> WithCurrentMap(string? currentMap)
		{
			if (string.IsNullOrEmpty(currentMap) || KnownMaps.Contains(currentMap, StringComparer.OrdinalIgnoreCase)) return KnownMaps;

			return KnownMaps.Append(currentMap).OrderBy(map => map, StringComparer.OrdinalIgnoreCase).ToList();
		}
	}
}
