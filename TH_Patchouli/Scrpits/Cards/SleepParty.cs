using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class SleepParty : PatchouliCardModel
	{
		public override bool GainsBlock => true;
		public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
		protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5, ValueProp.Move)];

		public SleepParty() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllAllies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			IEnumerable<Creature> players = from c in base.CombatState.GetTeammatesOf(base.Owner.Creature)
				where c != null && c.IsAlive && c.IsPlayer
				select c;
			foreach (Creature creature in players)
			{
				for(int i=0;i<players.Count()+1;i++)
				await CreatureCmd.GainBlock(creature, base.DynamicVars.Block, cardPlay);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Block.UpgradeValueBy(3);
		}
	}
}
