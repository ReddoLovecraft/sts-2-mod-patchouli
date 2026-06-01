using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchoulib.Scrpits.Main;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class ForbiddenBookPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("Element")];
		
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/FBP32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/FBP64.png";



		public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
		{
			if (player != Owner.Player)
			{
				return;
			}

			IEnumerable<CardModel> candidates =
				from c in player.Character.CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
				let pcm = c as PatchouliCardModel
				where pcm != null && pcm.ElementTypes.Any(e => e != ElementEnum.None)
				select c;

			for (int i = 0; i < Amount; i++)
			{
				CardModel generated = CardFactory.GetDistinctForCombat(player, candidates, 1, player.RunState.Rng.CombatCardGeneration).FirstOrDefault();
				if (generated == null)
				{
					return;
				}

				generated.EnergyCost.SetThisTurnOrUntilPlayed(0, reduceOnly: true);
				await CardPileCmd.AddGeneratedCardToCombat(generated, PileType.Hand, addedByPlayer: true);
			}
		}
	}
}
