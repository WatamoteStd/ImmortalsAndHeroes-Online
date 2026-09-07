using Godot;
using System;

public partial class CharacterWindow : PanelContainer
{

	[Export] private Label _nickname;
	[Export] private Label _id;
	[Export] private Label _silver;
	[Export] private MarginContainer _characterCard;
	[Export] private StatusWindow _statusWindow;
	[Export] private Button _characterCreateButton;
	[Export] private Button _enterWorldButton;

	public override void _Ready()
	{

		_enterWorldButton.Pressed += EnterWorld;
		
		_characterCard.Visible = false;
		_enterWorldButton.Visible = false;
		ServerCharacterRequest();
		
	}

	public void UpdateChracter(string nick, string id, string silver)
	{

		_nickname.Text = nick;
		_id.Text = id;
		_silver.Text = silver;
		_characterCard.Visible = true;
		_enterWorldButton.Visible = true;
		_characterCreateButton.Visible = false;

	}

	private async void EnterWorld()
	{

		var response = await HttpsMasterClient.Instanсe.EnterWorldAsync();

		if (response.isSucces) // CHANGE SCENE AND LOAD WORLD
		{

			_statusWindow.ShowMessage("Success!", response.message);
			_ = SceneManager.Instance.LoadRegion(GameSession.Instance.PlayerCache.RegionId);
			return;

		}
		else _statusWindow.ShowMessage("Fail!", response.message);


	}

	private async void ServerCharacterRequest()
	{

		var response = await HttpsMasterClient.Instanсe.GetCharacter();

		if (response.Item2 == null)
		{
			_statusWindow.ShowMessage("Server Information", response.message);
		}
		else
		{
			UpdateChracter(response.character.Nickname, response.character.Id.ToString(), response.character.Silver.ToString());
			_statusWindow.ShowMessage("Server Information", response.message);
			
		}

	}

}
