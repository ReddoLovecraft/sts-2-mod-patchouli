using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class CloudFormPower : CustomPowerModel
	{
		private bool _pendingDiscard;

		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Single;

		public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/CFP32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/CFP64.png";

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Block)];

		public override Task AfterApplied(Creature? applier, CardModel? cardSource)
		{
			PatchoulibEffectManager.OnPowerApplied(Owner, this);
			return Task.CompletedTask;
		}

		public override Task AfterRemoved(Creature oldOwner)
		{
			PatchoulibEffectManager.OnPowerRemoved(oldOwner, this);
			return Task.CompletedTask;
		}

		public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			if (target != Owner)
			{
				return 1m;
			}

			if (PileType.Draw.GetPile(Owner.Player).Cards.Count <= 0)
			{
				return 1m;
			}
			_pendingDiscard = true;
			return 0m;
		}

		public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			if (!_pendingDiscard)
			{
				return;
			}
			_pendingDiscard = false;

			CardPile drawPile = PileType.Draw.GetPile(Owner.Player);
			if (drawPile.Cards.Count <= 0)
			{
				return;
			}

			Flash();
			CardModel top = drawPile.Cards[^1];
			await CardCmd.Discard(choiceContext, top);
		}
	}
}
