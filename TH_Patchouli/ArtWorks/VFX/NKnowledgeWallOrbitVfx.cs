using Godot;
using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using TH_Patchouli.Scrpits.Powers;

public partial class NKnowledgeWallOrbitVfx : Node2D
{
	public int CardCount { get; set; }

	public PowerModel? Power { get; set; }

	[Export]
	public bool LoopForever { get; set; } = true;

	[Export]
	public int RootZIndex { get; set; } = 0;

	[Export]
	public float DurationSeconds { get; set; } = 1.6f;

	[Export]
	public float FadeSeconds { get; set; } = 0.35f;

	[Export]
	public float AngularSpeed { get; set; } = 2.4f;

	[Export]
	public float BackHideThreshold { get; set; } = -0.98f;

	[Export]
	public float HeightOffsetPixels { get; set; } = 135f;

	[Export]
	public float BaseRadiusX { get; set; } = 200f;

	[Export]
	public float BaseRadiusY { get; set; } = 75f;

	[Export]
	public float RingSpacing { get; set; } = 18f;

	[Export]
	public int MaxCardsPerRing { get; set; } = 60;

	private AnimatedSprite2D? _template;
	private readonly List<CardOrbitItem> _cards = new List<CardOrbitItem>();
	private double _elapsed;
	private Creature? _ownerCreature;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		ZAsRelative = false;
		ZIndex = RootZIndex;

		_template = GetNodeOrNull<AnimatedSprite2D>("CardTemplate");
		if (_template == null)
		{
			QueueFree();
			return;
		}

		_template.Visible = false;

		_ownerCreature = TryResolveOwnerCreature();
		RebuildCards(GetDesiredCardCount());
	}

	public override void _Process(double delta)
	{
		_elapsed += delta;
		float t = (float)_elapsed;

		if (NCombatRoom.Instance == null)
		{
			Visible = false;
			return;
		}

		if (Power is KnowledgeWallPower bound)
		{
			_ownerCreature = bound.Owner;
		}
		_ownerCreature ??= TryResolveOwnerCreature();

		if (_ownerCreature == null)
		{
			Visible = false;
			return;
		}

		var creatureNode = NCombatRoom.Instance.GetCreatureNode(_ownerCreature);
		if (creatureNode == null)
		{
			Visible = false;
			return;
		}
		GlobalPosition = creatureNode.VfxSpawnPosition;

		if (LoopForever)
		{
			int desired = GetDesiredCardCount();
			Visible = desired > 0;
			if (desired != _cards.Count)
			{
				RebuildCards(desired);
			}
		}

		float alpha = 1f;
		float duration = DurationSeconds <= 0f ? 1.6f : DurationSeconds;
		if (!LoopForever)
		{
			float fade = Mathf.Clamp(FadeSeconds, 0f, duration);
			if (fade > 0f && t >= duration - fade)
			{
				alpha = Mathf.Clamp((duration - t) / fade, 0f, 1f);
			}
		}

		Vector2 baseOffset = new Vector2(0f, -HeightOffsetPixels);
		float a0 = AngularSpeed * t;
		for (int i = 0; i < _cards.Count; i++)
		{
			CardOrbitItem item = _cards[i];
			float ang = a0 + item.Phase;

			float x = Mathf.Cos(ang) * item.RadiusX;
			float y = Mathf.Sin(ang) * item.RadiusY;

			item.Sprite.Position = baseOffset + new Vector2(x, y);

			float depth = Mathf.Sin(ang);
			if (depth < BackHideThreshold)
			{
				item.Sprite.Visible = false;
				continue;
			}

			item.Sprite.Visible = true;
			item.Sprite.ZIndex = -2 + (int)Mathf.Round(depth * 4f);

			Color m = item.Sprite.Modulate;
			m.A = alpha;
			item.Sprite.Modulate = m;
		}

		if (!LoopForever && t >= duration)
		{
			QueueFree();
		}
	}

	private int GetDesiredCardCount()
	{
		if (Power is KnowledgeWallPower kw && kw.Owner?.Player != null)
		{
			int mult = kw.Amount > 0 ? kw.Amount : 1;
			int handCount = kw.IsVfxLocked ? kw.LockedHandCount : PileType.Hand.GetPile(kw.Owner.Player).Cards.Count;
			return Math.Max(0, handCount * mult);
		}

		if (_ownerCreature?.Player != null)
		{
			KnowledgeWallPower? kwp = null;
			for (int i = 0; i < _ownerCreature.Powers.Count; i++)
			{
				if (_ownerCreature.Powers[i] is KnowledgeWallPower found)
				{
					kwp = found;
					break;
				}
			}

			if (kwp != null)
			{
				int mult = kwp.Amount > 0 ? kwp.Amount : 1;
				int handCount = kwp.IsVfxLocked ? kwp.LockedHandCount : PileType.Hand.GetPile(_ownerCreature.Player).Cards.Count;
				return Math.Max(0, handCount * mult);
			}
		}

		return Math.Max(0, CardCount);
	}

	private void RebuildCards(int count)
	{
		for (int i = 0; i < _cards.Count; i++)
		{
			if (GodotObject.IsInstanceValid(_cards[i].Sprite))
			{
				_cards[i].Sprite.QueueFree();
			}
		}
		_cards.Clear();

		if (_template == null)
		{
			return;
		}

		if (count <= 0)
		{
			if (!LoopForever)
			{
				QueueFree();
			}
			return;
		}

		int remaining = count;
		int ring = 0;
		while (remaining > 0)
		{
			int maxOnRing = Math.Max(1, MaxCardsPerRing);
			int onThisRing = Math.Min(remaining, maxOnRing);

			float rx = BaseRadiusX + ring * RingSpacing;
			float ry = BaseRadiusY + ring * (RingSpacing * 0.65f);

			float ringOffset = ring * 0.37f;
			for (int i = 0; i < onThisRing; i++)
			{
				float phase = ringOffset + (Mathf.Tau * i / onThisRing);

				var sprite = (AnimatedSprite2D)_template.Duplicate();
				sprite.Visible = true;
				sprite.Position = Vector2.Zero;
				sprite.ZAsRelative = false;
				AddChild(sprite);

				if (!sprite.IsPlaying())
				{
					sprite.Play("default");
				}

				_cards.Add(new CardOrbitItem(sprite, phase, rx, ry));
			}

			remaining -= onThisRing;
			ring++;
		}
	}

	private Creature? TryResolveOwnerCreature()
	{
		const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
		Node? n = this;
		while (n != null)
		{
			Type t = n.GetType();

			foreach (var p in t.GetProperties(flags))
			{
				if (!p.CanRead)
				{
					continue;
				}
				if (!typeof(Creature).IsAssignableFrom(p.PropertyType))
				{
					continue;
				}
				try
				{
					return p.GetValue(n) as Creature;
				}
				catch
				{
					break;
				}
			}

			foreach (var f in t.GetFields(flags))
			{
				if (!typeof(Creature).IsAssignableFrom(f.FieldType))
				{
					continue;
				}
				try
				{
					return f.GetValue(n) as Creature;
				}
				catch
				{
					break;
				}
			}

			n = n.GetParent();
		}
		return null;
	}

	private readonly record struct CardOrbitItem(AnimatedSprite2D Sprite, float Phase, float RadiusX, float RadiusY);
}
