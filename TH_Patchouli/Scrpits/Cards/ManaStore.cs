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
	public sealed class ManaStore : PatchouliCardModel
	{
		private int _playedThisTurn;

		protected override IEnumerable<DynamicVar> CanonicalVars =>
		[
			new EnergyVar(0),
			new CalculationBaseVar(2m),
			new CalculationExtraVar(-1m),
			new CalculatedVar("CalculatedEnergy").WithMultiplier(CalculateEnergyReduction),
		];

		public ManaStore() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
		{
		}
		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		base.EnergyHoverTip
	];
		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.CalculationBase.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			int energy = Math.Max(0, (int)((CalculatedVar)DynamicVars["CalculatedEnergy"]).Calculate(cardPlay.Target));
			if (energy > 0)
			{
				await PlayerCmd.GainEnergy(energy, Owner);
			}
			_playedThisTurn++;
		}

		public override Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
		{
			if (side == Owner.Creature.Side)
			{
				_playedThisTurn = 0;
			}
			return Task.CompletedTask;
		}

		protected override void OnUpgrade()
		{
			DynamicVars.CalculationBase.UpgradeValueBy(1);
		}

		private static decimal CalculateEnergyReduction(CardModel card, Creature? _)
		{
			ManaStore self = (ManaStore)card;
			int baseEnergy = self.DynamicVars.CalculationBase.IntValue;
			int reduction = Math.Min(self._playedThisTurn, baseEnergy);
			return Math.Max(0, reduction);
		}
	}
}
