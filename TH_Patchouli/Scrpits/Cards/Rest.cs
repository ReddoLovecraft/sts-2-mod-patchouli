using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Patchouib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;
using TH_Patchouli.Scrpits.Powers;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class Rest : PatchouliCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded ? [] : [CardKeyword.Exhaust];
		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
			 [ HoverTipFactory.FromKeyword(CardKeyword.Retain),base.EnergyHoverTip];
		public override bool GainsBlock => true;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(10m, ValueProp.Move), new CardsVar(2), new EnergyVar(2)];

		public Rest() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}
		protected override void OnUpgrade()
		{
			this.RemoveKeyword(CardKeyword.Exhaust);
		}
		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Block.UpgradeValueBy(boostAmount);
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
			DynamicVars.Energy.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

			foreach (CardModel c in PileType.Hand.GetPile(Owner).Cards)
			{
				c.GiveSingleTurnRetain();
			}

			await PowerCmd.Apply<EnergyNextTurnPower>(Owner.Creature, DynamicVars.Energy.IntValue, Owner.Creature, this);
			await PowerCmd.Apply<DrawCardsNextTurnPower>(Owner.Creature, DynamicVars.Cards.IntValue, Owner.Creature, this);
		}
	}
}
