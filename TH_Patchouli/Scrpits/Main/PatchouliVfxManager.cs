using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using System;

namespace TH_Patchouli.Scrpits.Main
{
	public static class PatchouliVfxManager
	{
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
	}
}
