using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchouib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(SpellCardPool))]
	public sealed class AshesRise : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = [ElementEnum.Fire, ElementEnum.Wood];
		public override List<ElementEnum> ElementTypes => _elementTypes;

		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IgnitePower>()];

		public AshesRise() : base(2, CardType.Skill, CardRarity.Ancient, TargetType.AllEnemies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (CombatState == null)
			{
				return;
			}

			List<Creature> enemies = CombatState.HittableEnemies.ToList();
			foreach (Creature enemy in enemies)
			{
				int heal = Math.Max(0, enemy.MaxHp - enemy.CurrentHp);
				if (heal <= 0)
				{
					continue;
				}

				await CreatureCmd.Heal(enemy, heal);
				await PowerCmd.Apply<IgnitePower>(enemy, heal, Owner.Creature, this);
			}
		}

		protected override void OnUpgrade()
		{
			EnergyCost.UpgradeBy(-1);
		}
	}
}
