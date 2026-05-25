using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class GreenFlash : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Wood };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4m, ValueProp.Move), new CardsVar(3)];

		public GreenFlash() : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
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
			await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).WithHitCount(base.DynamicVars.Cards.IntValue).FromCard(this)
			.TargetingRandomOpponents(base.CombatState)
			.WithHitVfxNode((Creature t) => NScratchVfx.Create(t, goingRight: true))
			.Execute(choiceContext);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(1);
		}
	}
}
