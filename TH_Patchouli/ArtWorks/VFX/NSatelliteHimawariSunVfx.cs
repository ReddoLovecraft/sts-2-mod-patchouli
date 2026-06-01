using Godot;

public partial class NSatelliteHimawariSunVfx : Node2D
{
	[Export]
	public float DurationSeconds { get; set; } = 1.75f;

	[Export]
	public float SpitSeconds { get; set; } = 0.18f;

	[Export]
	public float SpitSpeedMin { get; set; } = 620f;

	[Export]
	public float SpitSpeedMax { get; set; } = 860f;

	[Export]
	public float SpitAngleMinDeg { get; set; } = -112f;

	[Export]
	public float SpitAngleMaxDeg { get; set; } = -68f;

	[Export]
	public float FallSpeedMin { get; set; } = 180f;

	[Export]
	public float FallSpeedMax { get; set; } = 320f;

	[Export]
	public float FallAngleRightMinDeg { get; set; } = 15f;

	[Export]
	public float FallAngleRightMaxDeg { get; set; } = 65f;

	[Export]
	public float FallAngleLeftMinDeg { get; set; } = 115f;

	[Export]
	public float FallAngleLeftMaxDeg { get; set; } = 165f;

	[Export]
	public float RotationSpeedDegPerSecMin { get; set; } = -35f;

	[Export]
	public float RotationSpeedDegPerSecMax { get; set; } = 35f;

	private AnimatedSprite2D? _sprite;
	private readonly RandomNumberGenerator _rng = new RandomNumberGenerator();
	private Vector2 _spitVelocity;
	private Vector2 _fallVelocity;
	private float _rotationSpeedRadPerSec;
	private float _elapsed;
	private float _baseAlpha = 1f;

	public override void _Ready()
	{
		ZAsRelative = false;
		ZIndex = 0;

		_sprite = GetNodeOrNull<AnimatedSprite2D>("%Sprite");
		if (_sprite != null)
		{
			_baseAlpha = _sprite.Modulate.A;
			if (!_sprite.IsPlaying())
			{
				_sprite.Play("default");
			}
		}

		InitMotion();
		_elapsed = 0f;
		Visible = true;
	}

	public override void _Process(double delta)
	{
		float d = (float)delta;
		_elapsed += d;

		float spitSeconds = Mathf.Max(0f, SpitSeconds);
		if (_elapsed < spitSeconds)
		{
			GlobalPosition += _spitVelocity * d;
		}
		else
		{
			GlobalPosition += _fallVelocity * d;
		}

		Rotation += _rotationSpeedRadPerSec * d;

		UpdateOpacity();

		if (_elapsed >= DurationSeconds)
		{
			QueueFree();
		}
	}

	private void InitMotion()
	{
		float spitAng = Mathf.DegToRad(_rng.RandfRange(Mathf.Min(SpitAngleMinDeg, SpitAngleMaxDeg), Mathf.Max(SpitAngleMinDeg, SpitAngleMaxDeg)));
		float spitSpeed = _rng.RandfRange(Mathf.Min(SpitSpeedMin, SpitSpeedMax), Mathf.Max(SpitSpeedMin, SpitSpeedMax));
		_spitVelocity = new Vector2(Mathf.Cos(spitAng), Mathf.Sin(spitAng)) * spitSpeed;

		bool goLeft = _rng.Randf() < 0.5f;
		float angDeg = goLeft
			? _rng.RandfRange(FallAngleLeftMinDeg, FallAngleLeftMaxDeg)
			: _rng.RandfRange(FallAngleRightMinDeg, FallAngleRightMaxDeg);

		float speed = _rng.RandfRange(Mathf.Min(FallSpeedMin, FallSpeedMax), Mathf.Max(FallSpeedMin, FallSpeedMax));
		float ang = Mathf.DegToRad(angDeg);
		_fallVelocity = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * speed;

		float rotDeg = _rng.RandfRange(RotationSpeedDegPerSecMin, RotationSpeedDegPerSecMax);
		_rotationSpeedRadPerSec = Mathf.DegToRad(rotDeg);
	}

	private void UpdateOpacity()
	{
		if (_sprite == null)
		{
			return;
		}

		float duration = DurationSeconds <= 0f ? 1.75f : DurationSeconds;
		float spitSeconds = Mathf.Max(0f, SpitSeconds);
		float fallElapsed = Mathf.Max(0f, _elapsed - spitSeconds);
		float fallDuration = Mathf.Max(0.01f, duration - spitSeconds);
		float alpha = _elapsed < spitSeconds ? _baseAlpha : (1f - (fallElapsed / fallDuration)) * _baseAlpha;
		alpha = Mathf.Clamp(alpha, 0f, _baseAlpha);

		Color m = _sprite.Modulate;
		m.A = alpha;
		_sprite.Modulate = m;
	}
}
