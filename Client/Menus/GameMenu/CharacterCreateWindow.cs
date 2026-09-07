using Godot;
using Shared.Characters;
using System;

public partial class CharacterCreateWindow : PanelContainer
{

	public event Action<string, EntityType> OnCreateCharacter;
	
	[Export] private Button _backButton;
	[Export] private Button _createCharacter;
	[Export] private LineEdit _nickname;
	[Export] private OptionButton _skinType;
	

	public override void _Ready()
	{

		_nickname.GrabFocus();
		
		_backButton.Pressed += () =>
		{
			_nickname.Text = "";
			ChangeVisiblity();
		};

		_createCharacter.Pressed += CreateCharacter;

	}


	public void ChangeVisiblity()
	{
		
		Visible = !Visible;

	}

	private void CreateCharacter()
	{
		
		_createCharacter.Disabled = true;
		OnCreateCharacter?.Invoke(_nickname.Text, (EntityType)_skinType.GetSelectedId());


	}

}
