using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class EverythingCirclePower : CustomPowerModel, ITransformListener
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Transform)];

		public async Task AfterCardTransformed(PlayerChoiceContext choiceContext, CardModel transformedCard, CardModel resultCard, Creature player)
		{
			if (player != Owner)
			{
				return;
			}

			int draw = Amount;
			if (draw <= 0)
			{
				return;
			}

			this.Flash();
			await CardPileCmd.Draw(choiceContext, draw, Owner.Player);
		}
	}
}
