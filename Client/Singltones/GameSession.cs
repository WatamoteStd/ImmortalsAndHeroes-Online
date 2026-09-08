using Godot;
using Shared.Udp.Packets.Category;
using Shared.Udp.Packets.Category.Game;
using System;

public partial class GameSession : Node
{

	public static GameSession Instance {get; private set;}
	public event Action OnPlayerDataUpdated;

	public string Username {get; set;}
	public uint NetworkId {get; set;}
	public long GlobalId { get; set;}
	
	public string MasterToken { get; set; }
	public string UdpToken { get; set; }
	public string UdpIp { get; set; }
	public int UdpPort { get; set; }

	public uint PlayerExpCache {get; set;}
	public S2C_HandshakeSuccessPacket PlayerCache;
	public S2C_StatsSyncPacket StatsCache;


	public void UpdateSilver(long newSilver)
    {
        PlayerCache.Silver = (uint)newSilver; 
        OnPlayerDataUpdated?.Invoke();  
    }
	public void UpdateExp(float newExp)
	{
		PlayerCache.Exp = newExp;
		OnPlayerDataUpdated?.Invoke();
	}

	public enum State
	{
		Authorizing,
		Menu,
		Loading,
		InGame,
		Disconnected,
		Afk
	}
	public State CurrentSessionState = State.Authorizing;

	public override void _EnterTree()
	{
		
		if (Instance != null)
		{
			
			QueueFree();
			return;

		}
		else
		{
			Instance = this;
		}

	}



}
