using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchoulib.Scrpits.Main;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Powers;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(CurseCardPool))]
	public sealed class Tired : PatchouliCardModel
	{
		public override int MaxUpgradeLevel => 0;
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [this.EnergyHoverTip,HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1),new EnergyVar(1)];

		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate,CardKeyword.Retain,PatchoulibCardModifier.CannotEscapeKeyword,CardKeyword.Eternal];

		public Tired() : base(1, CardType.Curse, CardRarity.Curse, TargetType.None)
		{
		}
		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CardPileCmd.Add(this, PileType.Draw, CardPilePosition.Top);
		}
		public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		int num = CombatManager.Instance.History.CardPlaysFinished.Count((CardPlayFinishedEntry e) => e.HappenedThisTurn(base.CombatState) && e.CardPlay.Card.Owner == base.Owner);
		CardPile? pile = base.Pile;
		if (pile != null && pile.Type == PileType.Hand &&cardPlay.Card.Owner == base.Owner)
		{
			if(num > 5)
			{
			    await CardCmd.Exhaust(context,cardPlay.Card);
			}
			if(num > 9)
			{
				await PowerCmd.Apply<MindRotPower>(Owner.Creature,1,Owner.Creature,this);
				await PowerCmd.Apply<WasteAwayPower>(Owner.Creature,1,Owner.Creature,this);
			}
		}
	}
		public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
		{
			if (card != this)
			{
				return;
			}
			CardModel copy = this.CreateClone();
			await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, addedByPlayer: true);
		}
        public override void AfterTransformedFrom()
        {
			CardModel card = this.CreateClone();
			CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, addedByPlayer: true);
        }
    }
}
