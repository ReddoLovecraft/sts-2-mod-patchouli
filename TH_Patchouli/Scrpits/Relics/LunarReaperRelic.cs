using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using TH_Patchouli.Scrpits.Cards;

namespace TH_Patchouli.Relics
{
	[Pool(typeof(PatchouliRelicPool))]
	public sealed class LunarReaperRelic : CustomRelicModel
	{
		private int _bonusDamage;

		public override string PackedIconPath => $"res://TH_Patchouli/ArtWorks/Relics/{Id.Entry}.png";
		protected override string PackedIconOutlinePath => $"res://TH_Patchouli/ArtWorks/Relics/Outlines/{Id.Entry}.png";
		protected override string BigIconPath => $"res://TH_Patchouli/ArtWorks/Relics/{Id.Entry}.png";

		public override RelicRarity Rarity => RelicRarity.Event;
		public override bool ShowCounter => true;
		public override int DisplayAmount => BonusDamage;

		[SavedProperty]
		public int BonusDamage
		{
			get => _bonusDamage;
			set
			{
				AssertMutable();
				_bonusDamage = value;
				InvokeDisplayAmountChanged();
			}
		}

		public override bool IsAllowed(IRunState runState) => false;

			protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromCard<LunarReaper>()
		];

		public void AddBonus(int amount)
		{
			if (amount <= 0)
			{
				return;
			}

			BonusDamage += amount;
			Status = RelicStatus.Active;
			Flash();
			Status = RelicStatus.Normal;
		}
	}
}
