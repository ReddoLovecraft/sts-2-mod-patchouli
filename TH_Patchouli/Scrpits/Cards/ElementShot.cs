using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class ElementShot : PatchouliCardModel
	{
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("Element")];
		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Move)];

		public ElementShot() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Damage.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			ArgumentNullException.ThrowIfNull(cardPlay.Target);
			Creature target = cardPlay.Target;

			List<ElementEnum> elements = GetElementsOnOwner(Owner.Creature);

			await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
				.FromCard(this)
				.Targeting(target)
				.WithHitVfxNode(t => CreateElementShotProjectileToHitbox("elementshot/none", Owner.Creature, t))
				.Execute(choiceContext);

			for (int i = 0; i < elements.Count; i++)
			{
				string scene = GetElementShotSceneKey(elements[i]);
				await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
					.FromCard(this)
					.Targeting(target)
					.WithHitVfxNode(t => CreateElementShotProjectileToHitbox(scene, Owner.Creature, t))
					.Execute(choiceContext);
			}
		}

		private static Node2D? CreateElementShotProjectileToHitbox(string effectName, Creature from, Creature to)
		{
			if (MegaCrit.Sts2.Core.Nodes.Rooms.NCombatRoom.Instance == null)
			{
				return null;
			}

			var fromNode = MegaCrit.Sts2.Core.Nodes.Rooms.NCombatRoom.Instance.GetCreatureNode(from);
			var toNode = MegaCrit.Sts2.Core.Nodes.Rooms.NCombatRoom.Instance.GetCreatureNode(to);
			if (fromNode == null || toNode == null)
			{
				return null;
			}

			Vector2 startAnchor = PatchouliVfxManager.GetCreatureHitboxCenterPosition(from) ?? fromNode.VfxSpawnPosition;
			Vector2 endAnchor = PatchouliVfxManager.GetCreatureHitboxCenterPosition(to) ?? toNode.VfxSpawnPosition;

			Vector2 start = startAnchor + new Vector2(0f, -180f);
			Vector2 end = endAnchor + new Vector2(0f, 0f);
			float angle = (end - start).Angle();

			string scenePath = SceneHelper.GetScenePath(PatchouliVfxManager.ToPatchouliVfxPath(effectName));
			PackedScene scene = PreloadManager.Cache.GetScene(scenePath);
			Node2D vfx = scene.Instantiate<Node2D>(PackedScene.GenEditState.Disabled);

			vfx.Scale = new Vector2(2f, 2f);
			vfx.Rotation = angle;
			if (vfx.GetNodeOrNull<AnimationPlayer>("AnimationPlayer") is AnimationPlayer ap)
			{
				ap.Stop();
			}
			if (vfx.GetNodeOrNull<Godot.Timer>("LifeTimer") is Godot.Timer timer)
			{
				timer.Stop();
			}

			Node2D wrapper = new Node2D();
			wrapper.GlobalPosition = start;
			wrapper.AddChild(vfx);

			const float speed = 900f;
			float dist = start.DistanceTo(end);
			float seconds = dist / Mathf.Max(0.001f, speed);
			seconds = Mathf.Max(0.12f, seconds);

			Tween tween = wrapper.CreateTween();
			tween.TweenProperty(wrapper, "global_position", end, seconds)
				.SetTrans(Tween.TransitionType.Sine)
				.SetEase(Tween.EaseType.Out);
			tween.TweenInterval(0.06);
			tween.TweenProperty(vfx, "modulate:a", 0f, 0.07)
				.SetTrans(Tween.TransitionType.Sine)
				.SetEase(Tween.EaseType.In);
			tween.TweenCallback(Callable.From(wrapper.QueueFree));

			return wrapper;
		}

		private static List<ElementEnum> GetElementsOnOwner(Creature owner)
		{
			var res = new List<ElementEnum>();
			if (owner.HasPower<TH_Patchouli.Scrpits.Powers.GoldElement>())
			{
				res.Add(ElementEnum.Gold);
			}
			if (owner.HasPower<TH_Patchouli.Scrpits.Powers.LunarElement>())
			{
				res.Add(ElementEnum.Lunar);
			}
			if (owner.HasPower<TH_Patchouli.Scrpits.Powers.SunElement>())
			{
				res.Add(ElementEnum.Sun);
			}
			if (owner.HasPower<TH_Patchouli.Scrpits.Powers.FireElement>())
			{
				res.Add(ElementEnum.Fire);
			}
			if (owner.HasPower<TH_Patchouli.Scrpits.Powers.WaterElement>())
			{
				res.Add(ElementEnum.Water);
			}
			if (owner.HasPower<TH_Patchouli.Scrpits.Powers.WoodElement>())
			{
				res.Add(ElementEnum.Wood);
			}
			if (owner.HasPower<TH_Patchouli.Scrpits.Powers.DirtElement>())
			{
				res.Add(ElementEnum.Dirt);
			}
			return res;
		}

		private static string GetElementShotSceneKey(ElementEnum element)
		{
			return element switch
			{
				ElementEnum.Gold => "elementshot/gold",
				ElementEnum.Lunar => "elementshot/lunar",
				ElementEnum.Sun => "elementshot/sun",
				ElementEnum.Fire => "elementshot/fire",
				ElementEnum.Water => "elementshot/water",
				ElementEnum.Wood => "elementshot/wood",
				ElementEnum.Dirt => "elementshot/dirt",
				_ => "elementshot/none"
			};
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(3);
		}
	}
}
