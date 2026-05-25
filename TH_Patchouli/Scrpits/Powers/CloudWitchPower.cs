using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class CloudWitchPower : CustomPowerModel
	{
		private bool _pendingDiscard;

		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Single;

		public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/WE32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/WE64.png";

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Block)];

		public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			if (target != Owner || amount <= 0 || dealer == null || dealer.Side == Owner.Side)
			{
				return 0m;
			}

			if (PileType.Draw.GetPile(Owner.Player).Cards.Count <= 0)
			{
				return 0m;
			}

			_pendingDiscard = true;
			return -amount;
		}

		public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			if (!_pendingDiscard)
			{
				return;
			}
			_pendingDiscard = false;

			CardPile drawPile = PileType.Draw.GetPile(Owner.Player);
			if (drawPile.Cards.Count <= 0)
			{
				return;
			}

			Flash();
			CardModel top = drawPile.Cards[^1];
			await CardCmd.Discard(choiceContext, top);
		}
	}
}
