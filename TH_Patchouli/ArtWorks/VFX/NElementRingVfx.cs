using Godot;

public partial class NElementRingVfx : Node2D
{
	[Export]
	public float DurationSeconds { get; set; } = 1.6f;

	private const float MainRotationRadiansPerSecond = 2.4f;

	private Sprite2D? _main;
	private float _elapsedSeconds;

	public override void _Ready()
	{
		_main = GetNodeOrNull<Sprite2D>("%Main");
		_elapsedSeconds = 0f;
	}

	public override void _Process(double delta)
	{
		float d = (float)delta;
		_elapsedSeconds += d;

		if (_main != null)
		{
			_main.Rotation += d * MainRotationRadiansPerSecond;
		}

		if (_elapsedSeconds >= DurationSeconds)
		{
			QueueFree();
		}
	}

	public void Play(float durationSeconds = 1.6f)
	{
		DurationSeconds = durationSeconds;
		_elapsedSeconds = 0f;
	}
}
