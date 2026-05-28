using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using System.Reflection;

public partial class NPatchouliEnergyCounter : NEnergyCounter
{
	private static readonly BindingFlags _privateInstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;

	private Player? _player;
	private Label? _label;
	private Control? _rotationLayers;
	private int _lastEnergy = int.MinValue;
	private int _lastMaxEnergy = int.MinValue;

	private Node2D? _vfxBack;
	private Node2D? _vfxFront;
	private float _vfxBackBaseRotation;
	private float _vfxFrontBaseRotation;
	private Color _vfxBackBaseModulate;
	private Color _vfxFrontBaseModulate;
	private float _vfxRemainingSeconds;
	private float _vfxTotalSeconds;

	[Export]
	public float VfxBackRotationSpeed { get; set; } = 25f;

	[Export]
	public float VfxFrontRotationSpeed { get; set; } = -18f;

	[Export]
	public float VfxDurationSeconds { get; set; } = 2.0f;

	[Export]
	public float VfxFadeOutSeconds { get; set; } = 1.0f;

	public override void _EnterTree()
	{
	}

	public override void _ExitTree()
	{
	}

	public override void _Ready()
	{
		_player = ResolvePlayer();
		_label = GetNodeOrNull<Label>("Label");
		_rotationLayers = GetNodeOrNull<Control>("%RotationLayers");
		_vfxBack = GetNodeOrNull<Node2D>("%EnergyVfxBack");
		_vfxFront = GetNodeOrNull<Node2D>("%EnergyVfxFront");

		if (_vfxBack != null)
		{
			_vfxBackBaseRotation = _vfxBack.Rotation;
			_vfxBackBaseModulate = _vfxBack.Modulate;
			_vfxBack.Visible = false;
		}

		if (_vfxFront != null)
		{
			_vfxFrontBaseRotation = _vfxFront.Rotation;
			_vfxFrontBaseModulate = _vfxFront.Modulate;
			_vfxFront.Visible = false;
		}

		RefreshVisualState(force: true);
	}

	public override void _Process(double delta)
	{
		_player ??= ResolvePlayer();
		RefreshVisualState(force: false);
		RotateLayers(delta);
		UpdateVfx(delta);
	}

	private void RotateLayers(double delta)
	{
		Player? player = _player;
		Control? rotationLayers = _rotationLayers;
		if (player == null || rotationLayers == null)
		{
			return;
		}

		float speed = (player.PlayerCombatState.Energy == 0) ? 5f : 30f;
		for (int i = 0; i < rotationLayers.GetChildCount(); i++)
		{
			if (rotationLayers.GetChild(i) is Control c)
			{
				c.RotationDegrees += (float)delta * speed * (i + 1);
			}
		}
	}

	private void UpdateVfx(double delta)
	{
		if (_vfxRemainingSeconds <= 0f)
		{
			return;
		}

		_vfxRemainingSeconds -= (float)delta;
		UpdateVfxOpacity();

		if (_vfxBack != null)
		{
			_vfxBack.RotationDegrees += (float)delta * VfxBackRotationSpeed;
		}

		if (_vfxFront != null)
		{
			_vfxFront.RotationDegrees += (float)delta * VfxFrontRotationSpeed;
		}

		if (_vfxRemainingSeconds <= 0f)
		{
			StopVfx();
		}
	}

	private void UpdateVfxOpacity()
	{
		float total = _vfxTotalSeconds;
		if (total <= 0.01f)
		{
			return;
		}

		float fadeSeconds = Mathf.Clamp(VfxFadeOutSeconds, 0f, total);
		float alphaMultiplier;
		if (fadeSeconds <= 0.01f)
		{
			alphaMultiplier = 1f;
		}
		else
		{
			alphaMultiplier = (_vfxRemainingSeconds >= fadeSeconds)
				? 1f
				: Mathf.Clamp(_vfxRemainingSeconds / fadeSeconds, 0f, 1f);
		}

		if (_vfxBack != null)
		{
			Color c = _vfxBackBaseModulate;
			c.A *= alphaMultiplier;
			_vfxBack.Modulate = c;
		}

		if (_vfxFront != null)
		{
			Color c = _vfxFrontBaseModulate;
			c.A *= alphaMultiplier;
			_vfxFront.Modulate = c;
		}
	}

	private void StartVfx()
	{
		float duration = VfxDurationSeconds;
		if (duration <= 0.01f)
		{
			return;
		}

		_vfxRemainingSeconds = duration;
		_vfxTotalSeconds = duration;

		if (_vfxBack != null)
		{
			_vfxBack.Rotation = _vfxBackBaseRotation;
			_vfxBack.Modulate = _vfxBackBaseModulate;
			_vfxBack.Visible = true;
		}

		if (_vfxFront != null)
		{
			_vfxFront.Rotation = _vfxFrontBaseRotation;
			_vfxFront.Modulate = _vfxFrontBaseModulate;
			_vfxFront.Visible = true;
		}

		UpdateVfxOpacity();
	}

	private void StopVfx()
	{
		_vfxRemainingSeconds = 0f;
		_vfxTotalSeconds = 0f;

		if (_vfxBack != null)
		{
			_vfxBack.Visible = false;
			_vfxBack.Rotation = _vfxBackBaseRotation;
			_vfxBack.Modulate = _vfxBackBaseModulate;
		}

		if (_vfxFront != null)
		{
			_vfxFront.Visible = false;
			_vfxFront.Rotation = _vfxFrontBaseRotation;
			_vfxFront.Modulate = _vfxFrontBaseModulate;
		}
	}

	private void RefreshVisualState(bool force)
	{
		Player? player = _player;
		Label? label = _label;
		if (player == null || label == null)
		{
			return;
		}

		int energy;
		int maxEnergy;
		try
		{
			energy = player.PlayerCombatState.Energy;
			maxEnergy = player.PlayerCombatState.MaxEnergy;
		}
		catch
		{
			return;
		}

		int oldEnergy = _lastEnergy;

		if (!force && energy == oldEnergy && maxEnergy == _lastMaxEnergy)
		{
			return;
		}

		_lastEnergy = energy;
		_lastMaxEnergy = maxEnergy;
		label.Text = $"{energy}/{maxEnergy}";

		if (oldEnergy != int.MinValue && energy > oldEnergy)
		{
			StartVfx();
		}
	}

	private Player? ResolvePlayer()
	{
		FieldInfo? field = typeof(NEnergyCounter).GetField("_player", _privateInstanceFlags);
		return field?.GetValue(this) as Player;
	}
}
