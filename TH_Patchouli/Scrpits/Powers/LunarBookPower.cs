using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers.NewPowers
{
	public sealed class LunarBookPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;

		public override async Task AfterCombatEnd(CombatRoom room)
		{
			Player? player = Owner.Player;
			if (player?.PlayerCombatState == null)
			{
				return;
			}

			List<CardModel> cards = player.PlayerCombatState.AllCards.ToList();
			if (cards.Count <= 0)
			{
				return;
			}

			int count = System.Math.Min(Amount, cards.Count);
			if (count <= 0)
			{
				return;
			}

			CardSelectorPrefs prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, count);
			List<CardModel> selected = (await CardSelectCmd.FromSimpleGrid(new BlockingPlayerChoiceContext(), cards, player, prefs)).ToList();
			if (selected.Count <= 0)
			{
				return;
			}

			foreach (CardModel s in selected)
			{
				CardModel deckCard = player.RunState.CreateCard(s.CanonicalInstance, player);
				await CardPileCmd.Add(deckCard, PileType.Deck);
			}

			Flash();
		}
	}
}
