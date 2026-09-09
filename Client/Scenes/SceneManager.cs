using Godot;
using Shared.Items;
using Shared.MasteryTree;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class SceneManager : CanvasLayer
{

	[Export] private AnimationPlayer _animator;
	[Export] public Hud PlayerHud;
	[Export] private Inventory _inventory;
	public static SceneManager Instance { get; private set; }

	[Export] private PanelContainer _connectionLostWindow;
	[Export] private Button _connectionLostButton;

	// MARKET
	[Export] public MarketWindow MarketManagerWindow {get; private set;}
	[Export] public MasteryTree MasteryTreeWindow {get; private set;}
	[Export] public DebugConsole ConsoleWindow {get; private set;}
	[Export] public CityControlWindow SettlementManageWindow {get; private set;}

	public Dictionary<uint, string> regIdToScenePath;

	public override void _Ready()
	{

		if (Instance != null)
		{
			QueueFree();
			return;
		}
		else Instance = this;

		Layer = 200;
		
		Visible = false;
		MarketManagerWindow.Visible = false;
		SettlementManageWindow.Visible = false;

		regIdToScenePath = new Dictionary<uint, string>
		{
			
			{0, "res://World/Regions/Region_0.tscn"},
			{1, "res://World/Regions/Region_1_City.tscn"},
			{2, "res://World/Regions/Region_2.tscn"},
			{3, "res://World/Regions/Region_3.tscn"}

		};

		_connectionLostButton.Pressed += () =>
		{
			BackToMenuConnectionLost();
		};
		_connectionLostWindow.Visible = false;

		PlayerController.OnInventoryAction -= InventoryAction;
		PlayerController.OnInventoryAction += InventoryAction;

	}

	public async Task AuthToMainMenu() // only for auth menu -> main menu (i don't know why i did this, god bless me)
	{
		
		Visible = true;
		_animator.Play("Idle");
		GetTree().ChangeSceneToFile("res://Menus/GameMenu/GameMenu.tscn");
		var timer = GetTree().CreateTimer(6.5f);
		await ToSignal(timer, SceneTreeTimer.SignalName.Timeout);

		Visible = false;


	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("Back"))
		{
			
			SettlementManageWindow.Visible = false;
			MasteryTreeWindow.Visible = false;
			MarketManagerWindow.Visible = false;

		}
	}

   public async Task LoadRegion(uint regionId)
	{
		HideHud();

		if (!regIdToScenePath.TryGetValue(regionId, out string path))
		{
			GD.PrintErr($"[SCENE MANAGER] Unknown RegionId: {regionId}");
			return;
		}
		
		GameSession.Instance.CurrentSessionState = GameSession.State.Loading;
		Visible = true;
		_animator.Play("Idle");
		GetTree().ChangeSceneToFile(path);
		var timer = GetTree().CreateTimer(6.5f);
		await ToSignal(timer, SceneTreeTimer.SignalName.Timeout);

		GameSession.Instance.CurrentSessionState = GameSession.State.InGame;
		Visible = false;
		ShowHud();

	}

	public void ConnectionLostScren()
	{
		HideHud();
		Visible = true;
		_inventory.Visible = false;
		_connectionLostWindow.Visible = true;
	}
	private void BackToMenuConnectionLost()
	{
		GetTree().ChangeSceneToFile("res://Menus/MainMenu/LoginMenu.tscn");
		_connectionLostWindow.Visible = false;
		Visible = false;
	}
	
	private void ShowHud()
	{
		
		PlayerHud.Visible = true;
		PlayerHud.ProcessMode = ProcessModeEnum.Always;

	}
	private void HideHud()
	{
		PlayerHud.Visible = false;
		PlayerHud.ProcessMode = ProcessModeEnum.Disabled;
	}


	public void ShowSelectedEntityWindow(Entity entity)
	{
		PlayerHud.ShowSelectedEntity(entity);
	}
	public void HideSelectedEntityWindow()
	{
		PlayerHud.HideSelectedEntity();
	}



	public void InitPlayerHud(uint hp, uint mp, uint silver, string name)
	{
		
		PlayerHud.InitHud(hp,mp,silver,name);

	}

	private void InventoryAction()
	{
		_inventory.Visible = !_inventory.Visible;
	}
	public void UpdateInventoryCell(ushort slotIndex, ItemType item, ushort count)
	{
		_inventory.UpdateCell(slotIndex,item, count);
	}

	public void SwitchVisiblityCityMarket()
	{
		MarketManagerWindow.Visible = !MarketManagerWindow.Visible;
	}
	public void SwitchVisiblityMasteryTree()
	{
		MasteryTreeWindow.Visible = !MasteryTreeWindow.Visible;
	}

	public void SwitchVisiblitySettlementManage()
	{
		SettlementManageWindow.Visible = !SettlementManageWindow.Visible;
	}
}
