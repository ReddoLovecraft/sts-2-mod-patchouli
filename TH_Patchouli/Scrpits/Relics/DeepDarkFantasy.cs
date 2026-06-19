using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
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
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Patchouib.Scrpits.Main;
using Patchoulib.Scrpits.Main;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Powers;


namespace TH_Patchouli.Relics
{
[Pool(typeof(PatchouliRelicPool))]
public class DeepDarkFantasy : CustomRelicModel
{
	public override string PackedIconPath => $"res://TH_Patchouli/ArtWorks/Relics/{Id.Entry}.png";
    protected override string PackedIconOutlinePath => $"res://TH_Patchouli/ArtWorks/Relics/Outlines/{Id.Entry}.png";
    protected override string BigIconPath => $"res://TH_Patchouli/ArtWorks/Relics/{Id.Entry}.png";
    public override RelicRarity Rarity => RelicRarity.Rare;
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];
	 protected override IEnumerable<DynamicVar> CanonicalVars => (new DynamicVar[1]
    {
        new EnergyVar(1)
    });
	 public override decimal ModifyMaxEnergy(Player player, decimal amount)
    {
        if (player != base.Owner)
        {
            return amount;
        }
        return amount + 1;
    }

	public override Task BeforeCombatStart()
	{
		TrySetEnemyIntentHidden(hidden: true);
		return Task.CompletedTask;
	}

	public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		TrySetEnemyIntentHidden(hidden: true);
		return Task.CompletedTask;
	}

	public override Task AfterRemoved()
	{
		TrySetEnemyIntentHidden(hidden: false);
		return Task.CompletedTask;
	}

	private void TrySetEnemyIntentHidden(bool hidden)
	{
		if (Owner == null || !LocalContext.IsMe(Owner))
		{
			return;
		}

		ICombatState? combatState = Owner.Creature.CombatState;
		if (combatState == null)
		{
			return;
		}

		NCombatRoom? room = NCombatRoom.Instance;
		if (room == null)
		{
			return;
		}

		Color modulate = hidden ? Colors.Transparent : Colors.White;
		foreach (Creature enemy in combatState.Enemies.Where(e => e.IsAlive))
		{
			NCreature? enemyNode = room.GetCreatureNode(enemy);
			if (enemyNode == null)
			{
				continue;
			}

			foreach (NIntent intent in enemyNode.IntentContainer.GetChildren().OfType<NIntent>())
			{
				Control? holder = intent.GetNodeOrNull<Control>("%IntentHolder");
				if (holder != null)
				{
					holder.Modulate = modulate;
					holder.MouseFilter = hidden ? Control.MouseFilterEnum.Ignore : Control.MouseFilterEnum.Stop;
				}

				intent.MouseFilter = hidden ? Control.MouseFilterEnum.Ignore : Control.MouseFilterEnum.Stop;
			}
		}
	}
  

}
}

