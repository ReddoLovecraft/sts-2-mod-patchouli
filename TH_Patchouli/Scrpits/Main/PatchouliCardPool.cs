using BaseLib.Abstracts;
using Godot;

namespace TH_Patchouli.Scripts.Main
{
	public class PatchouliCardPool : CustomCardPoolModel
{
	public override string Title => "TH_Patchouli";

 	public override Color ShaderColor => new Color("c66ffcff");
	public override Color DeckEntryCardColor => new Color("b972fcff");
  	public override string? BigEnergyIconPath => "res://TH_Patchouli/ArtWorks/Character/card_orb.png";
	public override string? TextEnergyIconPath => "res://TH_Patchouli/ArtWorks/Character/cost_orb.png";
	public override bool IsColorless => false;
}
}
