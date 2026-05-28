using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;
using TH_Patchouli.Scrpits.Powers.NewPowers;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(TH_Patchouli.Scripts.Main.SpellCardPool))]
	public sealed class CircularLavaBelt : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new() { ElementEnum.Fire, ElementEnum.Dirt };
		public override List<ElementEnum> ElementTypes => _elementTypes;

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromPower<IgnitePower>(),HoverTipFactory.Static(StaticHoverTip.Block)
		];

		public CircularLavaBelt() : base(2, CardType.Power, CardRarity.Ancient, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await PowerCmd.Apply<CircularLavaBeltPower>(Owner.Creature, 1, Owner.Creature, this);
		}
	}
}
