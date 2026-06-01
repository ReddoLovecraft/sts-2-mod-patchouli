using Godot;
using System;

public partial class NPhotosynthesisVfx : Node2D
{
	[Export]
	public Vector2 OffsetFromAnchor { get; set; } = Vector2.Zero;

	[Export]
	public float BaseAlpha { get; set; } = 0.55f;

	[Export]
	public Color LightColor { get; set; } = Colors.White;

	private Sprite2D? _spotlight;
	private Tween? _pulseTween;
	private Control? _hitbox;

	public override void _Ready()
	{
		ZAsRelative = false;
		ZIndex = 70;

		_spotlight = GetNodeOrNull<Sprite2D>("%Spotlight");
		_hitbox = ResolveHitbox();
		UpdateLayout();

		if (_spotlight != null)
		{
			Color m = LightColor;
			m.A = BaseAlpha;
			_spotlight.Modulate = m;
			StartPulseTween();
		}

		Visible = true;
	}

	public override void _Process(double delta)
	{
		UpdateLayout();
	}

	private void UpdateLayout()
	{
		_hitbox ??= ResolveHitbox();
		Control? hitbox = _hitbox;
		if (hitbox == null || _spotlight == null || _spotlight.Texture == null)
		{
			return;
		}

		Position = Vector2.Zero;

		Vector2 tex = _spotlight.Texture.GetSize();
		float sx = (hitbox.Size.X * 1.06f) / Mathf.Max(1f, tex.X);
		float sy = (hitbox.Size.Y * 1.06f) / Mathf.Max(1f, tex.Y);
		float lightScale = Mathf.Max(sx, sy);

		float scaledLightHeight = tex.Y * lightScale;
		float desiredBottomY = hitbox.Size.Y * 0.60f;
		float lightPosY = desiredBottomY - scaledLightHeight * 0.5f;
		lightPosY = Mathf.Clamp(lightPosY, -hitbox.Size.Y * 0.85f, -hitbox.Size.Y * 0.05f);

		_spotlight.Scale = Vector2.One * lightScale;
		_spotlight.Position = new Vector2(0f, lightPosY) + OffsetFromAnchor;
		_spotlight.ZAsRelative = false;
		_spotlight.ZIndex = 0;
	}

	private void StartPulseTween()
	{
		if (_spotlight == null)
		{
			return;
		}

		_pulseTween?.Kill();

		float a0 = Mathf.Clamp(_spotlight.Modulate.A, 0f, 1f);
		_pulseTween = _spotlight.CreateTween().SetLoops();
		_pulseTween.TweenProperty(_spotlight, "modulate:a", a0 * 0.72f, 0.85).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
		_pulseTween.TweenProperty(_spotlight, "modulate:a", a0, 0.85).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
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
