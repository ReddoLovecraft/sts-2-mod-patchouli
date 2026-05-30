using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
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
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Powers;

namespace TH_Patchouli.Scrpits.Powers 
{
	public sealed class TransformPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Single;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("Element")];

		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/TP32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/TP64.png";

		public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
		{
			if (player != Owner.Player)
			{
				return;
			}

			int total = 0;
			foreach (PowerModel p in Owner.Powers.ToList())
			{
				if (p is GoldElement or WoodElement or WaterElement or FireElement or DirtElement or SunElement or LunarElement)
				{
					int amt = Math.Max(0, p.Amount);
					if (amt > 0)
					{
						total += amt;
						await PowerCmd.ModifyAmount(p, -amt, Owner, null);
					}
				}
			}
			if (total <= 0)
			{
				return;
			}
			this.Flash();
			for (int i = 0; i < total; i++)
			{
				ToolBox.GainElementRandomly(1,Owner);
			}
		}
	}
}
