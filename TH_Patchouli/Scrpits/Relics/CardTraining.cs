using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Patchouib.Scrpits.Main;
using Patchoulib.Scrpits.Main;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Powers;


namespace TH_Patchouli.Relics
{
[Pool(typeof(PatchouliRelicPool))]
public class CardTraining : CustomRelicModel
{
	public override string PackedIconPath => $"res://TH_Patchouli/ArtWorks/Relics/{Id.Entry}.png";
    protected override string PackedIconOutlinePath => $"res://TH_Patchouli/ArtWorks/Relics/Outlines/{Id.Entry}.png";
    protected override string BigIconPath => $"res://TH_Patchouli/ArtWorks/Relics/{Id.Entry}.png";
    public override RelicRarity Rarity => RelicRarity.Shop;
   [SavedProperty]
    public  int Counter
    {
        get{return counter;}
        set
        {
            AssertMutable();
			counter=value;
			InvokeDisplayAmountChanged();
        }
    }
	int counter=0;
	public override bool ShowCounter => true;
    public override int DisplayAmount => counter;
	public override async Task AfterCombatVictory(CombatRoom room)
	{
		Flash();
		Counter++;
		if (room.RoomType == RoomType.Elite)
		Counter+=2;
		if (room.RoomType == RoomType.Boss)
		Counter+=4;
	}
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Block)];
	public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (!props.IsPoweredAttack())
		{
			return 0m;
		}
		if (cardSource == null||cardSource.Rarity!=CardRarity.Basic)
		{
			return 0m;
		}
		if (dealer != base.Owner.Creature && cardSource.Owner != base.Owner)
		{
			return 0m;
		}
		return Counter;
	}
		public override decimal ModifyBlockAdditive(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
	{
		if (base.Owner.Creature != target)
		{
			return 0m;
		}
		if (!props.IsPoweredCardOrMonsterMoveBlock())
		{
			return 0m;
		}
		if (cardSource == null||cardSource.Rarity!=CardRarity.Basic)
		{
			return 0m;
		}
		return Counter;
	}

	public override Task AfterModifyingBlockAmount(decimal modifiedBlock, CardModel? cardSource, CardPlay? cardPlay)
	{
		Flash();
		return Task.CompletedTask;
	}
	public override Task AfterModifyingDamageAmount(CardModel? cardSource)
	{
		Flash();
		return Task.CompletedTask;
	}

}
}
