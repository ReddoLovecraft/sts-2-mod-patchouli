using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using Patchoulib.Scrpits.Main;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scrpits.Powers.NewPowers;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class ElementMagician : PatchouliCardModel
	{
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("Element")];

		public ElementMagician() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await PowerCmd.Apply<ElementMagicianPower>(Owner.Creature, 1, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			EnergyCost.UpgradeBy(-1);
		}
	}
}
