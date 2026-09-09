using Godot;
using Shared.Udp.Packets.Category.Settlement;
using System;

public partial class TreasuryWindow : Control
{
	
	[Export] private Label _currentBalance;
	[Export] private Button _withdrawButton;
	[Export] private Button _depositButton;
	[Export] private LineEdit _amountEdit;

	public override void _Ready()
	{
		
		_withdrawButton.Pressed += () =>
		{
			ServerRequest(C2S_TreasuryActionPacket.TreasuryActionType.Withdraw);
		};
		_depositButton.Pressed += () =>
		{
			ServerRequest(C2S_TreasuryActionPacket.TreasuryActionType.Deposit);
		};

	}

	private void ServerRequest(C2S_TreasuryActionPacket.TreasuryActionType type)
	{
		
		if (string.IsNullOrEmpty(_amountEdit.Text)) return;
		if (!int.TryParse(_amountEdit.Text, out var amount) || amount == 0)
		{
			_amountEdit.Text = "0";
			return;
		}

		var packet = new C2S_TreasuryActionPacket
		{
			ActionType = type,
			Amount = (ulong)amount
		};
		ServerMaster.Instance.Settlement_TreasuryRequest(packet);
		_amountEdit.Text = "0";


	}


}
