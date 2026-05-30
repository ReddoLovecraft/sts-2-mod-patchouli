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
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/UP232.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/UP264.png";
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
