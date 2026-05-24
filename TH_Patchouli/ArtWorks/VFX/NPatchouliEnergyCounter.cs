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
	private TextureRect? _layer2;
	private int _lastEnergy = int.MinValue;
	private int _lastMaxEnergy = int.MinValue;

	private readonly string[] _orbTexturePaths =
	[
		"res://TH_Patchouli/ArtWorks/Character/gold_card_orb.png",
		"res://TH_Patchouli/ArtWorks/Character/wood_card_orb.png",
		"res://TH_Patchouli/ArtWorks/Character/water_card_orb.png",
		"res://TH_Patchouli/ArtWorks/Character/fire_card_orb.png",
		"res://TH_Patchouli/ArtWorks/Character/dirt_card_orb.png",
		"res://TH_Patchouli/ArtWorks/Character/sun_card_orb.png",
		"res://TH_Patchouli/ArtWorks/Character/lunar_card_orb.png",
	];

	private Texture2D?[]? _orbTextures;
	private int _orbFrameIndex;
	private double _orbFrameTimer;

	[Export]
	public float OrbFramesPerSecond { get; set; } = 10f;

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
		_layer2 = GetNodeOrNull<TextureRect>("%Layers/Layer2");

		_orbTextures = new Texture2D?[_orbTexturePaths.Length];
		for (int i = 0; i < _orbTexturePaths.Length; i++)
		{
			_orbTextures[i] = ResourceLoader.Load<Texture2D>(_orbTexturePaths[i]);
		}

		RefreshLabel(force: true);
		UpdateLayer2Visibility();
		UpdateLayer2Texture(force: true);
	}

	public override void _Process(double delta)
	{
		RefreshLabel(force: false);
		UpdateLayer2Visibility();
		RotateLayers(delta);
		AdvanceOrbAnimation(delta);
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
		var children = rotationLayers.GetChildren();
		for (int i = 0; i < children.Count; i++)
		{
			if (children[i] is Control c)
			{
				c.RotationDegrees += (float)delta * speed * (i + 1);
			}
		}
	}

	private void RefreshLabel(bool force)
	{
		Player? player = _player;
		Label? label = _label;
		if (player == null || label == null)
		{
			return;
		}

		int energy = player.PlayerCombatState.Energy;
		int maxEnergy = player.PlayerCombatState.MaxEnergy;

		if (!force && energy == _lastEnergy && maxEnergy == _lastMaxEnergy)
		{
			return;
		}

		_lastEnergy = energy;
		_lastMaxEnergy = maxEnergy;
		label.Text = $"{energy}/{maxEnergy}";
	}

	private void UpdateLayer2Visibility()
	{
		if (_layer2 == null)
		{
			return;
		}

		int energy = _player?.PlayerCombatState.Energy ?? 1;
		_layer2.Visible = energy > 0;
	}

	private void AdvanceOrbAnimation(double delta)
	{
		if (_layer2 == null || _orbTextures == null || _orbTextures.Length == 0 || !_layer2.Visible)
		{
			return;
		}

		float fps = OrbFramesPerSecond;
		if (fps <= 0.01f)
		{
			return;
		}

		_orbFrameTimer += delta;
		double frameDuration = 1.0 / fps;
		if (_orbFrameTimer < frameDuration)
		{
			return;
		}

		int steps = (int)(_orbFrameTimer / frameDuration);
		_orbFrameTimer -= steps * frameDuration;
		_orbFrameIndex = (_orbFrameIndex + steps) % _orbTextures.Length;
		UpdateLayer2Texture(force: false);
	}

	private void UpdateLayer2Texture(bool force)
	{
		if (_layer2 == null || _orbTextures == null || _orbTextures.Length == 0)
		{
			return;
		}

		Texture2D? tex = _orbTextures[_orbFrameIndex];
		if (tex == null)
		{
			return;
		}

		if (force || _layer2.Texture != tex)
		{
			_layer2.Texture = tex;
		}
	}

	private Player? ResolvePlayer()
	{
		FieldInfo? field = typeof(NEnergyCounter).GetField("_player", _privateInstanceFlags);
		return field?.GetValue(this) as Player;
	}
}
