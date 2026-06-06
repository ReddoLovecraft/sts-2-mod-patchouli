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
using BaseLib.Config;

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
		ModConfigRegistry.Register("TH_Patchouli", new PatchouliModConfig());
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
		private static readonly HashSet<string> _loggedSfx = new(StringComparer.OrdinalIgnoreCase);
		private static readonly Dictionary<string, float> GainOverrides = new()
		{
			["TH_Patchouli/ArtWorks/SFX/characterselect.wav"] = 0.1f,
			["TH_Patchouli/ArtWorks/SFX/spell.wav"] = 0.1f,
		};
		private static bool TryGetOverrideGain(string localPath, out float overrideGain)
	{
		if (GainOverrides.TryGetValue(localPath, out overrideGain))
		{
			return true;
		}

		string fileName = Path.GetFileName(localPath);
		if (!string.IsNullOrEmpty(fileName) && GainOverrides.TryGetValue(fileName, out overrideGain))
		{
			return true;
		}

		overrideGain = default;
		return false;
	}

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

		internal static void PlayModSfx(string path, float volume)
		{
			string localPath = path.Substring(ModSfxPrefix.Length);
			localPath = localPath.Replace('\\', '/').TrimStart('/');
			if (localPath.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
			{
				localPath = localPath.Substring("res://".Length).TrimStart('/');
			}

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
			if (TryGetOverrideGain(localPath, out float overrideGain))
			{
				gain *= overrideGain;
			}
			float linearVolume = volume * gain;
			if (linearVolume <= 0f || float.IsNaN(linearVolume) || float.IsInfinity(linearVolume))
			{
				player.QueueFree();
				return;
			}
			player.VolumeDb = Mathf.LinearToDb(linearVolume);
			if (_loggedSfx.Add(localPath))
			{
				Log.Debug($"mod_sfx '{localPath}' vol={volume} gain={gain} linear={linearVolume} db={player.VolumeDb} bus='{player.Bus}'");
			}

			if (NGame.Instance != null)
			{
				NGame.Instance.AddChild(player);
			}
			else
			{
				Log.Error($"TH_Patchouli mod_sfx can't play because NGame.Instance is null. Path: {path}");
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
}
