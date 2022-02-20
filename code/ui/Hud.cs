using System.Collections.Generic;

using Sandbox;
using Sandbox.UI;

namespace TTT.UI;

public partial class Hud : HudEntity<RootPanel>
{
	public Hud()
	{
		if ( Host.IsServer )
		{
			return;
		}

		RootPanel.StyleSheet.Load( "/ui/Hud.scss" );
		RootPanel.AddClass( "panel" );

		RootPanel.AddChild<GeneralHud>();
	}

	[Event.Hotload]
	public static void OnReload()
	{
		if ( Host.IsClient )
			return;

		Game.Current.Hud?.Delete();
		Game.Current.Hud = new Hud();
	}

	public class GeneralHud : Panel
	{
		public static GeneralHud Instance;
		private List<Panel> _aliveHud = new();
		public bool AliveHudEnabled
		{
			get => _enabled;
			internal set
			{
				_enabled = value;

				if ( value )
				{
					CreateAliveHud();
				}
				else
				{
					DeleteAliveHud();
				}
			}
		}
		private bool _enabled = false;

		public GeneralHud()
		{
			Instance = this;

			AddClass( "fullscreen" );
			AddChild<WIPDisclaimer>();

			AddChild<HintDisplay>();
			AddChild<PlayerRoleDisplay>();
			AddChild<PlayerInfoDisplay>();
			AddChild<InventoryWrapper>();
			AddChild<ChatBox>();

			AddChild<VoiceChatDisplay>();
			AddChild<GameTimerDisplay>();

			AddChild<VoiceList>();

			AddChild<InfoFeed>();
			AddChild<FullScreenHintMenu>();
			AddChild<PostRoundMenu>();
			AddChild<Scoreboard>();
			AddChild<SettingsMenu>();
		}

		public void AddChildToAliveHud( Panel panel )
		{
			AddChild( panel );
			_aliveHud.Add( panel );
		}

		private void CreateAliveHud()
		{
			if ( _aliveHud.Count != 0 ) return;
			_aliveHud = new()
			{
				AddChild<Crosshair>(),
				AddChild<BreathIndicator>(),
				AddChild<QuickShop>(),
				AddChild<DamageIndicator>()
			};
		}

		private void DeleteAliveHud()
		{
			if ( _aliveHud.Count == 0 ) return;
			_aliveHud.ForEach( ( panel ) => panel.Delete( true ) );
			_aliveHud.Clear();
		}

		public override void Tick()
		{
			if ( Local.Pawn is not Player player )
			{
				return;
			}

			if ( Instance != null )
			{
				Instance.AliveHudEnabled = player.LifeState == LifeState.Alive && !player.IsForcedSpectator;
			}
		}

		// Use "GeneralHud" as the Panel that displays any s&box popups.
		public override Panel FindPopupPanel()
		{
			return this;
		}
	}
}
