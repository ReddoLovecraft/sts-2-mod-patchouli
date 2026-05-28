using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Patchoulib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(SpellCardPool))]
	public sealed class WaterElf : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Water, ElementEnum.Wood };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<WaterSpirit>(IsUpgraded)];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

		public WaterElf() : base(1, CardType.Skill, CardRarity.Ancient, TargetType.Self)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (CombatState == null)
			{
				return;
			}

			int count = Math.Max(0, DynamicVars.Cards.IntValue);
			if (count > 0)
			{
				List<CardModel> generated = new List<CardModel>(count);
				for (int i = 0; i < count; i++)
				{
					generated.Add(CombatState.CreateCard(ModelDb.Card<WaterSpirit>(), Owner));
				}
				await CardPileCmd.AddGeneratedCardsToCombat(generated, PileType.Hand, addedByPlayer: true);
			}

			DynamicVars.Cards.BaseValue = Math.Max(0m, DynamicVars.Cards.BaseValue + 1m);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}
