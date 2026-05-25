using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.RichTextTags;

namespace TH_Patchouli.Scripts.Main
{
	[GlobalClass]
	[Tool]
	public partial class RichTextBrown : AbstractMegaRichTextEffect
	{
		public new string bbcode = "brown";

		protected override string Bbcode => bbcode;

		public override bool _ProcessCustomFX(CharFXTransform charFx)
		{
			charFx.Color = new Color("8B5A2B");
			return true;
		}
	}

	[HarmonyPatch(typeof(MegaRichTextLabel), "InstallEffectsIfNeeded")]
	public static class PatchouliMegaRichTextBrownTagPatch
	{
		private static readonly AbstractMegaRichTextEffect Brown = new RichTextBrown();

		static void Postfix(MegaRichTextLabel __instance)
		{
			if (!__instance.BbcodeEnabled)
			{
				return;
			}

			Godot.Collections.Array effects = __instance.CustomEffects;
			for (int i = 0; i < effects.Count; i++)
			{
				if (effects[i].AsGodotObject() is AbstractMegaRichTextEffect effect && effect.bbcode == "brown")
				{
					return;
				}
			}

			effects.Add(Brown);
			__instance.CustomEffects = effects;
		}
	}
}
