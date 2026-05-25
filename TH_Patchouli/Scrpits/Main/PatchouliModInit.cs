using Godot;
using Timer = Godot.Timer;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Localization.Formatters;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.RichTextTags;
using Patchouib.Scrpits.Main;
using System.Runtime.CompilerServices;
using System.Reflection;
using MegaCrit.Sts2.Core.Commands;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scripts.Main
{
	[ModInitializer("Init")]
	public class PatchouliInit
	{
   private const string ModSfxPrefix = "mod_sfx://";

		public static string ToModSfxPath(string localPath)
		{
			return ModSfxPrefix + localPath;
		}
	private static Harmony? _harmony;
	public static void Init()
	{
		 TryRegisterGodotScriptAssembly();
		_harmony = new Harmony("TH_Patchouli");
		_harmony.PatchAll();
		Log.Debug("Patchouli mod has been loaded successfully");
	}
   
	private static void TryRegisterGodotScriptAssembly()
	{
		try
		{
			Assembly modAssembly = typeof(PatchouliInit).Assembly;
			Type? scriptManagerBridgeType = Type.GetType("Godot.Bridge.ScriptManagerBridge, GodotSharp");

			if (scriptManagerBridgeType == null)
			{
				return;
			}

			MethodInfo? lookupMethod = scriptManagerBridgeType.GetMethod(
				"LookupScriptsInAssembly",
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
				binder: null,
				types: [typeof(Assembly)],
				modifiers: null
			);

			lookupMethod ??= scriptManagerBridgeType
				.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
				.FirstOrDefault(m =>
				{
					ParameterInfo[] ps = m.GetParameters();
					return ps.Length == 1
						&& ps[0].ParameterType == typeof(Assembly)
						&& (m.Name.Contains("Lookup", StringComparison.OrdinalIgnoreCase)
							|| m.Name.Contains("Load", StringComparison.OrdinalIgnoreCase)
							|| m.Name.Contains("Register", StringComparison.OrdinalIgnoreCase));
				});

			lookupMethod?.Invoke(null, [modAssembly]);
		}
		catch (Exception e)
		{
			Log.Error($"Failed to register Godot scripts for TH_Patchouli: {e}");
		}
	}
	}

	[HarmonyPatch]
	public static class ModSfxCmdPatch
	{
		static IEnumerable<MethodBase> TargetMethods()
		{
			return typeof(SfxCmd)
				.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
				.Where(m =>
				{
					if (m.Name != "Play")
					{
						return false;
					}

					ParameterInfo[] ps = m.GetParameters();
					return ps.Length >= 1 && ps[0].ParameterType == typeof(string);
				});
		}

		static bool Prefix(MethodBase __originalMethod, object[] __args)
		{
			return ModSfxPatch.HandlePlay(__originalMethod, __args);
		}
	}

	[HarmonyPatch]
	public static class ModSfxPatch
	{
		private const string ModSfxPrefix = "mod_sfx://";
		private const float DefaultGain = 0.45f;
		private static readonly Dictionary<string, float> GainOverrides = new()
		{
			["TH_Patchouli/ArtWorks/SFX/characterselect.wav"] = 2.8f
		};

		static IEnumerable<MethodBase> TargetMethods()
		{
			return typeof(NAudioManager)
				.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
				.Where(m =>
				{
					if (m.Name != "PlayOneShot")
					{
						return false;
					}

					ParameterInfo[] ps = m.GetParameters();
					return ps.Length >= 1 && ps[0].ParameterType == typeof(string);
				});
		}

		static bool Prefix(MethodBase __originalMethod, object[] __args)
		{
			return HandlePlay(__originalMethod, __args);
		}

		public static bool HandlePlay(MethodBase __originalMethod, object[] __args)
		{
			if (__args.Length < 1 || __args[0] is not string path || !path.StartsWith(ModSfxPrefix))
			{
				return true;
			}

			float volume = 1f;
			ParameterInfo[] ps = __originalMethod.GetParameters();
			for (int i = 1; i < __args.Length && i < ps.Length; i++)
			{
				if (__args[i] is float f && ps[i].ParameterType == typeof(float) && ps[i].Name != null && ps[i].Name.Contains("volume", StringComparison.OrdinalIgnoreCase))
				{
					volume = f;
					break;
				}
			}
			if (volume == 1f)
			{
				for (int i = 1; i < __args.Length; i++)
				{
					if (__args[i] is float f)
					{
						volume = f;
					}
				}
			}

			try
			{
				PlayModSfx(path, volume);
			}
			catch (System.Exception e)
			{
				Log.Error($"Failed to play mod sfx: {path}. Error: {e.Message}");
			}

			return false;
		}

		private static void PlayModSfx(string path, float volume)
		{
			string localPath = path.Substring(ModSfxPrefix.Length);
			string resPath = "res://" + localPath;
			AudioStream? stream = ResourceLoader.Load<AudioStream>(resPath);
			if (stream == null)
			{
				return;
			}

			var player = new AudioStreamPlayer();
			player.Stream = stream;
			player.Bus = FindSfxBusName();

			float gain = DefaultGain;
			if (GainOverrides.TryGetValue(localPath, out float overrideGain))
			{
				gain *= overrideGain;
			}
			player.VolumeDb = Mathf.LinearToDb(Mathf.Max(0.0001f, volume * gain));

			if (NGame.Instance != null)
			{
				NGame.Instance.AddChild(player);
			}
			else
			{
				Log.Error($"TH_Rin mod_sfx can't play because NGame.Instance is null. Path: {path}");
				player.QueueFree();
				return;
			}

			player.Play();
			player.Connect("finished", Callable.From(player.QueueFree));
		}

		private static string FindSfxBusName()
		{
			int count = AudioServer.BusCount;
			for (int i = 0; i < count; i++)
			{
				string bus = AudioServer.GetBusName(i);
				if (string.Equals(bus, "SFX", StringComparison.OrdinalIgnoreCase))
				{
					return bus;
				}
			}

			for (int i = 0; i < count; i++)
			{
				string bus = AudioServer.GetBusName(i);
				string lower = bus.ToLowerInvariant();
				if (lower.Contains("sfx") || lower.Contains("soundeffect") || lower.Contains("sound_effect") || lower == "se")
				{
					return bus;
				}
			}

			return "Master";
		}
	}

	[HarmonyPatch(typeof(Player), "PopulateStartingDeck")]
	public static class PatchouliStartingDeckCapturePatch
	{
		static void Prefix(Player __instance)
		{
			if (__instance.Character is PatchouliCharacter)
			{
				StartingDeckElementCapture.Begin();
			}
		}

		static void Postfix(Player __instance)
		{
			if (__instance.Character is PatchouliCharacter)
			{
				StartingDeckElementCapture.End();
			}
		}
	}

	[HarmonyPatch(typeof(CardModel), nameof(CardModel.ToMutable))]
	public static class PatchouliCardToMutablePatch
	{
		static void Postfix(CardModel __instance, ref CardModel __result)
		{
			if (!StartingDeckElementCapture.Active)
			{
				return;
			}
			if (__instance is not PatchouliCardModel)
			{
				return;
			}
			if (__result is not PatchouliCardModel patchouliResult)
			{
				return;
			}
			if (StartingDeckElementCapture.TryDequeue(out List<ElementEnum> elements))
			{
				patchouliResult.SetElementTypes(elements);
			}
		}
	}

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
				if (timer.IsStopped())
				{
					timer.Start();
				}
			}
			else
			{
				Timer? timer = card.GetNodeOrNull<Timer>(TimerName);
				if (timer != null && !timer.IsStopped())
				{
					timer.Stop();
				}
			}

			Apply(card);
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
				if (timer.IsStopped())
				{
					timer.Start();
				}
			}
			else
			{
				Timer? timer = node.GetNodeOrNull<Timer>(TimerName);
				if (timer != null && !timer.IsStopped())
				{
					timer.Stop();
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

	[HarmonyPatch(typeof(EnergyIconHelper), nameof(EnergyIconHelper.GetPrefix))]
	public static class PatchouliEnergyIconPrefixPatch
	{
		static void Postfix(AbstractModel model, ref string __result)
		{
			if (model is not PatchouliCardModel patchouli || !patchouli.HasElementVisuals)
			{
				return;
			}
			string? key = PatchouliCardModel.GetElementIconKey(patchouli.GetVisualElementNow());
			if (key != null)
			{
				__result = key;
			}
		}
	}

	[HarmonyPatch(typeof(EnergyIconHelper), nameof(EnergyIconHelper.GetPath), typeof(string))]
	public static class PatchouliEnergyIconPathPatch
	{
		static void Postfix(string prefix, ref string __result)
		{
			string lower = prefix?.ToLowerInvariant() ?? string.Empty;
			if (lower is "fire" or "gold" or "water" or "wood" or "dirt" or "lunar" or "sun")
			{
				__result = $"res://TH_Patchouli/ArtWorks/Character/{lower}_card_orb.png";
			}
		}
	}

	[HarmonyPatch(typeof(EnergyIconsFormatter), nameof(EnergyIconsFormatter.TryEvaluateFormat))]
	public static class PatchouliEnergyIconsFormatterPatch
	{
		static bool Prefix(object formattingInfo, ref bool __result)
		{
			if (formattingInfo == null)
			{
				return true;
			}

			object? currentValue = GetCurrentValue(formattingInfo);
			if (currentValue is not EnergyVar energyVar)
			{
				return true;
			}

			string prefix = energyVar.ColorPrefix?.ToLowerInvariant() ?? string.Empty;
			if (prefix is not ("fire" or "gold" or "water" or "wood" or "dirt" or "lunar" or "sun"))
			{
				return true;
			}

			int amount = Convert.ToInt32(energyVar.PreviewValue);
			string iconTag = "[img]res://TH_Patchouli/ArtWorks/Character/" + prefix + "_cost_orb.png[/img]";
			string output = (amount > 0 && amount < 4)
				? string.Concat(Enumerable.Repeat(iconTag, amount))
				: energyVar.ToHighlightedString(inverse: false) + iconTag;

			Write(formattingInfo, output);
			__result = true;
			return false;
		}

		private static object? GetCurrentValue(object formattingInfo)
		{
			PropertyInfo? prop = formattingInfo.GetType().GetProperty("CurrentValue", BindingFlags.Public | BindingFlags.Instance);
			return prop?.GetValue(formattingInfo);
		}

		private static void Write(object formattingInfo, string text)
		{
			MethodInfo? method = formattingInfo.GetType().GetMethod("Write", BindingFlags.Public | BindingFlags.Instance, binder: null, types: new[] { typeof(string) }, modifiers: null);
			method?.Invoke(formattingInfo, new object[] { text });
		}
	}

	[HarmonyPatch]
	public static class PatchouliCardDescriptionSuffixPatch
	{
		static MethodBase? TargetMethod()
		{
			Type? previewType = typeof(CardModel).GetNestedType("DescriptionPreviewType", BindingFlags.NonPublic);
			if (previewType == null)
			{
				return null;
			}
			return AccessTools.Method(typeof(CardModel), "GetDescriptionForPile", new[] { typeof(PileType), previewType, typeof(Creature) });
		}

		static void Postfix(CardModel __instance, ref string __result)
		{
			if (__instance is not PatchouliCardModel patchouli)
			{
				return;
			}
			string suffix = patchouli.GetElementSuffixText();
			if (string.IsNullOrEmpty(suffix))
			{
				return;
			}
			__result = string.IsNullOrEmpty(__result) ? suffix : __result.TrimEnd('\n') + "\n" + suffix;
		}
	}

	
}
