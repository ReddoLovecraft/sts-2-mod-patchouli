using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(EventCardPool))]
	public sealed class DoubleSevenGroup : PatchouliCardModel
	{
		public override bool GainsBlock => true;
		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7, ValueProp.Move),new BlockVar(7,ValueProp.Move)];

		public DoubleSevenGroup() : base(1, CardType.Attack, CardRarity.Event, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			int damageMultiplier=0;
			int blockMultiplier=0;
			foreach(CardModel card in PileType.Deck.GetPile(base.Owner).Cards)
			{
				if(card.Type==CardType.Attack)
				{
					damageMultiplier++;
				}
				else if(card.Type==CardType.Skill)
				{
					blockMultiplier++;
				}
			}
			damageMultiplier/=7;
			blockMultiplier/=7;
			damageMultiplier++;
			blockMultiplier++;
			decimal final_damage=DynamicVars.Damage.BaseValue*damageMultiplier;
			decimal final_block=DynamicVars.Block.BaseValue*blockMultiplier;
			await DamageCmd.Attack(final_damage).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
			await CreatureCmd.GainBlock(base.Owner.Creature, final_block, ValueProp.Move, cardPlay);
		}
			
		protected override void OnUpgrade()
		{
			this.EnergyCost.UpgradeBy(-1);
		}
	}
}
