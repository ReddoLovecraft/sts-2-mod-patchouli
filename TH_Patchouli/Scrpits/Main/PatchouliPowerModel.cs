using BaseLib.Abstracts;

namespace TH_Patchouli.Scripts.Main
{
	public abstract class PatchouliPowerModel : CustomPowerModel
	{
		public virtual void Trigger()
		{
			Flash();
		}
	}
	
}
