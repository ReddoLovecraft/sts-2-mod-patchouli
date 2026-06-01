using Godot;

public partial class NEmeraldCityBarrierVfx : Node2D
{
	public Vector2 TargetGlobalPosition { get; set; }

	[Export]
	public bool FlipVisualX { get; set; }

	[Export]
	public float RiseDistance { get; set; } = 84f;

	[Export]
	public float RiseSeconds { get; set; } = 0.18f;

	[Export]
	public float HoldSeconds { get; set; } = 0.25f;

	[Export]
	public float FadeSeconds { get; set; } = 0.16f;

	private Sprite2D? _sprite;

	public override void _Ready()
	{
		_sprite = GetNodeOrNull<Sprite2D>("Sprite");
		if (_sprite == null || _sprite.Texture == null)
		{
			QueueFree();
			return;
		}

		Vector2 texSize = _sprite.Texture.GetSize() * _sprite.Scale;
		_sprite.Offset = new Vector2(0f, -texSize.Y * 0.5f);
		_sprite.FlipH = FlipVisualX;

		GlobalPosition = TargetGlobalPosition + Vector2.Down * RiseDistance;
		Modulate = Colors.White;

		Tween tween = CreateTween();
		tween.TweenProperty(this, "global_position", TargetGlobalPosition, RiseSeconds)
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.Out);
		tween.TweenInterval(HoldSeconds);
		tween.TweenProperty(this, "modulate:a", 0f, FadeSeconds)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.In);
		tween.TweenCallback(Callable.From(QueueFree));
	}
}
