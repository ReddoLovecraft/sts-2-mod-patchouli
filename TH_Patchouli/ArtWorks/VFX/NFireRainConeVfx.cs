using Godot;

public partial class NFireRainConeVfx : Node2D
{
	public Vector2 HitGlobalPosition { get; set; }

	[Export]
	public float StartHeight { get; set; } = 220f;

	[Export]
	public float FallSeconds { get; set; } = 0.45f;

	private AnimatedSprite2D? _sprite;

	public override void _Ready()
	{
		_sprite = GetNodeOrNull<AnimatedSprite2D>("Sprite");
		if (_sprite == null || _sprite.SpriteFrames == null)
		{
			QueueFree();
			return;
		}

		Texture2D? tex = _sprite.SpriteFrames.GetFrameTexture("default", 0);
		if (tex != null)
		{
			float h = tex.GetSize().Y * _sprite.Scale.Y;
			_sprite.Offset = new Vector2(0f, -h * 0.5f);
		}

		_sprite.AnimationFinished += OnSpriteAnimationFinished;
		_sprite.Stop();
		_sprite.Frame = 0;

		Vector2 startPosition = HitGlobalPosition + new Vector2(0f, -StartHeight);
		Vector2 halfPosition = startPosition.Lerp(HitGlobalPosition, 0.5f);
		GlobalPosition = startPosition;

		float fall = FallSeconds <= 0f ? 0.35f : FallSeconds;
		Tween tween = CreateTween();
		tween.TweenProperty(this, "global_position", halfPosition, fall * 0.5f)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.In);
		tween.TweenCallback(Callable.From(() =>
		{
			if (GodotObject.IsInstanceValid(this))
			{
				_sprite?.Play("default");
			}
		}));
		tween.TweenProperty(this, "global_position", HitGlobalPosition, fall * 0.5f)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.In);
	}

	private void OnSpriteAnimationFinished()
	{
		if (GodotObject.IsInstanceValid(this))
		{
			QueueFree();
		}
	}
}
