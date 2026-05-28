using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class LunarCloak : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = [ElementEnum.Lunar];
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(6m, ValueProp.Move)];

		public LunarCloak() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Block.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

			List<CardModel> discardCards = PileType.Discard.GetPile(Owner).Cards.ToList();
			if (discardCards.Count == 0)
			{
				return;
			}

			List<CardModel> selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, discardCards, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1))).ToList();
			CardModel card = selected.FirstOrDefault();
			if (card == null)
			{
				return;
			}
			await CardPileCmd.Add(card, PileType.Hand);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Block.UpgradeValueBy(3);
		}
	}
}
