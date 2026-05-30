using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using Patchouib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class BubbleShield : CustomPowerModel
	{
		private int _pendingDecrement;
		private int _originalAmount;
		private bool _originalAmountVarInitialized;
		private bool _suppressExplodeOnRemove;

		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override bool IsInstanced => true;
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/BS32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/BS64.png";

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(0)];

		public BubbleShield()
		{
		}

		public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
		{
			if (_originalAmount <= 0)
			{
				_originalAmount = (int)Math.Max(0m, amount);
			}
		}

		public override Task AfterApplied(Creature? applier, CardModel? cardSource)
		{
			if (_originalAmount <= 0)
			{
				_originalAmount = Amount;
			}
			EnsureOriginalAmountVar();
			PatchoulibEffectManager.OnPowerApplied(Owner, this);
			return Task.CompletedTask;
		}
		
		public override decimal ModifyHpLostBeforeOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			if (target != Owner || Amount <= 0 || amount <= 0)
			{
				return amount;
			}

			int prevent = (int)Math.Min(amount, Amount);
			if (prevent <= 0)
			{
				return amount;
			}

			_pendingDecrement += prevent;
			Flash();
			return amount - prevent;
		}
		public override Task AfterModifyingHpLostBeforeOsty()
	{
		Flash();
		return Task.CompletedTask;
	}
		public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			if (target != Owner)
			{
				return;
			}

			if (_pendingDecrement > 0)
			{
				int dec = _pendingDecrement;
				_pendingDecrement = 0;
				await PowerCmd.ModifyAmount(this, -dec, null, null);
			}
		}

		public override async Task AfterRemoved(Creature oldOwner)
		{
			PatchoulibEffectManager.OnPowerRemoved(oldOwner, this);
			if (_suppressExplodeOnRemove || _originalAmount <= 0 || oldOwner.CombatState == null)
			{
				return;
			}

			IReadOnlyList<Creature> enemies = oldOwner.CombatState.GetOpponentsOf(oldOwner);
			if (enemies.Count == 0)
			{
				return;
			}

			await CreatureCmd.Damage(new BlockingPlayerChoiceContext(), enemies, _originalAmount, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, dealer: oldOwner, cardSource: null);
		}

		private void EnsureOriginalAmountVar()
		{
			if (_originalAmountVarInitialized || _originalAmount <= 0)
			{
				return;
			}

			_originalAmountVarInitialized = true;

			try
			{
				DynamicVars.Cards.BaseValue = _originalAmount;
			}
			catch
			{
				try
				{
					var dynVarsProp = GetType().GetProperty("DynamicVars");
					object? dynVars = dynVarsProp?.GetValue(this);
					if (dynVars == null)
					{
						return;
					}

					object? cardsVar = dynVars.GetType().GetProperty("Cards")?.GetValue(dynVars);
					if (cardsVar == null)
					{
						return;
					}

					var baseValueProp = cardsVar.GetType().GetProperty("BaseValue");
					if (baseValueProp?.CanWrite == true)
					{
						baseValueProp.SetValue(cardsVar, (decimal)_originalAmount);
					}
				}
				catch
				{
				}
			}
		}
	}
}
