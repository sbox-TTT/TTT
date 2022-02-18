namespace TTT.Events;

public static partial class TTTEvent
{
	public static class Player
	{
		/// <summary>
		/// Occurs when a player dies.
		/// <para>Event is passed the <strong><see cref="TTT.Player.TTTPlayer"/></strong> instance of the player who died.</para>
		/// </summary>
		public const string Died = "TTT.player.died";

		public static class Role
		{
			/// <summary>
			/// Occurs when a player selects their role.
			/// <para>Event is passed the <strong><see cref="TTT.Player.TTTPlayer"/></strong> instance of the player whose role was set.</para>
			/// </summary>
			public const string Select = "TTT.player.role.select";
		}
	}
}
