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
	public sealed class GreenStorm : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Wood };
		public override List<ElementEnum> ElementTypes => _elementTypes;
		public override bool CanBeGeneratedInCombat => false;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<RegenPower>()];

		protected override bool HasEnergyCostX => true;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5m, ValueProp.Move)];

		public GreenStorm() : base(-1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (CombatState == null)
			{
				return;
			}

			int x = ResolveEnergyXValue();
			int bonus = IsUpgraded ? 2 : 0;
			int hitCount = Math.Max(0, x + bonus);

			if (hitCount > 0)
			{
				await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).SpawningHitVfxOnEachCreature().TargetingAllOpponents(CombatState) .WithHitFx(PatchouliVfxManager.ToPatchouliVfxPath("greenstorm"), null, "blunt_attack.mp3").WithHitCount(hitCount).Execute(choiceContext);
				await PowerCmd.Apply<RegenPower>(Owner.Creature, hitCount, Owner.Creature, this);
			}
		}
	}
}
