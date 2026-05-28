using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(SpellCardPool))]
	public sealed class ElementalHarvester : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Gold, ElementEnum.Wood };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			Tools.GetStaticKeyword("Element")
		];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(20m, ValueProp.Move)];

		public ElementalHarvester() : base(3, CardType.Skill, CardRarity.Ancient, TargetType.AnyEnemy)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Damage.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (CombatState == null)
			{
				return;
			}

			await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);

			Player player = Owner;
			IEnumerable<CardModel> candidates =
				from c in player.Character.CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
				let pcm = c as PatchouliCardModel
				where pcm != null && pcm.ElementTypes.Any(e => e != ElementEnum.None)
				select c;

			List<CardModel> generatedCards = CardFactory.GetDistinctForCombat(player, candidates, 3, player.RunState.Rng.CombatCardGeneration).ToList();
			for (int i = 0; i < generatedCards.Count; i++)
			{
				generatedCards[i].EnergyCost.SetThisTurnOrUntilPlayed(0, reduceOnly: true);
			}

			await CardPileCmd.AddGeneratedCardsToCombat(generatedCards, PileType.Hand, addedByPlayer: true);
		}

		protected override void OnUpgrade()
		{
			EnergyCost.UpgradeBy(-1);
		}
	}
}
