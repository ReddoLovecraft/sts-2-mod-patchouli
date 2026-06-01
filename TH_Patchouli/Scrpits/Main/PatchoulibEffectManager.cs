using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using TH_Patchouli.Scrpits.Powers;

namespace TH_Patchouli.Scrpits.Main
{
	public static partial class PatchoulibEffectManager
	{
		private static readonly IEqualityComparer<PowerModel> _powerComparer = new ReferenceEqualityComparer<PowerModel>();
		private static readonly Dictionary<PowerModel, Node> _vfxByPower = new Dictionary<PowerModel, Node>(_powerComparer);
		private static readonly Dictionary<PowerModel, PendingAttach> _pendingByPower = new Dictionary<PowerModel, PendingAttach>(_powerComparer);
		private static readonly Dictionary<Creature, Node> _bubbleShieldByOwner = new Dictionary<Creature, Node>(new ReferenceEqualityComparer<Creature>());
		private static readonly Dictionary<Creature, Node> _lavaCromlechByOwner = new Dictionary<Creature, Node>(new ReferenceEqualityComparer<Creature>());
		private static RunnerNode? _runner;

		public static void OnPowerApplied(Creature owner, PowerModel power)
		{
			if (power is BubbleShield)
			{
				if (_bubbleShieldByOwner.TryGetValue(owner, out Node? bubble) && bubble != null && GodotObject.IsInstanceValid(bubble))
				{
					return;
				}

				foreach (PendingAttach pending in _pendingByPower.Values)
				{
					if (ReferenceEquals(pending.Owner, owner) && string.Equals(pending.ScenePath, "res://TH_Patchouli/ArtWorks/VFX/bubble_shield.tscn", StringComparison.Ordinal))
					{
						return;
					}
				}
			}

			if (power is LavaCromlechPower)
			{
				if (_lavaCromlechByOwner.TryGetValue(owner, out Node? existingLava) && existingLava != null && GodotObject.IsInstanceValid(existingLava))
				{
					return;
				}

				foreach (PendingAttach pending in _pendingByPower.Values)
				{
					if (ReferenceEquals(pending.Owner, owner) && string.Equals(pending.ScenePath, "res://TH_Patchouli/ArtWorks/VFX/lava_cromlech_orbit.tscn", StringComparison.Ordinal))
					{
						return;
					}
				}
			}

			string? scenePath = GetScenePath(power);
			if (scenePath == null)
			{
				return;
			}

			if (_vfxByPower.TryGetValue(power, out Node? existing) && existing != null && GodotObject.IsInstanceValid(existing))
			{
				return;
			}

			bool useCombatVfxContainer = power is CloudFormPower || power is KnowledgeWallPower || power is LavaCromlechPower;
			bool combatInFront = power is not LavaCromlechPower;
			Node? parent = useCombatVfxContainer ? TryGetCombatVfxContainer(combatInFront) : FindHitboxAnchorFor(owner);
			if (parent == null)
			{
				_pendingByPower[power] = new PendingAttach(owner, scenePath, AttemptsLeft: 240, UseCombatVfxContainer: useCombatVfxContainer, CombatInFront: combatInFront);
				EnsureRunner();
				return;
			}

			Node? instance = InstantiateAndAttach(parent, owner, power, scenePath);
			if (instance == null)
			{
				return;
			}

			if (power is LavaCromlechPower)
			{
				_lavaCromlechByOwner[owner] = instance;
				return;
			}

			_vfxByPower[power] = instance;

			if (power is BubbleShield)
			{
				_bubbleShieldByOwner[owner] = instance;
			}
		}

		public static void OnPowerRemoved(Creature owner, PowerModel power)
		{
			if (power is BubbleShield)
			{
				if (owner.HasPower<BubbleShield>())
				{
					return;
				}

				if (_bubbleShieldByOwner.TryGetValue(owner, out Node? bubble) && bubble != null)
				{
					_bubbleShieldByOwner.Remove(owner);
					if (GodotObject.IsInstanceValid(bubble))
					{
						bubble.QueueFree();
					}
				}
			}

			if (power is LavaCromlechPower)
			{
				if (owner.HasPower<LavaCromlechPower>())
				{
					return;
				}

				if (_lavaCromlechByOwner.TryGetValue(owner, out Node? lava) && lava != null)
				{
					_lavaCromlechByOwner.Remove(owner);
					if (GodotObject.IsInstanceValid(lava))
					{
						lava.QueueFree();
					}
				}
			}

			_pendingByPower.Remove(power);
			if (_vfxByPower.TryGetValue(power, out Node? node) && node != null)
			{
				_vfxByPower.Remove(power);
				if (GodotObject.IsInstanceValid(node))
				{
					node.QueueFree();
				}
			}
		}

		private static void EnsureRunner()
		{
			if (_runner != null && GodotObject.IsInstanceValid(_runner))
			{
				return;
			}

			if (NGame.Instance == null)
			{
				return;
			}

			_runner = new RunnerNode();
			NGame.Instance.AddChild(_runner);
		}

		private static void Tick()
		{
			if (_pendingByPower.Count == 0)
			{
				return;
			}

			List<PowerModel> done = new List<PowerModel>();
			foreach (KeyValuePair<PowerModel, PendingAttach> kv in _pendingByPower)
			{
				PendingAttach p = kv.Value;
				if (p.AttemptsLeft <= 0)
				{
					done.Add(kv.Key);
					continue;
				}

				Node? parent = p.UseCombatVfxContainer ? TryGetCombatVfxContainer(p.CombatInFront) : FindHitboxAnchorFor(p.Owner);
				if (parent == null)
				{
					_pendingByPower[kv.Key] = p with { AttemptsLeft = p.AttemptsLeft - 1 };
					continue;
				}

				Node? instance = InstantiateAndAttach(parent, p.Owner, kv.Key, p.ScenePath);
				if (instance == null)
				{
					done.Add(kv.Key);
					continue;
				}

				_vfxByPower[kv.Key] = instance;
				done.Add(kv.Key);
			}

			foreach (PowerModel k in done)
			{
				_pendingByPower.Remove(k);
			}
		}

		private static Node? TryGetCombatVfxContainer(bool inFront)
		{
			return inFront ? NCombatRoom.Instance?.CombatVfxContainer : NCombatRoom.Instance?.BackCombatVfxContainer;
		}

		private static Node? InstantiateAndAttach(Node parent, Creature owner, PowerModel power, string scenePath)
		{
			PackedScene? scene = ResourceLoader.Load<PackedScene>(scenePath, null, ResourceLoader.CacheMode.Reuse);
			if (scene == null)
			{
				return null;
			}

			Node instance = scene.Instantiate();
			TryBindPower(instance, power);

			if (instance is Node2D n2d && NCombatRoom.Instance != null)
			{
				var creatureNode = NCombatRoom.Instance.GetCreatureNode(owner);
				if (creatureNode != null)
				{
					if (power is CloudFormPower or KnowledgeWallPower or LavaCromlechPower)
					{
						n2d.GlobalPosition = creatureNode.Hitbox.GlobalPosition + new Vector2(creatureNode.Hitbox.Size.X/2f, creatureNode.Hitbox.Size.Y);
					}
					else
					{
						n2d.GlobalPosition = creatureNode.VfxSpawnPosition;
					}
				}
			}

			parent.AddChild(instance);
			return instance;
		}

		private static void TryBindPower(Node instance, PowerModel power)
		{
			const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			Type t = instance.GetType();
			PropertyInfo? prop = t.GetProperty("Power", flags);
			if (prop?.CanWrite == true && typeof(PowerModel).IsAssignableFrom(prop.PropertyType))
			{
				try
				{
					prop.SetValue(instance, power);
				}
				catch
				{
				}
			}
		}

		private static string? GetScenePath(PowerModel power)
		{
			if (power is IgniteMark)
			{
				return "res://TH_Patchouli/ArtWorks/VFX/ignite_mark.tscn";
			}
			if (power is CloudFormPower)
			{
				return "res://TH_Patchouli/ArtWorks/VFX/cloud_form_weather.tscn";
			}
			if (power is KnowledgeWallPower)
			{
				return "res://TH_Patchouli/ArtWorks/VFX/knowledge_wall_orbit.tscn";
			}
			if (power is LavaCromlechPower)
			{
				return "res://TH_Patchouli/ArtWorks/VFX/lava_cromlech_orbit.tscn";
			}
			if (power is BubbleShield)
			{
				return "res://TH_Patchouli/ArtWorks/VFX/bubble_shield.tscn";
			}
			if (power is StickyBubble)
			{
				return "res://TH_Patchouli/ArtWorks/VFX/sticky_bubble.tscn";
			}
			if (power is PhotosynthesisPower)
			{
				return "res://TH_Patchouli/ArtWorks/VFX/photosynthesis_light.tscn";
			}

			return null;
		}

		private static Node? FindHitboxAnchorFor(Creature owner)
		{
			if (NGame.Instance == null)
			{
				return null;
			}

			Node root = NGame.Instance.GetTree().Root;
			var stack = new Stack<Node>();
			stack.Push(root);

			while (stack.Count > 0)
			{
				Node n = stack.Pop();

				if (IsCreatureNodeFor(n, owner, out Node? anchor))
				{
					return anchor ?? n;
				}

				foreach (Node child in n.GetChildren())
				{
					stack.Push(child);
				}
			}

			return null;
		}

		private static bool IsCreatureNodeFor(Node node, Creature owner, out Node? anchor)
		{
			anchor = null;

			Type t = node.GetType();
			string? fullName = t.FullName;
			if (fullName == null || !fullName.EndsWith(".NCreature", StringComparison.Ordinal))
			{
				return false;
			}

			Creature? model = TryExtractCreatureModel(node);
			if (!ReferenceEquals(model, owner))
			{
				return false;
			}

			anchor = node.FindChild("CenterPos", recursive: true, owned: false);
			if (anchor == null)
			{
				anchor = node.FindChild("Hitbox", recursive: true, owned: false);
			}
			return true;
		}

		private static Creature? TryExtractCreatureModel(object nCreature)
		{
			const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			Type t = nCreature.GetType();

			foreach (FieldInfo f in t.GetFields(flags))
			{
				if (typeof(Creature).IsAssignableFrom(f.FieldType))
				{
					return f.GetValue(nCreature) as Creature;
				}
			}

			foreach (PropertyInfo p in t.GetProperties(flags))
			{
				if (!p.CanRead || !typeof(Creature).IsAssignableFrom(p.PropertyType))
				{
					continue;
				}

				try
				{
					return p.GetValue(nCreature) as Creature;
				}
				catch
				{
					return null;
				}
			}

			return null;
		}

		private sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
		{
			public bool Equals(T? x, T? y) => ReferenceEquals(x, y);
			public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
		}

		private readonly record struct PendingAttach(Creature Owner, string ScenePath, int AttemptsLeft, bool UseCombatVfxContainer, bool CombatInFront);

		private sealed partial class RunnerNode : Node
		{
			public override void _Process(double delta)
			{
				Tick();
			}
		}
	}
}
