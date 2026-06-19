using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Patchouib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Relics;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(SpellCardPool))]
	public sealed class LunarReaper : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Gold, ElementEnum.Lunar };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.ForEnergy(this)
		];

		protected override IEnumerable<DynamicVar> CanonicalVars =>
		[
			new EnergyVar(2),
			new CardsVar(2),
			new CalculationBaseVar(13m),
			new ExtraDamageVar(1m),
			new CalculatedDamageVar(ValueProp.Move).WithMultiplier(CalculateBonusDamage)
		];

		public LunarReaper() : base(2, CardType.Attack, CardRarity.Ancient, TargetType.AllEnemies)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.CalculationBase.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			Player? player = Owner;
			SfxCmd.Play("event:/sfx/characters/ironclad/ironclad_whirlwind");
			AttackCommand attack = await DamageCmd.Attack(DynamicVars.CalculatedDamage)	.WithHitFx("vfx/vfx_giant_horizontal_slash").FromCard(this).TargetingAllOpponents(CombatState).Execute(choiceContext);
			int killCount = attack.Results.SelectMany(r => r).Count(r => r.WasTargetKilled);
			for (int i = 0; i < killCount; i++)
			{
				await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, player);
				await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, player);
			}
			LunarReaperRelic? relic = player.GetRelic<LunarReaperRelic>();
			if (relic == null)
			{
				relic = (LunarReaperRelic)await RelicCmd.Obtain<LunarReaperRelic>(player);
			}

			int add = DynamicVars.Cards.IntValue;
			if (add > 0)
			{
				relic.AddBonus(add);
			}

			
		}

		private static decimal CalculateBonusDamage(CardModel card, Creature? _)
		{
			Player? owner = card.Owner;
			if (owner == null)
			{
				return 0m;
			}

			return Math.Max(0, owner.GetRelic<LunarReaperRelic>()?.BonusDamage ?? 0);
		}
	}
}
