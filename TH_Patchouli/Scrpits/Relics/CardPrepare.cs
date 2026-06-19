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
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using Patchouib.Scrpits.Main;
using Patchoulib.Scrpits.Main;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Powers;


namespace TH_Patchouli.Relics
{
[Pool(typeof(PatchouliRelicPool))]
public class CardPrepare : CustomRelicModel
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
	  protected override IEnumerable<DynamicVar> CanonicalVars => (new DynamicVar[1]
    {
        new EnergyVar(1)
    });
	public override bool ShowCounter => true;
    public override int DisplayAmount => counter;
	public override decimal ModifyMaxEnergy(Player player, decimal amount)
    {
        if (player != base.Owner)
        {
            return amount;
        }
        return amount +Counter;
    }
	public override Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (side != CombatSide.Player)
		{
			return Task.CompletedTask;
		}
		if (base.Owner.PlayerCombatState.Energy > 0)
		{
			Counter++;
		}
		return Task.CompletedTask;
	}
	public override Task AfterCombatEnd(CombatRoom room)
	{
		Counter=0;
		return Task.CompletedTask;
	}
}
}

