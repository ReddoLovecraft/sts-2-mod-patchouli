using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Patchouib.Scrpits.Main;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class SummerRed : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Fire };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromPower<TH_Patchouli.Scrpits.Powers.FireElement>(),
			HoverTipFactory.FromPower<IgnitePower>()
		];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(2, ValueProp.Move), new CardsVar(2)];

		public SummerRed() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Damage.UpgradeValueBy(boostAmount);
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			//NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NGroundFireVfx.Create(cardPlay.Target, VfxColor.Red));
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).WithHitFx(null, null, "blunt_attack.mp3").WithHitVfxNode(target => PatchouliVfxManager.CreateProjectileToTarget("fireball", Owner.Creature, target, new Vector2(0f, -180f),  new Vector2(0f, -40f))).Targeting(cardPlay.Target).Execute(choiceContext);
			await PowerCmd.Apply<IgnitePower>(choiceContext, cardPlay.Target, DynamicVars.Cards.IntValue, Owner.Creature, this);
		}

		public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
		{
			
			if (CombatState == null || Owner == null)
			{
				return;
			}
			if(cardPlay.Card==this)
			{
				await base.AfterCardPlayed(context, cardPlay);
				return;
			}
			if (cardPlay.Card is SummerRed)
			{
				return;
			}
			if (cardPlay.Card is PatchouliCardModel pcm && pcm.ElementTypes.Contains(ElementEnum.Fire))
			{
				await CardPileCmd.Add(this, PileType.Hand);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(1);
			DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}

