using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Random;
using Patchoulib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scrpits.Main;
using TH_Patchouli.Scrpits.Powers;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class ElementPredictPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("Element"), HoverTipFactory.ForEnergy(this)];
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/EPP232.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/EPP264.png";



		public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
		{
			if (player != Owner.Player)
			{
				return;
			}

			int amount = Math.Max(0, Amount);
			if (amount > 0)
			{
				await PlayerCmd.GainEnergy(amount, player);
			}

			if (amount > 0)
			{
				Rng rng = player.RunState.Rng.CombatCardGeneration;
				for (int i = 0; i < amount; i++)
				{
					int roll = rng.NextInt(0, 7);
					switch (roll)
					{
						case 0:
							await PowerCmd.Apply<SunElement>(choiceContext, Owner, 1, Owner, null);
							break;
						case 1:
							await PowerCmd.Apply<LunarElement>(choiceContext, Owner, 1, Owner, null);
							break;
						case 2:
							await PowerCmd.Apply<FireElement>(choiceContext, Owner, 1, Owner, null);
							break;
						case 3:
							await PowerCmd.Apply<WaterElement>(choiceContext, Owner, 1, Owner, null);
							break;
						case 4:
							await PowerCmd.Apply<WoodElement>(choiceContext, Owner, 1, Owner, null);
							break;
						case 5:
							await PowerCmd.Apply<GoldElement>(choiceContext, Owner, 1, Owner, null);
							break;
						case 6:
							await PowerCmd.Apply<DirtElement>(choiceContext, Owner, 1, Owner, null);
							break;
					}
				}
			}

			await PowerCmd.Remove(this);
		}
	}
}

