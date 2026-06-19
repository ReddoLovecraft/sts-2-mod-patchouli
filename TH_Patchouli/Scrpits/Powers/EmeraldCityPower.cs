using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Combat;
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
	public sealed class EmeraldCityPower : CustomPowerModel
	{
		private const string EmeraldCityBarrierScenePath = "res://TH_Patchouli/ArtWorks/VFX/emerald_city_barrier.tscn";

		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/ECP32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/ECP64.png";

		public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			if (CombatState == null || target != Owner || amount <= 0 || dealer == null || dealer.Side == Owner.Side)
			{
				return;
			}
			Flash();
			await CreatureCmd.TriggerAnim(base.Owner, "Cast", base.Owner.Player.Character.CastAnimDelay);
			await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies, amount, ValueProp.Unpowered | ValueProp.Move, dealer: Owner, cardSource: null);
			TryPlayVfx();
		}

		public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
		{
			if (side != Owner.Side)
			{
				await PowerCmd.Decrement(this);
			}
		}

		private void TryPlayVfx()
		{
			Vector2? basePosition = PatchouliVfxManager.GetCreatureBasePosition(Owner);
			Vector2? hitboxCenter = PatchouliVfxManager.GetCreatureHitboxCenterPosition(Owner);
			Vector2? hitboxSize = PatchouliVfxManager.GetCreatureHitboxSize(Owner);
			Node? frontContainer = PatchouliVfxManager.GetCombatVfxContainer(inFront: true);
			Node? backContainer = PatchouliVfxManager.GetCombatVfxContainer(inFront: false);
			if (!basePosition.HasValue || !hitboxCenter.HasValue || !hitboxSize.HasValue || frontContainer == null || backContainer == null)
			{
				return;
			}

			float sideOffset = (hitboxSize.Value.X * 0.5f) + Mathf.Max(33f, hitboxSize.Value.X * 0.6f);
			float riseDistance = Mathf.Max(92f, hitboxSize.Value.Y * 0.65f);

			Vector2 leftTarget = new Vector2(hitboxCenter.Value.X - sideOffset, basePosition.Value.Y);
			Vector2 rightTarget = new Vector2(hitboxCenter.Value.X + sideOffset, basePosition.Value.Y);

			PatchouliVfxManager.SpawnScene(EmeraldCityBarrierScenePath, backContainer, node =>
			{
				if (node is NEmeraldCityBarrierVfx vfx)
				{
					vfx.TargetGlobalPosition = leftTarget;
					vfx.RiseDistance = riseDistance;
				}
			});

			PatchouliVfxManager.SpawnScene(EmeraldCityBarrierScenePath, frontContainer, node =>
			{
				if (node is NEmeraldCityBarrierVfx vfx)
				{
					vfx.TargetGlobalPosition = rightTarget;
					vfx.RiseDistance = riseDistance;
					vfx.FlipVisualX = true;
				}
			});
		}
	}
}

