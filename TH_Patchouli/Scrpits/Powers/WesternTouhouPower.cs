using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class WesternTouhouPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/WTP32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/WTP64.png";

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DexterityPower>(),HoverTipFactory.FromPower<StrengthPower>()];

		public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner.Creature == base.Owner)
		{
		   if(cardPlay.Card.Type==CardType.Attack)
		   {
			this.Flash();
			await PowerCmd.Apply<SpeedPotionPower>(context, base.Owner, Amount, base.Owner, null);
		   }
		   if(cardPlay.Card.Type==CardType.Skill)
		   {
			this.Flash();
			await PowerCmd.Apply<FlexPotionPower>(context, base.Owner, Amount, base.Owner, null);
		   }
		}
	}
	}
}

