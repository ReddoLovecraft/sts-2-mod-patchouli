using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Patchouib.Scrpits.Main;
using Patchoulib.Scrpits.Main;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Powers;


namespace TH_Patchouli.Relics
{
[Pool(typeof(PatchouliRelicPool))]
public class SkyStone : CustomRelicModel
{
	public override string PackedIconPath => $"res://TH_Patchouli/ArtWorks/Relics/{Id.Entry}.png";
    protected override string PackedIconOutlinePath => $"res://TH_Patchouli/ArtWorks/Relics/Outlines/{Id.Entry}.png";
    protected override string BigIconPath => $"res://TH_Patchouli/ArtWorks/Relics/{Id.Entry}.png";
    public override RelicRarity Rarity => RelicRarity.Uncommon;
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<LunarElement>(),HoverTipFactory.FromPower<SunElement>()];
	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (dealer!=null&&props.IsPoweredAttack()&&dealer == base.Owner.Creature &&Owner.Creature.HasPower<SunElement>()!=null&&target!=Owner.Creature)
		{
				return 1.5m;
		}
		if (Owner.Creature.HasPower<LunarElement>()!=null&&target==Owner.Creature)
		{
				return 0.5m;
		}
			return 1m;
	}
  

}
}
