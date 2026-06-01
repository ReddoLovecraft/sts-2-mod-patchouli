using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
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
	public sealed class PowerThrough : PatchouliCardModel
	{
		public override bool GainsBlock => true;
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Overdraft>(false)];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(17m, ValueProp.Move)];

		public PowerThrough() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Block.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

			if (CombatState == null)
			{
				return;
			}

			CardModel a = CombatState.CreateCard(ModelDb.Card<Overdraft>(), Owner);
			CardModel b = CombatState.CreateCard(ModelDb.Card<Overdraft>(), Owner);
			await CardPileCmd.AddGeneratedCardsToCombat([a, b], PileType.Hand, addedByPlayer: true, position: CardPilePosition.Top);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Block.UpgradeValueBy(5);
		}
	}
}
