using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class LunarDrop : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = [ElementEnum.Lunar];
		public override List<ElementEnum> ElementTypes => _elementTypes;

		public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded ? [] : [CardKeyword.Ethereal];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6m, ValueProp.Move), new CardsVar(6)];

		public LunarDrop() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Damage.UpgradeValueBy(boostAmount);
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (CombatState == null)
			{
				return;
			}
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(CombatState).WithHitCount(DynamicVars.Cards.IntValue).Execute(choiceContext);
		}

		protected override void OnUpgrade()
		{
			RemoveKeyword(CardKeyword.Ethereal);
		}
	}
}

