using Godot;
using System;

public partial class ContractsWindow : Control
{
	
	[Export] private Label _totalPriceLabel;
	[Export] private LineEdit _countEdit;
	[Export] private SpinBox _costEdit;
	[Export] private CheckBox _isTaxFreeBox;
	[Export] private SpinBox _taxEdit;

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


}
