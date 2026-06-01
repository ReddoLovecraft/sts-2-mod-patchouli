using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using Godot;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scrpits.Main;
using TH_Patchouli.Scrpits.Powers;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class ElementManagerPower : CustomPowerModel
	{
		private const string ElementManagerProjectileScenePath = "res://TH_Patchouli/ArtWorks/VFX/element_manager_projectile.tscn";

		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("Element")];
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/EMP432.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/EMP464.png";

		public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
		{
			if (CombatState == null || power.Owner != Owner)
			{
				return;
			}
			if (power is not (GoldElement or WoodElement or WaterElement or FireElement or DirtElement or SunElement or LunarElement))
			{
				return;
			}

			TryPlayVfx(power);
			this.Flash();
			await CreatureCmd.Damage(new BlockingPlayerChoiceContext(), CombatState.HittableEnemies, Amount, ValueProp.Unpowered | ValueProp.Move, Owner);
		}

		private void TryPlayVfx(PowerModel power)
		{
			string? elementScenePath = GetElementScenePath(power);
			Vector2? source = PatchouliVfxManager.GetCreatureCenterPosition(Owner);
			Node? container = PatchouliVfxManager.GetCombatVfxContainer();
			if (CombatState == null || string.IsNullOrWhiteSpace(elementScenePath) || !source.HasValue || container == null)
			{
				return;
			}

			foreach (Creature enemy in CombatState.HittableEnemies)
			{
				Vector2? target = PatchouliVfxManager.GetCreatureCenterPosition(enemy);
				if (!target.HasValue)
				{
					continue;
				}

				PatchouliVfxManager.SpawnScene(ElementManagerProjectileScenePath, container, node =>
				{
					if (node is NElementManagerProjectileVfx vfx)
					{
						vfx.ElementScenePath = elementScenePath;
						vfx.StartGlobalPosition = source.Value;
						vfx.TargetGlobalPosition = target.Value;
					}
				});
			}
		}

		private static string? GetElementScenePath(PowerModel power)
		{
			return power switch
			{
				GoldElement e => e.TscnPath,
				WoodElement e => e.TscnPath,
				WaterElement e => e.TscnPath,
				FireElement e => e.TscnPath,
				DirtElement e => e.TscnPath,
				SunElement e => e.TscnPath,
				LunarElement e => e.TscnPath,
				_ => null,
			};
		}
	}
}
