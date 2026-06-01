using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Patchoulib.Scrpits.Main;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class BodyCreate : PatchouliCardModel
	{
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Transform),HoverTipFactory.FromCard<BasicBody>()];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

		public BodyCreate() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			VfxCmd.PlayOnCreatureCenter(base.Owner.Creature, PatchouliVfxManager.ToPatchouliVfxPath("doublemagic"));
			int maxSelect = DynamicVars.Cards.IntValue;
			CardSelectorPrefs prefs = new CardSelectorPrefs(SelectionScreenPrompt, 0, maxSelect);
			List<CardModel> selected = (await CardSelectCmd.FromHand(choiceContext, Owner, prefs, c => c.IsTransformable && c != this, this)).ToList();
			if (selected.Count == 0)
			{
				return;
			}

			for (int i = 0; i < selected.Count; i++)
			{
				CardModel original = selected[i];
				CardModel replacement = CombatState.CreateCard<BasicBody>(Owner);
				await CardCmd.Transform(original, replacement);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}
