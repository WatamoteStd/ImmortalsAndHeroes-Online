using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class SettingWindow : Control
{

    public event Action OnWindowClose;
    
    [Export] private CheckBox _attackSpace;
    [Export] private Label _attackSpaceLabel;
    [Export] private CheckBox _attackLmb;
    [Export] private Label _attackLmbLabel;
	[Export] private CheckBox _overheatingBox;
    [Export] private Label _overheatingLabel;

	[Export] private Button _closeWindow;
    private Tween _activeTween;

    private readonly Dictionary<CheckBox, Label> _settingToLabel = new();

    private readonly Color _activeColor = new(0.843f, 0.824f, 0.812f);
    private readonly Color _disabledColor = new(0.506f, 0.525f, 0.506f);

    public override void _Ready()
    {
        _settingToLabel[_attackSpace] = _attackSpaceLabel;
        _settingToLabel[_attackLmb] = _attackLmbLabel;
		_settingToLabel[_overheatingBox] = _overheatingLabel;

        foreach (var (checkBox, label) in _settingToLabel)
        {
            checkBox.Toggled += (isToggled) => UpdateSettingState(checkBox, isToggled);

            UpdateSettingState(checkBox, checkBox.ButtonPressed);
        }


		_closeWindow.Pressed += () =>
        {
            _ = CloseWindow();
        };
       
    }

    private void UpdateSettingState(CheckBox checkBox, bool isToggled)
    {
        if (_settingToLabel.TryGetValue(checkBox, out Label label))
        {
            label.AddThemeColorOverride("font_color", isToggled ? _activeColor : _disabledColor);

			if (checkBox == _attackLmb)
			{
				SettingsManager.Instance.AttackOnFirstLmb = isToggled;
			}

        }
    }


    public void OpenWindow()
    {
        _activeTween?.Kill();
        float curAlpha = Modulate.A;
        float remainDuration = 0.45f * (1.0f - curAlpha);

        _activeTween = CreateTween();
        _activeTween.SetEase(Tween.EaseType.Out);
        _activeTween.SetTrans(Tween.TransitionType.Quint);
        _activeTween.TweenProperty(this, "modulate:a", 1.0f, remainDuration);

        Visible = true;


    }
    public async Task CloseWindow()
    {
        _activeTween?.Kill();

        float currentAlpha = Modulate.A;
        float remainingDuration = 0.3f * currentAlpha;

        _activeTween = CreateTween();
        _activeTween.SetEase(Tween.EaseType.Out);
        _activeTween.SetTrans(Tween.TransitionType.Quint);
        _activeTween.TweenProperty(this, "modulate:a", 0.0f, remainingDuration);

        await ToSignal(_activeTween, Tween.SignalName.Finished);
        
        if (Mathf.IsZeroApprox(Modulate.A))
        {
            Visible = false;
            OnWindowClose?.Invoke();
        }


    }


}