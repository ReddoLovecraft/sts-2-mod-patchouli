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
public sealed class BottleOfDemon : CustomPotionModel
{
    public override PotionRarity Rarity => PotionRarity.Rare;

    public override PotionUsage Usage => PotionUsage.CombatOnly;

    public override TargetType TargetType => TargetType.Self;

    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];
    public override string? CustomPackedImagePath => "res://TH_Patchouli/ArtWorks/Potions/BOTTLE_OF_DEMON.png";
    public override string? CustomPackedOutlinePath => "res://TH_Patchouli/ArtWorks/Potions/Outlines/BOTTLE_OF_DEMON.png"; 
    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
       List<CardModel> cardsIn = (from c in PileType.Draw.GetPile(base.Owner).Cards
			orderby c.Rarity, c.Id
			select c).ToList();
		List<CardModel> list = (await CardSelectCmd.FromSimpleGrid(choiceContext, cardsIn, base.Owner, new CardSelectorPrefs(base.SelectionScreenPrompt, 1))).ToList();
		foreach (CardModel item in list)
		{
			await CardCmd.AutoPlay(choiceContext, item, null);
			await CardCmd.AutoPlay(choiceContext, item, null);
            await CardCmd.Exhaust(choiceContext,item);
		}
    }
}
