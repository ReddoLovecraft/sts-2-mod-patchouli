using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class SunBurst : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = [ElementEnum.Sun];
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromPower<VulnerablePower>(),
			HoverTipFactory.FromPower<WeakPower>(),
			HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
		];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(20m, ValueProp.Move), new CardsVar(2)];

		public SunBurst() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Damage.UpgradeValueBy(boostAmount);
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (cardPlay.Target == null)
			{
				return;
			}
			AttackCommand attack = await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).WithHitFx(PatchouliVfxManager.ToPatchouliVfxPath("sunburst"), null, "blunt_attack.mp3").Targeting(cardPlay.Target).Execute(choiceContext);
			var result = attack.Results.SelectMany(r => r).FirstOrDefault();
			await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target, DynamicVars.Cards.IntValue, Owner.Creature, this);
			await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target, DynamicVars.Cards.IntValue, Owner.Creature, this);
			if (result != null && result.WasTargetKilled)
			{
				await CardCmd.Exhaust(choiceContext, this);
				if (result.TotalDamage > 0)
				{
					await PlayerCmd.GainGold(result.TotalDamage+result.OverkillDamage, Owner);
				}
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(5);
		}
	}
}

