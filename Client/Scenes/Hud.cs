using Godot;
using System;

public partial class Hud : CanvasLayer
{
	
	[Export] private ProgressBar _healthBar;
	[Export] private Label _healthBarLabel;
	[Export] private ProgressBar _manaBar;
	[Export] private Label _manaBarLabel;
	[Export] private Label _silver;
	[Export] private Label _nickname;
	[Export] private SelectedEntityWindow _selectEntityWindow;

	private uint _maxHealth;
	private uint _maxMana;

    public override void _Ready()
    {
        GameSession.Instance.OnPlayerDataUpdated += UpdateSilver;
    }


	public void InitHud(float hp, float mp, uint silver, string name)
	{
		
		_healthBar.MaxValue = hp;
		_healthBar.Value = hp;
		_manaBar.MaxValue = mp;
		_manaBar.Value = mp;

		_maxHealth = (uint)hp;
		_maxMana = (uint)mp;

		_healthBarLabel.Text = hp.ToString() + " / " + hp.ToString();
		_manaBarLabel.Text = mp.ToString() + " / " + mp.ToString();

		_silver.Text = silver.ToString();
		_nickname.Text = name;


	}
	public void UpdateHealth(uint actualHealth)
	{
		
		_healthBar.Value = actualHealth;
		_healthBarLabel.Text = actualHealth.ToString() + " / " + _maxHealth.ToString();

	}

	public void UpdateMana(uint actualMana)
	{
		
		_manaBar.Value = actualMana;
		_manaBarLabel.Text = actualMana.ToString() + " / " + _maxMana.ToString();

	}
	public void ReplaceMana(float mana, float maxMana)
	{
		_maxMana = (uint)maxMana;
		_manaBar.MaxValue = maxMana;
		_manaBar.Value = mana;
		_manaBarLabel.Text = mana.ToString() + " / " + maxMana.ToString();
	}

	public void ReplaceHealth(float health, float maxHealth)
	{
		_maxHealth = (uint)maxHealth;
		_healthBar.MaxValue = maxHealth;
		_healthBar.Value = health;
		_healthBarLabel.Text = health.ToString() + " / " + maxHealth.ToString();
	}

	public void UpdateSilver()
	{
		_silver.Text = GameSession.Instance.PlayerCache.Silver.ToString();
	}

	public void ShowSelectedEntity(Entity entity)
	{
		_selectEntityWindow.ShowWindow(entity);
	}
	public void HideSelectedEntity()
	{
		_selectEntityWindow.HideWindow();
	}



}
