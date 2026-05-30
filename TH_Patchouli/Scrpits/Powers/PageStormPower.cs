using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class PageStormPower : CustomPowerModel
	{
		private const string PageStormBulletScenePath = "res://TH_Patchouli/ArtWorks/VFX/page_storm_bullet.tscn";
		private static readonly BindingFlags _instanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/PSP32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/PSP64.png";

		public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
		{
			if (CombatState == null)
			{
				return;
			}
			TryPlayVfx();
			Flash();
			await CreatureCmd.Damage(new BlockingPlayerChoiceContext(), CombatState.HittableEnemies, Amount, ValueProp.Unpowered, Owner,null);
		}

		private void TryPlayVfx()
		{
			if (Owner == null || NGame.Instance == null)
			{
				return;
			}

			Control? hitbox = FindOwnerHitbox(Owner);
			if (hitbox == null)
			{
				return;
			}

			PackedScene? scene = ResourceLoader.Load<PackedScene>(PageStormBulletScenePath, null, ResourceLoader.CacheMode.Reuse);
			if (scene == null)
			{
				return;
			}

			Node instance = scene.Instantiate();
			if (instance is Node2D node2D)
			{
				node2D.GlobalPosition = hitbox.GlobalPosition + hitbox.Size * 0.5f;
			}
			if (instance is NPageStormBulletVfx vfx)
			{
				vfx.Direction = (Owner.Side == CombatSide.Player) ? 1 : -1;
			}

			NGame.Instance.AddChild(instance);
		}

		private static Control? FindOwnerHitbox(Creature owner)
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

				Type t = n.GetType();
				string? fullName = t.FullName;
				if (fullName != null && fullName.EndsWith(".NCreature", StringComparison.Ordinal))
				{
					Creature? model = TryExtractCreatureModel(n);
					if (ReferenceEquals(model, owner))
					{
						if (n.FindChild("Hitbox", recursive: true, owned: false) is Control c)
						{
							return c;
						}
					}
				}

				foreach (Node child in n.GetChildren())
				{
					stack.Push(child);
				}
			}

			return null;
		}

		private static Creature? TryExtractCreatureModel(object nCreature)
		{
			Type t = nCreature.GetType();

			foreach (FieldInfo f in t.GetFields(_instanceFlags))
			{
				if (typeof(Creature).IsAssignableFrom(f.FieldType))
				{
					return f.GetValue(nCreature) as Creature;
				}
			}

			foreach (PropertyInfo p in t.GetProperties(_instanceFlags))
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
	}
}
