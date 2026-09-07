using Godot;
using Shared.Characters;
using System;

public partial class GameMenu : Control
{
	
	[Export] private Button _openCreateMenu;
	[Export] private CharacterCreateWindow _createMenu;
	[Export] private CharacterWindow _characterInfoPanel;
	[Export] private Label _userId;
	[Export] private Label _username;
	[Export] private StatusWindow _statusWindow;
	[Export] private TextureButton _settingButton;
	[Export] private Control _settingWindow;
	
	// CHARACTER WINDOW

	public override void _Ready()
	{
		
		_openCreateMenu.Pressed += () =>
		{
			_createMenu.ChangeVisiblity();
		};

		_userId.Text = GameSession.Instance.GlobalId.ToString();
		_username.Text = GameSession.Instance.Username;

		_settingButton.Pressed += () =>
		{
			_settingWindow.Visible = true;
		};


		_createMenu.OnCreateCharacter += CreateCharacterRequestAsync;

	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("HideMenu"))
		{
			
			Visible = !Visible;

		}
	}

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



}
