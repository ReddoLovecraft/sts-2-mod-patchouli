using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers 
{
	public sealed class UnpredictablePower : CustomPowerModel, ITransformListener
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.None;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Transform)];

		public Task AfterCardTransformed(PlayerChoiceContext choiceContext, CardModel transformedCard, CardModel resultCard, Creature player)
		{
			if (transformedCard.Owner != Owner.Player)
			{
				return Task.CompletedTask;
			}
			this.Flash();
			resultCard.EnergyCost.SetThisTurnOrUntilPlayed(0);
			return Task.CompletedTask;
		}
	}
}
