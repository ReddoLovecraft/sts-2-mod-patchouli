using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Patchoulib.Scrpits.Main;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scrpits.Powers;
using static Godot.Node;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class Unpredictable : PatchouliCardModel
	{
		private const string NoiseTexturePath = "res://TH_Patchouli/ArtWorks/VFX/tileable_noise_2.png";

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.Static(StaticHoverTip.Transform)
		];

		public Unpredictable() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			TryPlayScreenDistortVfx();
			
			await PowerCmd.Apply<UnpredictablePower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
		}

		private static void TryPlayScreenDistortVfx()
		{
			Godot.Node? root = (NCombatRoom.Instance as Godot.Node) ?? (NGame.Instance as Godot.Node);
			if (root == null)
			{
				return;
			}

			Texture2D? noise = ResourceLoader.Load<Texture2D>(NoiseTexturePath);
			if (noise == null)
			{
				return;
			}

			SceneTree tree = root.GetTree();
			Godot.Collections.Array<Godot.Node> existing = tree.GetNodesInGroup("UnpredictableDistortVfx");
			for (int i = 0; i < existing.Count; i++)
			{
				Godot.Node n = existing[i];
				if (GodotObject.IsInstanceValid(n))
				{
					n.QueueFree();
				}
			}

			var wrapper = new Node2D();
			wrapper.ProcessMode = ProcessModeEnum.Always;
			wrapper.AddToGroup("UnpredictableDistortVfx");

			var layer = new CanvasLayer();
			layer.Layer = 999;
			wrapper.AddChild(layer);

			var rect = new ColorRect();
			rect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			rect.OffsetLeft = 0f;
			rect.OffsetTop = 0f;
			rect.OffsetRight = 0f;
			rect.OffsetBottom = 0f;
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
				"uniform vec2 noise_scale = vec2(1.6, 1.2);\n" +
				"uniform vec2 noise_speed = vec2(0.25, -0.18);\n" +
				"\n" +
				"void fragment() {\n" +
				"\tvec2 uv = SCREEN_UV;\n" +
				"\tvec4 base = texture(screen_texture, uv);\n" +
				"\tfloat s = clamp(strength, 0.0, 1.0);\n" +
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
			mat.SetShaderParameter("noise_scale", new Vector2(1.6f, 1.2f));
			mat.SetShaderParameter("noise_speed", new Vector2(0.25f, -0.18f));
			rect.Material = mat;

			root.AddChild(wrapper);

			const float tin = 0.18f;
			const float thold = 0.16f;
			const float tout = 0.24f;

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
			EnergyCost.UpgradeBy(-1);
		}
	}
}

