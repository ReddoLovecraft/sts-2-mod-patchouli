using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class WaterMoonPower : CustomPowerModel
	{
		private const string FlipVfxScenePath = "res://TH_Patchouli/ArtWorks/VFX/water_moon_flip_vfx.tscn";

		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/WMP32.png";
		public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/WMP64.png";

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Ethereal)];

		private CardModel? _sourceCard;

		public override Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
		{
			_sourceCard = cardSource;
			return Task.CompletedTask;
		}

		public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
		{
			if (Owner.Player == null || CombatState == null || cardPlay.Card.Owner != Owner.Player)
			{
				return;
			}
			if (_sourceCard != null && ReferenceEquals(cardPlay.Card, _sourceCard))
			{
				return;
			}

			int count = Math.Max(0, Amount);
			if (count <= 0)
			{
				return;
			}

			List<CardModel> generated = new List<CardModel>(count);
			for (int i = 0; i < count; i++)
			{
				CardModel clone = CombatState.CloneCard(cardPlay.Card);
				CardCmd.ApplyKeyword(clone, CardKeyword.Ethereal);
				generated.Add(clone);
			}

			Flash();
			TryPlayFlipVfx();
			await CardPileCmd.AddGeneratedCardsToCombat(generated, PileType.Hand, addedByPlayer: true);
		}

		public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
		{
			if (side == Owner.Side)
			{
				await PowerCmd.Remove(this);
			}
		}

		private void TryPlayFlipVfx()
		{
			Godot.Node? root = (NCombatRoom.Instance as Godot.Node) ?? (NGame.Instance as Godot.Node);
			if (root == null)
			{
				return;
			}

			SceneTree tree = root.GetTree();
			Godot.Collections.Array<Godot.Node> existing = tree.GetNodesInGroup("WaterMoonFlipVfx");
			for (int i = 0; i < existing.Count; i++)
			{
				Godot.Node n = existing[i];
				if (GodotObject.IsInstanceValid(n))
				{
					n.QueueFree();
				}
			}

			PatchouliVfxManager.SpawnScene(FlipVfxScenePath, root, node =>
			{
				node.AddToGroup("WaterMoonFlipVfx");
			});
		}
	}
}
