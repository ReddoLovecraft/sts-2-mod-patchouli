using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchoulib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;
using TH_Patchouli.Scrpits.Powers;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class ElementPeriodPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("Element")];
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/EPP32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/EPP64.png";



		public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
		{
			if (power.Owner != Owner || amount >= 0m)
			{
				return;
			}

			int dec = (int)Math.Abs(amount);
			if (dec <= 0)
			{
				return;
			}

			int gainEach = Math.Max(0, Amount);
			if (gainEach <= 0)
			{
				return;
			}

			ElementEnum? next = power switch
			{
				GoldElement => ElementEnum.Wood,
				WoodElement => ElementEnum.Water,
				WaterElement => ElementEnum.Fire,
				FireElement => ElementEnum.Dirt,
				DirtElement => ElementEnum.Sun,
				SunElement => ElementEnum.Lunar,
				LunarElement => ElementEnum.Gold,
				_ => null,
			};
			if (next == null)
			{
				return;
			}

			await ToolBox.GainElement([next.Value], gainEach * dec, Owner);
		}
	}
}
