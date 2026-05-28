using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(SpellCardPool))]
	public sealed class SquareRingSword : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Sun, ElementEnum.Dirt };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8m, ValueProp.Move)];

		public SquareRingSword() : base(1, CardType.Attack, CardRarity.Ancient, TargetType.AllEnemies)
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

			decimal damage = Math.Max(0m, DynamicVars.Damage.BaseValue);
			while (damage >= 1m)
			{
				await DamageCmd.Attack(damage).FromCard(this).TargetingRandomOpponents(CombatState).Execute(choiceContext);
				damage = Math.Floor(damage / 2m);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(4);
		}
	}
}
