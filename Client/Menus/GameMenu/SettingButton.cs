using Godot;
using System;

public partial class SettingButton : TextureButton
{
	private Tween _tween;
	
	public void RotateOnOpen()
	{
		
		_tween?.Kill();

		PivotOffset = Size / 2;

		_tween = CreateTween()
			.SetTrans(Tween.TransitionType.Quint)
			.SetEase(Tween.EaseType.Out);

		_tween.TweenProperty(this, "rotation_degrees", 90.0f, 0.36f);

	}
	public void RotateOnClose()
	{
		
		_tween?.Kill();

		PivotOffset = Size / 2;

		_tween = CreateTween()
			.SetTrans(Tween.TransitionType.Back)
			.SetEase(Tween.EaseType.Out);

		_tween.TweenProperty(this, "rotation_degrees", 0.0f, 0.21f);

	}

}
