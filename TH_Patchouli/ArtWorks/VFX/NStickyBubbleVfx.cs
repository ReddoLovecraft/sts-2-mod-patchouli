using Godot;
using MegaCrit.Sts2.Core.Models.Powers;
using System;
using System.Reflection;

public partial class NStickyBubbleVfx : Node2D
{
	private static readonly BindingFlags _instanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	[Export]
	public int MaxBubbles { get; set; } = 30;

	[Export]
	public float OrbitRadiusMultiplier { get; set; } = 0.62f;

	[Export]
	public float WobbleScaleAmplitude { get; set; } = 0.12f;

	[Export]
	public float WobbleRotationAmplitudeRadians { get; set; } = 0.18f;

	[Export]
	public float WobbleSkewAmplitude { get; set; } = 0.10f;

	[Export]
	public float OrbitSpeed { get; set; } = 0.55f;

	public PowerModel? Power { get; set; }

	private PowerModel? _power;
	private Node2D? _bubbleRoot;
	private Texture2D? _bubbleTexture;
	private Material? _bubbleMaterial;
	private int _lastAmount = int.MinValue;
	private readonly RandomNumberGenerator _rng = new RandomNumberGenerator();
	private float _timeSeconds;
	private Vector2 _lastHitboxSize;

	public override void _Ready()
	{
		_bubbleRoot = GetNodeOrNull<Node2D>("%BubbleRoot");
		_bubbleTexture = GetNodeOrNull<Sprite2D>("%BubbleTemplate")?.Texture;
		_bubbleMaterial = GetNodeOrNull<Sprite2D>("%BubbleTemplate")?.Material;
		GetNodeOrNull<Node>("%BubbleTemplate")?.QueueFree();

		_rng.Randomize();
		Refresh(force: true);
	}

	public override void _Process(double delta)
	{
		_timeSeconds += (float)delta;
		Refresh(force: false);
		UpdateOrbitAndWobble();
	}

	private void Refresh(bool force)
	{
		if (_bubbleRoot == null || _bubbleTexture == null)
		{
			return;
		}

		_power ??= Power ?? ResolvePowerFromAncestors();
		if (_power == null)
		{
			return;
		}

		int amount = Mathf.Clamp((int)_power.Amount, 0, MaxBubbles);
		Visible = amount > 0;

		if (!force && amount == _lastAmount)
		{
			return;
		}

		_lastAmount = amount;
		RebuildBubbles(amount);
	}

	private void RebuildBubbles(int count)
	{
		if (_bubbleRoot == null || _bubbleTexture == null)
		{
			return;
		}

		foreach (Node child in _bubbleRoot.GetChildren())
		{
			child.QueueFree();
		}

		if (count <= 0)
		{
			return;
		}

		if (!TryResolveHitboxSize(out Vector2 size))
		{
			size = new Vector2(120f, 120f);
		}
		_lastHitboxSize = size;

		bool topLeftMode = GetParent() is Control;
		if (topLeftMode)
		{
			Position = Vector2.Zero;
		}
		else
		{
			Position = Vector2.Zero;
		}

		for (int i = 0; i < count; i++)
		{
			var s = new Sprite2D
			{
				Texture = _bubbleTexture,
				Centered = true
			};
			if (_bubbleMaterial != null)
			{
				s.Material = _bubbleMaterial;
			}

			float phase = _rng.RandfRange(0f, Mathf.Tau);
			s.SetMeta("phase", phase);
			_bubbleRoot.AddChild(s);
		}
	}

	private void UpdateOrbitAndWobble()
	{
		if (_bubbleRoot == null)
		{
			return;
		}

		Vector2 size = _lastHitboxSize;
		if (TryResolveHitboxSize(out Vector2 live))
		{
			size = live;
			_lastHitboxSize = live;
		}
		if (size.X <= 1f || size.Y <= 1f)
		{
			size = new Vector2(120f, 120f);
		}

		bool topLeftMode = GetParent() is Control;
		Vector2 center = topLeftMode ? size * 0.5f : Vector2.Zero;
		Vector2 half = size * 0.5f;
		float radius = Mathf.Max(half.X, half.Y) * OrbitRadiusMultiplier;

		int count = _bubbleRoot.GetChildCount();
		if (count <= 0)
		{
			return;
		}

		for (int i = 0; i < count; i++)
		{
			if (_bubbleRoot.GetChild(i) is not Sprite2D s)
			{
				continue;
			}

			float basePhase = 0f;
			try
			{
				basePhase = (float)s.GetMeta("phase");
			}
			catch
			{
			}

			float t = _timeSeconds;
			float angle = basePhase + t * OrbitSpeed + i * (Mathf.Tau / Math.Max(1, count));
			Vector2 pos = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
			s.Position = pos;

			float a = Mathf.Sin(t * 2.20f + basePhase) * WobbleScaleAmplitude;
			float b = Mathf.Sin(t * 1.70f + basePhase * 0.7f + 1.3f) * (WobbleScaleAmplitude * 0.85f);
			float r = Mathf.Sin(t * 1.20f + basePhase) * WobbleRotationAmplitudeRadians;
			float k = Mathf.Sin(t * 2.60f + basePhase * 1.1f) * WobbleSkewAmplitude;

			s.Scale = new Vector2(1f + a, 1f - b);
			s.Rotation = r;
			s.Skew = k;
		}
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
