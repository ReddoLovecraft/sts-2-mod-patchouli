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
public sealed class MagicPotion : CustomPotionModel
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;

    public override PotionUsage Usage => PotionUsage.CombatOnly;

    public override TargetType TargetType => TargetType.Self;

    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];
     protected override IEnumerable<DynamicVar> CanonicalVars => (new DynamicVar[1]
    {
        new EnergyVar(1)
    });
    public override string? CustomPackedImagePath => "res://TH_Patchouli/ArtWorks/Potions/MAGIC_POTION.png";
    public override string? CustomPackedOutlinePath => "res://TH_Patchouli/ArtWorks/Potions/Outlines/MAGIC_POTION.png"; 
    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        await PlayerCmd.SetEnergy(Owner.Creature.Player.MaxEnergy, Owner.Creature.Player);
    }
}
