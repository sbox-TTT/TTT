using Sandbox;

namespace TTT;

public partial class Player : Sandbox.Player
{
	[BindComponent]
	public Perks Perks { get; }

	[Net, Local]
	public int Credits { get; set; } = 0;

	[Net]
	public bool IsForcedSpectator { get; set; } = false;

	public new Inventory Inventory
	{
		get => base.Inventory as Inventory;
		private init => base.Inventory = value;
	}

	public new DefaultWalkController Controller
	{
		get => base.Controller as DefaultWalkController;
		private set => base.Controller = value;
	}

	private static int CarriableDropVelocity { get; set; } = 300;
	private DamageInfo _lastDamageInfo;

	public Player()
	{
		Inventory = new Inventory( this );
	}

	public override void Spawn()
	{
		base.Spawn();

		Role = new NoneRole();
		Components.GetOrCreate<Perks>();

		bool isPostRound = Game.Current.Round is PostRound;
		if ( isPostRound || Game.Current.Round is InProgressRound )
		{
			MakeSpectator( false );
		}

		Respawn();

		using ( Prediction.Off() )
		{
			foreach ( Player player in Utils.GetPlayers() )
			{
				if ( isPostRound || player.IsConfirmed )
				{
					player.SendClientRole( To.Single( this ) );
				}
			}
		}
	}

	// Let's clean this up at some point, it's poorly written.
	public override void Respawn()
	{
		base.Respawn();

		SetModel( "models/citizen/citizen.vmdl" );

		Animator = new StandardPlayerAnimator();

		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		EnableDrawing = true;

		Credits = 0;

		SetRole( new NoneRole() );

		IsMissingInAction = false;

		using ( Prediction.Off() )
		{
			RPCs.ClientOnPlayerSpawned( this );
			SendClientRole();
		}

		if ( !IsForcedSpectator )
		{
			Controller = new DefaultWalkController();
			Camera = new FirstPersonCamera();
			EnableAllCollisions = true;
		}
		else
		{
			MakeSpectator( false );
		}

		DeleteItems();
		Game.Current.Round.OnPlayerSpawn( this );
	}

	// Let's clean this up at some point, it's poorly written.
	public override void OnKilled()
	{
		base.OnKilled();

		BecomePlayerCorpseOnServer( _lastDamageInfo.Force, GetHitboxBone( _lastDamageInfo.HitboxIndex ) );

		Inventory.DropAll();
		DeleteItems();

		IsMissingInAction = true;

		using ( Prediction.Off() )
		{
			RPCs.ClientOnPlayerDied( this );
			Role?.OnKilled( _lastDamageInfo.Attacker as Player );

			if ( Game.Current.Round is InProgressRound )
			{
				SyncMIA();
			}
			else if ( Game.Current.Round is PostRound && PlayerCorpse != null && !PlayerCorpse.IsIdentified )
			{
				PlayerCorpse.IsIdentified = true;

				RPCs.ClientConfirmPlayer( null, PlayerCorpse, this, PlayerCorpse.DeadPlayerClientData.Name, PlayerCorpse.DeadPlayerClientData.PlayerId, Role.ClassInfo.Name, PlayerCorpse.GetConfirmationData(), PlayerCorpse.KillerWeapon.LibraryName, PlayerCorpse.Perks );
			}
		}
	}

	public override void Simulate( Client client )
	{
		if ( IsClient )
		{
			TickPlayerVoiceChat();
		}
		else
		{
			TickAFKSystem();
		}

		TickEntityHints();

		if ( LifeState != LifeState.Alive )
		{
			TickPlayerChangeSpectateCamera();

			return;
		}

		// Input requested a carriable entity switch
		if ( Input.ActiveChild != null )
		{
			ActiveChild = Input.ActiveChild;
		}

		SimulateActiveChild( client, ActiveChild );

		TickPerkSimulate();
		TickPlayerUse();
		TickPlayerDropCarriable();
		// TickPlayerShop();
		TickLogicButtonActivate();

		PawnController controller = GetActiveController();
		controller?.Simulate( client, this, GetActiveAnimator() );
	}

	public override void StartTouch( Entity other )
	{
		if ( IsClient )
			return;

		if ( other is PickupTrigger )
			StartTouch( other.Parent );
	}

	public void DeleteItems()
	{
		Perks.Clear();
		ClearAmmo();
		Inventory?.DeleteContents();
		RemoveClothing();
	}

	private void TickPlayerDropCarriable()
	{
		if ( Input.Pressed( InputButton.Drop ) && !Input.Down( InputButton.Run ) && ActiveChild != null && Inventory != null )
		{
			Entity droppedEntity = Inventory.DropActive();

			if ( droppedEntity != null )
			{
				if ( droppedEntity.PhysicsGroup != null )
				{
					droppedEntity.PhysicsGroup.Velocity = Velocity + (EyeRotation.Forward + EyeRotation.Up) * CarriableDropVelocity;
				}
			}
		}
	}

	private void TickPlayerChangeSpectateCamera()
	{
		if ( !Input.Pressed( InputButton.Jump ) || !IsServer )
		{
			return;
		}

		using ( Prediction.Off() )
		{
			Camera = Camera switch
			{
				RagdollSpectateCamera => new FreeSpectateCamera(),
				FreeSpectateCamera => new ThirdPersonSpectateCamera(),
				ThirdPersonSpectateCamera => new FirstPersonSpectatorCamera(),
				FirstPersonSpectatorCamera => new FreeSpectateCamera(),
				_ => Camera
			};
		}
	}

	private void TickPerkSimulate()
	{
		for ( int i = 0; i < Perks.Count; ++i )
		{
			Perks.Get( i ).Simulate( this );
		}
	}

	protected override void OnDestroy()
	{
		RemovePlayerCorpse();

		base.OnDestroy();
	}
}
