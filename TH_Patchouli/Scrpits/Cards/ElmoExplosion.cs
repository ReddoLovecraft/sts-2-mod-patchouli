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
	public sealed class ElmoExplosion : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Fire };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<TH_Patchouli.Scrpits.Powers.FireElement>()];

		protected override IEnumerable<DynamicVar> CanonicalVars =>
		[
			new CalculationBaseVar(20m),
			new ExtraDamageVar(5m),
			new CalculatedDamageVar(ValueProp.Move).WithMultiplier(CalculateFireCardCount),
		];

		public ElmoExplosion() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.CalculationBase.UpgradeValueBy(boostAmount);
			DynamicVars.ExtraDamage.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (CombatState == null)
			{
				return;
			}
			await DamageCmd.Attack(DynamicVars.CalculatedDamage).FromCard(this).SpawningHitVfxOnEachCreature().TargetingAllOpponents(CombatState) .WithHitFx(PatchouliVfxManager.ToPatchouliVfxPath("boom"), null, "blunt_attack.mp3").Execute(choiceContext);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.CalculationBase.UpgradeValueBy(2);
			DynamicVars.ExtraDamage.UpgradeValueBy(3);
		}

		private static decimal CalculateFireCardCount(CardModel card, Creature? _)
		{
			if (card.Owner == null)
			{
				return 0m;
			}

			HashSet<CardModel> allCards = [];
			foreach (PileType pt in new[] { PileType.Draw, PileType.Hand, PileType.Discard, PileType.Exhaust, PileType.Play })
			{
				foreach (CardModel c in pt.GetPile(card.Owner).Cards)
				{
					allCards.Add(c);
				}
			}

			int fireCount = allCards.OfType<PatchouliCardModel>().Count(c => c.ElementTypes.Contains(ElementEnum.Fire));
			return Math.Max(0, fireCount);
		}
	}
}
