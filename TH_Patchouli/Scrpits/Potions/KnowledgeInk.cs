using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchoulib.Scrpits.Main;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scrpits.Powers;

namespace TH_Patchouli.Scrpits.Potions;
[Pool(typeof(PatchouliPotionPool))]
public sealed class KnowledgeInk : CustomPotionModel
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;

    public override PotionUsage Usage => PotionUsage.CombatOnly;

    public override TargetType TargetType => TargetType.Self;

    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Retain)];
    public override string? CustomPackedImagePath => "res://TH_Patchouli/ArtWorks/Potions/KNOWLEDGE_INK.png";
    public override string? CustomPackedOutlinePath => "res://TH_Patchouli/ArtWorks/Potions/Outlines/KNOWLEDGE_INK.png"; 
    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        List<CardModel> cardModels = (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(base.SelectionScreenPrompt, 0,999), context: choiceContext, player: base.Owner, filter: (CardModel c) => !c.Keywords.Contains(CardKeyword.Retain), source: this)).ToList();
		if (cardModels.Count != 0)
		{
			foreach (CardModel cardModel in cardModels)
			{
				CardCmd.ApplyKeyword(cardModel, CardKeyword.Retain);
			}
		}
    }
}
