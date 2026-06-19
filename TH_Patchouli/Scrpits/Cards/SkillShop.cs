using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class SkillShop : PatchouliCardModel
	{
		public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(50)];
		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}
		public SkillShop() : base(1, CardType.Skill, CardRarity.Rare, TargetType.AllAllies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			int gainGold = 0;
			IEnumerable<Creature> players = from c in base.CombatState.GetTeammatesOf(base.Owner.Creature)
				where c != null && c.IsAlive && c.IsPlayer&&c.Player!=base.Owner
				select c;
			foreach (Creature creature in players)
			{
				await PlayerCmd.LoseGold(this.DynamicVars.Cards.IntValue,creature.Player,GoldLossType.Spent);
				gainGold+=this.DynamicVars.Cards.IntValue;
				List<CardModel> canonicalLibraryCards = creature.Player.Character.CardPool.AllCards
					.Where(c => c.ShouldShowInCardLibrary && c.Type ==CardType.Skill)
					.ToList();
				List<CardModel> selectableCards = canonicalLibraryCards
					.Select(c => base.CombatState.CreateCard(c, creature.Player))
					.ToList();
				CardSelectorPrefs prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1);
				CardModel? selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, selectableCards, creature.Player, prefs)).FirstOrDefault();
				if (selected != null)
				{
					await CardPileCmd.AddGeneratedCardToCombat(selected, PileType.Hand, creator: Owner);
				}
			}
			await PlayerCmd.GainGold(gainGold,base.Owner);
		}
		protected override void OnUpgrade()
		{
			this.EnergyCost.UpgradeBy(-1);
		}
	}
}

