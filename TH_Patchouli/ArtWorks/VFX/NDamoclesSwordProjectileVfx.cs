using Godot;

public partial class NDamoclesSwordProjectileVfx : Node2D
{
	public Vector2 SpawnGlobalPosition { get; set; }

	public Vector2 TargetGlobalPosition { get; set; }

	public Vector2 BodyLocalOffset { get; set; } = new Vector2(22f, -10f);

	[Export]
	public float VisualScale { get; set; } = 2f;

	[Export]
	public float GhostAngleDegrees { get; set; } = 20f;

	[Export]
	public float BodyAngleDegrees { get; set; } = -20f;

	[Export]
	public float BodyAppearDelaySeconds { get; set; } = 0.05f;

	[Export]
	public float LaunchDelaySeconds { get; set; } = 0.12f;

	[Export]
	public float TravelSeconds { get; set; } = 0.3f;

	[Export]
	public float FadeSeconds { get; set; } = 0.1f;

	[Export]
	public float TrailLength { get; set; } = 184f;

	[Export]
	public float AngleRecoverSeconds { get; set; } = 0.16f;

	public float TotalDurationSeconds => Mathf.Max(LaunchDelaySeconds, BodyAppearDelaySeconds) + Mathf.Max(TravelSeconds, 0.01f) + Mathf.Max(FadeSeconds, 0f);

	private Node2D? _projectileRoot;
	private Sprite2D? _ghostSprite;
	private Sprite2D? _bodySprite;
	private Line2D? _outerTrail;
	private Line2D? _innerTrail;
	private Vector2 _lastDirection = Vector2.Right;

	public override void _Ready()
	{
		_projectileRoot = GetNodeOrNull<Node2D>("%ProjectileRoot");
		_ghostSprite = GetNodeOrNull<Sprite2D>("%GhostSprite");
		_bodySprite = GetNodeOrNull<Sprite2D>("%BodySprite");
		_outerTrail = GetNodeOrNull<Line2D>("%OuterTrail");
		_innerTrail = GetNodeOrNull<Line2D>("%InnerTrail");
		if (_projectileRoot == null || _ghostSprite == null || _bodySprite == null || _outerTrail == null || _innerTrail == null)
		{
			QueueFree();
			return;
		}

		_projectileRoot.GlobalPosition = SpawnGlobalPosition;
		_projectileRoot.Scale = Vector2.One * VisualScale;
		_ghostSprite.Position = Vector2.Zero;
		_bodySprite.Position = BodyLocalOffset;
		_ghostSprite.RotationDegrees = GhostAngleDegrees;
		_bodySprite.RotationDegrees = BodyAngleDegrees;
		_bodySprite.Visible = false;

		Vector2 direction = TargetGlobalPosition - SpawnGlobalPosition;
		if (direction.LengthSquared() > 0.001f)
		{
			_lastDirection = direction.Normalized();
			_projectileRoot.Rotation = _lastDirection.Angle();
		}

		UpdateTrail();

		Tween tween = CreateTween();
		tween.TweenInterval(Mathf.Max(BodyAppearDelaySeconds, 0f));
		tween.TweenCallback(Callable.From(ShowBody));
		tween.TweenInterval(Mathf.Max(LaunchDelaySeconds - BodyAppearDelaySeconds, 0f));
		tween.TweenMethod(Callable.From<float>(UpdateTravel), 0f, 1f, Mathf.Max(TravelSeconds, 0.01f))
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.Out);
		tween.TweenProperty(this, "modulate:a", 0f, Mathf.Max(FadeSeconds, 0.01f))
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.In);
		tween.TweenCallback(Callable.From(QueueFree));
	}

	private void ShowBody()
	{
		if (_ghostSprite == null || _bodySprite == null)
		{
			return;
		}

		_bodySprite.Visible = true;

		Tween tween = CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(_ghostSprite, "rotation_degrees", 0f, Mathf.Max(AngleRecoverSeconds, 0.01f))
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.Out);
		tween.TweenProperty(_bodySprite, "rotation_degrees", 0f, Mathf.Max(AngleRecoverSeconds, 0.01f))
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.Out);
		tween.TweenProperty(_bodySprite, "position", Vector2.Zero, Mathf.Max(AngleRecoverSeconds, 0.01f))
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.Out);
	}

	private void UpdateTravel(float progress)
	{
		if (_projectileRoot == null)
		{
			return;
		}

		Vector2 previous = _projectileRoot.GlobalPosition;
		Vector2 current = SpawnGlobalPosition.Lerp(TargetGlobalPosition, progress);
		_projectileRoot.GlobalPosition = current;

		Vector2 delta = current - previous;
		if (delta.LengthSquared() > 0.001f)
		{
			_lastDirection = delta.Normalized();
			_projectileRoot.Rotation = _lastDirection.Angle();
		}

		UpdateTrail();
	}

	private void UpdateTrail()
	{
		if (_projectileRoot == null || _outerTrail == null || _innerTrail == null)
		{
			return;
		}

		Vector2 head = _projectileRoot.GlobalPosition;
		float distance = Mathf.Min(TrailLength, head.DistanceTo(SpawnGlobalPosition));
		Vector2 tail = head - (_lastDirection * distance);
		SetTrailPoints(_outerTrail, tail, head);
		SetTrailPoints(_innerTrail, tail, head);
	}

	private static void SetTrailPoints(Line2D trail, Vector2 tail, Vector2 head)
	{
		trail.ClearPoints();
		trail.AddPoint(tail);
		trail.AddPoint(head);
	}
}
