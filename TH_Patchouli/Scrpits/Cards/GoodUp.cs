using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
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
	public sealed class GoodUp : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = [ElementEnum.Dirt, ElementEnum.Wood];
		public override List<ElementEnum> ElementTypes => _elementTypes;

		public override bool GainsBlock => true;
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(5)];

		public GoodUp() : base(1, CardType.Skill, CardRarity.Ancient, TargetType.Self)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);	
			List<PowerModel> debuffs = Owner.Creature.Powers.Where(p => p.Type == PowerType.Debuff).ToList();
			if (debuffs.Count == 0)
			{
				return;
			}
			Color color = new Color("81ff6a80");
			NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NHorizontalLinesVfx.Create(color, 2.8f));
			HashSet<ModelId> removedTypes = new HashSet<ModelId>();
			foreach (PowerModel debuff in debuffs)
			{
				removedTypes.Add(debuff.Id);
				await PowerCmd.Remove(debuff);
			}

			int gain = removedTypes.Count * Math.Max(0, DynamicVars.Cards.IntValue);
			if (gain > 0)
			{
				await CreatureCmd.GainBlock(Owner.Creature, gain, ValueProp.Move, cardPlay);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(3);
		}
	}
}
