using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class LunarLight : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = [ElementEnum.Lunar];
		public override List<ElementEnum> ElementTypes => _elementTypes;

		public override bool GainsBlock => true;
		protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(8m, ValueProp.Move), new CardsVar(2)];

		public LunarLight() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Block.UpgradeValueBy(boostAmount);
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
			List<CardModel> candidates = PileType.Discard.GetPile(Owner).Cards.Where(c => c.IsUpgradable).ToList();
			if (candidates.Count == 0)
			{
				return;
			}

			Rng rng = Owner.RunState.Rng.CombatCardGeneration;
			int times = Math.Max(0, DynamicVars.Cards.IntValue);
			for (int i = 0; i < times && candidates.Count > 0; i++)
			{
				CardModel pick = rng.NextItem(candidates);
				if (pick == null)
				{
					break;
				}
				candidates.Remove(pick);
				CardCmd.Upgrade(pick);
				CardCmd.Preview(pick);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Block.UpgradeValueBy(2);
			DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}

