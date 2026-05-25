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
public sealed class ElementApply : PatchouliCardModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust),Tools.GetStaticKeyword("Element")];
	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> { new BlockVar(6m, ValueProp.Move) };
	public override void BoostWhenElementEnhanced(int boostAmount)
	{
		this.DynamicVars.Block.UpgradeValueBy(boostAmount);
	}

	public ElementApply() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
	{
	}
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
		CardModel cardModel = (await CardSelectCmd.FromHand(choiceContext, base.Owner, new CardSelectorPrefs(base.SelectionScreenPrompt, 1), card=>card is PatchouliCardModel plcm,null)).FirstOrDefault();
			if (cardModel != null&&cardModel is PatchouliCardModel plcm)
			{
				if(!plcm.ElementTypes.Contains(ElementEnum.None))
				{
					int amount = plcm.EnergyCost.GetWithModifiers(CostModifiers.Local);
					if(amount<=0)amount=1;
					await ToolBox.GainElement(plcm.ElementTypes,amount,base.Owner.Creature);
				}
				else
				{
					await ToolBox.OpenElementSelectGirdForCard(choiceContext,Owner.Creature,plcm);
				}
			}
		
	
	}
	protected override void OnUpgrade()
	{
		this.DynamicVars.Block.UpgradeValueBy(3);
	}
}

}
