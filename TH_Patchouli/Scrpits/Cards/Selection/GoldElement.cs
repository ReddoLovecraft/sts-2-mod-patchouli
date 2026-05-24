using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TH_Patchouli.Scrpits.Main;
using TH_Patchouli.Scrpits.Powers;
using TH_Patchouli.Scrpits.Main;
using TH_Patchouli.Scripts.Main;
using MegaCrit.Sts2.Core.Models.Relics;

namespace TH_Patchouli.Scrpits.Cards
{
    [Pool(typeof(QuestCardPool))]
    public sealed class GoldElement : PatchouliCardModel
    {
        public override int MaxUpgradeLevel => 0;
      
        protected override IEnumerable<IHoverTip> ExtraHoverTips => (new IHoverTip[1]
{
    HoverTipFactory.FromPower<TH_Patchouli.Scrpits.Powers.GoldElement>()
});
        public GoldElement()
            : base(-1, CardType.Quest, CardRarity.Quest, TargetType.None)
        {
        }

        public override async Task<PowerModel> OnChosen(int amount)
        {
           return await PowerCmd.Apply<TH_Patchouli.Scrpits.Powers.GoldElement>(Owner.Creature,amount,Owner.Creature,null);
        }
    }

}
