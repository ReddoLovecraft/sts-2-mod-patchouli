using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchoulib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Main;

namespace TH_Patchouli.Scrpits.Powers
{
	public sealed class WeeklyGirlPower : CustomPowerModel
	{
		private static readonly ElementEnum[] _cycle =
		[
			ElementEnum.Sun,
			ElementEnum.Lunar,
			ElementEnum.Fire,
			ElementEnum.Water,
			ElementEnum.Wood,
			ElementEnum.Gold,
			ElementEnum.Dirt,
		];

		private int _index;

		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override bool IsInstanced => true;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("Element")];
		protected override IEnumerable<DynamicVar> CanonicalVars => [new StringVar("NextElement")];

		public override Task AfterApplied(Creature? applier, CardModel? cardSource)
		{
			UpdateNextElementVar();
			return Task.CompletedTask;
		}

		public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
		{
			if (player != Owner.Player)
			{
				return;
			}

			int gain = Math.Max(0, Amount);
			this.Flash();
			UpdateNextElementVar();
			ElementEnum element = _cycle[_index];
			_index = (_index + 1) % _cycle.Length;
			if (gain > 0)
			{
				await ToolBox.GainElement([element], gain, Owner);
			}
			UpdateNextElementVar();
		}

		private void UpdateNextElementVar()
		{
			((StringVar)DynamicVars["NextElement"]).StringValue = GetElementText(_cycle[_index]);
			InvokeDisplayAmountChanged();
		}

		private static string GetElementText(ElementEnum element)
		{
			string lang = LocManager.Instance.Language;
			return element switch
			{
				ElementEnum.Sun => lang == "zhs" ? "[orange]日[/orange]" : (lang == "jpn" ? "[orange]日[/orange]" : "[orange]Sun[/orange]"),
				ElementEnum.Lunar => lang == "zhs" ? "[purple]月[/purple]" : (lang == "jpn" ? "[purple]月[/purple]" : "[purple]Moon[/purple]"),
				ElementEnum.Fire => lang == "zhs" ? "[red]火[/red]" : (lang == "jpn" ? "[red]火[/red]" : "[red]Fire[/red]"),
				ElementEnum.Water => lang == "zhs" ? "[blue]水[/blue]" : (lang == "jpn" ? "[blue]水[/blue]" : "[blue]Water[/blue]"),
				ElementEnum.Wood => lang == "zhs" ? "[green]木[/green]" : (lang == "jpn" ? "[green]木[/green]" : "[green]Wood[/green]"),
				ElementEnum.Gold => lang == "zhs" ? "[gold]金[/gold]" : (lang == "jpn" ? "[gold]金[/gold]" : "[gold]Gold[/gold]"),
				ElementEnum.Dirt => lang == "zhs" ? "[brown]土[/brown]" : (lang == "jpn" ? "[brown]土[/brown]" : "[brown]Dirt[/brown]"),
				_ => lang == "zhs" ? "无" : (lang == "jpn" ? "無" : "None"),
			};
		}
	}
}
