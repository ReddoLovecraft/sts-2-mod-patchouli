using Godot;
using Timer = Godot.Timer;

public partial class NPageStormBulletVfx : Node2D
{
	[Export]
	public int Direction { get; set; } = 1;

	[Export]
	public float TravelSeconds { get; set; } = 0.65f;

	[Export]
	public float EndScaleMultiplier { get; set; } = 2.5f;

	[Export]
	public float OffscreenMargin { get; set; } = 240f;

	private Node2D? _visual;
	private AnimationPlayer? _animationPlayer;
	private Timer? _lifeTimer;

	public override void _Ready()
	{
		if (GetViewport() == null)
		{
			QueueFree();
			return;
		}

		_visual = GetNodeOrNull<Node2D>("Visual");
		_animationPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
		_lifeTimer = GetNodeOrNull<Timer>("LifeTimer");

		if (_visual == null || _animationPlayer == null || _lifeTimer == null)
		{
			QueueFree();
			return;
		}

		RetuneAnimation();

		_lifeTimer.WaitTime = TravelSeconds;
		if (!_lifeTimer.IsStopped())
		{
			_lifeTimer.Stop();
		}
		_lifeTimer.Start();

		_animationPlayer.Play("play");
	}

	private void RetuneAnimation()
	{
		if (_animationPlayer == null)
		{
			return;
		}

		Rect2 rect = GetViewport().GetVisibleRect();
		float endX = (Direction >= 0) ? (rect.Size.X + OffscreenMargin) : (-OffscreenMargin);
		float dx = endX - GlobalPosition.X;

		Animation? anim = _animationPlayer.GetAnimation("play");
		if (anim == null)
		{
			return;
		}

		float originalLength = (float)anim.Length;
		if (originalLength <= 0f)
		{
			originalLength = 0.65f;
		}

		float ratio = (TravelSeconds > 0f) ? (TravelSeconds / originalLength) : 1f;
		if (!Mathf.IsEqualApprox(ratio, 1f))
		{
			for (int ti = 0; ti < anim.GetTrackCount(); ti++)
			{
				int kc = anim.TrackGetKeyCount(ti);
				for (int ki = 0; ki < kc; ki++)
				{
					double t = anim.TrackGetKeyTime(ti, ki);
					anim.TrackSetKeyTime(ti, ki, t * ratio);
				}
			}
			anim.Length = TravelSeconds;
		}

		for (int ti = 0; ti < anim.GetTrackCount(); ti++)
		{
			string path = anim.TrackGetPath(ti).ToString();
			int kc = anim.TrackGetKeyCount(ti);
			if (kc <= 0)
			{
				continue;
			}

			int last = kc - 1;
			if (path == "Visual:position")
			{
				anim.TrackSetKeyValue(ti, last, new Vector2(dx, 0f));
			}
			else if (path == "Visual:scale")
			{
				anim.TrackSetKeyValue(ti, last, Vector2.One * EndScaleMultiplier);
			}
		}
	}
}
