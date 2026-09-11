using Godot;
using Shared.Udp.Packets.Category.Settlement;
using System;

public partial class TreasuryWindow : Control
{
	
	[Export] private Label _currentBalance;
	[Export] private Button _withdrawButton;
	[Export] private Button _depositButton;
	[Export] private LineEdit _amountEdit;
	[Export] private Label _serverAnswerLabel;

	// HISTORY =======================================
	[Export] private PackedScene _historyElementScene;
	[Export] private VBoxContainer _historyBox;
	public override void _Ready()
	{

		_serverAnswerLabel.Visible = false;
		
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
		_serverAnswerLabel.Visible = false;
		
		//_withdrawButton.Disabled = true;
		//_depositButton.Disabled = true;


	}

	public void ServerAnswerReceived(S2C_TreasureUpdatePacket packet)
	{
		
		_withdrawButton.Disabled = false;
		_depositButton.Disabled = false;

		_currentBalance.Text = packet.Amount.ToString();

		if (packet.Success)
		{
			_serverAnswerLabel.Text = "Success!";
			_serverAnswerLabel.SelfModulate = Colors.ForestGreen;
		}
		else
		{
			_serverAnswerLabel.Text = "Failed!";
			_serverAnswerLabel.SelfModulate = Colors.PaleVioletRed;
		}
		_serverAnswerLabel.Visible = true;
		

	}

	public void CreateHistoryElement(S2C_TreasureHistoryUpdatePacket packet)
	{
		
		var element = _historyElementScene.Instantiate<TreasureHistoryElement>();
		_historyBox.AddChild(element);
		_historyBox.MoveChild(element, 0);
		element.Initiate(packet);

	}


}
