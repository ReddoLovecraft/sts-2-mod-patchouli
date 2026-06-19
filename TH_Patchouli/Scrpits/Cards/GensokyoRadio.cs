using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Patchoulib.Scrpits.Main;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Powers;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class GensokyoRadio : PatchouliCardModel
	{
		public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
		public GensokyoRadio() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllAllies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			CardModel selectedCard = (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(base.SelectionScreenPrompt, 1), context: choiceContext, player: base.Owner, filter: null, source: this)).FirstOrDefault();
			if (selectedCard == null)
			{
				return;
			}
			IEnumerable<Creature> enumerable = from c in base.CombatState.GetTeammatesOf(base.Owner.Creature)
            where c != null && c.IsAlive && c.IsPlayer && c.Player != null && c != this.Owner.Creature
            select c;
       		 foreach (Creature creature in enumerable)
        	{
				CardModel copy = CombatState.CreateCard(selectedCard.CanonicalInstance, creature.Player);
				if (selectedCard.IsUpgraded)
				{
					CardCmd.Upgrade(copy);
				}
				if (this.IsUpgraded)
				{
					copy.EnergyCost.SetThisTurnOrUntilPlayed(0);
				}
				await CardPileCmd.AddGeneratedCardsToCombat(new List<CardModel> { copy }, PileType.Hand, creator: Owner);
        	}
		}
	}
}

