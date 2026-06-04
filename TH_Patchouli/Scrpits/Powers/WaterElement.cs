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
    public sealed class WaterElement : CustomPowerModel,IVisiblePower
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;
        public override string? CustomPackedIconPath => "res://TH_Patchouli/ArtWorks/Powers/WE32.png";
        public override string? CustomBigIconPath => "res://TH_Patchouli/ArtWorks/Powers/WE64.png";

        public string TscnPath => "res://TH_Patchouli/ArtWorks/VFX/water.tscn";

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<FreezePower>()];
        public WaterElement() { }
         public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (player != base.Owner.Player)
            {
                return;
            }
            this.Flash();
            await PowerCmd.Apply<FreezePower>(CombatState.HittableEnemies,Amount,Owner,null);
			 if(Owner.Player.GetRelic<EmeraldTablet>()==null)
			await PowerCmd.Decrement(this);
        }
    }
}
