using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchouib.Scripts.Main;
using TH_Patchouli.Scrpits.Cards;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class SunflareTempStrengthPower : CustomTempStrengthPower
	{
		public override AbstractModel OriginModel => ModelDb.Card<Sunflare>();
		public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/SF32.png";  
        public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/SF64.png";
	}
}

