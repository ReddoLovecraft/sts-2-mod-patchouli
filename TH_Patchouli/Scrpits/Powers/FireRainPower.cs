using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchouib.Scrpits.Main;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class FireRainPower : CustomPowerModel
	{
		private const string FireRainConeScenePath = "res://TH_Patchouli/ArtWorks/VFX/fire_rain_cone.tscn";

		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/FRP32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/FRP64.png";

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(0)];
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IgnitePower>()];

		public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
		{
			if (cardSource is PatchouliCardModel pcm)
			{
				int ignite = pcm.DynamicVars["Power"].IntValue;
				DynamicVars.Cards.BaseValue = ignite;
			}
			await Task.CompletedTask;
		}

		public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
		{
			if (player != Owner.Player || CombatState == null)
			{
				return;
			}

			int ignite = DynamicVars.Cards.IntValue;
			if (ignite <= 0 || Amount <= 0)
			{
				return;
			}

			TryPlayVfx();

			for (int i = 0; i < Amount; i++)
			{
				Flash();
				await PowerCmd.Apply<IgnitePower>(CombatState.HittableEnemies, ignite, Owner, null);
			}
		}

		private void TryPlayVfx()
		{
			if (CombatState == null)
			{
				return;
			}

			foreach (Creature enemy in CombatState.HittableEnemies)
			{
				PatchouliVfxManager.PlayOnCreatureBase(enemy, FireRainConeScenePath, new Vector2(0f, -75f), (node, targetPos) =>
				{
					if (node is NFireRainConeVfx vfx)
					{
						vfx.HitGlobalPosition = targetPos;
						vfx.StartHeight = 70f;
						return;
					}
					node.GlobalPosition = targetPos;
					PatchouliVfxManager.TrySetPropertyIfExists(node, "HitGlobalPosition", targetPos);
					PatchouliVfxManager.TrySetPropertyIfExists(node, "StartHeight", 70f);
				});
			}
		}
	}
}
