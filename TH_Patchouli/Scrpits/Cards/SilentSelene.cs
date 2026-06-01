using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using Patchouib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;
using TH_Patchouli.Scrpits.Powers;
using static Godot.Node;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class SilentSelene : PatchouliCardModel
	{
		private const string LunarMaskTexturePath = "res://TH_Patchouli/ArtWorks/VFX/lunar.png";
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Lunar };
		public override List<ElementEnum> ElementTypes => _elementTypes;
		protected override bool ShouldGlowGoldInternal => CombatManager.Instance.History.CardPlaysFinished.Count(
                    (CardPlayFinishedEntry e) => 
                        e.CardPlay.Card.Type == CardType.Attack && 
                        e.CardPlay.Card.Owner == base.Owner && 
                        e.HappenedThisTurn(base.CombatState))<=0;
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [base.EnergyHoverTip];
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1), new EnergyVar(2)];

		public SilentSelene() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			TryPlayMoonVfx();
			bool playedAttackThisTurn = CombatManager.Instance.History.CardPlaysFinished.Any((CardPlayFinishedEntry e) =>
				e.HappenedThisTurn(CombatState) && e.CardPlay.Card.Owner == Owner && e.CardPlay.Card.Type == CardType.Attack);

			int mult = playedAttackThisTurn ? 1 : 2;
			await CardPileCmd.Draw(choiceContext, mult * DynamicVars.Cards.IntValue, Owner);
			await PlayerCmd.GainEnergy(mult * DynamicVars.Energy.IntValue, Owner);
		}

		private static void TryPlayMoonVfx()
		{
			Godot.Node? root = (NCombatRoom.Instance as Godot.Node) ?? (NGame.Instance as Godot.Node);
			if (root == null)
			{
				return;
			}

			SceneTree tree = root.GetTree();
			Godot.Collections.Array<Godot.Node> existing = tree.GetNodesInGroup("SilentSeleneMoonVfx");
			for (int i = 0; i < existing.Count; i++)
			{
				Godot.Node n = existing[i];
				if (GodotObject.IsInstanceValid(n))
				{
					n.QueueFree();
				}
			}

			Texture2D? mask = ResourceLoader.Load<Texture2D>(LunarMaskTexturePath);
			if (mask == null)
			{
				return;
			}

			var wrapper = new Node2D();
			wrapper.ProcessMode = ProcessModeEnum.Always;
			wrapper.AddToGroup("SilentSeleneMoonVfx");

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
				"uniform sampler2D mask_texture : filter_linear_mipmap;\n" +
				"\n" +
				"uniform float mask_threshold : hint_range(0.0, 1.0) = 0.35;\n" +
				"uniform float mask_softness : hint_range(0.0, 0.5) = 0.08;\n" +
				"uniform float mask_gamma : hint_range(0.1, 3.0) = 1.0;\n" +
				"uniform float invert_mask : hint_range(0.0, 1.0) = 0.0;\n" +
				"\n" +
				"uniform vec4 moon_color : source_color = vec4(0.72, 0.52, 1.0, 1.0);\n" +
				"uniform vec4 glow_color : source_color = vec4(0.95, 0.60, 1.0, 1.0);\n" +
				"uniform float strength : hint_range(0.0, 1.0) = 0.0;\n" +
				"uniform float glow_radius : hint_range(0.0, 0.03) = 0.010;\n" +
				"uniform float glow_power : hint_range(0.1, 4.0) = 1.8;\n" +
				"uniform float darken : hint_range(0.0, 1.0) = 0.25;\n" +
				"\n" +
				"float mask_shape(vec2 uv) {\n" +
				"\tvec3 m = texture(mask_texture, uv).rgb;\n" +
				"\tfloat v = max(m.r, max(m.g, m.b));\n" +
				"\tv = pow(clamp(v, 0.0, 1.0), mask_gamma);\n" +
				"\tif (invert_mask > 0.5) {\n" +
				"\t\tv = 1.0 - v;\n" +
				"\t}\n" +
				"\tfloat lo = mask_threshold - mask_softness;\n" +
				"\tfloat hi = mask_threshold + mask_softness;\n" +
				"\treturn smoothstep(lo, hi, v);\n" +
				"}\n" +
				"\n" +
				"void fragment() {\n" +
				"\tvec2 uv = SCREEN_UV;\n" +
				"\tvec4 base = texture(screen_texture, uv);\n" +
				"\n" +
				"\tfloat a = mask_shape(uv);\n" +
				"\tvec2 r = vec2(glow_radius, glow_radius);\n" +
				"\n" +
				"\tfloat g = 0.0;\n" +
				"\tg = max(g, mask_shape(uv + vec2(r.x, 0.0)));\n" +
				"\tg = max(g, mask_shape(uv + vec2(-r.x, 0.0)));\n" +
				"\tg = max(g, mask_shape(uv + vec2(0.0, r.y)));\n" +
				"\tg = max(g, mask_shape(uv + vec2(0.0, -r.y)));\n" +
				"\tg = max(g, mask_shape(uv + vec2(r.x, r.y)));\n" +
				"\tg = max(g, mask_shape(uv + vec2(-r.x, r.y)));\n" +
				"\tg = max(g, mask_shape(uv + vec2(r.x, -r.y)));\n" +
				"\tg = max(g, mask_shape(uv + vec2(-r.x, -r.y)));\n" +
				"\n" +
				"\tfloat glow = clamp(g - a, 0.0, 1.0);\n" +
				"\tglow = pow(glow, glow_power);\n" +
				"\n" +
				"\tfloat s = clamp(strength, 0.0, 1.0);\n" +
				"\n" +
				"\tvec3 tinted = base.rgb * (1.0 - (darken * a * s));\n" +
				"\tvec3 moon = moon_color.rgb * a * s;\n" +
				"\tvec3 add_glow = glow_color.rgb * glow * s;\n" +
				"\n" +
				"\tvec3 rgb = tinted + moon + add_glow;\n" +
				"\tfloat out_a = s * clamp(a + glow * 0.85, 0.0, 1.0);\n" +
				"\tCOLOR = vec4(rgb, out_a);\n" +
				"}\n";

			var mat = new ShaderMaterial();
			mat.Shader = shader;
			mat.SetShaderParameter("mask_texture", mask);
			mat.SetShaderParameter("moon_color", new Color(0.72f, 0.52f, 1f, 1f));
			mat.SetShaderParameter("glow_color", new Color(0.95f, 0.60f, 1f, 1f));
			mat.SetShaderParameter("glow_radius", 0.010f);
			mat.SetShaderParameter("glow_power", 1.8f);
			mat.SetShaderParameter("darken", 0.25f);
			mat.SetShaderParameter("mask_threshold", 0.35f);
			mat.SetShaderParameter("mask_softness", 0.08f);
			mat.SetShaderParameter("mask_gamma", 1.0f);
			mat.SetShaderParameter("invert_mask", 0.0f);
			mat.SetShaderParameter("strength", 0f);
			rect.Material = mat;

			root.AddChild(wrapper);

			const float tin = 0.22f;
			const float thold = 0.35f;
			const float tout = 0.40f;

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
			DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}
