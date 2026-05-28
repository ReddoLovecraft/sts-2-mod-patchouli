using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class StrikeFire : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = new List<ElementEnum> { ElementEnum.Fire };
		public override List<ElementEnum> ElementTypes => _elementTypes;
		protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Strike };
		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6, ValueProp.Move)];

		public StrikeFire() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
		{
		}
		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Damage.UpgradeValueBy(boostAmount);
		}
		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(3);
		}
	}
}
