using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Random;
using TH_Patchouli.Scripts.Main;
using TH_Patchouli.Scrpits.Powers;

namespace TH_Patchouli.Scrpits.Main
{
    public static class ToolBox
    {
        
       public static int GetElementKinds(Creature owner)
       {
         int res=0;
         if(owner.HasPower<GoldElement>())res++;
         if(owner.HasPower<LunarElement>())res++;
         if(owner.HasPower<SunElement>())res++;
         if(owner.HasPower<FireElement>())res++;
         if(owner.HasPower<WaterElement>())res++;
         if(owner.HasPower<WoodElement>())res++;
         if(owner.HasPower<DirtElement>())res++;
         return res;
       }
          public static int GetDebuffTotalCount(Creature target) 
        {
            int result = 0;
            foreach(PowerModel debuff in target.Powers) 
            {
                if(debuff.Type==PowerType.Debuff) 
                {
                    if (debuff.Amount > 0)
                        result += debuff.Amount;
                    else
                        result++;
                }
            }
            return result;

        }
        public static int GetDebuffKind(Creature target)
        {
            int result = 0;
            foreach (PowerModel debuff in target.Powers)
            {
                if (debuff.Type == PowerType.Debuff)
                {
                        result++;
                }
            }
            return result;
        }
       public static async Task OpenElementSelectGirdForGain(PlayerChoiceContext choiceContext,Creature owner,int GainedAmount)
    {
            CombatState combatState = owner.CombatState;
            Player player = owner.Player;
            CardSelectorPrefs prefs = new CardSelectorPrefs(GetCustomText("static_hover_tips","element",".selectionScreenPrompt"), 1);
            List<CardModel> cards =
            [
                combatState.CreateCard<Cards.GoldElement>(player),
                combatState.CreateCard<Cards.LunarElement>(player),
                combatState.CreateCard<Cards.SunElement>(player),
                combatState.CreateCard<Cards.FireElement>(player),
                combatState.CreateCard<Cards.WaterElement>(player),
                combatState.CreateCard<Cards.WoodElement>(player),
                combatState.CreateCard<Cards.DirtElement>(player),
            ];
            CardModel cardModel = (await CardSelectCmd.FromSimpleGrid(choiceContext, cards, owner.Player, prefs)).FirstOrDefault();
            if (cardModel != null)
            {
                await ((PatchouliCardModel)cardModel).OnChosen(GainedAmount);
            }
        
    }
         public static async Task OpenElementSelectGirdForCard(PlayerChoiceContext choiceContext,Creature owner,PatchouliCardModel toGainElementCard)
    {
            CombatState combatState = owner.CombatState;
            Player player = owner.Player;
            CardSelectorPrefs prefs = new CardSelectorPrefs(GetCustomText("static_hover_tips","element",".selectionScreenPrompt"), 1);
            List<CardModel> cards =
            [
                combatState.CreateCard<Cards.GoldElement>(player),
                combatState.CreateCard<Cards.LunarElement>(player),
                combatState.CreateCard<Cards.SunElement>(player),
                combatState.CreateCard<Cards.FireElement>(player),
                combatState.CreateCard<Cards.WaterElement>(player),
                combatState.CreateCard<Cards.WoodElement>(player),
                combatState.CreateCard<Cards.DirtElement>(player),
            ];
            CardModel cardModel = (await CardSelectCmd.FromSimpleGrid(choiceContext, cards, owner.Player, prefs)).FirstOrDefault();
            if (cardModel != null)
            {
               if(cardModel is Cards.GoldElement )
               { 
                 toGainElementCard.SetElementTypes(new List<ElementEnum>{ ElementEnum.Gold});
               }
               else if(cardModel is Cards.LunarElement )
               { 
                 toGainElementCard.SetElementTypes(new List<ElementEnum>{ ElementEnum.Lunar});
               }
               else if(cardModel is Cards.SunElement )
               { 
                 toGainElementCard.SetElementTypes(new List<ElementEnum>{ ElementEnum.Sun});
               }
               else if(cardModel is Cards.FireElement )
               { 
                 toGainElementCard.SetElementTypes(new List<ElementEnum>{ ElementEnum.Fire});
               }
               else if(cardModel is Cards.WaterElement )
               { 
                 toGainElementCard.SetElementTypes(new List<ElementEnum>{ ElementEnum.Water});
               }
               else if(cardModel is Cards.WoodElement )
               { 
                 toGainElementCard.SetElementTypes(new List<ElementEnum>{ ElementEnum.Wood});
               }
               else if(cardModel is Cards.DirtElement )
               { 
                 toGainElementCard.SetElementTypes(new List<ElementEnum>{ ElementEnum.Dirt});
               }
            }
        
    }
        public static LocString L10NStatic(string entry,string targetTable="static_hover_tips")
        {
                return new LocString(targetTable, entry);
        }
        public static LocString GetCustomText(string targetTable,string entry,string postfix)
        {
                string text = StringHelper.Slugify(entry);
                LocString res = L10NStatic(text + postfix, targetTable);
                return res;
        }
        public static async Task GainElement(List<ElementEnum> elementList,int amount,Creature owner)
        {
            foreach(ElementEnum e in elementList)
            {
               switch(e)
               {
                    case ElementEnum.Gold:
                        await PowerCmd.Apply<GoldElement>(owner,amount,owner,null);
                        break;
                    case ElementEnum.Lunar:
                        await PowerCmd.Apply<LunarElement>(owner,amount,owner,null);
                        break;
                    case ElementEnum.Sun:
                        await PowerCmd.Apply<SunElement>(owner,amount,owner,null);
                        break;
                    case ElementEnum.Fire:
                        await PowerCmd.Apply<FireElement>(owner,amount,owner,null);
                        break;
                    case ElementEnum.Water:
                        await PowerCmd.Apply<WaterElement>(owner,amount,owner,null);
                        break;
                    case ElementEnum.Wood:
                        await PowerCmd.Apply<WoodElement>(owner,amount,owner,null);
                        break;
                    case ElementEnum.Dirt:
                        await PowerCmd.Apply<DirtElement>(owner,amount,owner,null);
                        break;
                    default:
                        break;
               }
            }

            Player? player = owner.Player;
            if (player == null || owner.CombatState == null)
            {
                return;
            }

            int totalGained = Math.Max(0, amount) * Math.Max(0, elementList.Count);
            if (totalGained <= 0)
            {
                return;
            }

            if (elementList.Contains(ElementEnum.Sun))
            {
                List<CardModel> drawCards = PileType.Draw.GetPile(player).Cards.ToList();
                foreach (CardModel card in drawCards)
                {
                    if (card is TH_Patchouli.Scrpits.Cards.SunRiseLight)
                    {
                        await CardCmd.AutoPlay(new BlockingPlayerChoiceContext(), card, null);
                    }
                }
            }

            foreach (PileType pileType in new[] { PileType.Hand, PileType.Draw, PileType.Discard, PileType.Exhaust, PileType.Play })
            {
                foreach (CardModel card in pileType.GetPile(player).Cards)
                {
                    if (card is TH_Patchouli.Scrpits.Cards.SevenWitch)
                    {
                        card.EnergyCost.AddThisCombat(-totalGained, reduceOnly: true);
                    }
                }
            }
        }
        public static async Task<List<ElementEnum>> GainElementRandomly(int amount,Creature owner)
        {
            var randomElement = new List<ElementEnum>();
            if (amount <= 0 || owner.Player?.RunState?.Rng == null)
            {
                return randomElement;
            }

            Rng rng = owner.Player.RunState.Rng.CombatCardGeneration;
            for (int i = 0; i < amount; i++)
            {
                int randomNumber = rng.NextInt(1, 8);
                switch(randomNumber)
                {
                    case 1:
                        randomElement.Add(ElementEnum.Gold);
                        break;
                    case 2:
                        randomElement.Add(ElementEnum.Lunar);
                        break;
                    case 3:
                        randomElement.Add(ElementEnum.Sun);
                        break;
                    case 4:
                        randomElement.Add(ElementEnum.Fire);
                        break;
                    case 5:
                        randomElement.Add(ElementEnum.Water);
                        break;
                    case 6:
                        randomElement.Add(ElementEnum.Wood);
                        break;
                    case 7:
                        randomElement.Add(ElementEnum.Dirt);
                        break;
                    default:
                        break;
                }
            }
            if (randomElement.Count > 0)
            {
                await GainElement(randomElement, 1, owner);
            }
            return randomElement;
        }
    }
    
}
