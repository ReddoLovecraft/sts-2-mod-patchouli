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
using MegaCrit.Sts2.Core.HoverTips;
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
	public sealed class AgniRadiance : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Fire };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IgnitePower>()];

		protected override IEnumerable<DynamicVar> CanonicalVars =>
		[
			new CalculationBaseVar(10m),
			new ExtraDamageVar(1m),
			new CalculatedDamageVar(ValueProp.Move).WithMultiplier(CalculateTotalIgnite),
		];

		public AgniRadiance() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.CalculationBase.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (CombatState == null)
			{
				return;
			}
			await DamageCmd.Attack(DynamicVars.CalculatedDamage).FromCard(this).TargetingAllOpponents(CombatState).Execute(choiceContext);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.CalculationBase.UpgradeValueBy(3);
		}

		private static decimal CalculateTotalIgnite(CardModel card, Creature? _)
		{
			if (card.CombatState == null)
			{
				return 0m;
			}
			int totalIgnite = card.CombatState.HittableEnemies.Sum(e => e.GetPower<IgnitePower>()?.Amount ?? 0);
			return Math.Max(0, totalIgnite);
		}
	}
}
