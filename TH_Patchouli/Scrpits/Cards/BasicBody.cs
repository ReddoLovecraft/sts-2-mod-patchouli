using BaseLib.Utils;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using Patchoulib.Scrpits.Main;
using System.Collections.Generic;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(StatusCardPool))]
	public sealed class BasicBody : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes =
		[
			ElementEnum.Gold,
			ElementEnum.Wood,
			ElementEnum.Water,
			ElementEnum.Fire,
			ElementEnum.Dirt,
			ElementEnum.Sun,
			ElementEnum.Lunar,
		];

		public override List<ElementEnum> ElementTypes => _elementTypes;
		public override bool isSingleElement => false;
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable, CardKeyword.Retain];
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("Element")];

		public BasicBody() : base(-1, CardType.Status, CardRarity.Status, TargetType.None)
		{
		}
	}
}
