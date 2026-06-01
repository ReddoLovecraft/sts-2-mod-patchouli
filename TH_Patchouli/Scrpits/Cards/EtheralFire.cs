using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using Patchouib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(SpellCardPool))]
	public sealed class EtheralFire : PatchouliCardModel
	{
		private const string NoiseTexturePath = "res://TH_Patchouli/ArtWorks/VFX/tileable_noise_2.png";

		private static readonly List<ElementEnum> _elementTypes = [ElementEnum.Dirt, ElementEnum.Fire];
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IgnitePower>()];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Power", 1m), new CardsVar(6)];

		public EtheralFire() : base(2, CardType.Skill, CardRarity.Ancient, TargetType.AllEnemies)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			int ignite = Math.Max(0, DynamicVars["Power"].IntValue);
			int times = Math.Max(0, DynamicVars.Cards.IntValue);
			if (ignite <= 0 || times <= 0)
			{
				return;
			}

			for (int i = 0; i < times; i++)
			{
				foreach (Creature enemy in CombatState.HittableEnemies.ToList())
				{
					NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NGroundFireVfx.Create(enemy,VfxColor.Red));
					TryPlayLocalDistortVfx(enemy);
					
					await PowerCmd.Apply<IgnitePower>(enemy, ignite, Owner.Creature, this);
				}
			}
		}

		private static void TryPlayLocalDistortVfx(Creature target)
		{
			if (NCombatRoom.Instance == null)
			{
				return;
			}

			var creatureNode = NCombatRoom.Instance.GetCreatureNode(target);
			if (creatureNode == null)
			{
				return;
			}

			Control hitbox = creatureNode.Hitbox;
			if (hitbox == null)
			{
				return;
			}

			Texture2D? noise = ResourceLoader.Load<Texture2D>(NoiseTexturePath);
			if (noise == null)
			{
				return;
			}

			var wrapper = new Node2D();
			wrapper.ProcessMode = Node.ProcessModeEnum.Always;

			var layer = new CanvasLayer();
			layer.Layer = 999;
			wrapper.AddChild(layer);

			var rect = new ColorRect();
			rect.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
			rect.Size = hitbox.Size * 1.08f;
			rect.GlobalPosition = hitbox.GlobalPosition - (rect.Size - hitbox.Size) * 0.5f;
			rect.MouseFilter = Control.MouseFilterEnum.Ignore;
			layer.AddChild(rect);

			var shader = new Shader();
			shader.Code =
				"shader_type canvas_item;\n" +
				"render_mode unshaded;\n" +
				"\n" +
				"uniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap;\n" +
				"uniform sampler2D noise_texture : filter_linear_mipmap;\n" +
				"\n" +
				"uniform float strength : hint_range(0.0, 1.0) = 0.0;\n" +
				"uniform float amplitude : hint_range(0.0, 0.08) = 0.028;\n" +
				"uniform vec2 noise_scale = vec2(2.1, 1.6);\n" +
				"uniform vec2 noise_speed = vec2(0.32, -0.22);\n" +
				"\n" +
				"void fragment() {\n" +
				"\tvec2 uv = SCREEN_UV;\n" +
				"\tvec4 base = texture(screen_texture, uv);\n" +
				"\tfloat s = clamp(strength, 0.0, 1.0);\n" +
				"\n" +
				"\tvec2 c = UV - vec2(0.5);\n" +
				"\tfloat m = smoothstep(0.62, 0.12, length(c) * 2.0);\n" +
				"\ts *= m;\n" +
				"\n" +
				"\tvec2 n_uv = uv * noise_scale + (TIME * noise_speed);\n" +
				"\tvec2 n = texture(noise_texture, n_uv).rg * 2.0 - 1.0;\n" +
				"\tvec2 off = n * (amplitude * s);\n" +
				"\n" +
				"\tvec4 warped = texture(screen_texture, uv + off);\n" +
				"\tCOLOR = vec4(mix(base.rgb, warped.rgb, s), 1.0);\n" +
				"}\n";

			var mat = new ShaderMaterial();
			mat.Shader = shader;
			mat.SetShaderParameter("noise_texture", noise);
			mat.SetShaderParameter("strength", 0f);
			mat.SetShaderParameter("amplitude", 0.028f);
			mat.SetShaderParameter("noise_scale", new Vector2(2.1f, 1.6f));
			mat.SetShaderParameter("noise_speed", new Vector2(0.32f, -0.22f));
			rect.Material = mat;

			(NCombatRoom.Instance as Node)?.AddChildSafely(wrapper);

			const float tin = 0.14f;
			const float thold = 0.18f;
			const float tout = 0.22f;

			Tween t = wrapper.CreateTween();
			t.TweenMethod(Callable.From<float>(v => mat.SetShaderParameter("strength", v)), 0f, 1f, tin)
				.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
			t.TweenInterval(thold);
			t.TweenMethod(Callable.From<float>(v => mat.SetShaderParameter("strength", v)), 1f, 0f, tout)
				.SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Sine);
			t.TweenCallback(Callable.From(wrapper.QueueFree));
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(2);
		}
	}
}
