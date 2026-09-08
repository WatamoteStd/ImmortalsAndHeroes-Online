using Godot;
using System;

public partial class FadePanel : PanelContainer
{
	
	private Tween _tween;
	[Export] private Label _silverCount;

    public override void _Ready()
    {
        GameSession.Instance.OnPlayerDataUpdated += UpdateSilver;
    }



	public void UpdateSilver()
	{
		_silverCount.Text = GameSession.Instance.PlayerCache.Silver.ToString();
	}


	public void ShowSmooth()
	{
		
		SetInteractive(this, true);
		CheckTween();
		_tween = CreateTween();
		_tween.TweenProperty(this, "modulate", Colors.White, 0.7f);
		MouseFilter = MouseFilterEnum.Stop;

	}

	public void HideSmooth()
	{
		SetInteractive(this, false);
		CheckTween();
		_tween = CreateTween();
		_tween.TweenProperty(this, "modulate", new Color(0.7f, 0.7f, 0.7f, 0.55f), 0.7f);

	}

	private void CheckTween()
	{
		
		if (_tween != null && _tween.IsValid())
		{
			_tween.Kill();
		}

	}

	private void SetInteractive(Node node, bool isInteractive)
	{
		
		if (node is Control control)
		{
			control.MouseFilter = isInteractive ? MouseFilterEnum.Stop : MouseFilterEnum.Ignore;
		}

		foreach (Node child in node.GetChildren())
		{
			SetInteractive(child, isInteractive);
		}

	}

}
