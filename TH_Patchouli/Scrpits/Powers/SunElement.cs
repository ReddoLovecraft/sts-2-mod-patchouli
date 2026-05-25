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


namespace TH_Patchouli.Scrpits.Powers
{
    public sealed class SunElement : CustomPowerModel,IVisiblePower
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;
        public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/SE32.png";  
        public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/SE64.png";

        public string TscnPath => "res://TH_Patchouli/ArtWorks/VFX/sun.tscn";

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];
        public SunElement() { }
         public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (player != base.Owner.Player)
            {
                return;
            }
            this.Flash();
            await PlayerCmd.GainEnergy(Amount/2,Owner.Player);
			await PowerCmd.Decrement(this);
        }
    }
}