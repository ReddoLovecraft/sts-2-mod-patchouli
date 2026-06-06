using BaseLib.Config;

namespace TH_Patchouli.Scripts.Main;

[ConfigHoverTipsByDefault]
public sealed class PatchouliModConfig : SimpleModConfig
{
	[ConfigSection("OPTIONS")]
	[ConfigHoverTip]
	public static bool Frailty { get; set; } = false;
}
