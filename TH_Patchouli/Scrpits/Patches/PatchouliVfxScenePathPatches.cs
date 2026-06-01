using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scripts.Main
{
	[HarmonyPatch(typeof(SceneHelper))]
	public static class PatchouliVfxScenePathPatches
	{
		[HarmonyPatch(nameof(SceneHelper.GetScenePath))]
		[HarmonyPrefix]
		public static bool GetScenePath_Prefix(string innerPath, ref string __result)
		{
			if (innerPath == null)
			{
				return true;
			}

			if (!innerPath.StartsWith(PatchouliVfxManager.PatchouliVfxPrefix))
			{
				return true;
			}

			string local = innerPath.Substring(PatchouliVfxManager.PatchouliVfxPrefix.Length);
			if (local.StartsWith('/'))
			{
				local = local.Substring(1);
			}
			if (local.EndsWith(".tscn"))
			{
				local = local.Substring(0, local.Length - ".tscn".Length);
			}

			__result = PatchouliVfxManager.PatchouliVfxBaseResPath + local + ".tscn";
			return false;
		}
	}
}
