using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Patchoulib.Scrpits.Main;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(SpellCardPool))]
	public sealed class WhisperRoot : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Wood, ElementEnum.Lunar };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

		public WhisperRoot() : base(2, CardType.Skill, CardRarity.Ancient, TargetType.Self)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			HashSet<CardModel> before = PileType.Hand.GetPile(Owner).Cards.ToHashSet();
			await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
			foreach (CardModel c in PileType.Hand.GetPile(Owner).Cards)
			{
				if (!before.Contains(c))
				{
					c.EnergyCost.SetThisTurnOrUntilPlayed(0, reduceOnly: true);
				}
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}
