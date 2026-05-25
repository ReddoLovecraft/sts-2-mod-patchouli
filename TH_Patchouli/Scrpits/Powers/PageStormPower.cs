using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class PageStormPower : CustomPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/GE32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/GE64.png";

		public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
		{
			if (CombatState == null)
			{
				return;
			}
			await CreatureCmd.Damage(new BlockingPlayerChoiceContext(), CombatState.HittableEnemies, Amount, ValueProp.Unpowered, Owner,null);
		}
	}
}
