using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Patchouli.Scripts.Main;
namespace TH_Patchouli.Scrpits.Cards
{
[Pool(typeof(PatchouliCardPool))]
public sealed class DefendSun : PatchouliCardModel
{
	private static readonly List<ElementEnum> _elementTypes = new List<ElementEnum> { ElementEnum.Sun };
	public override List<ElementEnum> ElementTypes => _elementTypes;

	public override bool GainsBlock => true;

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Defend };

	protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> { new BlockVar(5m, ValueProp.Move) };

	public DefendSun()
		: base(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Block.UpgradeValueBy(3);
	}
}

[Pool(typeof(PatchouliCardPool))]
public sealed class DefendLunar : PatchouliCardModel
{
	private static readonly List<ElementEnum> _elementTypes = new List<ElementEnum> { ElementEnum.Lunar };
	public override List<ElementEnum> ElementTypes => _elementTypes;

	public override bool GainsBlock => true;

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Defend };

	protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> { new BlockVar(5m, ValueProp.Move) };

	public DefendLunar()
		: base(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Block.UpgradeValueBy(3);
	}
}

}
