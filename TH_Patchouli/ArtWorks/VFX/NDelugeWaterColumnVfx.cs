using Godot;

public partial class NDelugeWaterColumnVfx : Node2D
{
	public Vector2 GroundGlobalPosition { get; set; }

	private AnimatedSprite2D? _sprite;

	public override void _Ready()
	{
		_sprite = GetNodeOrNull<AnimatedSprite2D>("Sprite");
		if (_sprite == null || _sprite.SpriteFrames == null)
		{
			QueueFree();
			return;
		}

		GlobalPosition = GroundGlobalPosition;

		Texture2D? tex = _sprite.SpriteFrames.GetFrameTexture("default", 0);
		if (tex != null)
		{
			float h = tex.GetSize().Y * _sprite.Scale.Y;
			_sprite.Offset = new Vector2(0f, -h * 0.5f);
		}

		_sprite.AnimationFinished += () =>
		{
			if (GodotObject.IsInstanceValid(this))
			{
				QueueFree();
			}
		};

		_sprite.Play("default");
	}
}

