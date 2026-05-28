using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers.NewPowers
{
	public sealed class HephaestusHammerPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;

		public override async Task AfterCombatEnd(CombatRoom room)
		{
			Player? player = Owner.Player;
			if (player == null)
			{
				return;
			}

			List<CardModel> upgradable = PileType.Deck.GetPile(player).Cards.Where(c => c.IsUpgradable).ToList();
			if (upgradable.Count <= 0)
			{
				return;
			}

			int upgradeCount = System.Math.Min(Amount, upgradable.Count);
			if (upgradeCount <= 0)
			{
				return;
			}

			Rng rng = player.RunState.Rng.Niche;
			for (int i = 0; i < upgradeCount; i++)
			{
				int idx = rng.NextInt(upgradable.Count);
				CardModel card = upgradable[idx];
				upgradable.RemoveAt(idx);

				player.RunState.CurrentMapPointHistoryEntry?.GetEntry(player.NetId).UpgradedCards.Add(card.Id);
				card.UpgradeInternal();
				card.FinalizeUpgradeInternal();
				CardCmd.Preview(card);
			}

			Flash();
			await Task.CompletedTask;
		}
	}
}
