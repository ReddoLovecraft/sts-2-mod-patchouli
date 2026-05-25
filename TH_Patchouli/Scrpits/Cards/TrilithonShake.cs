using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Patchouib.Scrpits.Main;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;
using TH_Patchouli.Scrpits.Powers;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class TrilithonShake : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Dirt };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

		public TrilithonShake() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			int dmg = Owner.Creature.Block * DynamicVars.Cards.IntValue;
			await DamageCmd.Attack(dmg).FromCard(this).TargetingAllOpponents(CombatState).Execute(choiceContext);
			await CreatureCmd.LoseBlock(Owner.Creature, Owner.Creature.Block);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}
