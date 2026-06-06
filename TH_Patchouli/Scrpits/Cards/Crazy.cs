using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using Patchoulib.Scrpits.Main;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Powers;

namespace TH_Patchouli.Scrpits.Cards
{
	[Pool(typeof(CurseCardPool))]
	public sealed class Crazy : PatchouliCardModel
	{
		public override int MaxUpgradeLevel => 0;
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable,CardKeyword.Eternal];

		public Crazy() : base(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
		{
		}
		 public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card == this)
        {
            await Cmd.Wait(0.25f);
            foreach(CardModel cardModel in PileType.Hand.GetPile(Owner).Cards.ToList())
			{
				await CardCmd.AutoPlay(choiceContext,cardModel,null);
				cardModel.EnergyCost.AddThisCombat(1);
			}
        }
    }
	}
}
