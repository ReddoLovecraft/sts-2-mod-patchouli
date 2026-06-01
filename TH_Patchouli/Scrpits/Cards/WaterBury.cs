using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed partial class WaterBury : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = [ElementEnum.Water];
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromPower<FreezePower>()
		];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5m, ValueProp.Move), new CardsVar(2), new DynamicVar("Power", 2)];

		public WaterBury() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Damage.UpgradeValueBy(boostAmount);
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
			DynamicVars["Power"].UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (cardPlay.Target == null)
			{
				return;
			}
			await PowerCmd.Apply<FreezePower>(cardPlay.Target, DynamicVars["Power"].IntValue, Owner.Creature, this);
			TryPlayWaterBuryXStreamVfx(Owner.Creature, cardPlay.Target);
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(DynamicVars.Cards.IntValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
		}

		private static void TryPlayWaterBuryXStreamVfx(Creature owner, Creature target)
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

			var vfx = new NWaterBuryXStreamVfx(ownerNode.VfxSpawnPosition, targetHitboxCenter.Value);
			NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
		}

		private sealed partial class NWaterBuryXStreamVfx : Node2D
		{
			private const float DurationSeconds = 1.6f;
			private const float FadeOutSeconds = 0.25f;
			private const float ParticleLifetime = 0.55f;

			private static readonly string DropletTexturePath = "res://TH_Patchouli/ArtWorks/VFX/touhoueffect/bulletGa000.png";
			private static readonly string AdditiveMaterialPath = "res://TH_Patchouli/ArtWorks/VFX/canvas_item_material_additive_shared.tres";

			private readonly Vector2 _source;
			private readonly Vector2 _target;

			private GpuParticles2D? _streamA;
			private GpuParticles2D? _streamB;

			public NWaterBuryXStreamVfx(Vector2 source, Vector2 target)
			{
				_source = source;
				_target = target;
			}

			public override void _Ready()
			{
				BuildStreams();
				TaskHelper.RunSafely(PlaySequence());
			}

			private void BuildStreams()
			{
				Vector2 delta = _target - _source;
				float len = Mathf.Max(64f, delta.Length());
				Vector2 dir = delta.Normalized();
				Vector2 perp = new Vector2(-dir.Y, dir.X);

				Vector2 startA = _source + (perp * 36f);
				Vector2 endA = _target - (perp * 22f);
				Vector2 startB = _source - (perp * 36f);
				Vector2 endB = _target + (perp * 22f);

				float baseSpeed = Mathf.Clamp(len / ParticleLifetime, 700f, 2400f);

				_streamA = CreateStream("StreamA", startA, endA, baseSpeed);
				_streamB = CreateStream("StreamB", startB, endB, baseSpeed);
				AddChild(_streamA);
				AddChild(_streamB);
			}

			private static GpuParticles2D CreateStream(string name, Vector2 start, Vector2 end, float baseSpeed)
			{
				var texture = ResourceLoader.Load<Texture2D>(DropletTexturePath);
				var additiveMaterial = ResourceLoader.Load<Material>(AdditiveMaterialPath);

				var mat = new ParticleProcessMaterial
				{
					ParticleFlagDisableZ = true,
					Gravity = Vector3.Zero,
					Direction = new Vector3(1f, 0f, 0f),
					Spread = 10.0f,
					InitialVelocityMin = baseSpeed * 0.85f,
					InitialVelocityMax = baseSpeed * 1.05f,
					DampingMin = 0.08f,
					DampingMax = 0.12f,
					ScaleMin = 0.22f,
					ScaleMax = 0.34f,
					Color = new Color(0.55f, 0.85f, 1.0f, 0.95f)
				};

				var p = new GpuParticles2D
				{
					Name = name,
					GlobalPosition = start,
					Rotation = (end - start).Angle(),
					Amount = 260,
					Lifetime = ParticleLifetime,
					OneShot = false,
					Explosiveness = 0.0f,
					Preprocess = ParticleLifetime,
					Emitting = true,
					Texture = texture,
					ProcessMaterial = mat,
					Material = additiveMaterial
				};

				return p;
			}

			private async Task PlaySequence()
			{
				await PatchouliVfxManager.WaitSeconds(DurationSeconds);

				if (_streamA != null && GodotObject.IsInstanceValid(_streamA))
				{
					_streamA.Emitting = false;
				}
				if (_streamB != null && GodotObject.IsInstanceValid(_streamB))
				{
					_streamB.Emitting = false;
				}

				Tween tween = CreateTween();
				tween.TweenProperty(this, "modulate:a", 0f, FadeOutSeconds)
					.SetTrans(Tween.TransitionType.Sine)
					.SetEase(Tween.EaseType.In);

				await PatchouliVfxManager.WaitSeconds(FadeOutSeconds + ParticleLifetime);
				this.QueueFreeSafely();
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(2);
		}
	}
}
