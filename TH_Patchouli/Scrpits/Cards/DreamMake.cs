using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Patchoulib.Scrpits.Main;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class DreamMake : PatchouliCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];
		public override bool CanBeGeneratedInCombat => false;

		public DreamMake() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			Player player = Owner;
			player.PopulateRelicGrabBagIfNecessary(player.RunState.Rng.UpFront);
			VfxCmd.PlayOnCreatureCenter(base.Owner.Creature, PatchouliVfxManager.ToPatchouliVfxPath("dream"));
			for (int i = 0; i < DynamicVars.Cards.IntValue; i++)
			{
				var relic = RelicFactory.PullNextRelicFromFront(player, player.RunState.Rng.Niche).ToMutable();
				await RelicCmd.Obtain(relic, player);
			}

			int baseCost = EnergyCost.GetWithModifiers(CostModifiers.None);
			if (baseCost >= 0)
			{
				EnergyCost.SetCustomBaseCost(baseCost * 2);
			}
		}

		protected override void OnUpgrade()
		{
			EnergyCost.UpgradeBy(-1);
		}
	}
}
