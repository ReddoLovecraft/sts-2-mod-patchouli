using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchoulib.Scrpits.Main;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class BookwormWitch : PatchouliCardModel
	{
		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
			HoverTipFactory.FromPower<StrengthPower>(),
			HoverTipFactory.FromPower<DexterityPower>()
		];
		
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1), new DynamicVar("Power", 1)];

		public BookwormWitch() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
			DynamicVars["Power"].UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			List<CardModel> selected = (await CardSelectCmd.FromHand(choiceContext, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1), c => c != this, this)).ToList();
			CardModel card = selected.FirstOrDefault();
			if (card != null)
			{
				await CardCmd.Exhaust(choiceContext, card);
			}
			VfxCmd.PlayOnCreatureCenter(Owner.Creature, "vfx/vfx_bite");
			int amt = DynamicVars["Power"].IntValue;
			await PowerCmd.Apply<StrengthPower>(Owner.Creature, amt, Owner.Creature, this);
			await PowerCmd.Apply<DexterityPower>(Owner.Creature, amt, Owner.Creature, this);
			await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
		}

		protected override void OnUpgrade()
		{
			DynamicVars["Power"].UpgradeValueBy(1);
		}
	}
}
