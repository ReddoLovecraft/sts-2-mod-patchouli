using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class WaterBury : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = [ElementEnum.Water];
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromPower<FreezePower>()
		];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5m, ValueProp.Move), new CardsVar(2), new DynamicVar("Power", 2)];

		public WaterBury() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Damage.UpgradeValueBy(boostAmount);
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
			DynamicVars["Power"].UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (cardPlay.Target == null)
			{
				return;
			}

			await PowerCmd.Apply<FreezePower>(cardPlay.Target, DynamicVars["Power"].IntValue, Owner.Creature, this);
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(DynamicVars.Cards.IntValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(2);
		}
	}
}

