using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Patchouib.Scrpits.Main;
using Patchoulib.Scrpits.Main;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Powers;


namespace TH_Patchouli.Relics
{
[Pool(typeof(PatchouliRelicPool))]
public class PhilosophersStone : CustomRelicModel
{
	public override string PackedIconPath => $"res://TH_Patchouli/ArtWorks/Relics/{Id.Entry}.png";
    protected override string PackedIconOutlinePath => $"res://TH_Patchouli/ArtWorks/Relics/Outlines/{Id.Entry}.png";
    protected override string BigIconPath => $"res://TH_Patchouli/ArtWorks/Relics/{Id.Entry}.png";
    public override RelicRarity Rarity => RelicRarity.Rare;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("Element")];
    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
	{
		if (side == base.Owner.Creature.Side)
		{
		  Flash();
		  await PowerCmd.Apply<FireElement>(Owner.Creature,1,Owner.Creature,null);
		  await PowerCmd.Apply<WaterElement>(Owner.Creature,1,Owner.Creature,null);
		  await PowerCmd.Apply<GoldElement>(Owner.Creature,1,Owner.Creature,null);
		  await PowerCmd.Apply<WoodElement>(Owner.Creature,1,Owner.Creature,null);
		  await PowerCmd.Apply<DirtElement>(Owner.Creature,1,Owner.Creature,null);
		  await PowerCmd.Apply<SunElement>(Owner.Creature,1,Owner.Creature,null);
		  await PowerCmd.Apply<LunarElement>(Owner.Creature,1,Owner.Creature,null);
		}
	}

}
}
