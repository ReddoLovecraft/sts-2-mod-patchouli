using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using Patchoulib.Scrpits.Main;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scrpits.Powers;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class WitchTea : PatchouliCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate];
		public override bool CanBeGeneratedInCombat => false;
		public WitchTea() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			while (Owner.HasOpenPotionSlots)
			{
				PotionModel potion = PotionFactory.CreateRandomPotionOutOfCombat(Owner, Owner.RunState.Rng.CombatPotionGeneration).ToMutable();
				if (!(await PotionCmd.TryToProcure(potion, Owner)).success)
				{
					break;
				}
			}

			if (IsUpgraded)
			{
				await PowerCmd.Apply<WitchTeaEndOfCombatPower>(Owner.Creature, 1, Owner.Creature, this);
			}
		}
	}
}

