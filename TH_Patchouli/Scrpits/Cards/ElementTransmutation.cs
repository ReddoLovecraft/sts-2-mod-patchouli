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
public sealed class ElementTransmutation : PatchouliCardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain,CardKeyword.Exhaust];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("Element")];
	protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> { new CardsVar(1),new DynamicVar("Power",3m) };

	public override void BoostWhenElementEnhanced(int boostAmount)
	{
		this.DynamicVars["Power"].UpgradeValueBy(boostAmount);
		this.DynamicVars.Cards.UpgradeValueBy(boostAmount);
	}
	public ElementTransmutation() : base(0, CardType.Skill, CardRarity.Basic, TargetType.Self)
	{
	}
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		await ToolBox.OpenElementSelectGirdForGain(choiceContext,Owner.Creature,base.DynamicVars["Power"].IntValue);
		await CardPileCmd.Draw(choiceContext,base.DynamicVars.Cards.IntValue,Owner);
	}
	protected override void OnUpgrade()
	{
	   this.DynamicVars["Power"].UpgradeValueBy(2);
	}
}

}
