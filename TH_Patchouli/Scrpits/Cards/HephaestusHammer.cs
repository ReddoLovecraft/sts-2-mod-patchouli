using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scrpits.Main;
using TH_Patchouli.Scrpits.Powers;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(SpellCardPool))]
	public sealed class HephaestusHammer : PatchouliCardModel
	{
		private static readonly List<TH_Patchouli.Scripts.Main.ElementEnum> _elementTypes = new() { TH_Patchouli.Scripts.Main.ElementEnum.Gold, TH_Patchouli.Scripts.Main.ElementEnum.Fire };
		public override List<TH_Patchouli.Scripts.Main.ElementEnum> ElementTypes => _elementTypes;
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

		public HephaestusHammer() : base(1, CardType.Power, CardRarity.Ancient, TargetType.Self)
		{
		}

		public override void BoostWhenElementEnhanced(int boostAmount)
		{
			DynamicVars.Cards.UpgradeValueBy(boostAmount);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			await PowerCmd.Apply<HephaestusHammerPower>(Owner.Creature, DynamicVars.Cards.IntValue, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}
