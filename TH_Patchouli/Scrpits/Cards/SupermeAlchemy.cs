using System;
using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;
using TH_Patchouli.Scrpits.Powers;

namespace TH_Patchouli.Scripts.Cards
{
[Pool(typeof(PatchouliCardPool))]
public sealed class SupermeAlchemy : PatchouliCardModel
{
	 public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];
	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromKeyword(CardKeyword.Retain),
		HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
		HoverTipFactory.FromCard<ElementTransmutation>(base.IsUpgraded)
	];

	public SupermeAlchemy() : base(1, CardType.Skill, CardRarity.Ancient, TargetType.Self)
	{
	}
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		foreach (CardModel item in await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(base.SelectionScreenPrompt, 0, 999), context: choiceContext, player: base.Owner, filter: null, source: this))
        {
			await CardCmd.Exhaust(choiceContext, item);
			CardModel element = base.CombatState.CreateCard(ModelDb.Card<ElementTransmutation>(), base.Owner);
			if(IsUpgraded)
				CardCmd.Upgrade(element);
			CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(new[] { element }, PileType.Hand, addedByPlayer: true));
		}
	}
	protected override void OnUpgrade()
	{
	   
	}
}

}
