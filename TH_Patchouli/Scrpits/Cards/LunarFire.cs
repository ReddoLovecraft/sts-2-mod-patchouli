using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchoulib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(SpellCardPool))]
	public sealed class LunarFire : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Lunar, ElementEnum.Fire };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IgnitePower>()];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(5)];

		public LunarFire() : base(1, CardType.Skill, CardRarity.Ancient, TargetType.AnyEnemy)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (cardPlay.Target == null)
			{
				return;
			}

			int debuffs = cardPlay.Target.Powers.Count(p => p.Type == PowerType.Debuff);
			int ignite = Math.Max(0, debuffs) * Math.Max(0, DynamicVars.Cards.IntValue);
			if (ignite > 0)
			{
				await PowerCmd.Apply<IgnitePower>(cardPlay.Target, ignite, Owner.Creature, this);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(2);
		}
	}
}
