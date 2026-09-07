using Godot;
using Shared.Characters;
using System;

public partial class CharacterCreateWindow : PanelContainer
{

	public event Action<string, EntityType> OnCreateCharacter;
	public event Action OnWindowClose;
	
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
			CloseWindow();
		};

		_createCharacter.Pressed += CreateCharacter;

	}


	public void OpenWindow()
	{
		
		Visible = true;
		var tween = CreateTween()
			.TweenProperty(this, "modulate:a", 0.0f, 0.75f);

	}
	public void CloseWindow()
	{
		
		var tween = CreateTween()
			.TweenProperty(this, "modulate:a", 0.0f, 1f);

		tween.Finished += () => {Visible = false; OnWindowClose?.Invoke();};


	}

	private void CreateCharacter()
	{
		
		_createCharacter.Disabled = true;
		OnCreateCharacter?.Invoke(_nickname.Text, (EntityType)_skinType.GetSelectedId());


	}

}
