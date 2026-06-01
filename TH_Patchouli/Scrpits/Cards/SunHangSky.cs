using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using Patchouib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(SpellCardPool))]
	public sealed class SunHangSky : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = [ElementEnum.Sun, ElementEnum.Fire];
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromKeyword(CardKeyword.Retain),
			HoverTipFactory.FromPower<IgnitePower>(),
			HoverTipFactory.FromPower<StrengthPower>()
		];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(5), new DynamicVar("Power", 8m)];

		public SunHangSky() : base(1, CardType.Skill, CardRarity.Ancient, TargetType.Self)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(-boostAmount);
			DynamicVars["Power"].UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			PlaySunHangVfxAtScreenTopCenter();
			int divisor = Math.Max(1, DynamicVars.Cards.IntValue);
			int totalIgnite = CombatState.HittableEnemies.Sum(e => Math.Max(0, e.GetPower<IgnitePower>()?.Amount ?? 0));
			int strength = Math.Max(0, totalIgnite) / divisor;
			if (strength > 0)
			{
				await PowerCmd.Apply<StrengthPower>(Owner.Creature, strength, Owner.Creature, this);
			}
		}

		private static void PlaySunHangVfxAtScreenTopCenter()
		{
			if (NCombatRoom.Instance == null)
			{
				return;
			}

			Node? container = NCombatRoom.Instance.CombatVfxContainer;
			if (container == null)
			{
				return;
			}

			PackedScene? scene = ResourceLoader.Load<PackedScene>("res://TH_Patchouli/ArtWorks/VFX/sunhang.tscn");
			if (scene == null)
			{
				return;
			}

			Node2D vfx = scene.Instantiate<Node2D>(PackedScene.GenEditState.Disabled);

			Vector2 size = container.GetViewport().GetVisibleRect().Size;
			vfx.GlobalPosition = new Vector2(size.X * 0.5f, size.Y * 0.22f);
			container.AddChild(vfx);
		}

		public override async Task AfterCardRetained(CardModel card)
		{
			if (card != this || CombatState == null)
			{
				return;
			}

			int ignite = Math.Max(0, DynamicVars["Power"].IntValue);
			if (ignite <= 0)
			{
				return;
			}

			await PowerCmd.Apply<IgnitePower>(CombatState.HittableEnemies, ignite, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(-2);
			DynamicVars["Power"].UpgradeValueBy(2);
		}
	}
}
