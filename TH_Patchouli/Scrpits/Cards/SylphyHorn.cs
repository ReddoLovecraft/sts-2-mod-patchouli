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
	public sealed class SylphyHorn : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Wood };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		private bool _active;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

		public SylphyHorn() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			_active = true;
			await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
			_active = false;
		}

		public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
		{
			if (!_active || card.Owner != Owner)
			{
				return;
			}
			if (card.Type != CardType.Skill)
			{
				return;
			}
			await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}
