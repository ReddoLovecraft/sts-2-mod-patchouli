using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;
using TH_Patchouli.Scrpits.Powers.NewPowers;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(SpellCardPool))]
	public sealed class SunshineReflector : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Lunar, ElementEnum.Gold };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			this.EnergyHoverTip
		];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

		public SunshineReflector() : base(2, CardType.Power, CardRarity.Ancient, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await PowerCmd.Apply<SunshineReflectorPower>(Owner.Creature, 1, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			EnergyCost.UpgradeBy(-1);
		}
	}
}
