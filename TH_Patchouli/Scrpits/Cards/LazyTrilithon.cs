using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.ValueProps;
using Patchouib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;
using TH_Patchouli.Scrpits.Powers;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class LazyTrilithon : PatchouliCardModel
	{
		public override bool GainsBlock => true;
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Dirt };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<SlowPower>()];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

		public LazyTrilithon() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NHorizontalLinesVfx.Create(new Color("fad68ab7"), 1.2000000476837158, movingRightwards: false));
			NCombatRoom.Instance?.RadialBlur(VfxPosition.Right);
			NGame.Instance?.ScreenShake(ShakeStrength.Strong, ShakeDuration.Normal, 180f + MegaCrit.Sts2.Core.Random.Rng.Chaotic.NextFloat(-10f, 10f));
			VfxCmd.PlayOnCreatureCenters(CombatState.HittableEnemies, "vfx/vfx_attack_blunt");
			await PowerCmd.Apply<SlowPower>(choiceContext, CombatState.HittableEnemies, 1, Owner.Creature, this);
			NGame.Instance?.DoHitStop(ShakeStrength.Strong, ShakeDuration.Normal);
				SfxCmd.Play("event:/sfx/enemy/enemy_attacks/ceremonial_beast/ceremonial_beast_plow_end");
			int mult = Math.Max(0, DynamicVars.Cards.IntValue);
			if (mult <= 1)
			{
				return;
			}
			int currentBlock = Owner.Creature.Block;
			if (currentBlock <= 0)
			{
				return;
			}
			int extra = currentBlock * (mult - 1);
			await CreatureCmd.GainBlock(Owner.Creature, extra, ValueProp.Move, cardPlay);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}

