using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchouib.Scrpits.Main;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class UnhurriedPower : CustomPowerModel
	{
		private bool _wasDrawPileEmpty;
		private int _draw;

		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/GE32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/GE64.png";

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(0)];
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];

		public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
		{
			if (cardSource is PatchouliCardModel pcm)
			{
				_draw = pcm.DynamicVars.Cards.IntValue;
				DynamicVars.Cards.BaseValue += _draw;
			}
		}

		public override async Task AfterCardChangedPilesLate(CardModel card, PileType oldPileType, AbstractModel? source)
		{
			if (card.Owner != Owner.Player)
			{
				return;
			}

			CardPile drawPile = PileType.Draw.GetPile(Owner.Player);
			bool empty = drawPile.Cards.Count == 0;
			if (empty && !_wasDrawPileEmpty)
			{
				_wasDrawPileEmpty = true;
				Flash();
				await CardPileCmd.Draw(new BlockingPlayerChoiceContext(), _draw, Owner.Player);
				await PlayerCmd.GainEnergy(Amount, Owner.Player);
				return;
			}

			_wasDrawPileEmpty = empty;
		}
	}
}
