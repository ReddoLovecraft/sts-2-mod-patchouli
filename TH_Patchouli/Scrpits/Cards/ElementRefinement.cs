using System;
using BaseLib.Abstracts;
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
public sealed class ElementRefinement : PatchouliCardModel,ITranscendenceCard
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust),Tools.GetStaticKeyword("Element")];
	public CardModel GetTranscendenceTransformedCard() => ModelDb.Card<SupermeAlchemy>();
	public ElementRefinement() : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
	{
	}
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		CardModel cardModel = (await CardSelectCmd.FromHand(choiceContext, base.Owner, new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1), null, this)).FirstOrDefault();
		if (cardModel != null)
		{
			await CardCmd.Exhaust(choiceContext, cardModel);
			if(cardModel is PatchouliCardModel plcm&&plcm.ElementTypes.Count>0&&!plcm.ElementTypes.Contains(ElementEnum.None))
			{
				int amount = plcm.EnergyCost.GetWithModifiers(CostModifiers.Local);
				if(amount>0)
				await ToolBox.GainElement(plcm.ElementTypes,amount*3,Owner.Creature);
			}
			else
			{
				int amount = cardModel.EnergyCost.GetWithModifiers(CostModifiers.Local);
				if(amount>0)
				ToolBox.GainElementRandomly(amount*3,Owner.Creature);
			}
		}
	}
	protected override void OnUpgrade()
	{
	   this.EnergyCost.UpgradeBy(-1);
	}
}

}
