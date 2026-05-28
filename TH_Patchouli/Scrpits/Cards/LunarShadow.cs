using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(PatchouliCardPool))]
	public sealed class LunarShadow : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = [ElementEnum.Lunar];
		public override List<ElementEnum> ElementTypes => _elementTypes;
		protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7m, ValueProp.Move)];

		public LunarShadow() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Block.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
			await PowerCmd.Apply<BlurPower>(Owner.Creature, 1, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Block.UpgradeValueBy(3);
		}
	}
}

