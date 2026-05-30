using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchoulib.Scrpits.Main;
using System;
using System.Collections.Generic;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Powers;

namespace TH_Patchouli.Scrpits.Powers 
{
	public sealed class ElementMagicianPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.None;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("Element")];
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/EMP332.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/EMP364.png";



		public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
		{
			if (originalCost < 0m || Owner?.Player == null || card.Owner != Owner.Player)
			{
				modifiedCost = originalCost;
				return false;
			}
			if (card is not TH_Patchouli.Scripts.Main.PatchouliCardModel pcm)
			{
				modifiedCost = originalCost;
				return false;
			}

			int reduction = 0;
			foreach (ElementEnum e in pcm.ElementTypes)
			{
				reduction += e switch
				{
					ElementEnum.Gold => Owner.GetPower<GoldElement>()?.Amount ?? 0,
					ElementEnum.Wood => Owner.GetPower<WoodElement>()?.Amount ?? 0,
					ElementEnum.Water => Owner.GetPower<WaterElement>()?.Amount ?? 0,
					ElementEnum.Fire => Owner.GetPower<FireElement>()?.Amount ?? 0,
					ElementEnum.Dirt => Owner.GetPower<DirtElement>()?.Amount ?? 0,
					ElementEnum.Sun => Owner.GetPower<SunElement>()?.Amount ?? 0,
					ElementEnum.Lunar => Owner.GetPower<LunarElement>()?.Amount ?? 0,
					_ => 0
				};
			}

			modifiedCost = originalCost - Math.Max(0, reduction);
			return (int)modifiedCost != (int)originalCost;
		}
	}
}
