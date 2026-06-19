using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System.Threading.Tasks;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class EmeraldMegalopolisPower : CustomPowerModel
	{
		private bool _isStartingOwnerTurn;
		private int _pendingNextTurnBlock;

		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Single;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Block)];
		
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/EMP232.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/EMP264.png";
		public override Task AfterApplied(Creature? applier, CardModel? cardSource)
		{
			Owner.BlockChanged += OnOwnerBlockChanged;
			return Task.CompletedTask;
		}

		public override Task AfterRemoved(Creature oldOwner)
		{
			oldOwner.BlockChanged -= OnOwnerBlockChanged;
			_isStartingOwnerTurn = false;
			_pendingNextTurnBlock = 0;
			return Task.CompletedTask;
		}

		public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
		{
			if (side == Owner.Side)
			{
				_isStartingOwnerTurn = true;
			}
			return Task.CompletedTask;
		}

		public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
		{
			if (side != Owner.Side)
			{
				return;
			}

			_isStartingOwnerTurn = false;

			int block = _pendingNextTurnBlock;
			if (block <= 0)
			{
				return;
			}

			_pendingNextTurnBlock = 0;
			await PowerCmd.Apply<BlockNextTurnPower>(new ThrowingPlayerChoiceContext(), Owner, block, Owner, null);
		}

		private void OnOwnerBlockChanged(int oldBlock, int newBlock)
		{
			if (CombatState == null || oldBlock <= newBlock)
			{
				return;
			}

			int lost = oldBlock - newBlock;
			if (lost <= 0)
			{
				return;
			}

			Flash();

			if (_isStartingOwnerTurn)
			{
				_pendingNextTurnBlock += lost;
				return;
			}

			_ = PowerCmd.Apply<BlockNextTurnPower>(new ThrowingPlayerChoiceContext(), Owner, lost, Owner, null);
		}
	}
}


