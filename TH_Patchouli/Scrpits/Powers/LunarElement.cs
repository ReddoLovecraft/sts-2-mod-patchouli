using BaseLib.Abstracts;
using BaseLib.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Patchouib.Scrpits.Main;
using TH_Patchouli.Relics;


namespace TH_Patchouli.Scrpits.Powers
{
    public sealed class LunarElement : CustomPowerModel,IVisiblePower
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;
        public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/LE32.png";
        public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/LE64.png";

        public string TscnPath => "res://TH_Patchouli/ArtWorks/VFX/lunar.tscn";

        //protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IgnitePower>()];
        public LunarElement() { }
         public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (player != base.Owner.Player)
            {
                return;
            }
            this.Flash();
            await CardPileCmd.Draw(choiceContext,Amount/2,Owner.Player);
			 if(Owner.Player.GetRelic<EmeraldTablet>()==null)
			await PowerCmd.Decrement(this);
        }
    }
}
