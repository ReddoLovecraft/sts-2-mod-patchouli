using Godot;

public partial class NFallSlasherProjectileVfx : Node2D
{
	private const string AdditiveMaterialPath = "res://TH_Patchouli/ArtWorks/VFX/canvas_item_material_additive_shared.tres";

	public Vector2 SpawnGlobalPosition { get; set; }

	public Vector2 TargetGlobalPosition { get; set; }

	[Export]
	public float TravelSeconds { get; set; } = 0.26f;

	[Export]
	public float FadeSeconds { get; set; } = 0.08f;

	[Export]
	public float TailFragmentSpacing { get; set; } = 18f;

	[Export]
	public float TailFragmentLifeSeconds { get; set; } = 0.18f;

	public float TotalDurationSeconds => Mathf.Max(TravelSeconds, 0.01f) + Mathf.Max(FadeSeconds, 0f);

	private Node2D? _projectileRoot;
	private Vector2 _lastDirection = Vector2.Right;
	private float _tailDistanceCarry;
	private Material? _additiveMaterial;

	public override void _Ready()
	{
		_projectileRoot = GetNodeOrNull<Node2D>("%ProjectileRoot");
		if (_projectileRoot == null)
		{
			QueueFree();
			return;
		}

		_additiveMaterial = ResourceLoader.Load<Material>(AdditiveMaterialPath, null, ResourceLoader.CacheMode.Reuse);
		_projectileRoot.GlobalPosition = SpawnGlobalPosition;
		Vector2 direction = TargetGlobalPosition - SpawnGlobalPosition;
		if (direction.LengthSquared() > 0.001f)
		{
			_lastDirection = direction.Normalized();
			_projectileRoot.Rotation = _lastDirection.Angle();
		}

		Tween tween = CreateTween();
		tween.TweenMethod(Callable.From<float>(UpdateTravel), 0f, 1f, Mathf.Max(TravelSeconds, 0.01f))
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.Out);
		tween.TweenProperty(this, "modulate:a", 0f, Mathf.Max(FadeSeconds, 0.01f))
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.In);
		tween.TweenCallback(Callable.From(QueueFree));
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

		EmitTailFragments(previous, current, progress);
	}

	private void EmitTailFragments(Vector2 previous, Vector2 current, float progress)
	{
		Node? parent = GetParent();
		Vector2 segment = current - previous;
		float segmentLength = segment.Length();
		if (parent == null || segmentLength <= 0.001f)
		{
			return;
		}

		float spacing = Mathf.Max(TailFragmentSpacing, 1f);
		float consumed = 0f;
		while (_tailDistanceCarry + (segmentLength - consumed) >= spacing)
		{
			float need = spacing - _tailDistanceCarry;
			consumed += need;
			float t = consumed / segmentLength;
			Vector2 spawn = previous.Lerp(current, t);
			SpawnTailFragment(parent, spawn, progress);
			_tailDistanceCarry = 0f;
		}

		_tailDistanceCarry += segmentLength - consumed;
	}

	private void SpawnTailFragment(Node parent, Vector2 headPosition, float progress)
	{
		var outer = new Polygon2D();
		outer.Polygon = CreateTrailPolygon(progress, isInner: false);
		outer.Color = new Color(0.26f, 0.57f, 1f, Mathf.Lerp(0.8f, 0.14f, progress));
		outer.GlobalPosition = headPosition;
		outer.Rotation = _lastDirection.Angle();
		if (_additiveMaterial != null)
		{
			outer.Material = _additiveMaterial;
		}

		var inner = new Polygon2D();
		inner.Polygon = CreateTrailPolygon(progress, isInner: true);
		inner.Color = new Color(0.75f, 0.94f, 1f, Mathf.Lerp(0.72f, 0.1f, progress));
		inner.GlobalPosition = headPosition;
		inner.Rotation = _lastDirection.Angle();
		if (_additiveMaterial != null)
		{
			inner.Material = _additiveMaterial;
		}

		parent.AddChild(outer);
		parent.AddChild(inner);
		AnimateTailFragment(outer, progress, isInner: false);
		AnimateTailFragment(inner, progress, isInner: true);
	}

	private Vector2[] CreateTrailPolygon(float progress, bool isInner)
	{
		float length = isInner ? Mathf.Lerp(46f, 68f, progress) : Mathf.Lerp(62f, 96f, progress);
		float headHalf = isInner ? Mathf.Lerp(5f, 9f, progress) : Mathf.Lerp(8f, 14f, progress);
		float tailHalf = isInner ? Mathf.Lerp(12f, 22f, progress) : Mathf.Lerp(18f, 34f, progress);

		return new Vector2[]
		{
			new Vector2(0f, headHalf),
			new Vector2(-length, tailHalf),
			new Vector2(-length, -tailHalf),
			new Vector2(0f, -headHalf)
		};
	}

	private void AnimateTailFragment(Polygon2D fragment, float progress, bool isInner)
	{
		float life = Mathf.Max(TailFragmentLifeSeconds, 0.01f) * Mathf.Lerp(0.9f, 1.2f, progress);
		float widthScale = isInner ? Mathf.Lerp(1.45f, 2.2f, progress) : Mathf.Lerp(1.8f, 2.9f, progress);
		float lengthScale = isInner ? 1.08f : 1.14f;

		Tween tween = fragment.CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(fragment, "scale:y", widthScale, life)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.Out);
		tween.TweenProperty(fragment, "scale:x", lengthScale, life)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.Out);
		tween.TweenProperty(fragment, "modulate:a", 0f, life)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.In);
		tween.TweenProperty(fragment, "global_position", fragment.GlobalPosition - (_lastDirection * 10f), life)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.Out);
		tween.Chain().TweenCallback(Callable.From(fragment.QueueFree));
	}
}
