using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.ValueProps;
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
	public sealed partial class GoldSun : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Gold, ElementEnum.Sun };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		private const string CoinFlipTexturePath = "res://TH_Patchouli/ArtWorks/VFX/coin_flip_anim.png";

		protected override IEnumerable<DynamicVar> CanonicalVars =>
		[
			new CalculationBaseVar(6m),
			new ExtraDamageVar(1m),
			new CalculatedDamageVar(ValueProp.Move).WithMultiplier(CalculateGoldBonus),
			new CardsVar(6),
		];

		public GoldSun() : base(2, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.CalculationBase.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (cardPlay.Target == null)
			{
				return;
			}

			AttackCommand attack = await DamageCmd.Attack(DynamicVars.CalculatedDamage).FromCard(this)  .WithHitFx(PatchouliVfxManager.ToPatchouliVfxPath("sundot"), null, "blunt_attack.mp3").Targeting(cardPlay.Target).Execute(choiceContext);
			int totalDamage = attack.Results.SelectMany(r => r).Sum(r => r.TotalDamage);
			if (totalDamage > 0)
			{
				PlayTreasureGoldVfx(cardPlay.Target, totalDamage);
			}

			Player player = Owner;
			int lose = Math.Max(0, DynamicVars.Cards.IntValue);
			if (lose > 0)
			{
				await PlayerCmd.LoseGold(lose, player, GoldLossType.Spent);
			}
		}

		protected override void OnUpgrade()
		{
			EnergyCost.UpgradeBy(-1);
		}

		private static decimal CalculateGoldBonus(CardModel card, Creature? _)
		{
			Player? owner = card.Owner;
			if (owner == null)
			{
				return 0m;
			}

			int gold = Math.Max(0, owner.Gold);
			int bonus = (int)Math.Floor(gold * 0.3m);
			return Math.Max(0, bonus);
		}

		private static void PlayTreasureGoldVfx(Creature target, int amount)
		{
			if (amount <= 0 || TestMode.IsOn || NCombatRoom.Instance == null || target.IsDead)
			{
				return;
			}

			Node? container = NCombatRoom.Instance.CombatVfxContainer;
			if (container == null)
			{
				return;
			}

			var creatureNode = NCombatRoom.Instance.GetCreatureNode(target);
			if (creatureNode == null)
			{
				return;
			}

			Texture2D tex = ResourceLoader.Load<Texture2D>(CoinFlipTexturePath);
			if (tex == null)
			{
				return;
			}

			var particles = new GpuParticles2D
			{
				Emitting = false,
				Amount = Math.Max(1, amount),
				Texture = tex,
				Lifetime = 2.5f,
				OneShot = true,
				SpeedScale = 1.8f,
				Explosiveness = 0.5f,
				FixedFps = 60
			};

			particles.Material = new CanvasItemMaterial
			{
				ParticlesAnimation = true,
				ParticlesAnimHFrames = 4,
				ParticlesAnimVFrames = 3,
				ParticlesAnimLoop = false
			};

			var alphaCurve = new Curve();
			alphaCurve.AddPoint(new Vector2(0f, 0f));
			alphaCurve.AddPoint(new Vector2(0.210821f, 0.94636f));
			alphaCurve.AddPoint(new Vector2(0.636194f, 0.823755f));
			alphaCurve.AddPoint(new Vector2(1f, 0f));

			var colorGradient = new Gradient();
			colorGradient.AddPoint(0f, new Color(0.342581f, 0.342581f, 0.342581f, 1f));
			colorGradient.AddPoint(0.595745f, Colors.White);

			var process = new ParticleProcessMaterial
			{
				LifetimeRandomness = 0.2f,
				ParticleFlagDisableZ = true,
				EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Sphere,
				EmissionSphereRadius = 150f,
				AngleMin = -180f,
				AngleMax = 180f,
				Direction = new Vector3(0f, -1f, 0f),
				Spread = 40f,
				InitialVelocityMin = 150f,
				InitialVelocityMax = 600f,
				AngularVelocityMin = -720f,
				AngularVelocityMax = 720f,
				Gravity = new Vector3(0f, 800f, 0f),
				ScaleMin = 0.8f,
				ScaleMax = 1.2f,
				ColorRamp = new GradientTexture1D
				{
					Gradient = colorGradient,
					Width = 64
				},
				AlphaCurve = new CurveTexture
				{
					Curve = alphaCurve,
					Width = 128
				},
				HueVariationMin = -0.01f,
				HueVariationMax = 0.01f,
				AnimSpeedMax = 0.56f,
				AnimOffsetMax = 1f
			};

			particles.ProcessMaterial = process;
			Vector2 pos = creatureNode.Hitbox.GlobalPosition + creatureNode.Hitbox.Size * 0.5f;
			pos.Y -= creatureNode.Hitbox.Size.Y * 0.18f;
			particles.GlobalPosition = pos;
			container.AddChildSafely(particles);
			particles.Emitting = true;

			TaskHelper.RunSafely(FreeAfter(particles));
		}

		private static async Task FreeAfter(GpuParticles2D particles)
		{
			await Cmd.Wait((float)particles.Lifetime + 0.1f);
			if (GodotObject.IsInstanceValid(particles))
			{
				particles.QueueFree();
			}
		}
	}
}
