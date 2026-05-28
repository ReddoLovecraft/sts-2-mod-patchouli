using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using System;
using System.Collections.Generic;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class RainbowShot : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes =
		[
			ElementEnum.Gold,
			ElementEnum.Wood,
			ElementEnum.Water,
			ElementEnum.Fire,
			ElementEnum.Dirt,
			ElementEnum.Sun,
			ElementEnum.Lunar,
		];

		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("Element")];
		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(66m, ValueProp.Move)];

		public RainbowShot() : base(7, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Damage.UpgradeValueBy(boostAmount);
		}

		public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
		{
			if (originalCost < 0m || card != this)
			{
				modifiedCost = originalCost;
				return false;
			}

			int kinds = ToolBox.GetElementKinds(Owner.Creature);
			modifiedCost = originalCost - Math.Max(0, kinds);
			return (int)modifiedCost != (int)originalCost;
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (CombatState == null)
			{
				return;
			}

			await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(11);
		}
	}
}
