using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(SpellCardPool))]
	public sealed partial class SquareRingSword : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Sun, ElementEnum.Dirt };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8m, ValueProp.Move)];

		public SquareRingSword() : base(1, CardType.Attack, CardRarity.Ancient, TargetType.AllEnemies)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Damage.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (CombatState == null)
			{
				return;
			}

			NSquareRingSwordBounceVfx? vfx = null;
			if (TestMode.IsOff && NCombatRoom.Instance?.CombatVfxContainer != null)
			{
				var ownerNode = NCombatRoom.Instance.GetCreatureNode(Owner.Creature);
				if (ownerNode != null)
				{
					vfx = new NSquareRingSwordBounceVfx(ownerNode.VfxSpawnPosition);
					NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
				}
			}

			try
			{
				decimal damage = Math.Max(0m, DynamicVars.Damage.BaseValue);
				while (damage >= 1m)
				{
					var enemies = CombatState.HittableEnemies;
					if (enemies.Count <= 0)
					{
						return;
					}

					Rng rng = Owner.RunState.Rng.CombatTargets;
					int idx = rng.NextInt(enemies.Count);
					Creature target = enemies[idx];

					if (vfx != null && GodotObject.IsInstanceValid(vfx))
					{
						Vector2? targetPos = PatchouliVfxManager.GetCreatureHitboxCenterPosition(target);
						if (targetPos.HasValue)
						{
							await vfx.BounceTo(targetPos.Value);
						}
					}

					await DamageCmd.Attack(damage).FromCard(this).Targeting(target).Execute(choiceContext);
					damage = Math.Floor(damage / 2m);
				}
			}
			finally
			{
				if (vfx != null && GodotObject.IsInstanceValid(vfx))
				{
					vfx.QueueFreeSafely();
				}
			}
		}

		private sealed partial class NSquareRingSwordBounceVfx : Node2D
		{
			private const float BounceSeconds = 0.50f;

			private static readonly string CircleScenePath = "res://TH_Patchouli/ArtWorks/VFX/circle.tscn";
			private static readonly string ExtraTexturePath = "res://TH_Patchouli/ArtWorks/VFX/touhoueffect/patchouli/spellBulletAe000.png";
			private static readonly string AdditiveMaterialPath = "res://TH_Patchouli/ArtWorks/VFX/canvas_item_material_additive_shared.tres";

			private Node2D? _rotator;
			private Sprite2D? _extra;

			public NSquareRingSwordBounceVfx(Vector2 startGlobalPosition)
			{
				GlobalPosition = startGlobalPosition;
			}

			public override void _Ready()
			{
				ZAsRelative = false;
				ZIndex = 85;

				_rotator = new Node2D { Name = "Rotator" };
				AddChild(_rotator);

				Material? additive = ResourceLoader.Load<Material>(AdditiveMaterialPath, null, ResourceLoader.CacheMode.Reuse);
				Texture2D? extraTex = ResourceLoader.Load<Texture2D>(ExtraTexturePath, null, ResourceLoader.CacheMode.Reuse);
				PackedScene? circleScene = ResourceLoader.Load<PackedScene>(CircleScenePath, null, ResourceLoader.CacheMode.Reuse);

				if (circleScene != null)
				{
					Node2D circle = circleScene.Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
					if (circle.GetNodeOrNull<AnimationPlayer>("AnimationPlayer") is AnimationPlayer ap)
					{
						ap.Stop();
						ap.Autoplay = string.Empty;
					}
					if (circle.GetNodeOrNull<Godot.Timer>("LifeTimer") is Godot.Timer timer)
					{
						timer.Autostart = false;
						timer.Stop();
					}
					if (circle.GetNodeOrNull<Node2D>("Visual") is Node2D visual)
					{
						visual.Position = Vector2.Zero;
					}

					_rotator.AddChild(circle);
				}

				if (extraTex != null)
				{
					_extra = new Sprite2D
					{
						Texture = extraTex,
						Centered = true,
						Scale = new Vector2(1.35f, 1.35f),
						Material = additive
					};
					_rotator.AddChild(_extra);
				}

				_rotator.Scale = new Vector2(0.55f, 0.55f);
			}

			public override void _Process(double delta)
			{
				if (_rotator != null)
				{
					_rotator.Rotation += (float)delta * 8.0f;
				}
				if (_extra != null)
				{
					_extra.Rotation -= (float)delta * 10.0f;
				}
			}

			public async Task BounceTo(Vector2 targetGlobalPosition)
			{
				Vector2 start = GlobalPosition;
				Vector2 end = targetGlobalPosition;
				Vector2 delta = end - start;

				if (delta.LengthSquared() < 0.01f)
				{
					return;
				}

				float dist = Mathf.Max(1f, delta.Length());
				float height = Mathf.Clamp(dist * 0.18f, 80f, 180f);
				Vector2 mid = (start + end) * 0.5f + new Vector2(0f, -height);

				Tween tween = CreateTween();
				tween.TweenMethod(Callable.From((float t) =>
				{
					if (!GodotObject.IsInstanceValid(this))
					{
						return;
					}
					GlobalPosition = QuadraticBezier(start, mid, end, t);
				}), 0f, 1f, BounceSeconds).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);

				await Cmd.Wait(BounceSeconds);
			}

			private static Vector2 QuadraticBezier(Vector2 a, Vector2 b, Vector2 c, float t)
			{
				float u = 1f - t;
				return (u * u * a) + (2f * u * t * b) + (t * t * c);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(4);
		}
	}
}
