using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchouib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(SpellCardPool))]
	public sealed class EtheralFire : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = [ElementEnum.Dirt, ElementEnum.Fire];
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IgnitePower>()];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Power", 1m), new CardsVar(6)];

		public EtheralFire() : base(2, CardType.Skill, CardRarity.Ancient, TargetType.AllEnemies)
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

			int ignite = Math.Max(0, DynamicVars["Power"].IntValue);
			int times = Math.Max(0, DynamicVars.Cards.IntValue);
			if (ignite <= 0 || times <= 0)
			{
				return;
			}

			for (int i = 0; i < times; i++)
			{
				await PowerCmd.Apply<IgnitePower>(CombatState.HittableEnemies, ignite, Owner.Creature, this);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(2);
		}
	}
}
