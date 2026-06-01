using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;
using Patchoulib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(SpellCardPool))]
	public sealed class WhisperRoot : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Wood, ElementEnum.Lunar };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

		public WhisperRoot() : base(2, CardType.Skill, CardRarity.Ancient, TargetType.Self)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			TryPlayWhisperRootStarMotes();
			HashSet<CardModel> before = PileType.Hand.GetPile(Owner).Cards.ToHashSet();
			await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
			foreach (CardModel c in PileType.Hand.GetPile(Owner).Cards)
			{
				if (!before.Contains(c))
				{
					c.EnergyCost.SetThisTurnOrUntilPlayed(0, reduceOnly: true);
				}
			}
		}

		private void TryPlayWhisperRootStarMotes()
		{
			if (TestMode.IsOn || NCombatRoom.Instance == null)
			{
				return;
			}

			var creatureNode = NCombatRoom.Instance.GetCreatureNode(Owner.Creature);
			if (creatureNode == null)
			{
				return;
			}

			Material? additive = ResourceLoader.Load<Material>("res://TH_Patchouli/ArtWorks/VFX/canvas_item_material_additive_shared.tres", null, ResourceLoader.CacheMode.Reuse);
			Texture2D? tex = ResourceLoader.Load<Texture2D>("res://TH_Patchouli/ArtWorks/VFX/touhoueffect/patchouli/spellBulletCb000.png", null, ResourceLoader.CacheMode.Reuse);
			if (additive == null || tex == null)
			{
				return;
			}

			Node2D anchor = new Node2D
			{
				Name = "WhisperRootStarMotes",
				ZAsRelative = false,
				ZIndex = 90
			};
			creatureNode.AddChild(anchor);

			Vector2 hitboxSize = creatureNode.Hitbox.Size;
			var rng = new RandomNumberGenerator();
			rng.Randomize();

			const float totalSeconds = 1.10f;
			const float postSeconds = 0.80f;

			TaskHelper.RunSafely(PlayMoteLoop(anchor, tex, additive, hitboxSize, rng, totalSeconds, postSeconds));
		}

		private static async Task PlayMoteLoop(Node2D anchor, Texture2D tex, Material additive, Vector2 hitboxSize, RandomNumberGenerator rng, float totalSeconds, float postSeconds)
		{
			float elapsed = 0f;
			while (elapsed < totalSeconds && GodotObject.IsInstanceValid(anchor))
			{
				SpawnMote(anchor, tex, additive, hitboxSize, rng);
				SpawnMote(anchor, tex, additive, hitboxSize, rng);
				SpawnMote(anchor, tex, additive, hitboxSize, rng);
				float wait = rng.RandfRange(0.05f, 0.09f);
				elapsed += wait;
				await PatchouliVfxManager.WaitSeconds(wait);
			}

			await PatchouliVfxManager.WaitSeconds(postSeconds);
			if (GodotObject.IsInstanceValid(anchor))
			{
				anchor.QueueFree();
			}
		}

		private static void SpawnMote(Node2D anchor, Texture2D tex, Material additive, Vector2 hitboxSize, RandomNumberGenerator rng)
		{
			float radiusX = hitboxSize.X * 0.65f;
			float radiusY = hitboxSize.Y * 0.70f;
			Vector2 offset = new Vector2(rng.RandfRange(-radiusX, radiusX), rng.RandfRange(-radiusY, radiusY));
			float baseScale = Mathf.Clamp(hitboxSize.Y / 320f, 0.75f, 1.45f);
			float scale = rng.RandfRange(0.35f, 0.65f) * baseScale;

			var mote = new Sprite2D
			{
				Texture = tex,
				Centered = true,
				Material = additive,
				Position = offset,
				Scale = Vector2.One * scale,
				Rotation = rng.RandfRange(-1.2f, 1.2f),
				Modulate = new Color(1f, 1f, 1f, 0f),
				ZAsRelative = false,
				ZIndex = 12
			};
			anchor.AddChild(mote);

			Vector2 drift = new Vector2(rng.RandfRange(-10f, 10f), rng.RandfRange(-30f, -10f));
			Tween tween = mote.CreateTween();
			tween.TweenProperty(mote, "modulate:a", 0.95f, 0.12).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
			tween.TweenProperty(mote, "position", mote.Position + drift, 0.55).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
			tween.TweenProperty(mote, "modulate:a", 0f, 0.22);
			tween.TweenCallback(Callable.From(mote.QueueFree));
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}
