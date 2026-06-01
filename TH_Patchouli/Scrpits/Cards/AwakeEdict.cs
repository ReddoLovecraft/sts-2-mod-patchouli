using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using Patchoulib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(SpellCardPool))]
	public sealed class AwakeEdict : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Wood, ElementEnum.Sun };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(12)];

		public AwakeEdict() : base(2, CardType.Skill, CardRarity.Ancient, TargetType.Self)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			int maxHp = Owner.Creature.MaxHp;
			int currentHp = Owner.Creature.CurrentHp;
			int pct = Math.Max(0, DynamicVars.Cards.IntValue);
			int amount = (int)Math.Floor(maxHp * (pct / 100m));
			if (amount <= 0)
			{
				return;
			}

			int missing = Math.Max(0, maxHp - currentHp);
			int heal = Math.Min(amount, missing);
			if (heal > 0)
			{
				PlayerFullscreenHealVfx.Play(Owner, heal, NCombatRoom.Instance);
				await CreatureCmd.Heal(Owner.Creature, heal);
			}

			int excess = amount - heal;
			if (excess > 0)
			{
				PlayerFullscreenHealVfx.Play(Owner, excess, NCombatRoom.Instance);
				await CreatureCmd.GainMaxHp(Owner.Creature, excess);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(8);
		}
	}
}
