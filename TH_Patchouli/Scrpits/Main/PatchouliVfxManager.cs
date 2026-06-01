using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using System;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Main
{
	public static class PatchouliVfxManager
	{
		public static Node? GetCombatVfxContainer(bool inFront = true)
		{
			if (inFront)
			{
				return NCombatRoom.Instance?.CombatVfxContainer;
			}

			return NCombatRoom.Instance?.BackCombatVfxContainer;
		}

		public static Vector2? GetCreatureCenterPosition(Creature target)
		{
			var creatureNode = NCombatRoom.Instance?.GetCreatureNode(target);
			return creatureNode?.VfxSpawnPosition;
		}

		public static Vector2? GetCreatureHitboxCenterPosition(Creature target)
		{
			var creatureNode = NCombatRoom.Instance?.GetCreatureNode(target);
			if (creatureNode == null)
			{
				return null;
			}

			return creatureNode.Hitbox.GlobalPosition + (creatureNode.Hitbox.Size * 0.5f);
		}

		public static Vector2? GetCreatureHitboxSize(Creature target)
		{
			var creatureNode = NCombatRoom.Instance?.GetCreatureNode(target);
			return creatureNode?.Hitbox.Size;
		}

		public static Vector2? GetCreatureBasePosition(Creature target)
		{
			var creatureNode = NCombatRoom.Instance?.GetCreatureNode(target);
			if (creatureNode == null)
			{
				return null;
			}

			return creatureNode.GetBottomOfHitbox();
		}

		public static Node2D? SpawnScene(string scenePath, Node parent, Action<Node2D>? configure = null)
		{
			PackedScene? scene = ResourceLoader.Load<PackedScene>(scenePath, null, ResourceLoader.CacheMode.Reuse);
			if (scene == null)
			{
				return null;
			}

			Node2D node = scene.Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
			configure?.Invoke(node);
			parent.AddChild(node);
			return node;
		}

		public static Node2D? PlayOnCreature(Creature target, string scenePath, Vector2? positionOffset = null, Action<Node2D, Vector2>? configure = null)
		{
			return Play(target, scenePath, false, positionOffset, configure);
		}

		public static Node2D? PlayOnCreatureBase(Creature target, string scenePath, Vector2? positionOffset = null, Action<Node2D, Vector2>? configure = null)
		{
			return Play(target, scenePath, true, positionOffset, configure);
		}

		private static Node2D? Play(Creature target, string scenePath, bool spawnAtBase, Vector2? positionOffset, Action<Node2D, Vector2>? configure)
		{
			Node? container = NCombatRoom.Instance?.CombatVfxContainer;
			if (container == null)
			{
				return null;
			}

			var creatureNode = NCombatRoom.Instance?.GetCreatureNode(target);
			if (creatureNode == null)
			{
				return null;
			}

			PackedScene? scene = ResourceLoader.Load<PackedScene>(scenePath, null, ResourceLoader.CacheMode.Reuse);
			if (scene == null)
			{
				return null;
			}

			Node2D node = scene.Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
			Vector2 targetGlobalPosition = (spawnAtBase ? creatureNode.GetBottomOfHitbox() : creatureNode.VfxSpawnPosition) + (positionOffset ?? Vector2.Zero);

			if (configure != null)
			{
				configure(node, targetGlobalPosition);
			}
			else
			{
				node.GlobalPosition = targetGlobalPosition;
			}

			container.AddChild(node);
			return node;
		}

		public static bool TrySetPropertyIfExists(Node node, string propertyName, Variant value)
		{
			foreach (Godot.Collections.Dictionary dict in node.GetPropertyList())
			{
				if (!dict.TryGetValue("name", out Variant nameVar))
				{
					continue;
				}

				if (nameVar.VariantType == Variant.Type.String && ((string)nameVar) == propertyName)
				{
					node.Set(propertyName, value);
					return true;
				}
			}

			return false;
		}

		public static Vector2? GetTopSideSpawnPosition(CombatSide side, float preferredX, float topMargin = 152f)
		{
			Node? node = NCombatRoom.Instance;
			node ??= NGame.Instance;
			if (node == null)
			{
				return null;
			}

			Rect2 rect = node.GetViewport().GetVisibleRect();
			float left = rect.Position.X;
			float top = rect.Position.Y;
			float right = left + rect.Size.X;
			float centerX = left + (rect.Size.X * 0.5f);
			float padding = Mathf.Min(180f, rect.Size.X * 0.1f);
			float laneGap = Mathf.Min(120f, rect.Size.X * 0.07f);

			float minX = side == CombatSide.Player ? left + padding : centerX + laneGap;
			float maxX = side == CombatSide.Player ? centerX - laneGap : right - padding;
			float x = Mathf.Clamp(preferredX, Mathf.Min(minX, maxX), Mathf.Max(minX, maxX));
			return new Vector2(x, top + topMargin);
		}

		public static async Task WaitSeconds(float seconds)
		{
			if (seconds <= 0f)
			{
				return;
			}

			Node? node = NCombatRoom.Instance;
			node ??= NGame.Instance;
			if (node == null)
			{
				return;
			}

			SceneTreeTimer timer = node.GetTree().CreateTimer(seconds);
			await node.ToSignal(timer, SceneTreeTimer.SignalName.Timeout);
		}
	}
}
