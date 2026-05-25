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
	public sealed class BuryInLake : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Water, ElementEnum.Fire };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10m, ValueProp.Move), new EnergyVar(1), new CardsVar(1)];

		public BuryInLake() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Damage.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			ArgumentNullException.ThrowIfNull(cardPlay.Target);

			await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);

			CardModel selected = (await CardSelectCmd.FromHand(choiceContext, Owner, new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1), null, this)).FirstOrDefault();
			if (selected == null)
			{
				return;
			}

			await CardCmd.Exhaust(choiceContext, selected);
			if (selected is not PatchouliCardModel pcm)
			{
				return;
			}

			if (pcm.ElementTypes.Contains(ElementEnum.Water))
			{
				await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
			}
			if (pcm.ElementTypes.Contains(ElementEnum.Fire))
			{
				await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).WithHitCount(DynamicVars.Cards.IntValue).Execute(choiceContext);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(5);
		}
	}
}
