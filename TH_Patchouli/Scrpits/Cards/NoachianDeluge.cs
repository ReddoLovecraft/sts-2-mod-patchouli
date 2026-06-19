using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchouib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(SpellCardPool))]
	public sealed class NoachianDeluge : PatchouliCardModel
	{
		private const string DelugeWaterColumnScenePath = "res://TH_Patchouli/ArtWorks/VFX/deluge_water_column.tscn";
		private static readonly List<ElementEnum> _elementTypes = [ElementEnum.Dirt, ElementEnum.Water];
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<FreezePower>()];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3)];

		public NoachianDeluge() : base(1, CardType.Skill, CardRarity.Ancient, TargetType.AllEnemies)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			int freezePerDiscard = Math.Max(0, DynamicVars.Cards.IntValue);
			if (freezePerDiscard <= 0)
			{
				return;
			}
			List<CardModel> handCards = PileType.Hand.GetPile(Owner).Cards.ToList();
			if (handCards.Count == 0)
			{
				return;
			}

			foreach (CardModel card in handCards)
			{
				await CardCmd.Discard(choiceContext, card);
				foreach (Creature enemy in CombatState.HittableEnemies.ToList())
				{
					TryPlayVfx(enemy);
					await PowerCmd.Apply<FreezePower>(choiceContext, enemy, freezePerDiscard, Owner.Creature, this);
				}
			}
		}
		private void TryPlayVfx(Creature enemy)
		{
			PatchouliVfxManager.PlayOnCreatureBase(enemy, DelugeWaterColumnScenePath, configure: (node, targetPos) =>
			{
				if (node is NDelugeWaterColumnVfx vfx)
				{
					vfx.GroundGlobalPosition = targetPos;
					return;
				}
				node.GlobalPosition = targetPos;
				PatchouliVfxManager.TrySetPropertyIfExists(node, "GroundGlobalPosition", targetPos);
			});
		}
		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}

