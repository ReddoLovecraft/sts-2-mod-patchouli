using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scrpits.Cards;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class PrincessUndinePower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/WE32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/WE64.png";

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<WaterSpirit>(false)];

		public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
		{
			if (player != Owner.Player || CombatState == null)
			{
				return;
			}
			Flash();
			List<CardModel> cards = new();
			for (int i = 0; i < Amount; i++)
			{
				cards.Add(CombatState.CreateCard(ModelDb.Card<WaterSpirit>(), Owner.Player));
			}
			await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, addedByPlayer: true);
		}
	}
}
