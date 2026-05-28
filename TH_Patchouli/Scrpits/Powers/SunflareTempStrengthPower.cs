using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchouib.Scripts.Main;
using TH_Patchouli.Scrpits.Cards;

namespace TH_Patchouli.Scrpits.Powers.NewPowers
{
	public sealed class SunflareTempStrengthPower : CustomTempStrengthPower
	{
		public override AbstractModel OriginModel => ModelDb.Card<Sunflare>();
	}
}

