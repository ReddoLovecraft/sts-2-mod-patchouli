using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class DirtToMountain : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = [ElementEnum.Dirt];
		public override List<ElementEnum> ElementTypes => _elementTypes;

		public override bool GainsBlock => true;
		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromPower<PlatingPower>()
		];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(8m, ValueProp.Move), new CardsVar(8), new DynamicVar("Power", 1)];

		public DirtToMountain() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Block.UpgradeValueBy(boostAmount);
			DynamicVars.Cards.UpgradeValueBy(-boostAmount);
			DynamicVars["Power"].UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
			int threshold = Math.Max(1, DynamicVars.Cards.IntValue);
			int triggers = Owner.Creature.Block / threshold;
			int platingEach = Math.Max(0, DynamicVars["Power"].IntValue);
			if (triggers > 0 && platingEach > 0)
			{
				await PowerCmd.Apply<PlatingPower>(Owner.Creature, triggers * platingEach, Owner.Creature, this);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Block.UpgradeValueBy(2);
			DynamicVars.Cards.UpgradeValueBy(-3);
		}
	}
}

