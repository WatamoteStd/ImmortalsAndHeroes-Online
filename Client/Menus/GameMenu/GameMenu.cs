using Godot;
using GodotPlugins.Game;
using Shared.Characters;
using System;
using System.Collections.Generic;

public partial class GameMenu : Control
{

	public enum AllWindows { Main, Settings, CharacterCreate}
	public AllWindows CurrentOpenWindow = AllWindows.Main;
	private Dictionary<AllWindows, Control> _windowToPanel;
	
	[Export] private Button _openCreateMenu;
	[Export] private CharacterCreateWindow _createMenu;
	[Export] private CharacterWindow _characterInfoPanel;
	[Export] private Label _userId;
	[Export] private Label _username;
	[Export] private StatusWindow _statusWindow;
	[Export] private SettingButton _settingButton;
	[Export] private SettingWindow _settingWindow;
	[Export] private TextureButton _heroInfoButton;
	private bool _isHeroPanelOpen = false;
	
	// CHARACTER WINDOW

	public override void _Ready()
	{


		_windowToPanel = new Dictionary<AllWindows, Control>()
		{
			{AllWindows.Settings, _settingWindow},
			{AllWindows.CharacterCreate, _createMenu}
		};

		

		foreach (var win in _windowToPanel.Values)
		{
			win.Visible = false;
		}

		_userId.Text = GameSession.Instance.GlobalId.ToString();
		_username.Text = GameSession.Instance.Username;

		
		_openCreateMenu.Pressed += () =>
		{
			_createMenu.OpenWindow();
			CurrentOpenWindow = AllWindows.CharacterCreate;
		};

		_settingButton.Pressed += () =>
		{

			if (CurrentOpenWindow == AllWindows.Settings)
			{
				_settingButton.RotateOnClose();
				_ = _settingWindow.CloseWindow();
				CurrentOpenWindow = AllWindows.Main;
			}
			else
			{
				
				_settingWindow.OpenWindow();
				_settingButton.RotateOnOpen();
				CurrentOpenWindow = AllWindows.Settings;

			}
		};



		_heroInfoButton.Pressed += () =>
		{
			_isHeroPanelOpen = !_isHeroPanelOpen;
			_characterInfoPanel.Visible = _isHeroPanelOpen;
		};


		_settingWindow.OnWindowClose += () =>
		{
			CurrentOpenWindow = AllWindows.Main;
			_settingButton.RotateOnClose();
		};
		_createMenu.OnCreateCharacter += CreateCharacterRequestAsync;

	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("HideMenu"))
		{
			
			Visible = !Visible;

		}
		if (@event.IsActionPressed("MENU_CloseAll"))
		{
			foreach(var win in _windowToPanel.Values)
			{
				win.Visible = false;
			}
		}
	}



	#region HTTP / INTERNET 

	private async void CreateCharacterRequestAsync(string name, EntityType entityId)
	{
		
		var response = await HttpsMasterClient.Instanсe.CreateCharacterAsync(name, entityId);

		if (response.isSucces == true && response.character != null)
		{
			
			_statusWindow.ShowMessage("Success!", "Character created!");
			_characterInfoPanel.UpdateChracter(response.character.Nickname, response.character.Id.ToString(), response.character.Silver.ToString());
			

		}
		else 
		{
			_statusWindow.ShowMessage("Failure!", response.message);
		}

	}

	#endregion



}
