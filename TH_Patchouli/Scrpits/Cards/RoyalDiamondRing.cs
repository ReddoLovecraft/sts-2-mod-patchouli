using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;
using TH_Patchouli.Scrpits.Powers;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(SpellCardPool))]
	public sealed class RoyalDiamondRing : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Sun, ElementEnum.Lunar };
		public override List<ElementEnum> ElementTypes => _elementTypes;
		public RoyalDiamondRing() : base(2, CardType.Power, CardRarity.Ancient, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			if (Owner.PlayerCombatState != null)
			{
				foreach (CardModel card in Owner.PlayerCombatState.AllCards)
				{
					if (card != this && card.IsUpgradable)
					{
						CardCmd.Upgrade(card);
						CardCmd.Preview(card);
					}
				}
			}
			await PowerCmd.Apply<RoyalDiamondRingPower>(Owner.Creature, 1, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			EnergyCost.UpgradeBy(-1);
		}
	}
}
