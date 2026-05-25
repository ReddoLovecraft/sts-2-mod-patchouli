using Godot;
using Timer = Godot.Timer;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace TH_Patchouli.Scripts.Main
{
	internal static class PatchouliElementMaterialCache
	{
		private static readonly Dictionary<string, ShaderMaterial> _cache = new Dictionary<string, ShaderMaterial>();

		public static ShaderMaterial GetOrCreate(ShaderMaterial baseMat, string elementKey, float h, float s, float v)
		{
			string key = $"{baseMat.ResourcePath}|{elementKey}";
			if (_cache.TryGetValue(key, out ShaderMaterial existing))
			{
				return existing;
			}

			ShaderMaterial mat = (ShaderMaterial)baseMat.Duplicate(true);
			mat.ResourceLocalToScene = false;
			mat.SetShaderParameter("h", h);
			mat.SetShaderParameter("s", s);
			mat.SetShaderParameter("v", v);
			_cache[key] = mat;
			return mat;
		}
	}

	[HarmonyPatch(typeof(CardModel), "get_EnergyIcon")]
	public static class PatchouliCardEnergyIconGetterPatch
	{
		static void Postfix(CardModel __instance, ref Texture2D __result)
		{
			if (__instance is not PatchouliCardModel patchouli || !patchouli.HasElementVisuals)
			{
				return;
			}
			string? path = PatchouliCardModel.GetElementCardOrbPath(patchouli.GetVisualElementNow());
			if (path != null)
			{
				__result = ResourceLoader.Load<Texture2D>(path, null, ResourceLoader.CacheMode.Reuse);
			}
		}
	}

	[HarmonyPatch(typeof(CardModel), "get_FrameMaterial")]
	public static class PatchouliCardFrameMaterialGetterPatch
	{
		static void Postfix(CardModel __instance, ref Material __result)
		{
			if (__instance is not PatchouliCardModel patchouli || !patchouli.HasElementVisuals)
			{
				return;
			}
			(float h, float s, float v)? hsv = PatchouliCardModel.GetElementFrameHsv(patchouli.GetVisualElementNow());
			if (hsv == null)
			{
				return;
			}
			if (__result is not ShaderMaterial baseMat)
			{
				return;
			}

			string elementKey = PatchouliCardModel.GetElementIconKey(patchouli.GetVisualElementNow()) ?? "element";
			__result = PatchouliElementMaterialCache.GetOrCreate(baseMat, elementKey, hsv.Value.h, hsv.Value.s, hsv.Value.v);
		}
	}

	internal static class PatchouliElementVisuals
	{
		private const string TimerName = "__TH_Patchouli_ElementVisualTimer";
		private const string MetaKey = "__th_patchouli_element_visual_key";
		private const string MetaMatKey = "__th_patchouli_element_visual_mat_key";

		private static readonly FieldInfo FrameField = AccessTools.Field(typeof(NCard), "_frame");
		private static readonly FieldInfo EnergyIconField = AccessTools.Field(typeof(NCard), "_energyIcon");
		private static readonly FieldInfo UnplayableEnergyIconField = AccessTools.Field(typeof(NCard), "_unplayableEnergyIcon");

		public static void SyncAndApply(NCard card)
		{
			if (!card.IsInsideTree())
			{
				return;
			}

			if (card.Model is PatchouliCardModel patchouli && TryGetMultiElementCount(patchouli, out int count) && count > 1)
			{
				Timer timer = EnsureTimer(card);
				StartTimerSafe(timer);
			}
			else
			{
				Timer? timer = card.GetNodeOrNull<Timer>(TimerName);
				if (timer != null && !timer.IsStopped())
				{
					timer.Stop();
					timer.Autostart = false;
				}
			}

			Apply(card);
		}

		internal static void StartTimerSafe(Timer timer)
		{
			if (!GodotObject.IsInstanceValid(timer) || !timer.IsStopped())
			{
				return;
			}

			if (timer.IsInsideTree())
			{
				timer.Start();
			}
			else
			{
				timer.Autostart = true;
			}
		}

		private static Timer EnsureTimer(NCard card)
		{
			Timer? existing = card.GetNodeOrNull<Timer>(TimerName);
			if (existing != null)
			{
				return existing;
			}

			Timer timer = new Timer
			{
				Name = TimerName,
				WaitTime = 0.2,
				OneShot = false,
				Autostart = false
			};
			timer.Timeout += () =>
			{
				if (GodotObject.IsInstanceValid(card))
				{
					Apply(card);
				}
			};
			card.AddChild(timer);
			return timer;
		}

		private static bool TryGetMultiElementCount(PatchouliCardModel model, out int count)
		{
			count = 0;
			List<ElementEnum>? list = model.ElementTypes;
			if (list == null || list.Count == 0)
			{
				return false;
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] != ElementEnum.None)
				{
					count++;
				}
			}
			return true;
		}

		private static void Apply(NCard card)
		{
			if (!card.IsNodeReady())
			{
				return;
			}

			CardModel? model = card.Model;
			if (model == null)
			{
				return;
			}

			string desiredKey = "__default__";
			string desiredMatKey = "__default__";
			Texture2D desiredEnergyIcon = model.EnergyIcon;
			(float h, float s, float v)? desiredHsv = null;

			if (model is PatchouliCardModel patchouliModel && patchouliModel.HasElementVisuals)
			{
				ElementEnum element = patchouliModel.GetVisualElementNow();
				string? key = PatchouliCardModel.GetElementIconKey(element);
				string? orbPath = PatchouliCardModel.GetElementCardOrbPath(element);
				desiredHsv = PatchouliCardModel.GetElementFrameHsv(element);
				if (key != null && orbPath != null)
				{
					desiredKey = key;
					desiredMatKey = key;
					desiredEnergyIcon = ResourceLoader.Load<Texture2D>(orbPath, null, ResourceLoader.CacheMode.Reuse);
				}
			}

			string previousKey = card.HasMeta(MetaKey) ? card.GetMeta(MetaKey).AsString() : string.Empty;
			string previousMatKey = card.HasMeta(MetaMatKey) ? card.GetMeta(MetaMatKey).AsString() : string.Empty;
			bool elementChanged = previousKey != desiredKey || previousMatKey != desiredMatKey;
			if (elementChanged)
			{
				card.SetMeta(MetaKey, desiredKey);
				card.SetMeta(MetaMatKey, desiredMatKey);
			}

			TextureRect? frame = FrameField.GetValue(card) as TextureRect;
			if (frame != null)
			{
				frame.SelfModulate = Colors.White;
				if (desiredHsv != null)
				{
					ShaderMaterial? baseMat = model.FrameMaterial as ShaderMaterial ?? frame.Material as ShaderMaterial;
					if (baseMat != null)
					{
						ShaderMaterial? currentMat = frame.Material as ShaderMaterial;
						if (currentMat == null || currentMat.Shader != baseMat.Shader || !ShaderParamsMatch(currentMat, desiredHsv.Value))
						{
							ShaderMaterial mat = (ShaderMaterial)baseMat.Duplicate(true);
							mat.SetShaderParameter("h", desiredHsv.Value.h);
							mat.SetShaderParameter("s", desiredHsv.Value.s);
							mat.SetShaderParameter("v", desiredHsv.Value.v);
							frame.Material = mat;
						}
						else
						{
							currentMat.SetShaderParameter("h", desiredHsv.Value.h);
							currentMat.SetShaderParameter("s", desiredHsv.Value.s);
							currentMat.SetShaderParameter("v", desiredHsv.Value.v);
						}
					}
				}
				else
				{
					frame.Material = model.FrameMaterial;
				}
			}

			TextureRect? energyIcon = EnergyIconField.GetValue(card) as TextureRect;
			if (energyIcon != null)
			{
				energyIcon.Texture = desiredEnergyIcon;
			}

			TextureRect? unplayableEnergyIcon = UnplayableEnergyIconField.GetValue(card) as TextureRect;
			if (unplayableEnergyIcon != null)
			{
				unplayableEnergyIcon.Texture = desiredEnergyIcon;
			}

			if (elementChanged && model is PatchouliCardModel patchouliModel2 && TryGetMultiElementCount(patchouliModel2, out int count) && count > 1)
			{
				card.UpdateVisuals(card.DisplayingPile, CardPreviewMode.Normal);
			}
		}

		internal static bool ShaderParamsMatch(ShaderMaterial mat, (float h, float s, float v) hsv)
		{
			const float eps = 0.0005f;
			float h = (float)mat.GetShaderParameter("h");
			float s = (float)mat.GetShaderParameter("s");
			float v = (float)mat.GetShaderParameter("v");
			return Mathf.Abs(h - hsv.h) < eps && Mathf.Abs(s - hsv.s) < eps && Mathf.Abs(v - hsv.v) < eps;
		}
	}

	[HarmonyPatch(typeof(NCard), "_EnterTree")]
	public static class PatchouliNCardEnterTreePatch
	{
		static void Postfix(NCard __instance)
		{
			PatchouliElementVisuals.SyncAndApply(__instance);
		}
	}

	[HarmonyPatch(typeof(NCard), "_ExitTree")]
	public static class PatchouliNCardExitTreePatch
	{
		static void Postfix(NCard __instance)
		{
			Timer? timer = __instance.GetNodeOrNull<Timer>("__TH_Patchouli_ElementVisualTimer");
			if (timer != null && !timer.IsStopped())
			{
				timer.Stop();
			}
		}
	}

	[HarmonyPatch(typeof(NCard), "Reload")]
	public static class PatchouliNCardReloadPatch
	{
		static void Postfix(NCard __instance)
		{
			PatchouliElementVisuals.SyncAndApply(__instance);
		}
	}

	[HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals))]
	public static class PatchouliNCardUpdateVisualsPatch
	{
		static void Postfix(NCard __instance)
		{
			PatchouliElementVisuals.SyncAndApply(__instance);
		}
	}

	internal static class PatchouliTinyCardElementVisuals
	{
		private const string TimerName = "__TH_Patchouli_TinyCardElementVisualTimer";
		private const string MetaKey = "__th_patchouli_tiny_card_element_visual_key";

		private static readonly ConditionalWeakTable<NTinyCard, CardModel> ModelByNode = new ConditionalWeakTable<NTinyCard, CardModel>();
		private static readonly FieldInfo CardBackField = AccessTools.Field(typeof(NTinyCard), "_cardBack");

		public static void SyncAndApply(NTinyCard node, CardModel card)
		{
			ModelByNode.Remove(node);
			ModelByNode.Add(node, card);

			if (card is PatchouliCardModel patchouli && TryGetMultiElementCount(patchouli, out int count) && count > 1)
			{
				Timer timer = EnsureTimer(node);
				PatchouliElementVisuals.StartTimerSafe(timer);
			}
			else
			{
				Timer? timer = node.GetNodeOrNull<Timer>(TimerName);
				if (timer != null && !timer.IsStopped())
				{
					timer.Stop();
					timer.Autostart = false;
				}
			}

			Apply(node);
		}

		private static Timer EnsureTimer(NTinyCard node)
		{
			Timer? existing = node.GetNodeOrNull<Timer>(TimerName);
			if (existing != null)
			{
				return existing;
			}

			Timer timer = new Timer
			{
				Name = TimerName,
				WaitTime = 0.2,
				OneShot = false,
				Autostart = false
			};
			timer.Timeout += () =>
			{
				if (GodotObject.IsInstanceValid(node))
				{
					Apply(node);
				}
			};
			node.AddChild(timer);
			return timer;
		}

		private static bool TryGetMultiElementCount(PatchouliCardModel model, out int count)
		{
			count = 0;
			List<ElementEnum>? list = model.ElementTypes;
			if (list == null || list.Count == 0)
			{
				return false;
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] != ElementEnum.None)
				{
					count++;
				}
			}
			return true;
		}

		private static void Apply(NTinyCard node)
		{
			if (!node.IsNodeReady())
			{
				return;
			}

			if (!ModelByNode.TryGetValue(node, out CardModel? card) || card == null)
			{
				return;
			}

			string desiredKey = "__default__";
			(float h, float s, float v)? desiredHsv = null;

			if (card is PatchouliCardModel patchouli && patchouli.HasElementVisuals)
			{
				ElementEnum element = patchouli.GetVisualElementNow();
				string? key = PatchouliCardModel.GetElementIconKey(element);
				desiredHsv = PatchouliCardModel.GetElementFrameHsv(element);
				if (key != null)
				{
					desiredKey = key;
				}
			}

			string previousKey = node.HasMeta(MetaKey) ? node.GetMeta(MetaKey).AsString() : string.Empty;
			if (previousKey != desiredKey)
			{
				node.SetMeta(MetaKey, desiredKey);
			}

			TextureRect? cardBack = CardBackField.GetValue(node) as TextureRect;
			if (cardBack != null)
			{
				cardBack.Modulate = Colors.White;
				if (desiredHsv != null)
				{
					ShaderMaterial? baseMat = card.FrameMaterial as ShaderMaterial ?? cardBack.Material as ShaderMaterial;
					if (baseMat != null)
					{
						ShaderMaterial? currentMat = cardBack.Material as ShaderMaterial;
						if (currentMat == null || currentMat.Shader != baseMat.Shader || !PatchouliElementVisuals.ShaderParamsMatch(currentMat, desiredHsv.Value))
						{
							ShaderMaterial mat = (ShaderMaterial)baseMat.Duplicate(true);
							mat.SetShaderParameter("h", desiredHsv.Value.h);
							mat.SetShaderParameter("s", desiredHsv.Value.s);
							mat.SetShaderParameter("v", desiredHsv.Value.v);
							cardBack.Material = mat;
						}
						else
						{
							currentMat.SetShaderParameter("h", desiredHsv.Value.h);
							currentMat.SetShaderParameter("s", desiredHsv.Value.s);
							currentMat.SetShaderParameter("v", desiredHsv.Value.v);
						}
					}
				}
				else
				{
					cardBack.Material = card.FrameMaterial;
				}
			}
		}
	}

	[HarmonyPatch(typeof(NTinyCard), nameof(NTinyCard.SetCard))]
	public static class PatchouliTinyCardSetCardPatch
	{
		static void Postfix(NTinyCard __instance, CardModel card)
		{
			PatchouliTinyCardElementVisuals.SyncAndApply(__instance, card);
		}
	}
}
