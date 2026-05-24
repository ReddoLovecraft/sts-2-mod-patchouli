using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Patches.Content;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Patchouib.Scrpits.Main;
using TH_Patchouli.Scrpits.Powers;


namespace TH_Patchouli.Scripts.Main
{
	internal static class StartingDeckElementCapture
	{
		[ThreadStatic]
		private static Queue<List<ElementEnum>>? _queue;

		[ThreadStatic]
		private static bool _active;

		public static bool Active => _active;

		public static void Begin()
		{
			_active = true;
			_queue ??= new Queue<List<ElementEnum>>();
			_queue.Clear();
		}

		public static void End()
		{
			_active = false;
			_queue?.Clear();
		}

		public static void Enqueue(List<ElementEnum> elements)
		{
			if (!_active)
			{
				return;
			}
			_queue ??= new Queue<List<ElementEnum>>();
			_queue.Enqueue(elements);
		}

		public static bool TryDequeue(out List<ElementEnum> elements)
		{
			if (!_active || _queue == null || _queue.Count == 0)
			{
				elements = new List<ElementEnum> { ElementEnum.None };
				return false;
			}
			elements = _queue.Dequeue();
			return true;
		}
	}

	public abstract class PatchouliCardModel : CustomCardModel,IRightClickableCardModel
	{
		public List<ElementEnum> _ElementTypes = new List<ElementEnum>{ElementEnum.None};
		public virtual List<ElementEnum> ElementTypes => _ElementTypes;
		public virtual bool isSingleElement=>true;
		private const long ElementCycleMs = 1800;
		//public override string PortraitPath => $"res://TH_Patchouli/ArtWorks/Cards/{Id.Entry}.png";
		public PatchouliCardModel(int baseCost, CardType type, CardRarity rarity, TargetType target, bool showInCardLibrary = true, bool autoAdd = true)
	 : base(baseCost, type, rarity, target, showInCardLibrary)
		{
			if (autoAdd)
			{
				CustomContentDictionary.AddModel(GetType());
			}
		}
		public CardModel SetElementTypes(List<ElementEnum> elementTypes)
		{
			List<ElementEnum> copied = elementTypes == null ? new List<ElementEnum> { ElementEnum.None } : new List<ElementEnum>(elementTypes);
			if (!IsMutable && StartingDeckElementCapture.Active)
			{
				StartingDeckElementCapture.Enqueue(copied);
				return this;
			}
			_ElementTypes = copied;
			return this;
		}

		protected override void DeepCloneFields()
		{
			base.DeepCloneFields();
			_ElementTypes = new List<ElementEnum>(_ElementTypes);
		}

		public bool HasElementVisuals
		{
			get
			{
				List<ElementEnum>? list = ElementTypes ?? _ElementTypes;
				if (list == null || list.Count == 0)
				{
					return false;
				}
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] != ElementEnum.None)
					{
						return true;
					}
				}
				return false;
			}
		}

		public ElementEnum GetVisualElementNow()
		{
			List<ElementEnum>? list = ElementTypes ?? _ElementTypes;
			if (list == null || list.Count == 0)
			{
				return ElementEnum.None;
			}

			int visualCount = 0;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] != ElementEnum.None)
				{
					visualCount++;
				}
			}
			if (visualCount <= 0)
			{
				return ElementEnum.None;
			}

			int visualIndex = 0;
			if (visualCount > 1)
			{
				ulong ticks = Time.GetTicksMsec();
				visualIndex = (int)((ticks / (ulong)ElementCycleMs) % (ulong)visualCount);
			}

			int seen = 0;
			for (int i = 0; i < list.Count; i++)
			{
				ElementEnum e = list[i];
				if (e == ElementEnum.None)
				{
					continue;
				}
				if (seen == visualIndex)
				{
					return e;
				}
				seen++;
			}

			return ElementEnum.None;
		}

		public static string? GetElementIconKey(ElementEnum element)
		{
			return element switch
			{
				ElementEnum.Fire => "fire",
				ElementEnum.Gold => "gold",
				ElementEnum.Water => "water",
				ElementEnum.Wood => "wood",
				ElementEnum.Dirt => "dirt",
				ElementEnum.Lunar => "lunar",
				ElementEnum.Sun => "sun",
				_ => null
			};
		}

		public static string? GetElementCardOrbPath(ElementEnum element)
		{
			string? key = GetElementIconKey(element);
			return key == null ? null : $"res://TH_Patchouli/ArtWorks/Character/{key}_card_orb.png";
		}

		public static string? GetElementCostOrbPath(ElementEnum element)
		{
			string? key = GetElementIconKey(element);
			return key == null ? null : $"res://TH_Patchouli/ArtWorks/Character/{key}_cost_orb.png";
		}

		public static (float h, float s, float v)? GetElementFrameHsv(ElementEnum element)
		{
			return element switch
			{
				ElementEnum.Fire => (0.025f, 0.85f, 1.0f),
				ElementEnum.Water => (0.55f, 0.9f, 1.0f),
				ElementEnum.Wood => (0.32f, 0.45f, 1.2f),
				ElementEnum.Sun => (0.12f, 1.5f, 1.2f),
				ElementEnum.Lunar => (0.89f, 1.2f, 0.95f),
				ElementEnum.Gold => (0.18f, 1.25f, 1.35f),
				ElementEnum.Dirt => (0.07f, 1.25f, 0.9f),
				_ => null
			};
		}

		private static string? GetElementLocKey(ElementEnum element)
		{
			return element switch
			{
				ElementEnum.Gold => "GOLD.title",
				ElementEnum.Wood => "WOOD.title",
				ElementEnum.Water => "WATER.title",
				ElementEnum.Fire => "FIRE.title",
				ElementEnum.Dirt => "DIRT.title",
				ElementEnum.Sun => "SUN.title",
				ElementEnum.Lunar => "LUNAR.title",
				_ => null
			};
		}

		private static string? GetElementTextTag(ElementEnum element)
		{
			return element switch
			{
				ElementEnum.Fire => "red",
				ElementEnum.Gold => "gold",
				ElementEnum.Water => "blue",
				ElementEnum.Wood => "green",
				ElementEnum.Dirt => "brown",
				ElementEnum.Lunar => "purple",
				ElementEnum.Sun => "orange",
				_ => null
			};
		}

		private static string? GetElementFallbackText(ElementEnum element)
		{
			return element switch
			{
				ElementEnum.Gold => "金",
				ElementEnum.Wood => "木",
				ElementEnum.Water => "水",
				ElementEnum.Fire => "火",
				ElementEnum.Dirt => "土",
				ElementEnum.Sun => "日",
				ElementEnum.Lunar => "月",
				_ => null
			};
		}

		public string GetElementSuffixText()
		{
			List<ElementEnum>? list = GetSuffixElementTypes();
			if (list == null || list.Count == 0)
			{
				return string.Empty;
			}

			HashSet<ElementEnum> seen = new HashSet<ElementEnum>();
			List<string> parts = new List<string>();

			for (int i = 0; i < list.Count; i++)
			{
				ElementEnum element = list[i];
				if (element == ElementEnum.None || !seen.Add(element))
				{
					continue;
				}

				string? locKey = GetElementLocKey(element);
				string text = string.Empty;
				bool usedLoc = false;
				if (locKey != null)
				{
					text = new LocString("static_hover_tips", locKey).GetFormattedText();
					usedLoc = !string.IsNullOrEmpty(text);
				}
				if (string.IsNullOrEmpty(text))
				{
					text = GetElementFallbackText(element) ?? string.Empty;
				}
				if (string.IsNullOrEmpty(text))
				{
					continue;
				}

				if (usedLoc)
				{
					parts.Add(text);
				}
				else
				{
					string? tag = GetElementTextTag(element);
					parts.Add(tag == null ? text : $"[{tag}]{text}[/{tag}]");
				}
			}

			return parts.Count == 0 ? string.Empty : string.Join("+", parts);
		}

		private List<ElementEnum>? GetSuffixElementTypes()
		{
			if (_ElementTypes != null)
			{
				for (int i = 0; i < _ElementTypes.Count; i++)
				{
					if (_ElementTypes[i] != ElementEnum.None)
					{
						return _ElementTypes;
					}
				}
			}
			return ElementTypes ?? _ElementTypes;
		}

		public virtual void boostVars(int amount)
		{

		}
		 public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
		{
			if (cardPlay.Card.Owner == base.Owner)
			{
				if (!ReferenceEquals(cardPlay.Card, this))
				{
					return;
				}

				if(isSingleElement)
				{
					int amount = cardPlay.Card.EnergyCost.GetWithModifiers(CostModifiers.Local);
					if(amount<=0)amount=1;
					switch (ElementTypes[0])
				{
					case ElementEnum.Fire:
					await PowerCmd.Apply<FireElement>(Owner.Creature, amount,Owner.Creature,this);
					break;
					case ElementEnum.Water:
					await PowerCmd.Apply<WaterElement>(Owner.Creature, amount,Owner.Creature,this);
					break;
					case ElementEnum.Dirt:
					await PowerCmd.Apply<DirtElement>(Owner.Creature, amount,Owner.Creature,this);
					break;
					case ElementEnum.Gold:
					await PowerCmd.Apply<GoldElement>(Owner.Creature, amount,Owner.Creature,this);
					break;
					case ElementEnum.Wood:
					await PowerCmd.Apply<WoodElement>(Owner.Creature, amount,Owner.Creature,this);
					break;
					case ElementEnum.Lunar:
					await PowerCmd.Apply<LunarElement>(Owner.Creature, amount,Owner.Creature,this);
					break;
					case ElementEnum.Sun:
					await PowerCmd.Apply<SunElement>(Owner.Creature, amount,Owner.Creature,this);
					break;
					case ElementEnum.None:
					break;
				default:
					break;
				}
				}
				
			}
		}
		List<PileType> IRightClickableCardModel.Pile => new List<PileType>{PileType.Hand};

		bool IRightClickableCardModel.IsCombat => true;

		async Task IRightClickableCardModel.OnRightClick(PlayerChoiceContext context)
		{
			if(isSingleElement&&ElementTypes[0] == ElementEnum.None)
			{
				
			}
		}
	}
  
}
