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
using Patchouib.Scrpits.Main;
using Patchoulib.Scrpits.Main;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Powers;


namespace TH_Patchouli.Relics
{
[Pool(typeof(PatchouliRelicPool))]
public class Instrument : CustomRelicModel
{
	public override string PackedIconPath => $"res://TH_Patchouli/ArtWorks/Relics/{Id.Entry}.png";
    protected override string PackedIconOutlinePath => $"res://TH_Patchouli/ArtWorks/Relics/Outlines/{Id.Entry}.png";
    protected override string BigIconPath => $"res://TH_Patchouli/ArtWorks/Relics/{Id.Entry}.png";
    public override RelicRarity Rarity => RelicRarity.Rare;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<ArtifactPower>()];
	private int _attacksPlayedThisTurn;

	private int _skillsPlayedThisTurn;

	private int _powersPlayedThisTurn;
		private int AttacksPlayedThisTurn
	{
		get
		{
			return _attacksPlayedThisTurn;
		}
		set
		{
			AssertMutable();
			_attacksPlayedThisTurn = value;
		}
	}

	private int SkillsPlayedThisTurn
	{
		get
		{
			return _skillsPlayedThisTurn;
		}
		set
		{
			AssertMutable();
			_skillsPlayedThisTurn = value;
		}
	}

	private int PowersPlayedThisTurn
	{
		get
		{
			return _powersPlayedThisTurn;
		}
		set
		{
			AssertMutable();
			_powersPlayedThisTurn = value;
		}
	}

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (side == base.Owner.Creature.Side)
		{
		  	AttacksPlayedThisTurn = 0;
			SkillsPlayedThisTurn = 0;
			PowersPlayedThisTurn = 0;
		}
	}
	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner == base.Owner && CombatManager.Instance.IsInProgress)
		{
			AttacksPlayedThisTurn += ((cardPlay.Card.Type == CardType.Attack) ? 1 : 0);
			SkillsPlayedThisTurn += ((cardPlay.Card.Type == CardType.Skill) ? 1 : 0);
			PowersPlayedThisTurn += ((cardPlay.Card.Type == CardType.Power) ? 1 : 0);
			if (AttacksPlayedThisTurn > 0 && SkillsPlayedThisTurn > 0 && PowersPlayedThisTurn > 0)
			{
				Flash();
			    await PowerCmd.Apply<ArtifactPower>(context, Owner.Creature, 1, Owner.Creature, null);
				AttacksPlayedThisTurn --;
				SkillsPlayedThisTurn --;
				PowersPlayedThisTurn --;
			}
		}
	}

}
}


