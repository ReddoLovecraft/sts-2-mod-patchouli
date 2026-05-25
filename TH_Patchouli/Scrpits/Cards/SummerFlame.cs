using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchouib.Scrpits.Main;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class SummerFlame : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Fire };
		public override List<ElementEnum> ElementTypes => _elementTypes;
		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromPower<IgnitePower>()
		];
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(4)];

		public SummerFlame() : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
		{
		}
		protected override bool ShouldGlowGoldInternal => base.CombatState?.HittableEnemies.Any((Creature e) => e.HasPower<IgnitePower>()) ?? false;
		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			int amount = DynamicVars.Cards.IntValue;
			if (cardPlay.Target.HasPower<IgnitePower>())
			{
				await PowerCmd.Apply<IgnitePower>(cardPlay.Target, amount, Owner.Creature, this);
			}
			await PowerCmd.Apply<IgnitePower>(cardPlay.Target, amount, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(2);
		}
	}
}
