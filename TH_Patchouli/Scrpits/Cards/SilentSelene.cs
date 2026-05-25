using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
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
	public sealed class SilentSelene : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Lunar };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1), new EnergyVar(2)];

		public SilentSelene() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			bool playedAttackThisTurn = CombatManager.Instance.History.CardPlaysFinished.Any((CardPlayFinishedEntry e) =>
				e.HappenedThisTurn(CombatState) && e.CardPlay.Card.Owner == Owner && e.CardPlay.Card.Type == CardType.Attack);

			int mult = playedAttackThisTurn ? 1 : 2;
			await CardPileCmd.Draw(choiceContext, mult * DynamicVars.Cards.IntValue, Owner);
			await PlayerCmd.GainEnergy(mult * DynamicVars.Energy.IntValue, Owner);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}
