using Godot;

[Tool]
public partial class NRockShatterVfx : Node2D
{
	[Export] public bool Enabled { get; set; } = true;
	[Export] public float ShatterSeconds { get; set; } = 0.35f;
	[Export] public float SpreadPixels { get; set; } = 130f;
	[Export] public float RotateRadians { get; set; } = 0.65f;
	[Export] public float RandomJitterPixels { get; set; } = 6f;
	[Export] public int Columns { get; set; } = 5;
	[Export] public int Rows { get; set; } = 2;

	private Node2D? _visual;
	private Sprite2D? _intact;
	private Node2D? _piecesRoot;
	private bool _started;

	public override void _Ready()
	{
		_visual = GetNodeOrNull<Node2D>("%Visual") ?? GetNodeOrNull<Node2D>("Visual");
		_intact = GetNodeOrNull<Sprite2D>("%Intact") ?? GetNodeOrNull<Sprite2D>("Visual/Intact");
		_piecesRoot = GetNodeOrNull<Node2D>("%Pieces") ?? GetNodeOrNull<Node2D>("Visual/Pieces");
		if (!Enabled || _visual == null || _intact == null || _piecesRoot == null || _intact.Texture == null)
		{
			return;
		}

		Start();
	}

	private void Start()
	{
		if (_started)
		{
			return;
		}
		_started = true;

		_piecesRoot?.QueueFreeChildren();

		CreatePieces();
		BeginShatter();
	}

	private void CreatePieces()
	{
		if (_intact?.Texture == null || _piecesRoot == null)
		{
			return;
		}

		Vector2 texSize = _intact.Texture.GetSize();
		if (texSize.X <= 1f || texSize.Y <= 1f)
		{
			return;
		}

		int cols = Mathf.Clamp(Columns, 1, 32);
		int rows = Mathf.Clamp(Rows, 1, 32);
		float cellW = texSize.X / cols;
		float cellH = texSize.Y / rows;
		Vector2 texCenter = texSize * 0.5f;

		for (int y = 0; y < rows; y++)
		{
			for (int x = 0; x < cols; x++)
			{
				Rect2 rect = new Rect2(x * cellW, y * cellH, cellW, cellH);
				var piece = new Sprite2D
				{
					Centered = true,
					Texture = _intact.Texture,
					RegionEnabled = true,
					RegionRect = rect,
					Material = _intact.Material,
					ZAsRelative = true,
					ZIndex = 0,
					Modulate = new Color(1f, 1f, 1f, 1f)
				};

				Vector2 pieceCenter = rect.Position + rect.Size * 0.5f;
				piece.Position = pieceCenter - texCenter;
				if (RandomJitterPixels > 0.001f)
				{
					piece.Position += new Vector2(
						(float)GD.RandRange(-RandomJitterPixels, RandomJitterPixels),
						(float)GD.RandRange(-RandomJitterPixels, RandomJitterPixels)
					);
				}

				_piecesRoot.AddChild(piece);
			}
		}
	}

	private void BeginShatter()
	{
		if (_visual == null || _intact == null || _piecesRoot == null)
		{
			return;
		}

		_intact.Visible = false;
		_piecesRoot.Visible = true;

		Vector2 sum = Vector2.Zero;
		int count = 0;
		foreach (Node child in _piecesRoot.GetChildren())
		{
			if (child is Sprite2D s)
			{
				sum += s.Position;
				count++;
			}
		}
		Vector2 center = count > 0 ? sum / count : Vector2.Zero;

		float duration = Mathf.Max(0.05f, ShatterSeconds);
		var tween = CreateTween();

		foreach (Node child in _piecesRoot.GetChildren())
		{
			if (child is not Sprite2D piece)
			{
				continue;
			}

			double angle = GD.RandRange(0.0, Mathf.Tau);
			Vector2 dir = new Vector2((float)Mathf.Cos((float)angle), (float)Mathf.Sin((float)angle));

			float spread = SpreadPixels * (float)GD.RandRange(0.75, 1.30);
			Vector2 targetPos = center + dir * spread;
			targetPos += new Vector2(
				(float)GD.RandRange(-RandomJitterPixels, RandomJitterPixels),
				(float)GD.RandRange(-RandomJitterPixels, RandomJitterPixels)
			);

			float rot = RotateRadians * (float)GD.RandRange(-1.0, 1.0);

			tween.Parallel().TweenProperty(piece, "position", targetPos, duration)
				.SetTrans(Tween.TransitionType.Sine)
				.SetEase(Tween.EaseType.Out);
			tween.Parallel().TweenProperty(piece, "rotation", piece.Rotation + rot, duration)
				.SetTrans(Tween.TransitionType.Sine)
				.SetEase(Tween.EaseType.Out);
			tween.Parallel().TweenProperty(piece, "modulate:a", 0f, duration)
				.SetTrans(Tween.TransitionType.Sine)
				.SetEase(Tween.EaseType.In);
		}

		tween.TweenInterval(duration);
		tween.TweenCallback(Callable.From(QueueFree));
	}
}

internal static class NRockShatterVfxExtensions
{
	public static void QueueFreeChildren(this Node node)
	{
		foreach (Node child in node.GetChildren())
		{
			child.QueueFree();
		}
	}
}
