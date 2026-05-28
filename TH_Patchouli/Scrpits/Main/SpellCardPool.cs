using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Models.Cards;

namespace TH_Patchouli.Scripts.Main
{
	public class SpellCardPool : CustomCardPoolModel,IVisibleCardPool
{
	public override string Title => "SpellCardPool";

 	public override Color ShaderColor => new Color("c66ffcff");
	public override Color DeckEntryCardColor => new Color("b972fcff");
  	public override string? BigEnergyIconPath => "res://TH_Patchouli/ArtWorks/Character/card_orb.png";
	public override string? TextEnergyIconPath => "res://TH_Patchouli/ArtWorks/Character/cost_orb.png";
	public override bool IsColorless => false;

        public string GetCardLibraryIconPath()
        {
            return "res://TH_Patchouli/ArtWorks/Character/pool_icon.png";
        }
		 public string? GetCardLibraryHoverTipKey() => "Spellcard";
		
    }
}
