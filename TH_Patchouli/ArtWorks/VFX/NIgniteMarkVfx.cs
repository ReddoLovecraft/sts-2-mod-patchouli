using Godot;

public partial class NIgniteMarkVfx : Node2D
{
	[Export]
	public Vector2 OffsetFromAnchor { get; set; } = new Vector2(0f, -120f);

	private Sprite2D? _fireSign;
	private Sprite2D? _spotlight;
	private Tween? _pulseTween;
	private Color _spotlightBaseModulate;

	private Material? _additiveMat;
	private Texture2D? _moteTexture;
	private readonly RandomNumberGenerator _rng = new RandomNumberGenerator();
	private float _moteSeconds;
	private float _nextMoteSeconds;
	private Control? _hitbox;

	public override void _Ready()
	{
		ZAsRelative = false;
		ZIndex = 80;

		_fireSign = GetNodeOrNull<Sprite2D>("%FireSign");
		_spotlight = GetNodeOrNull<Sprite2D>("%Spotlight");
		_additiveMat = ResourceLoader.Load<Material>("res://TH_Patchouli/ArtWorks/VFX/canvas_item_material_additive_shared.tres", null, ResourceLoader.CacheMode.Reuse);
		_moteTexture = ResourceLoader.Load<Texture2D>("res://TH_Patchouli/ArtWorks/VFX/touhoueffect/healLightAb000.png", null, ResourceLoader.CacheMode.Reuse);

		_hitbox = ResolveHitbox();

		if (_spotlight != null)
		{
			_spotlightBaseModulate = _spotlight.Modulate;
			StartPulseTween();
		}

		_nextMoteSeconds = _rng.RandfRange(0.10f, 0.18f);
		UpdateLayout();
		Visible = true;
	}

	public override void _Process(double delta)
	{
		_moteSeconds += (float)delta;
		UpdateLayout();
		TrySpawnMote();
	}

	private void UpdateLayout()
	{
		_hitbox ??= ResolveHitbox();
		Control? hitbox = _hitbox;
		if (hitbox == null)
		{
			return;
		}

		Position = Vector2.Zero;

		if (_spotlight != null && _spotlight.Texture != null)
		{
			Vector2 tex = _spotlight.Texture.GetSize();
			float sx = (hitbox.Size.X * 1.06f) / Mathf.Max(1f, tex.X);
			float sy = (hitbox.Size.Y * 1.06f) / Mathf.Max(1f, tex.Y);
			float lightScale = Mathf.Max(sx, sy);
			float scaledLightHeight = tex.Y * lightScale;
			float desiredBottomY = hitbox.Size.Y * 0.60f;
			float lightPosY = desiredBottomY - scaledLightHeight * 0.5f;
			lightPosY = Mathf.Clamp(lightPosY, -hitbox.Size.Y * 0.85f, -hitbox.Size.Y * 0.05f);

			_spotlight.Scale = Vector2.One * lightScale;
			_spotlight.Position = new Vector2(0f, lightPosY);
			_spotlight.ZAsRelative = false;
			_spotlight.ZIndex = 0;
		}

		if (_fireSign != null)
		{
			_fireSign.Position = new Vector2(0f, -hitbox.Size.Y * 0.5f) + OffsetFromAnchor;
			_fireSign.ZAsRelative = false;
			_fireSign.ZIndex = 10;
		}
	}

	private void StartPulseTween()
	{
		if (_spotlight == null)
		{
			return;
		}

		_pulseTween?.Kill();

		float a0 = _spotlightBaseModulate.A;
		_pulseTween = _spotlight.CreateTween().SetLoops();
		_pulseTween.TweenProperty(_spotlight, "modulate:a", a0 * 0.70f, 0.85).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
		_pulseTween.TweenProperty(_spotlight, "modulate:a", a0, 0.85).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
	}

	private void TrySpawnMote()
	{
		if (_moteTexture == null || _additiveMat == null)
		{
			return;
		}

		_hitbox ??= ResolveHitbox();
		if (_hitbox == null)
		{
			return;
		}

		if (_moteSeconds < _nextMoteSeconds)
		{
			return;
		}

		_moteSeconds = 0f;
		_nextMoteSeconds = _rng.RandfRange(0.10f, 0.18f);
		SpawnMote(_hitbox.Size);
	}

	private void SpawnMote(Vector2 hitboxSize)
	{
		if (_moteTexture == null || _additiveMat == null)
		{
			return;
		}

		float radiusX = hitboxSize.X * 0.65f;
		float radiusY = hitboxSize.Y * 0.70f;
		Vector2 offset = new Vector2(_rng.RandfRange(-radiusX, radiusX), _rng.RandfRange(-radiusY, radiusY));
		float baseScale = Mathf.Clamp(hitboxSize.Y / 320f, 0.75f, 1.45f);
		float scale = _rng.RandfRange(0.28f, 0.48f) * baseScale;

		var mote = new Sprite2D
		{
			Texture = _moteTexture,
			Centered = true,
			Material = _additiveMat,
			Position = offset,
			Scale = Vector2.One * scale,
			Rotation = _rng.RandfRange(-1.2f, 1.2f),
			Modulate = new Color(1f, 0.55f, 0.2f, 0f),
			ZAsRelative = false,
			ZIndex = 12
		};
		AddChild(mote);

		Vector2 drift = new Vector2(_rng.RandfRange(-10f, 10f), _rng.RandfRange(-30f, -10f));
		Tween tween = mote.CreateTween();
		tween.TweenProperty(mote, "modulate:a", 0.9f, 0.12).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
		tween.TweenProperty(mote, "position", mote.Position + drift, 0.55).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
		tween.TweenProperty(mote, "modulate:a", 0f, 0.22);
		tween.TweenCallback(Callable.From(mote.QueueFree));
	}

	private Control? ResolveHitbox()
	{
		Node? n = this;
		while (n != null)
		{
			string? fullName = n.GetType().FullName;
			if (fullName != null && fullName.EndsWith(".NCreature", StringComparison.Ordinal))
			{
				if (n.FindChild("Hitbox", recursive: true, owned: false) is Control hb)
				{
					return hb;
				}
			}

			n = n.GetParent();
		}

		return null;
	}
}
