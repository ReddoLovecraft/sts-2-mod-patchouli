using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using Patchouib.Scrpits.Main;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(SpellCardPool))]
	public sealed class ElmoPillar : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Fire, ElementEnum.Gold };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromPower<IgnitePower>()
		];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8m, ValueProp.Move), new CardsVar(8)];

		public ElmoPillar() : base(2, CardType.Attack, CardRarity.Ancient, TargetType.AllEnemies)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Damage.UpgradeValueBy(boostAmount);
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			foreach (Creature enemy in CombatState.HittableEnemies)
			{
				bool isAttack = enemy.Monster?.NextMove?.Intents.Any(i => i.IntentType == IntentType.Attack || i.IntentType == IntentType.DeathBlow) ?? false;
				int hitCount = isAttack ? 2 : 1;
				await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).WithHitFx(PatchouliVfxManager.ToPatchouliVfxPath("royalfire"), null, "blunt_attack.mp3").Targeting(enemy).WithHitCount(hitCount).Execute(choiceContext);
				int stacks = DynamicVars.Cards.IntValue * (isAttack ? 1 : 2);
				if (stacks > 0)
				{
					await PowerCmd.Apply<IgnitePower>(enemy, stacks, Owner.Creature, this);
				}
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(2);
			DynamicVars.Cards.UpgradeValueBy(2);
		}
	}
}
