using Godot;
using Shared.Udp.Packets.Category.Settlement;
using System;

public partial class TreasureHistoryElement : PanelContainer
{
	
	[Export] private Label _name;
	[Export] private Label _count;
	[Export] private Label _action;
	[Export] private Label _time;

	public void Initiate(S2C_TreasureHistoryUpdatePacket packet)
	{
		
		_name.Text = packet.Name;
		_count.Text = packet.Amount.ToString();
		_action.Text = packet.ActionType == C2S_TreasuryActionPacket.TreasuryActionType.Deposit 
		? "Depost"
		: "Withdraw";

	}

}
