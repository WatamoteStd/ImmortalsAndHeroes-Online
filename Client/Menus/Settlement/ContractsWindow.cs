using Godot;
using Shared.Items;
using Shared.Udp.Packets.Category.Settlement;
using System;

public partial class ContractsWindow : Control
{
	
	[Export] private Label _totalPriceLabel;
	[Export] private LineEdit _countEdit;
	[Export] private SpinBox _costEdit;
	[Export] private CheckBox _isTaxFreeBox;
	[Export] private SpinBox _taxEdit;
	[Export] private OptionButton _resourceId;
	[Export] private OptionButton _resourseQualityId;
	[Export] private CheckBox _pplCheckBox;
	[Export] private SpinBox _pplSpinBox;
	[Export] private Button _createContractButton;
 
	public override void _Ready()
	{
		
		_countEdit.TextChanged += (text) =>
		{
			CalculateTotal();
		};
		_costEdit.ValueChanged += (value) =>
		{
			CalculateTotal();  
		};
		_isTaxFreeBox.Toggled += (toggl) =>
		{
			_taxEdit.Editable = !toggl;
			if (toggl)
			{
				_taxEdit.Value = 0;
			}
		};

		_createContractButton.Pressed += CreateContract;

	}

	private void CalculateTotal()
	{

		if (string.IsNullOrEmpty(_countEdit.Text))
		{
			_totalPriceLabel.Text = "0";
			return;
		}

		if (!int.TryParse(_countEdit.Text, out int count))
		{
			count = 0;
			_countEdit.Text = "0";
		}
		int price = (int)_costEdit.Value;

		_totalPriceLabel.Text = (count * price).ToString();

	}

	private void CreateContract()
	{
		
		int baseResourseId = _resourceId.GetSelectedId();
		int qualityOffset = _resourseQualityId.GetSelectedId();

		ItemType item = (ItemType)(baseResourseId + qualityOffset);


		if (!int.TryParse(_countEdit.Text, out int count))
		{
			count = 1;
			_countEdit.Text = "1";
		}

		var packet = new C2S_RoyalContractCreateRequestPacket
		{
			ItemId = item, 
			PerPlayerLimit = _pplCheckBox.ButtonPressed, 
			PerPlayerLimitCount = (uint)_pplSpinBox.Value,
			PricePerUnit = (uint)_costEdit.Value,
			TotalCount = (uint)count,
			TaxFree = _isTaxFreeBox.ButtonPressed,
			TaxPercent = (float)_taxEdit.Value
		};

		ServerMaster.Instance.Settlement_CreateRoyalContract(packet);
		_costEdit.Value = 0;
		_taxEdit.Value = 0;
		_countEdit.Text = string.Empty;
		_totalPriceLabel.Text = "0";
		_pplSpinBox.Value = 0;

	}


}
