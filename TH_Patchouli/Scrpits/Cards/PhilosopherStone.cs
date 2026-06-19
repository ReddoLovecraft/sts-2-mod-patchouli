using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Patchouib.Scrpits.Main;
using Patchoulib.Scrpits.Main;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Powers;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(SpellCardPool))]
	public sealed class PhilosopherStone : PatchouliCardModel
	{
		private static readonly List<ElementEnum> _elementTypes = [ElementEnum.Fire, ElementEnum.Water, ElementEnum.Wood, ElementEnum.Gold, ElementEnum.Dirt];
		public override List<ElementEnum> ElementTypes => _elementTypes;

		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("Element")];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

		public PhilosopherStone() : base(2, CardType.Skill, CardRarity.Ancient, TargetType.Self)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			List<PowerModel> bounsPowers = new();
			foreach(PowerModel buff in Owner.Creature.Powers)
			{
				if(buff.Type!=PowerType.Buff)
				{
					continue;
				}
				if(buff.StackType!=PowerStackType.Counter)
				{
					continue;
				}
				if (buff.InstanceType != PowerInstanceType.None)
				{
					continue;
				}
				bounsPowers.Add(buff);
			}
			foreach(PowerModel buff in bounsPowers)
			{
				await PowerCmd.Apply(choiceContext, buff, Owner.Creature, buff.Amount, Owner.Creature, null);
			}
		}

		protected override void OnUpgrade()
		{
			EnergyCost.UpgradeBy(-1);
		}
	}
}

