using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Patches.Content;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using Patchouib.Scrpits.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using TH_Patchouli.Scrpits.Cards;
using TH_Patchouli.Scrpits.Main;
using TH_Patchouli.Scrpits.Multiplayer;
using ElementPowers = TH_Patchouli.Scrpits.Powers;
using TH_Patchouli.Relics;


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
		public override string PortraitPath => $"res://TH_Patchouli/ArtWorks/Cards/{Id.Entry}.png";
		public PatchouliCardModel(int baseCost, CardType type, CardRarity rarity, TargetType target, bool showInCardLibrary = true, bool autoAdd = true)
	 : base(baseCost, type, rarity, target, showInCardLibrary)
		{
			if (autoAdd)
			{
				CustomContentDictionary.AddModel(GetType());
			}
		}
		public virtual void BoostWhenElementEnhanced(int boostAmount)
		{
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
				int amount = cardPlay.Card.EnergyCost.GetWithModifiers(CostModifiers.Local);
				if(amount<=0)amount=1;
				await ToolBox.GainElement(this.ElementTypes,amount,Owner.Creature);
			}
		}
		List<PileType> IRightClickableCardModel.Pile => new List<PileType>{PileType.Hand};

		bool IRightClickableCardModel.IsCombat => true;

		async Task IRightClickableCardModel.OnRightClick(PlayerChoiceContext context)
		{
			if (CombatState == null || Owner?.Creature == null)
			{
				return;
			}

			if (ToolBox.GetElementKinds(Owner.Creature) <= 0)
			{
				return;
			}

			bool isMultiplayer = RunManager.Instance?.NetService?.Type.IsMultiplayer() ?? false;
			if (isMultiplayer && !LocalContext.IsMe(Owner))
			{
				return;
			}

			List<ElementEnum> originalElements = GetDistinctElementsOrdered(ElementTypes);
			HashSet<ElementEnum> originalElementsSet = originalElements.Count == 0 ? new HashSet<ElementEnum>() : new HashSet<ElementEnum>(originalElements);
			bool originalHasAnyElement = originalElements.Count > 0;

			int enhanceConsume = 0;
			bool canEnhance = originalHasAnyElement && TryGetEnhanceConsumeAmount(Owner.Creature, originalElements, out enhanceConsume);

			List<CardModel> transformTargets = GetTransformTargetsFromSpellPool(CombatState, Owner, this, originalElements, originalElementsSet);
			bool canTransform = transformTargets.Count > 0;

			List<CardModel> options = new List<CardModel>(2);
			if (canEnhance)
			{
				options.Add(CreatePreviewCard<Enhancement>(Owner));
			}
			if (canTransform)
			{
				options.Add(CreatePreviewCard<Transformation>(Owner));
			}

			if (options.Count == 0)
			{
				return;
			}

			CardModel selectedOption;
			if (isMultiplayer)
			{
				NPlayerHand.Instance?.CancelAllCardPlay();
				NChooseACardSelectionScreen screen = NChooseACardSelectionScreen.ShowScreen(options, canSkip: true);
				selectedOption = (await screen.CardsSelected()).FirstOrDefault();
			}
			else
			{
				selectedOption = await CardSelectCmd.FromChooseACardScreen(context, options, Owner, canSkip: true);
			}
			if (selectedOption == null)
			{
				return;
			}
			
			if (selectedOption is Enhancement)
			{
				if (enhanceConsume <= 0 || !canEnhance)
				{
					return;
				}
				if (isMultiplayer)
				{
					await PatchouliEnhancementRightClickSync.DoLocalAndSync(Owner, this, enhanceConsume);
					if(Owner.Character is PatchouliCharacter pc)
					{
						await CreatureCmd.TriggerAnim(base.Owner.Creature, "Spell", base.Owner.Character.CastAnimDelay);
					}
					else
					{
						await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
					}
					SfxCmd.Play(PatchouliInit.ToModSfxPath("TH_Patchouli/ArtWorks/SFX/spell.wav"));
					return;
				}
				if(Owner.GetRelic<ElementBook>()!=null)
				{
					CardCmd.Upgrade(this);
				}
				await ConsumeElements(Owner.Creature, originalElements, enhanceConsume, this);
				BoostWhenElementEnhanced(enhanceConsume);
				if (Pile != null)
				{
					NCard.FindOnTable(this)?.UpdateVisuals(Pile.Type, CardPreviewMode.Normal);
				}
			}
			else if (selectedOption is Transformation)
			{
				if (!canTransform)
				{
					return;
				}
				CardSelectorPrefs prefs = new CardSelectorPrefs(ToolBox.GetCustomText("static_hover_tips", "transform", ".selectionScreenPrompt"), 0, 1);
				CardModel chosen;
				if (isMultiplayer)
				{
					NPlayerHand.Instance?.CancelAllCardPlay();
					NSimpleCardSelectScreen grid = NSimpleCardSelectScreen.Create(transformTargets.ToList(), prefs);
					NOverlayStack.Instance.Push(grid);
					chosen = (await grid.CardsSelected()).FirstOrDefault();
				}
				else
				{
					chosen = (await CardSelectCmd.FromSimpleGrid(context, transformTargets, Owner, prefs)).FirstOrDefault();
				}
				if (chosen == null)
				{
					return;
				}

				int poolIndex = GetSpellCardPoolIndex(CombatState, Owner, chosen);
				if (poolIndex < 0)
				{
					return;
				}

				if (isMultiplayer)
				{
					await PatchouliTransformationRightClickSync.DoLocalAndSync(Owner, this, poolIndex);
					return;
				}

				CardModel replacement = CreateSpellCardFromPoolIndex(CombatState, Owner, poolIndex);
				if (replacement == null)
				{
					return;
				}
				List<(ElementEnum element, int amount)> toConsume = CalculateTransformElementConsumption(this, replacement, originalElementsSet);
				if(Owner.Character is PatchouliCharacter pc)
				{
						await CreatureCmd.TriggerAnim(base.Owner.Creature, "Spell", base.Owner.Character.CastAnimDelay);
				}
				else
				{
						await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
				}
				SfxCmd.Play(PatchouliInit.ToModSfxPath("TH_Patchouli/ArtWorks/SFX/spell.wav"));
				await CardCmd.Transform(this, replacement);
				if(Owner.GetRelic<ElementBook>()!=null)
				{
					CardCmd.Upgrade(replacement);
				}
				CardCmd.Preview(replacement);
				for (int i = 0; i < toConsume.Count; i++)
				{
					(ElementEnum element, int amount) = toConsume[i];
					if (amount <= 0)
					{
						continue;
					}
					await ConsumeElement(Owner.Creature, element, amount, this);
				}
			}
		}

		private static int GetSpellCardPoolIndex(CombatState combatState, Player owner, CardModel card)
		{
			List<CardModel> pool = CreateSpellCardPoolPreviewCards(owner);
			for (int i = 0; i < pool.Count; i++)
			{
				if (pool[i].Id.Entry == card.Id.Entry)
				{
					return i;
				}
			}
			return -1;
		}

		private static T CreatePreviewCard<T>(Player owner) where T : CardModel
		{
			T card = (T)ModelDb.Card<T>().ToMutable();
			card.Owner = owner;
			return card;
		}

		private static List<ElementEnum> GetDistinctElementsOrdered(List<ElementEnum>? elements)
		{
			List<ElementEnum> result = new List<ElementEnum>();
			if (elements == null || elements.Count == 0)
			{
				return result;
			}

			HashSet<ElementEnum> seen = new HashSet<ElementEnum>();
			for (int i = 0; i < elements.Count; i++)
			{
				ElementEnum e = elements[i];
				if (e != ElementEnum.None && seen.Add(e))
				{
					result.Add(e);
				}
			}
			result.Sort();
			return result;
		}

		private static bool TryGetEnhanceConsumeAmount(Creature owner, List<ElementEnum> cardElements, out int consumeAmount)
		{
			consumeAmount = 0;
			if (cardElements == null || cardElements.Count == 0)
			{
				return false;
			}

			int min = int.MaxValue;
			foreach (ElementEnum e in cardElements)
			{
				int amount = GetElementAmount(owner, e);
				if (amount <= 0)
				{
					consumeAmount = 0;
					return false;
				}
				min = Math.Min(min, amount);
			}

			consumeAmount = min == int.MaxValue ? 0 : Math.Max(0, min);
			return consumeAmount > 0;
		}

		private static int GetElementAmount(Creature owner, ElementEnum element)
		{
			return element switch
			{
				ElementEnum.Gold => owner.GetPower<ElementPowers.GoldElement>()?.Amount ?? 0,
				ElementEnum.Lunar => owner.GetPower<ElementPowers.LunarElement>()?.Amount ?? 0,
				ElementEnum.Sun => owner.GetPower<ElementPowers.SunElement>()?.Amount ?? 0,
				ElementEnum.Fire => owner.GetPower<ElementPowers.FireElement>()?.Amount ?? 0,
				ElementEnum.Water => owner.GetPower<ElementPowers.WaterElement>()?.Amount ?? 0,
				ElementEnum.Wood => owner.GetPower<ElementPowers.WoodElement>()?.Amount ?? 0,
				ElementEnum.Dirt => owner.GetPower<ElementPowers.DirtElement>()?.Amount ?? 0,
				_ => 0
			};
		}

		private static async Task ConsumeElements(Creature owner, List<ElementEnum> elements, int amount, CardModel? source)
		{
			if (amount <= 0 || elements == null || elements.Count == 0)
			{
				return;
			}

			for (int i = 0; i < elements.Count; i++)
			{
				await ConsumeElement(owner, elements[i], amount, source);
			}
		}

		private static Task ConsumeElement(Creature owner, ElementEnum element, int amount, CardModel? source)
		{
			if (amount <= 0)
			{
				return Task.CompletedTask;
			}

			return element switch
			{
				ElementEnum.Gold => PowerCmd.Apply<ElementPowers.GoldElement>(owner, -amount, owner, source),
				ElementEnum.Lunar => PowerCmd.Apply<ElementPowers.LunarElement>(owner, -amount, owner, source),
				ElementEnum.Sun => PowerCmd.Apply<ElementPowers.SunElement>(owner, -amount, owner, source),
				ElementEnum.Fire => PowerCmd.Apply<ElementPowers.FireElement>(owner, -amount, owner, source),
				ElementEnum.Water => PowerCmd.Apply<ElementPowers.WaterElement>(owner, -amount, owner, source),
				ElementEnum.Wood => PowerCmd.Apply<ElementPowers.WoodElement>(owner, -amount, owner, source),
				ElementEnum.Dirt => PowerCmd.Apply<ElementPowers.DirtElement>(owner, -amount, owner, source),
				_ => Task.CompletedTask
			};
		}

		internal static int GetTransformEnergyCost(CardModel card)
		{
			if (card is BasicBody)
			{
				return 1;
			}

			int cost = card.EnergyCost.GetWithModifiers(CostModifiers.Local);
			return Math.Max(0, cost);
		}

		private static List<CardModel> GetTransformTargetsFromSpellPool(
			CombatState combatState,
			Player owner,
			PatchouliCardModel original,
			List<ElementEnum> originalElements,
			HashSet<ElementEnum> originalElementsSet)
		{
			List<CardModel> results = new List<CardModel>();
			List<CardModel> allSpellCards = CreateSpellCardPoolPreviewCards(owner);
			if (allSpellCards.Count == 0)
			{
				return results;
			}

			int originalCost = GetTransformEnergyCost(original);

			for (int i = 0; i < allSpellCards.Count; i++)
			{
				CardModel candidate = allSpellCards[i];
				if (candidate == null || ReferenceEquals(candidate, original) || candidate.GetType() == original.GetType())
				{
					continue;
				}

				if (candidate is not PatchouliCardModel candidatePcm)
				{
					continue;
				}

				List<ElementEnum> candidateElements = GetDistinctElementsOrdered(candidatePcm.ElementTypes);
				if (candidateElements.Count <= 0)
				{
					continue;
				}
				HashSet<ElementEnum> candidateElementsSet = new HashSet<ElementEnum>(candidateElements);

				if (original is not BasicBody)
				{
					bool containsAllOriginal = true;
					foreach (ElementEnum e in originalElements)
					{
						if (!candidateElementsSet.Contains(e))
						{
							containsAllOriginal = false;
							break;
						}
					}
					if (!containsAllOriginal)
					{
						continue;
					}
				}

				int candidateCost = GetTransformEnergyCost(candidatePcm);
				bool ok = true;
				for (int ei = 0; ei < candidateElements.Count; ei++)
				{
					ElementEnum e = candidateElements[ei];
					int playerAmount = GetElementAmount(owner.Creature, e);
					int credit = originalElementsSet.Contains(e) ? originalCost : 0;
					if (playerAmount + credit < candidateCost)
					{
						ok = false;
						break;
					}
				}
				if (!ok)
				{
					continue;
				}

				results.Add(candidatePcm);
			}

			return results;
		}

		private static List<CardModel> CreateSpellCardPoolPreviewCards(Player owner)
		{
			List<CardModel> cards =
			[
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.SecretChamber>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.PhilosopherStone>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.HydrogenousProminence>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.MoonDirt>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.GoodUp>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.GroundMouthpiece>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.BurnLunar>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.SunHangSky>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.EtheralFire>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.AshesRise>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.WaterMoon>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.NoachianDeluge>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.PhlogisticPillar>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.SunRise>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.Swamp>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.StarPredictor>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.WhisperRoot>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.AwakeEdict>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.GroundToSky>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.GoldSun>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.GoldSilver>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.WaterElf>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.LunarFire>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.SquareRingSword>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.LunarReaper>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.ElementalHarvester>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.LunarBook>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.RootActivation>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.SatelliteHimawari>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.SunshineReflector>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.EmeraldMegalith>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.DamoclesSword>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.RoyalDiamondRing>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.CircularLavaBelt>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.SunLunarTogether>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.HephaestusHammer>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.LavaCromlech>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.EmeraldMegalopolis>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.MercuryPoison>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.PhlogisticRain>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.Photosynthesis>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.TidalForce>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.GingerGust>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.ForestBlaze>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.FireWind>(owner),
				CreatePreviewCard<TH_Patchouli.Scrpits.Cards.ElmoPillar>(owner),
			];

			return cards;
		}

		internal static CardModel? CreateSpellCardFromPoolIndex(CombatState combatState, Player owner, int index)
		{
			return index switch
			{
				0 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.SecretChamber>(owner),
				1 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.PhilosopherStone>(owner),
				2 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.HydrogenousProminence>(owner),
				3 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.MoonDirt>(owner),
				4 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.GoodUp>(owner),
				5 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.GroundMouthpiece>(owner),
				6 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.BurnLunar>(owner),
				7 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.SunHangSky>(owner),
				8 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.EtheralFire>(owner),
				9 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.AshesRise>(owner),
				10 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.WaterMoon>(owner),
				11 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.NoachianDeluge>(owner),
				12 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.PhlogisticPillar>(owner),
				13 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.SunRise>(owner),
				14 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.Swamp>(owner),
				15 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.StarPredictor>(owner),
				16 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.WhisperRoot>(owner),
				17 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.AwakeEdict>(owner),
				18 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.GroundToSky>(owner),
				19 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.GoldSun>(owner),
				20 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.GoldSilver>(owner),
				21 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.WaterElf>(owner),
				22 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.LunarFire>(owner),
				23 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.SquareRingSword>(owner),
				24 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.LunarReaper>(owner),
				25 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.ElementalHarvester>(owner),
				26 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.LunarBook>(owner),
				27 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.RootActivation>(owner),
				28 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.SatelliteHimawari>(owner),
				29 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.SunshineReflector>(owner),
				30 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.EmeraldMegalith>(owner),
				31 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.DamoclesSword>(owner),
				32 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.RoyalDiamondRing>(owner),
				33 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.CircularLavaBelt>(owner),
				34 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.SunLunarTogether>(owner),
				35 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.HephaestusHammer>(owner),
				36 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.LavaCromlech>(owner),
				37 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.EmeraldMegalopolis>(owner),
				38 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.MercuryPoison>(owner),
				39 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.PhlogisticRain>(owner),
				40 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.Photosynthesis>(owner),
				41 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.TidalForce>(owner),
				42 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.GingerGust>(owner),
				43 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.ForestBlaze>(owner),
				44 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.FireWind>(owner),
				45 => combatState.CreateCard<TH_Patchouli.Scrpits.Cards.ElmoPillar>(owner),
				_ => null
			};
		}

		internal static List<CardModel> CreateSpellCardPoolCards(CombatState combatState, Player owner)
		{
			List<CardModel> cards =
			[
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.SecretChamber>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.PhilosopherStone>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.HydrogenousProminence>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.MoonDirt>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.GoodUp>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.GroundMouthpiece>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.BurnLunar>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.SunHangSky>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.EtheralFire>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.AshesRise>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.WaterMoon>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.NoachianDeluge>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.PhlogisticPillar>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.SunRise>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.Swamp>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.StarPredictor>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.WhisperRoot>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.AwakeEdict>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.GroundToSky>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.GoldSun>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.GoldSilver>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.WaterElf>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.LunarFire>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.SquareRingSword>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.LunarReaper>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.ElementalHarvester>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.LunarBook>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.RootActivation>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.SatelliteHimawari>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.SunshineReflector>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.EmeraldMegalith>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.DamoclesSword>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.RoyalDiamondRing>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.CircularLavaBelt>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.SunLunarTogether>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.HephaestusHammer>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.LavaCromlech>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.EmeraldMegalopolis>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.MercuryPoison>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.PhlogisticRain>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.Photosynthesis>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.TidalForce>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.GingerGust>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.ForestBlaze>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.FireWind>(owner),
				combatState.CreateCard<TH_Patchouli.Scrpits.Cards.ElmoPillar>(owner),
			];

			return cards;
		}

		internal static List<(ElementEnum element, int amount)> CalculateTransformElementConsumption(PatchouliCardModel original, CardModel transformed, HashSet<ElementEnum> originalElementsSet)
		{
			List<(ElementEnum element, int amount)> toConsume = new List<(ElementEnum element, int amount)>();
			if (transformed is not PatchouliCardModel transformedPcm)
			{
				return toConsume;
			}

			List<ElementEnum> transformedElements = GetDistinctElementsOrdered(transformedPcm.ElementTypes);

			int originalCost = GetTransformEnergyCost(original);
			int transformedCost = GetTransformEnergyCost(transformedPcm);

			for (int i = 0; i < transformedElements.Count; i++)
			{
				ElementEnum e = transformedElements[i];
				int credit = originalElementsSet.Contains(e) ? originalCost : 0;
				int need = Math.Max(0, transformedCost - credit);
				if (need > 0)
				{
					toConsume.Add((e, need));
				}
			}

			return toConsume;
		}

		public virtual async Task<PowerModel> OnChosen(int amount)
		{
		   return null;
		}
	}
  
}
