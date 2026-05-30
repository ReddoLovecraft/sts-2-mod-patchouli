using Godot;
using MegaCrit.Sts2.Core.Models.Powers;
using System;
using System.Reflection;

public partial class NBubbleShieldVfx : Node2D
{
	private static readonly BindingFlags _instanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	[Export]
	public float WrapPaddingMultiplier { get; set; } = 1.08f;

	public PowerModel? Power { get; set; }

	private PowerModel? _power;
	private Sprite2D? _sprite;
	private float _timeSeconds;
	private Vector2 _baseScale;
	private bool _baseScaleInitialized;

	public override void _Ready()
	{
		_sprite = GetNodeOrNull<Sprite2D>("%BubbleSprite");
		Refresh(force: true);
	}

	public override void _Process(double delta)
	{
		_timeSeconds += (float)delta;
		Refresh(force: false);
	}

	private void Refresh(bool force)
	{
		if (_sprite == null)
		{
			return;
		}

		_power ??= Power ?? ResolvePowerFromAncestors();
		if (_power == null)
		{
			return;
		}

		Visible = _power.Amount > 0m;
		if (!Visible)
		{
			return;
		}

		if (!TryResolveHitboxSize(out Vector2 size))
		{
			return;
		}

		Vector2 tex = _sprite.Texture?.GetSize() ?? Vector2.One;
		if (tex.X <= 0.01f || tex.Y <= 0.01f)
		{
			return;
		}

		float sx = (size.X / tex.X) * WrapPaddingMultiplier;
		float sy = (size.Y / tex.Y) * WrapPaddingMultiplier;
		float uniform = Mathf.Max(sx, sy);
		_baseScale = new Vector2(uniform, uniform);
		_baseScaleInitialized = true;
		ApplyWobble();

		if (GetParent() is Control)
		{
			Position = size * 0.5f;
		}
		else
		{
			Position = Vector2.Zero;
		}
	}

	private void ApplyWobble()
	{
		if (_sprite == null || !_baseScaleInitialized)
		{
			return;
		}

		float t = _timeSeconds;
		float a = Mathf.Sin(t * 0.85f) * 0.02f;
		float b = Mathf.Sin(t * 1.35f + 1.2f) * 0.017f;
		float r = Mathf.Sin(t * 0.55f + 0.3f) * 0.03f;
		float k = Mathf.Sin(t * 1.10f) * 0.02f;

		_sprite.Scale = new Vector2(_baseScale.X * (1f + a), _baseScale.Y * (1f - b));
		_sprite.Rotation = r;
		_sprite.Skew = k;
	}

	private bool TryResolveHitboxSize(out Vector2 size)
	{
		Node? n = this;
		while (n != null)
		{
			if (n is Control c && string.Equals(c.Name, "Hitbox", StringComparison.OrdinalIgnoreCase))
			{
				size = c.Size;
				if (size.X > 0.01f && size.Y > 0.01f)
				{
					return true;
				}
			}

			n = n.GetParent();
		}

		size = default;
		return false;
	}

	private PowerModel? ResolvePowerFromAncestors()
	{
		Node? n = this;
		while (n != null)
		{
			PowerModel? byName = TryGetPowerByCommonNames(n);
			if (byName != null)
			{
				return byName;
			}

			PowerModel? byScan = TryScanForPowerModel(n);
			if (byScan != null)
			{
				return byScan;
			}

			n = n.GetParent();
		}

		return null;
	}

	private static PowerModel? TryGetPowerByCommonNames(object obj)
	{
		Type t = obj.GetType();

		FieldInfo? field = t.GetField("_power", _instanceFlags) ?? t.GetField("Power", _instanceFlags);
		if (field != null && typeof(PowerModel).IsAssignableFrom(field.FieldType))
		{
			return field.GetValue(obj) as PowerModel;
		}

		PropertyInfo? prop = t.GetProperty("Power", _instanceFlags) ?? t.GetProperty("power", _instanceFlags);
		if (prop != null && typeof(PowerModel).IsAssignableFrom(prop.PropertyType))
		{
			try
			{
				return prop.GetValue(obj) as PowerModel;
			}
			catch
			{
				return null;
			}
		}

		return null;
	}

	private static PowerModel? TryScanForPowerModel(object obj)
	{
		Type t = obj.GetType();

		foreach (FieldInfo f in t.GetFields(_instanceFlags))
		{
			if (!typeof(PowerModel).IsAssignableFrom(f.FieldType))
			{
				continue;
			}

			PowerModel? value = f.GetValue(obj) as PowerModel;
			if (value != null)
			{
				return value;
			}
		}

		foreach (PropertyInfo p in t.GetProperties(_instanceFlags))
		{
			if (!p.CanRead || !typeof(PowerModel).IsAssignableFrom(p.PropertyType))
			{
				continue;
			}

			try
			{
				PowerModel? value = p.GetValue(obj) as PowerModel;
				if (value != null)
				{
					return value;
				}
			}
			catch
			{
			}
		}

		return null;
	}
}
