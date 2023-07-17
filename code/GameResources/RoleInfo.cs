using Sandbox;

namespace TTT;

[GameResource( "Role", "role", "TTT role template.", Icon = "🎭" )]
public class RoleInfo : GameResource
{
	public Team Team { get; set; }

	[Category( "UI" )]
	public Color Color { get; set; }

	[Title( "Icon" ), Category( "UI" ), ResourceType( "png" )]
	public string IconPath { get; set; } = "ui/none.png";

	[Category("Shop")]
	[Description( "Whether or not this role is allowed to use the team shop." )]
	public bool CanUseShop { get; set; }

	[Category( "Shop" )]
	[Description( "The amount of credits the player spawns with." )]
	[ShowIf("CanUseShop", true)]
	public int DefaultCredits { get; set; }

	[Description( "This includes sending messages and voice chat." )]
	public bool CanTeamChat { get; set; }

	public bool CanAttachCorpses { get; set; }

	[Description( "The minimum amount of karma a player has to have to be assigned this role." )]
	public int RequiredKarma { get; set; }

	public KarmaConfig Karma { get; set; }

	public ScoringConfig Scoring { get; set; }

	public struct KarmaConfig
	{
		[Description( "This gets multiplied with the max value for karma to calculate the kill reward." )]
		[Property]
		public float AttackerKillRewardMultiplier { get; set; } = 0;

		[Description( "This gets multiplied with the victim's active karma to determine the kill penalty." )]
		[Property]
		public float TeamKillPenaltyMultiplier { get; set; } = 0;

		[Description( "This gets multiplied with the damage dealt to a player with this role to determine the hurt reward." )]
		[Property]
		public float AttackerHurtRewardMultiplier { get; set; } = 0;

		[Description( "This gets multiplied with the damage dealt to a teammate to determine the hurt penalty." )]
		[Property]
		public float TeamHurtPenaltyMultiplier { get; set; } = 0;

		public KarmaConfig() { }
	}

	public struct ScoringConfig
	{
		[Description( "The amount of score points rewarded for confirming a corpse." )]
		[Property]
		public int CorpseFoundReward { get; set; } = 0;

		[Description( "The amount of score points rewarded for killing a player with this role." )]
		[Property]
		public int AttackerKillReward { get; set; } = 0;

		[Description( "The amount of score points penalized for killing a player on the same team." )]
		[Property]
		public int TeamKillPenalty { get; set; } = 0;

		[Description( "The amount of score points rewarded for surviving the round." )]
		[Property]
		public int SurviveBonus { get; set; } = 0;

		[Description( "The amount of score points penalized for commiting suicide." )]
		[Property]
		public int SuicidePenalty { get; set; } = 0;

		public ScoringConfig() { }
	}
}
