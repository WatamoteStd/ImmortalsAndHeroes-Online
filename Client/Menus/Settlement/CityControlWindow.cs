using Godot;
using System;
using System.Collections.Generic;

public partial class CityControlWindow : Control
{

	public enum WindowTypes
	{
		None = 0,
		Tax = 1,
		Contract = 2
	}
	public WindowTypes CurrentWindow = WindowTypes.None;
	private Dictionary<WindowTypes, Control> windowToPanel = new Dictionary<WindowTypes, Control>();

	[Export] private Button _taxButton;
	[Export] private Control _taxWindow;
	[Export] private Button _contractButton;
	[Export] private Control _contractWindow;

	public override void _Ready()
	{
		
		windowToPanel[WindowTypes.Tax] = _taxWindow;
		windowToPanel[WindowTypes.Contract] = _contractWindow;

		_taxButton.Pressed += () => {OpenWindow(WindowTypes.Tax);};
		_contractButton.Pressed += () => {OpenWindow(WindowTypes.Contract);};

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


}
