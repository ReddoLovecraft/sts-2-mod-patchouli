using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchouib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Powers;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(SpellCardPool))]
	public sealed class BurnLunar : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = [ElementEnum.Fire, ElementEnum.Lunar];
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
			HoverTipFactory.FromPower<IgniteMark>(),
			HoverTipFactory.FromPower<IgnitePower>()
		];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(4)];

		public BurnLunar() : base(1, CardType.Skill, CardRarity.Ancient, TargetType.AnyEnemy)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (cardPlay.Target == null)
			{
				return;
			}
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);	
			List<CardModel> selected = (await CardSelectCmd.FromHand(
				choiceContext,
				Owner,
				new CardSelectorPrefs(SelectionScreenPrompt, 1),
				c => c != this,
				this)).ToList();

			CardModel toExhaust = selected.FirstOrDefault();
			if (toExhaust != null)
			{
				await CardCmd.Exhaust(choiceContext, toExhaust);
			}

			int amount = Math.Max(0, DynamicVars.Cards.IntValue);
			if (amount <= 0)
			{
				return;
			}

			if (cardPlay.Target.GetPower<IgniteMark>() != null)
			{
				await PowerCmd.Apply<IgnitePower>(cardPlay.Target, amount, Owner.Creature, this);
			}

			await PowerCmd.Apply<IgniteMark>(cardPlay.Target, amount, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(3);
		}
	}
}
