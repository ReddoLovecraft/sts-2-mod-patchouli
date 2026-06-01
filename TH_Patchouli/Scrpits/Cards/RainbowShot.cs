using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed partial class RainbowShot : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes =
		[
			ElementEnum.Gold,
			ElementEnum.Wood,
			ElementEnum.Water,
			ElementEnum.Fire,
			ElementEnum.Dirt,
			ElementEnum.Sun,
			ElementEnum.Lunar,
		];

		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("Element")];
		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(66m, ValueProp.Move)];

		public RainbowShot() : base(7, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Damage.UpgradeValueBy(boostAmount);
		}

		public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
		{
			if (originalCost < 0m || card != this)
			{
				modifiedCost = originalCost;
				return false;
			}

			int kinds = ToolBox.GetElementKinds(Owner.Creature);
			modifiedCost = originalCost - Math.Max(0, kinds);
			return (int)modifiedCost != (int)originalCost;
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (CombatState == null)
			{
				return;
			}

			ArgumentNullException.ThrowIfNull(cardPlay.Target);
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
				.FromCard(this)
				.Targeting(cardPlay.Target)
				.WithAttackerAnim("Cast", 0.5f)
				.BeforeDamage(async delegate
				{
					PlayRainbowHyperbeamVfx(Owner.Creature, cardPlay.Target);
					await Cmd.Wait(0.625f);
					PlayRainbowHyperbeamImpactVfx(Owner.Creature, cardPlay.Target);
				})
				.Execute(choiceContext);
		}

		private static void PlayRainbowHyperbeamVfx(Creature owner, Creature target)
		{
			if (NCombatRoom.Instance?.CombatVfxContainer == null)
			{
				return;
			}

			var ownerNode = NCombatRoom.Instance.GetCreatureNode(owner);
			Vector2? targetHitboxCenter = PatchouliVfxManager.GetCreatureHitboxCenterPosition(target);
			if (ownerNode == null || targetHitboxCenter == null)
			{
				return;
			}

			var beam = new NRainbowHyperbeamBeamVfx(ownerNode.VfxSpawnPosition, targetHitboxCenter.Value);
			NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(beam);
		}

		private static void PlayRainbowHyperbeamImpactVfx(Creature owner, Creature target)
		{
			if (NCombatRoom.Instance?.CombatVfxContainer == null)
			{
				return;
			}

			var ownerNode = NCombatRoom.Instance.GetCreatureNode(owner);
			Vector2? targetHitboxCenter = PatchouliVfxManager.GetCreatureHitboxCenterPosition(target);
			if (ownerNode == null || targetHitboxCenter == null)
			{
				return;
			}

			var impact = new NRainbowHyperbeamImpactVfx(ownerNode.VfxSpawnPosition, targetHitboxCenter.Value);
			NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(impact);
		}

		private sealed partial class NRainbowHyperbeamBeamVfx : Node2D
		{
			private const float AnticipationSeconds = 0.625f;
			private const float LaserSeconds = 0.8f;
			private const float PostLaserSeconds = 2.0f;

			private readonly Vector2 _source;
			private readonly Vector2 _target;

			private Node2D? _anticipation;
			private Sprite2D? _anticipationGlow;

			private Node2D? _laser;
			private Line2D? _laserOuter;
			private Line2D? _laserInner;
			private Sprite2D? _laserEndFlare;

			private Gradient? _rainbowGradient;
			private float _time;

			public NRainbowHyperbeamBeamVfx(Vector2 source, Vector2 target)
			{
				_source = source;
				_target = target;
			}

			public override void _Ready()
			{
				GlobalPosition = _source;

				Vector2 toTarget = _target - _source;
				Rotation = toTarget.Angle();

				float length = Mathf.Max(64f, toTarget.Length());

				_anticipation = new Node2D { Name = "Anticipation" };
				AddChild(_anticipation);

				_anticipationGlow = new Sprite2D
				{
					Name = "Glow",
					Texture = ResourceLoader.Load<Texture2D>("res://TH_Patchouli/ArtWorks/VFX/light.png"),
					Centered = true,
					Scale = new Vector2(0.55f, 0.55f),
					Modulate = new Color(1f, 1f, 1f, 0.0f),
					Material = ResourceLoader.Load<Material>("res://TH_Patchouli/ArtWorks/VFX/canvas_item_material_additive_shared.tres")
				};
				_anticipation.AddChild(_anticipationGlow);

				_laser = new Node2D { Name = "Laser", Visible = false };
				AddChild(_laser);

				_rainbowGradient = CreateRainbowGradient(0f);

				_laserOuter = CreateLaserLine("Outer", 160f, 0.55f, length, _rainbowGradient);
				_laserInner = CreateLaserLine("Inner", 90f, 0.85f, length, _rainbowGradient);
				_laser.AddChild(_laserOuter);
				_laser.AddChild(_laserInner);

				_laserEndFlare = new Sprite2D
				{
					Name = "EndFlare",
					Texture = ResourceLoader.Load<Texture2D>("res://TH_Patchouli/ArtWorks/VFX/light.png"),
					Centered = true,
					Position = new Vector2(length, 0f),
					Scale = new Vector2(0.60f, 0.60f),
					Modulate = new Color(1f, 1f, 1f, 0.0f),
					Material = ResourceLoader.Load<Material>("res://TH_Patchouli/ArtWorks/VFX/canvas_item_material_additive_shared.tres")
				};
				_laser.AddChild(_laserEndFlare);

				TaskHelper.RunSafely(PlaySequence());
			}

			public override void _Process(double delta)
			{
				_time += (float)delta;

				if (_rainbowGradient != null)
				{
					UpdateRainbowGradient(_rainbowGradient, _time * 0.55f);
				}

				if (_anticipationGlow != null && _anticipationGlow.Visible)
				{
					Color c = Color.FromHsv(Mathf.PosMod(_time * 0.35f, 1f), 0.85f, 1.0f, _anticipationGlow.Modulate.A);
					_anticipationGlow.Modulate = c;
				}

				if (_laserEndFlare != null && _laserEndFlare.Visible)
				{
					Color c = Color.FromHsv(Mathf.PosMod(_time * 0.55f, 1f), 0.85f, 1.0f, _laserEndFlare.Modulate.A);
					_laserEndFlare.Modulate = c;
				}
			}

			private async Task PlaySequence()
			{
				if (_anticipationGlow != null)
				{
					Tween t = CreateTween();
					t.TweenProperty(_anticipationGlow, "modulate:a", 0.95f, 0.10f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
					t.TweenProperty(_anticipationGlow, "scale", new Vector2(0.95f, 0.95f), AnticipationSeconds).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
				}

				await Cmd.Wait(AnticipationSeconds);

				if (_anticipation != null)
				{
					_anticipation.Visible = false;
				}
				if (_laser != null)
				{
					_laser.Visible = true;
				}
				if (_laserEndFlare != null)
				{
					_laserEndFlare.Modulate = new Color(_laserEndFlare.Modulate.R, _laserEndFlare.Modulate.G, _laserEndFlare.Modulate.B, 1f);
					Tween endT = CreateTween();
					endT.TweenProperty(_laserEndFlare, "scale", new Vector2(1.25f, 1.25f), 0.10f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
					endT.TweenProperty(_laserEndFlare, "scale", new Vector2(0.75f, 0.75f), 0.18f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);
				}

				NGame.Instance?.ScreenShake(ShakeStrength.Medium, ShakeDuration.Normal);

				await Cmd.Wait(LaserSeconds);

				if (_laser != null)
				{
					Tween fade = CreateTween();
					if (_laserOuter != null)
					{
						fade.Parallel().TweenProperty(_laserOuter, "modulate:a", 0f, 0.12f);
					}
					if (_laserInner != null)
					{
						fade.Parallel().TweenProperty(_laserInner, "modulate:a", 0f, 0.12f);
					}
					if (_laserEndFlare != null)
					{
						fade.Parallel().TweenProperty(_laserEndFlare, "modulate:a", 0f, 0.12f);
					}
				}

				NGame.Instance?.ScreenShake(ShakeStrength.Strong, ShakeDuration.Short);

				await Cmd.Wait(PostLaserSeconds);
				this.QueueFreeSafely();
			}

			private static Line2D CreateLaserLine(string name, float width, float alpha, float length, Gradient gradient)
			{
				var line = new Line2D
				{
					Name = name,
					Width = width,
					Gradient = gradient,
					BeginCapMode = Line2D.LineCapMode.Round,
					EndCapMode = Line2D.LineCapMode.Round,
					JointMode = Line2D.LineJointMode.Round,
					TextureMode = Line2D.LineTextureMode.Tile,
					Texture = ResourceLoader.Load<Texture2D>("res://TH_Patchouli/ArtWorks/VFX/trail.png"),
					Modulate = new Color(1f, 1f, 1f, alpha),
					Antialiased = true
				};
				line.AddPoint(Vector2.Zero);
				line.AddPoint(new Vector2(length, 0f));
				return line;
			}

			private static Gradient CreateRainbowGradient(float phase)
			{
				var g = new Gradient();
				g.Offsets = new float[] { 0f, 1f / 6f, 2f / 6f, 3f / 6f, 4f / 6f, 5f / 6f, 1f };
				g.Colors = new Color[7];
				UpdateRainbowGradient(g, phase);
				return g;
			}

			private static void UpdateRainbowGradient(Gradient gradient, float phase)
			{
				Color[] colors = gradient.Colors;
				for (int i = 0; i < colors.Length; i++)
				{
					float hue = Mathf.PosMod(phase + (i / (float)colors.Length), 1f);
					colors[i] = Color.FromHsv(hue, 0.85f, 1.0f, 1.0f);
				}
				gradient.Colors = colors;
			}
		}

		private sealed partial class NRainbowHyperbeamImpactVfx : Node2D
		{
			private readonly Vector2 _source;
			private readonly Vector2 _target;

			private Sprite2D? _core;
			private Sprite2D? _ring;
			private float _time;

			public NRainbowHyperbeamImpactVfx(Vector2 source, Vector2 target)
			{
				_source = source;
				_target = target;
			}

			public override void _Ready()
			{
				GlobalPosition = _target;
				Rotation = (_target - _source).Angle();

				_core = new Sprite2D
				{
					Name = "Core",
					Texture = ResourceLoader.Load<Texture2D>("res://TH_Patchouli/ArtWorks/VFX/light.png"),
					Centered = true,
					Scale = new Vector2(0.55f, 0.55f),
					Modulate = new Color(1f, 1f, 1f, 0.0f),
					Material = ResourceLoader.Load<Material>("res://TH_Patchouli/ArtWorks/VFX/canvas_item_material_additive_shared.tres")
				};
				AddChild(_core);

				_ring = new Sprite2D
				{
					Name = "Ring",
					Texture = ResourceLoader.Load<Texture2D>("res://TH_Patchouli/ArtWorks/VFX/trail2.png"),
					Centered = true,
					Scale = new Vector2(0.65f, 0.65f),
					Modulate = new Color(1f, 1f, 1f, 0.0f),
					Material = ResourceLoader.Load<Material>("res://TH_Patchouli/ArtWorks/VFX/canvas_item_material_additive_shared.tres")
				};
				AddChild(_ring);

				TaskHelper.RunSafely(PlaySequence());
			}

			public override void _Process(double delta)
			{
				_time += (float)delta;
				float hue = Mathf.PosMod(_time * 0.85f, 1f);
				Color c = Color.FromHsv(hue, 0.85f, 1.0f, 1.0f);
				if (_core != null)
				{
					_core.Modulate = new Color(c.R, c.G, c.B, _core.Modulate.A);
				}
				if (_ring != null)
				{
					_ring.Modulate = new Color(c.R, c.G, c.B, _ring.Modulate.A);
				}
			}

			private async Task PlaySequence()
			{
				NGame.Instance?.ScreenShake(ShakeStrength.Strong, ShakeDuration.Short);

				Tween t = CreateTween();
				if (_core != null)
				{
					t.Parallel().TweenProperty(_core, "modulate:a", 1f, 0.05f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
					t.Parallel().TweenProperty(_core, "scale", new Vector2(1.35f, 1.35f), 0.12f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
					t.TweenProperty(_core, "modulate:a", 0f, 0.22f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);
				}
				if (_ring != null)
				{
					t.Parallel().TweenProperty(_ring, "modulate:a", 0.85f, 0.06f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
					t.Parallel().TweenProperty(_ring, "rotation", Mathf.Pi, 0.30f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
					t.Parallel().TweenProperty(_ring, "scale", new Vector2(1.90f, 1.90f), 0.30f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
					t.TweenProperty(_ring, "modulate:a", 0f, 0.22f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);
				}

				await Cmd.Wait(0.75f);
				this.QueueFreeSafely();
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(11);
		}
	}
}
