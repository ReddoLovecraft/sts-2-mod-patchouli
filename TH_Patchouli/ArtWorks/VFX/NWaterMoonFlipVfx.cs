using Godot;

public partial class NWaterMoonFlipVfx : Node2D
{
	[Export]
	public float InSeconds { get; set; } = 0.14f;

	[Export]
	public float HoldSeconds { get; set; } = 0.12f;

	[Export]
	public float OutSeconds { get; set; } = 0.26f;

	private ColorRect? _rect;
	private CanvasLayer? _layer;

	public override void _Ready()
	{
		_layer = GetNodeOrNull<CanvasLayer>("%Layer");
		_rect = GetNodeOrNull<ColorRect>("%Rect");
		if (_layer == null || _rect == null)
		{
			QueueFree();
			return;
		}

		_layer.Layer = 999;
		ProcessMode = ProcessModeEnum.Always;
		StartTween();
	}

	private void StartTween()
	{
		if (_rect == null)
		{
			return;
		}

		ShaderMaterial? mat = _rect.Material as ShaderMaterial;
		if (mat == null)
		{
			QueueFree();
			return;
		}

		float tin = Mathf.Max(0f, InSeconds);
		float thold = Mathf.Max(0f, HoldSeconds);
		float tout = Mathf.Max(0f, OutSeconds);

		mat.SetShaderParameter("strength", 0f);
		Tween t = CreateTween();

		if (tin > 0f)
		{
			t.TweenMethod(Callable.From<float>(v => mat.SetShaderParameter("strength", v)), 0f, 1f, tin)
				.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
		}
		else
		{
			mat.SetShaderParameter("strength", 1f);
		}

		if (thold > 0f)
		{
			t.TweenInterval(thold);
		}

		if (tout > 0f)
		{
			t.TweenMethod(Callable.From<float>(v => mat.SetShaderParameter("strength", v)), 1f, 0f, tout)
				.SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Sine);
		}
		else
		{
			mat.SetShaderParameter("strength", 0f);
		}

		t.TweenCallback(Callable.From(QueueFree));
	}
}
