using Godot;
using Shared.DataTransferObjects.Market;
using Shared.Items;
using Shared.Udp.Packets.Category.Market;
using System;
using System.Collections.Generic;

public partial class MarketWindow : Control
{
	
	[Export] private VBoxContainer _itemList;
	[Export] private PackedScene _item;
	[Export] private RoyalContract _royalContractTab;
	private List<ItemCard> _createdCards = new();


	public void MARKET_AddItem(MarketItemDto dto)
	{
		
		var newItem = _item.Instantiate<ItemCard>();
		_itemList.AddChild(newItem);
		newItem.CreateItem((ItemType)dto.ItemType, dto.SellerName, dto.PricePerUnit, dto.Quality);
		_createdCards.Add(newItem);


	}
	public void MARKET_ClearData()
	{
		
		for (int i = _createdCards.Count -1; i >= 0; i--)
		{
			
			_createdCards[i].QueueFree();
			_createdCards.RemoveAt(i);

		}

	}

	public void LoadRoyalContractInfo(S2C_RoyalContractInfoResponsePacket packet)
	{
		_royalContractTab.Init(packet);
	}

}
