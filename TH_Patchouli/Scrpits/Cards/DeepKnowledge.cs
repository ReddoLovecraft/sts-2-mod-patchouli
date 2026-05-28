using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.ControllerInput.ControllerConfigs;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Patchouib.Scrpits.Main;
using Patchoulib.Scrpits.Main;
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
	public sealed class DeepKnowledge : PatchouliCardModel
	{
		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			Tools.GetStaticKeyword("Element"),
			HoverTipFactory.FromPower<StrengthPower>(),
			HoverTipFactory.FromPower<DexterityPower>()
		];

		protected override IEnumerable<DynamicVar> CanonicalVars =>
		[
			new CardsVar(1),
			new EnergyVar(0),
			new CalculationBaseVar(1m),
			new CalculationExtraVar(1m),
			new CalculatedVar("CalculatedPower").WithMultiplier(CalculatePositiveBuffs),
		];

		public DeepKnowledge() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
			DynamicVars.CalculationBase.UpgradeValueBy(boostAmount);
			DynamicVars.CalculationExtra.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			int total = Math.Max(0, (int)((CalculatedVar)DynamicVars["CalculatedPower"]).Calculate(cardPlay.Target));
			await PowerCmd.Apply<StrengthPower>(Owner.Creature, total, Owner.Creature, this);
			await PowerCmd.Apply<DexterityPower>(Owner.Creature, total, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			EnergyCost.UpgradeBy(-1);
		}

		private static decimal CalculatePositiveBuffs(CardModel card, Creature? _)
		{
			if (card.Owner?.Creature == null)
			{
				return 0m;
			}
			decimal result=0m;
			foreach(PowerModel p in card.Owner?.Creature.Powers)
			{
				if(p.Type==PowerType.Buff)
				{
					if(p is Powers.SunElement)
					continue;
					if(p is Powers.LunarElement)
					continue;
					if(p is Powers.GoldElement)
					continue;
					if(p is Powers.WoodElement)
					continue;
					if(p is Powers.WaterElement)
					continue;
					if(p is Powers.FireElement)
					continue;
					if(p is Powers.DirtElement)
					continue;
					result++;
				}
			}
			return result;
		}
	}
}
