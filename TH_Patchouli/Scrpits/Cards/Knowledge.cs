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
	public sealed class Knowledge : PatchouliCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars =>
		[
			new CardsVar(3),
			new EnergyVar(0),
			new CalculationBaseVar(0m),
			new CalculationExtraVar(1m),
			new CalculatedVar("CalculatedEnergy").WithMultiplier(CalculateEnergyGain),
		];	
			protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		base.EnergyHoverTip
	];

		public Knowledge() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(-boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			int gain = Math.Max(0, (int)((CalculatedVar)DynamicVars["CalculatedEnergy"]).Calculate(cardPlay.Target));
			if (gain > 0)
			{
				await PlayerCmd.GainEnergy(gain, Owner);
			}
			await PowerCmd.Apply<NoEnergyGainPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(-1);
		}

		private static decimal CalculateEnergyGain(CardModel card, Creature? _)
		{
			if (card.Owner == null)
			{
				return 0m;
			}

			int drawCount = PileType.Draw.GetPile(card.Owner).Cards.Count;
			int divisor = Math.Max(1, card.DynamicVars.Cards.IntValue);
			int gain = drawCount / divisor;
			return Math.Max(0, gain);
		}
	}
}

