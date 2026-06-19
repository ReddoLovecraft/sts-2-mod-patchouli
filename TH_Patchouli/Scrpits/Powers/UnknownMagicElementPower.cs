using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using System;
using System.Threading.Tasks;
using TH_Patchouli.Scrpits.Main;
using TH_Patchouli.Scrpits.Powers;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class UnknownMagicElementPower : CustomPowerModel
	{
		private const float ElementRingAlpha = 0.55f;

		private const string GoldRingScenePath = "res://TH_Patchouli/ArtWorks/VFX/elementring/gold_ring.tscn";
		private const string WoodRingScenePath = "res://TH_Patchouli/ArtWorks/VFX/elementring/wood_ring.tscn";
		private const string WaterRingScenePath = "res://TH_Patchouli/ArtWorks/VFX/elementring/water_ring.tscn";
		private const string FireRingScenePath = "res://TH_Patchouli/ArtWorks/VFX/elementring/fire_ring.tscn";
		private const string DirtRingScenePath = "res://TH_Patchouli/ArtWorks/VFX/elementring/dirt_ring.tscn";
		private const string SunRingScenePath = "res://TH_Patchouli/ArtWorks/VFX/elementring/sun_ring.tscn";
		private const string LunarRingScenePath = "res://TH_Patchouli/ArtWorks/VFX/elementring/lunar_ring.tscn";

		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/UMEP32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/UMEP64.png";

		public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
		{
			if (power.Owner != Owner || amount >= 0m)
			{
				return;
			}
			if (power is not (GoldElement or WoodElement or WaterElement or FireElement or DirtElement or SunElement or LunarElement))
			{
				return;
			}

			TryPlayElementRing(power);

			int dec = (int)Math.Abs(amount);
			if (dec <= 0)
			{
				return;
			}

			int gain = Math.Max(0, Amount) * dec;
			if (gain <= 0 || Owner.Player == null)
			{
				return;
			}
			await PlayerCmd.GainEnergy(gain, Owner.Player);
		}

		private void TryPlayElementRing(PowerModel element)
		{
			string? scenePath = GetElementRingScenePath(element);
			if (string.IsNullOrWhiteSpace(scenePath))
			{
				return;
			}

			PatchouliVfxManager.PlayOnCreature(Owner, scenePath, configure: (node, targetPos) =>
			{
				node.GlobalPosition = targetPos;
				if (node is NElementRingVfx vfx)
				{
					vfx.Play(0.5f);
				}

				Sprite2D? back = node.GetNodeOrNull<Sprite2D>("%Back");
				if (back != null)
				{
					Color c = back.Modulate;
					c.A = ElementRingAlpha;
					back.Modulate = c;
				}

				Sprite2D? main = node.GetNodeOrNull<Sprite2D>("%Main");
				if (main != null)
				{
					Color c = main.Modulate;
					c.A = ElementRingAlpha;
					main.Modulate = c;
				}
			});
		}

		private static string? GetElementRingScenePath(PowerModel element)
		{
			return element switch
			{
				GoldElement => GoldRingScenePath,
				WoodElement => WoodRingScenePath,
				WaterElement => WaterRingScenePath,
				FireElement => FireRingScenePath,
				DirtElement => DirtRingScenePath,
				SunElement => SunRingScenePath,
				LunarElement => LunarRingScenePath,
				_ => null,
			};
		}
	}
}

