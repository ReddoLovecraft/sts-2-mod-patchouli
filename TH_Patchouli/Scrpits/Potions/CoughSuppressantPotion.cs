using BaseLib.Abstracts;
using BaseLib.Utils;
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
public sealed class CoughSuppressantPotion : CustomPotionModel
{
    public override PotionRarity Rarity => PotionRarity.Rare;

    public override PotionUsage Usage => PotionUsage.CombatOnly;

    public override TargetType TargetType => TargetType.Self;

    public override string? CustomPackedImagePath => "res://TH_Patchouli/ArtWorks/Potions/COUGH_SUPPRESSANT_POTION.png";
    public override string? CustomPackedOutlinePath => "res://TH_Patchouli/ArtWorks/Potions/Outlines/COUGH_SUPPRESSANT_POTION.png"; 
    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        List<PowerModel> debuffs = Owner.Creature.Powers.Where(p => p.Type == PowerType.Debuff).ToList();
			if (debuffs.Count == 0)
			{
				return;
			}
			foreach (PowerModel debuff in debuffs)
			{
				await PowerCmd.Remove(debuff);
			}
    }
}
