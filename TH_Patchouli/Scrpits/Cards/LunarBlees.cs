using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchoulib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using LunarElementPower = TH_Patchouli.Scrpits.Powers.LunarElement;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class LunarBlees : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = [ElementEnum.Lunar];
		public override List<ElementEnum> ElementTypes => _elementTypes;

		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromPower<PlatingPower>(),
			HoverTipFactory.FromPower<Powers.LunarElement>(),
		];
		protected override IEnumerable<DynamicVar> CanonicalVars => [new GoldVar(50), new CardsVar(3), new DynamicVar("Power", 6)];
		public LunarBlees() : base(3, CardType.Skill, CardRarity.Rare, TargetType.None)
		{
		}
		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Gold.UpgradeValueBy(boostAmount*25);
			DynamicVars.Cards.UpgradeValueBy(boostAmount*1);
			DynamicVars["Power"].UpgradeValueBy(boostAmount*2);
		}
		protected override void OnUpgrade()
		{
			DynamicVars.Gold.UpgradeValueBy(25);
			DynamicVars.Cards.UpgradeValueBy(1);
			DynamicVars["Power"].UpgradeValueBy(2);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (CombatState == null)
			{
				return;
			}

			int lunar = Owner.Creature.GetPower<LunarElementPower>()?.Amount ?? 0;
			int picks = Math.Min(3, 1 + Math.Max(0, lunar));
			for (int i = 0; i < picks; i++)
			{
				List<CardModel> options =
				[
					CombatState.CreateCard(ModelDb.Card<SelectGold>(), Owner),
					CombatState.CreateCard(ModelDb.Card<SelectLunar>(), Owner),
					CombatState.CreateCard(ModelDb.Card<SelectArmor>(), Owner),
				];

				CardModel selected = await CardSelectCmd.FromChooseACardScreen(choiceContext, options, Owner, canSkip: false);
				if (selected is SelectGold)
				{
					await ((PatchouliCardModel)selected).OnChosen(DynamicVars.Gold.IntValue);
				}
				else if (selected is SelectLunar)
				{
					await ((PatchouliCardModel)selected).OnChosen(DynamicVars.Cards.IntValue);
				}
				else if (selected is SelectArmor)
				{
					await ((PatchouliCardModel)selected).OnChosen(DynamicVars["Power"].IntValue);
				}
			}
		}
	}
}
