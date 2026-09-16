using Godot;
using Shared.DataTransferObjects.Market;
using Shared.Udp.Packets.Category.Market;
using System;
using System.Collections.Generic;

public partial class CityMarket : Area3D
{
	
	[Export] private uint _regionId;
	private MarketWindow _marketWindow;
	private bool _isOpen;

	public override void _Ready()
	{
		
		_marketWindow = SceneManager.Instance.MarketManagerWindow;

		BodyEntered += LocalPlayerEntered;

	}

	private async void LocalPlayerEntered(Node3D body)
	{
		
		if (body is LocalPlayerEntity player)
		{
			
			_isOpen = true;
			SceneManager.Instance.SwitchVisiblityCityMarket();
			List<MarketItemDto> response = await HttpsMasterClient.Instanсe.MARKET_GetDefault(_regionId);

			if (response != null)
			{
				
				for (int i = response.Count -1; i >= 0; i--)
				{
					_marketWindow.MARKET_AddItem(response[i]);
				}

			}

			var packet = new C2S_RoyalContractInfoRequestPacket{};
			ServerMaster.Instance.Market_LoadlContractRequest(packet);

		}

	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("Back") && _isOpen)
		{
			SceneManager.Instance.SwitchVisiblityCityMarket();
			_marketWindow.MARKET_ClearData();
			_isOpen = false;
		}
	}



}
