using Sandbox;

namespace TTT;

public partial class WalkController
{
#if DEBUG
	[Net] public bool NoclipEnabled { get; set; } = false;

	private void NoclipMove()
	{
		var fwd = Player.InputDirection.x.Clamp( -1f, 1f );
		var left = Player.InputDirection.y.Clamp( -1f, 1f );
		var rotation = Player.ViewAngles.ToRotation();

		var vel = (rotation.Forward * fwd) + (rotation.Left * left);

		if ( Input.Down( InputButton.Jump ) )
			vel += Vector3.Up * 1;

		vel = vel.Normal * 2000;

		if ( Input.Down( InputButton.Run ) )
			vel *= 5.0f;

		if ( Input.Down( InputButton.Duck ) )
			vel *= 0.2f;

		Player.Velocity += vel * Time.Delta;

		if ( Player.Velocity.LengthSquared > 0.01f )
			Player.Position += Player.Velocity * Time.Delta;

		Player.Velocity = Player.Velocity.Approach( 0, Player.Velocity.Length * Time.Delta * 5.0f );

		Player.EyeRotation = rotation;
		WishVelocity = Player.Velocity;
		Player.GroundEntity = null;
		Player.BaseVelocity = Vector3.Zero;

		SetTag( "noclip" );
	}
#endif
}
