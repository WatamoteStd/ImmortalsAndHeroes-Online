using Godot;
using Shared.Udp.Packets.Category.Market;
using System;
using Shared.Items;

public partial class RoyalContract : PanelContainer
{
	
	[Export] private Label _itemName;
	[Export] private Label _itemCount;
	[Export] private Label _isTaxFree;
	[Export] private Label _costPerUnit;
	[Export] private Label _playerLimit;
	[Export] private Label _timeLeft;
	[Export] private Button _putItem;
	[Export] private HBoxContainer _taxContaienr;
	[Export] private HBoxContainer _playerLimitContrainer;

	public void Init(S2C_RoyalContractInfoResponsePacket data)
	{
		
		if (data.ItemId == ItemType.None)
		{
			NullContract();
			return;
		}

		_itemName.Visible = true;
		_costPerUnit.Visible = true;
		_playerLimit.Visible = true;
		_isTaxFree.Visible = true;
		_itemCount.Visible = true;
		_putItem.Disabled = false;

		_itemName.Text = data.ItemId.ToString();
		_itemCount.Text = data.Count.ToString();
		if (data.TaxPercent == 0)
		{
			_taxContaienr.Visible = false;
		}
		else _isTaxFree.Text = data.TaxPercent.ToString();

		_costPerUnit.Text = data.PricePerUnit.ToString();

		if (data.PerPlayerLimitCount == 0)
		{
			_playerLimitContrainer.Visible = false;
		}
		else _playerLimit.Text = data.PerPlayerLimitCount.ToString();



	}

	private void NullContract()
	{
		
		_itemName.Visible = false;
		_costPerUnit.Visible = false;
		_playerLimit.Visible = false;
		_isTaxFree.Visible = false;
		_itemCount.Visible = false;

		_putItem.Disabled = true;

	}

}
