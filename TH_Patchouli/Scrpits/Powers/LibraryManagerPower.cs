using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchouib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class LibraryManagerPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/LMP32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/LMP64.png";

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Retain)];

		public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
		{
			if (player != Owner.Player)
			{
				return;
			}
			Flash();
			int max = Math.Min(Amount, PileType.Hand.GetPile(Owner.Player).Cards.Count);
			if (max <= 0)
			{
				return;
			}
			IEnumerable<CardModel> selected = await CardSelectCmd.FromHand(choiceContext, Owner.Player, new CardSelectorPrefs(base.SelectionScreenPrompt, 0, max), null, null);
			foreach (CardModel c in selected)
			{
				CardCmd.ApplyKeyword(c, CardKeyword.Retain);
			}
		}
		public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
		{
			if (side != Owner.Side || CombatState == null)
			{
				return;
			}
				Flash();
			int max = Math.Min(Amount, PileType.Hand.GetPile(Owner.Player).Cards.Count);
			if (max <= 0)
			{
				return;
			}
			IEnumerable<CardModel> selected = await CardSelectCmd.FromHand(choiceContext, Owner.Player, new CardSelectorPrefs(base.SelectionScreenPrompt, 0, max), null, null);
			foreach (CardModel c in selected)
			{
				CardCmd.ApplyKeyword(c, CardKeyword.Retain);
			}
		}
	}
}

