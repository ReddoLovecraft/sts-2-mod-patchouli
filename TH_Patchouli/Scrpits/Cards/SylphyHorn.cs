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
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
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
	public sealed class SylphyHorn : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Wood };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

		public SylphyHorn() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

			int drawPerTrigger = Math.Max(0, DynamicVars.Cards.IntValue);
			if (drawPerTrigger <= 0)
			{
				return;
			}
			SfxCmd.Play("event:/sfx/characters/ironclad/ironclad_whirlwind");
			Color color = new Color("00b7746d");

			int pendingDraws = drawPerTrigger;
			int loopGuard = 0;
			while (pendingDraws > 0 && loopGuard++ < 500)
			{
				IEnumerable<CardModel> drawnCards = await CardPileCmd.Draw(choiceContext, 1, Owner);
				List<CardModel> drawnList = drawnCards.ToList();
				if (drawnList.Count == 0)
				{
					break;
				}

				pendingDraws -= drawnList.Count;

				foreach (CardModel drawn in drawnList)
				{
					NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NHorizontalLinesVfx.Create(color, 0.2));
					NRun.Instance?.GlobalUi.AddChildSafely(NSmokyVignetteVfx.Create(color, color));

					if (drawn.Owner == Owner && drawn.Type == CardType.Skill)
					{
						pendingDraws += drawPerTrigger;
					}
				}
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}
