using Godot;
using System;

public partial class Settlement : Area3D
{
	
	[Export] private uint RegionId;


	public override void _Ready()
	{
		BodyEntered += (body) =>
		{
			if (body is LocalPlayerEntity player)
			{
				SceneManager.Instance.SwitchVisiblitySettlementManage();
			}
		};
	}


}
