using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;
using TH_Patchouli.Scrpits.Powers.NewPowers;

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

		protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (Owner.PlayerCombatState != null)
			{
				foreach (CardModel card in Owner.PlayerCombatState.AllCards)
				{
					if (card != this && card.IsUpgradable)
					{
						CardCmd.Upgrade(card);
					}
				}
			}
			return PowerCmd.Apply<RoyalDiamondRingPower>(Owner.Creature, 1, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			EnergyCost.UpgradeBy(-1);
		}
	}
}
