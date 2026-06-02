using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scrpits.Powers;

namespace TH_Patchouli.Scrpits.Potions;
[Pool(typeof(PatchouliPotionPool))]
public sealed class FourMagicBomb : CustomPotionModel
{
    public override PotionRarity Rarity => PotionRarity.Common;

    public override PotionUsage Usage => PotionUsage.CombatOnly;

    public override TargetType TargetType => TargetType.AllEnemies;

    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IgnitePower>(),HoverTipFactory.FromPower<FreezePower>()];
    public override string? CustomPackedImagePath => "res://TH_Patchouli/ArtWorks/Potions/FOUR_MAGIC_BOMB.png";
    public override string? CustomPackedOutlinePath => "res://TH_Patchouli/ArtWorks/Potions/Outlines/FOUR_MAGIC_BOMB.png"; 
    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        Creature player = base.Owner.Creature;
		IReadOnlyList<Creature> targets = player.CombatState.HittableEnemies;
		foreach (Creature item in targets)
		{
			NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NFireSmokePuffVfx.Create(item));
		}
		await Cmd.CustomScaledWait(0.2f, 0.3f);
		await CreatureCmd.Damage(choiceContext, targets, new DamageVar(4,ValueProp.Unpowered), player, null);
        await PowerCmd.Apply<IgnitePower>(targets,4,Owner.Creature,null);
        await PowerCmd.Apply<FreezePower>(targets,4,Owner.Creature,null);
    }
}
