using Godot;

public partial class NElementManagerProjectileVfx : Node2D
{
	public string ElementScenePath { get; set; } = string.Empty;

	public Vector2 StartGlobalPosition { get; set; }

	public Vector2 TargetGlobalPosition { get; set; }

	[Export]
	public float TravelSeconds { get; set; } = 0.32f;

	[Export]
	public float FadeSeconds { get; set; } = 0.12f;

	[Export]
	public float VisualScale { get; set; } = 1.15f;

	private Node2D? _visualRoot;

	public override void _Ready()
	{
		_visualRoot = GetNodeOrNull<Node2D>("VisualRoot");
		if (_visualRoot == null || string.IsNullOrWhiteSpace(ElementScenePath))
		{
			QueueFree();
			return;
		}

		PackedScene? scene = ResourceLoader.Load<PackedScene>(ElementScenePath, null, ResourceLoader.CacheMode.Reuse);
		if (scene == null)
		{
			QueueFree();
			return;
		}

		Node2D visual = scene.Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
		visual.Position = Vector2.Zero;
		visual.Scale = Vector2.One * VisualScale;
		_visualRoot.AddChild(visual);

		GlobalPosition = StartGlobalPosition;

		Tween tween = CreateTween();
		tween.TweenProperty(this, "global_position", TargetGlobalPosition, TravelSeconds)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.Out);
		tween.TweenProperty(this, "modulate:a", 0f, FadeSeconds)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.In);
		tween.TweenCallback(Callable.From(QueueFree));
	}
}
