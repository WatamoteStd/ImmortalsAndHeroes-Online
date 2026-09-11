using Godot;
using Shared.Udp.Packets.Category.Settlement;
using System;
using System.Collections.Generic;

public partial class CityControlWindow : Control
{

	public enum WindowTypes
	{
		None = 0,
		Tax = 1,
		Contract = 2,
		Treasury = 3
	}
	public WindowTypes CurrentWindow = WindowTypes.None;
	private Dictionary<WindowTypes, Control> windowToPanel = new Dictionary<WindowTypes, Control>();

	[Export] private Button _taxButton;
	[Export] private Control _taxWindow;
	[Export] private Button _contractButton;
	[Export] private ContractsWindow _contractWindow;
	[Export] private Button _treasuryButton;
	[Export] private TreasuryWindow _treasuryWindow;

	public override void _Ready()
	{
		
		windowToPanel[WindowTypes.Tax] = _taxWindow;
		windowToPanel[WindowTypes.Contract] = _contractWindow;
		windowToPanel[WindowTypes.Treasury] = _treasuryWindow;

		_taxButton.Pressed += () => {OpenWindow(WindowTypes.Tax);};
		_contractButton.Pressed += () => {OpenWindow(WindowTypes.Contract);};
		_treasuryButton.Pressed += () => {OpenWindow(WindowTypes.Treasury);};


		foreach (var window in windowToPanel.Values)
		{
			window.Visible = false;
		}
		

	}

	private void OpenWindow(WindowTypes window)
	{

		if (window == CurrentWindow)
		{
			windowToPanel[CurrentWindow].Visible = false;
			CurrentWindow = WindowTypes.None;
			return;
		}
		
		if (CurrentWindow != WindowTypes.None)
		{
			windowToPanel[CurrentWindow].Visible = false;
		}

		CurrentWindow = window;
		windowToPanel[window].Visible = true;

	}

	public void Treasure_ServerAnswer(S2C_TreasureUpdatePacket packet)
	{
		_treasuryWindow.ServerAnswerReceived(packet);
	}
	public void Treasure_HistoryUpdate(S2C_TreasureHistoryUpdatePacket packet)
	{
		_treasuryWindow.CreateHistoryElement(packet);
	}
 

}
