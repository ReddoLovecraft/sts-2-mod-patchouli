using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using TH_Patchouli.Relics;
using TH_Patchouli.Scripts.Cards;
using TH_Patchouli.Scrpits.Cards;



namespace TH_Patchouli.Scripts.Main
{
	public class PatchouliCharacter : PlaceholderCharacterModel
	{
		public override Color NameColor => new Color("c863fdff");
		public override Color EnergyLabelOutlineColor => new Color("a049e8ff");
		public override Color DialogueColor => new Color("aa1ad6ff");
		public override Color MapDrawingColor => new Color("8b29cfff");
		public override Color RemoteTargetingLineColor => new Color("b325ecff");
		public override Color RemoteTargetingLineOutline => new Color("c32dddff");
		public override CharacterGender Gender => CharacterGender.Feminine;
		public override int StartingHp => 69;
		 public override string CustomVisualPath => "res://TH_Patchouli/ArtWorks/Character/patchouli.tscn";
		 public override string CustomTrailPath => "res://TH_Patchouli/ArtWorks/VFX/PatchouliCardTrail.tscn";
		public override string CustomIconTexturePath => "res://TH_Patchouli/ArtWorks/Character/patchouli_icon.png";
		public override string CustomIconPath => "res://TH_Patchouli/ArtWorks/Character/patchouli_icon.tscn";
		
		public override string CustomEnergyCounterPath => "res://TH_Patchouli/ArtWorks/Character/patchouli_energy_counter.tscn";
		// // 篝火休息动画。
		 public override string CustomRestSiteAnimPath => "res://TH_Patchouli/ArtWorks/Character/patchoulirest.tscn";
		// // 商店人物动画。
		 public override string CustomMerchantAnimPath => "res://TH_Patchouli/ArtWorks/Character/patchouli_merchant.tscn";
		 public override string CustomArmPointingTexturePath => "res://TH_Patchouli/ArtWorks/Character/multiplayer_hand_patchouli_point.png";
		 public override string CustomArmRockTexturePath => "res://TH_Patchouli/ArtWorks/Character/multiplayer_hand_patchouli_rock.png";
		 public override string CustomArmPaperTexturePath => "res://TH_Patchouli/ArtWorks/Character/multiplayer_hand_patchouli_paper.png";
		 public override string CustomArmScissorsTexturePath => "res://TH_Patchouli/ArtWorks/Character/multiplayer_hand_patchouli_scissors.png";
		 public override string CustomCharacterSelectBg => "res://TH_Patchouli/ArtWorks/Character/Patchouli_bg.tscn";
		 public override string CustomCharacterSelectIconPath => "res://TH_Patchouli/ArtWorks/Character/char_select_patchouli.png";
		 public override string CustomCharacterSelectLockedIconPath => "res://TH_Patchouli/ArtWorks/Character/char_select_patchouli_locked.png";
		 public override string CustomCharacterSelectTransitionPath => "res://materials/transitions/necrobinder_transition_mat.tres";
		 public override string CustomMapMarkerPath => "res://TH_Patchouli/ArtWorks/Character/map_marker_patchouli.png";
		// 攻击音效
		public override string CustomAttackSfx => PatchouliInit.ToModSfxPath("TH_Patchouli/ArtWorks/SFX/attack.wav");
		// 施法音效
		public override string CustomCastSfx => PatchouliInit.ToModSfxPath("TH_Patchouli/ArtWorks/SFX/cast.wav");
		// 死亡音效
		public override string CustomDeathSfx => PatchouliInit.ToModSfxPath("TH_Patchouli/ArtWorks/SFX/die.ogg");
		public override string CharacterSelectSfx  => PatchouliInit.ToModSfxPath("TH_Patchouli/ArtWorks/SFX/characterselect.wav");
		public override string CharacterTransitionSfx => PatchouliInit.ToModSfxPath("TH_Patchouli/ArtWorks/SFX/transition.wav");
		public override CardPoolModel CardPool => ModelDb.CardPool<PatchouliCardPool>();
		public override RelicPoolModel RelicPool => ModelDb.RelicPool<PatchouliRelicPool>();
		public override PotionPoolModel PotionPool => ModelDb.PotionPool<PatchouliPotionPool>();

		// 初始卡组
		public override IEnumerable<CardModel> StartingDeck => [
			 ModelDb.Card<StrikeGold>(),
			 ModelDb.Card<StrikeWood>(),
			 ModelDb.Card<StrikeWater>(),
			 ModelDb.Card<StrikeFire>(),
			 ModelDb.Card<StrikeDirt>(),
			 ModelDb.Card<DefendSun>(),
			 ModelDb.Card<DefendSun>(),
			 ModelDb.Card<DefendLunar>(),
			 ModelDb.Card<DefendLunar>(),
			 ModelDb.Card<ElementRefinement>()
		
	];

		// 初始遗物
		public override IReadOnlyList<RelicModel> StartingRelics => [
			ModelDb.Relic<UnstablePhilosophersStone>()
	];

		// 攻击建筑师的攻击特效列表
		public override List<string> GetArchitectAttackVfx() => [
		"vfx/vfx_attack_blunt",
		"vfx/vfx_heavy_blunt",
		"vfx/vfx_attack_slash",
		"vfx/vfx_bloody_impact",
        "vfx/vfx_rock_shatter"
		];
		public override CreatureAnimator GenerateAnimator(MegaSprite controller)
		{
			AnimState animState = new AnimState("idle", isLooping: true);
			AnimState animState2 = new AnimState("cast");
			AnimState animState3 = new AnimState("attack");
			AnimState animState4 = new AnimState("hit");
			AnimState animState6 = new AnimState("spell");
			AnimState state = new AnimState("die");
			AnimState animState5 = new AnimState("relaxed_loop", isLooping: true);
			animState2.NextState = animState;
			animState3.NextState = animState;
			animState4.NextState = animState;
			animState6.NextState = animState;
			animState5.AddBranch("Idle", animState);
			CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
			creatureAnimator.AddAnyState("Idle", animState);
			creatureAnimator.AddAnyState("Dead", state);
			creatureAnimator.AddAnyState("Hit", animState4);
			creatureAnimator.AddAnyState("Attack", animState3);
			creatureAnimator.AddAnyState("Cast", animState2);
			creatureAnimator.AddAnyState("Spell", animState6);
			creatureAnimator.AddAnyState("relaxed_loop", animState5);		
			return creatureAnimator;
		}
		
	}
}
/*
帕秋莉：
过劳 本回合内每当你打出卡牌时，失去1点能量，你不能再抽牌。
疲劳 下回合少获得1点能量并少抽1张牌。
初始带一张诅咒：体弱魔女 -1c不能被打出。永恒。无法逃脱(被变化或消耗后将一张复制品放回手中)。
无论这张牌在何处，你在一回合内打出6张牌后，获得1层疲劳，打出12张牌后，获得1层过劳。

三重伟大者 1c金能力 
(+固有)
每当你通过元素强化/变化卡牌时，将其升级。


火水符「Phlogistic Pillar」（燃素之柱）
1c先古攻击
给予5→7层点燃。
给予5→7层冻结。

日水符「Hydrogenous Prominence」（氢化日珥）
1c先古攻击
移除所有敌人的冻结，并造成冻结层数2->3倍的伤害。
你每有1点水元素，额外造成1次伤害。

土水符「Noachian Deluge」（诺亚的大洪水）
1c先古技能
丢弃所有手牌，每丢弃一张牌就给予所有敌人3→4层冻结。

水月符「镜花水月」
2→1c先古技能
本回合内打出卡牌时，将一张其带有虚无的复制品放入你的手中。
消耗。

火木符「炭辙蔓生」
2→1c先古技能
将所有敌人的生命回满，然后给予等同于回复量层点燃。
消耗。

土火符「幻重火场」
2c先古技能
给予所有敌人1层点燃6→8次。

日火符「烈日凌空」
1c先古
获得所有敌人点燃层数之和/5→3层力量。
每当这张牌在你手中保留时，给予所有敌人8→10层点燃。

火月符「望月焚诏」
1c先古技能
消耗一张牌。
如果敌人有点燃印记，额外给予4→7层点燃。
给予4→7层点燃印记。

土日符「地幔喉舌」
1c先古技能
从你的抽牌堆和弃牌堆中各选择1→2张牌放入手中。

土木符「欣欣向荣」
1c 先古将你
移除你所有的负面效果，每移除一种，就获得5->8点格挡。
消耗。

月土符「月壤」
1c先古能力
虚无（-）
被保留的卡牌在本场战斗中的耗能减少1点。
月+土。

火水木金土符「贤者之石」
2->1c 先古技能
本回合内打出卡牌时，额外触发自身所有元素的效果1次。
消耗。
火+水+木+金+土。

日土符「塑方环刃」
1c先古攻击
随机造成8->12点伤害N次。每次造成伤害时将伤害减半，重复造成直到伤害小于1点。
日+土。

月火符「胧月流火」
1c先古技能
虚无
敌人每有一种负面效果，就给予5→7层点燃。

火日符「天将启明」
1c先古能力
在你的回合开始时，触发1→2次所有敌人的点燃效果。
火+日

水木符「Water Elf」（水精灵）
1c先古技能
将2→3张水之精放入手中。
每次打出这张牌时，将这张牌的数值在本场战斗中提高1点。
水+木。

木金符「金缕银枝」
1c先古能力
每当你造成伤害时，获得2→3金币。
木+金

金日符「鎏金蚀日」
2→1c金攻击
造成6+你金币总数30%点伤害。
失去6金币。
金+日

木土符「地脉天顶」
1c先古攻击
对所有敌人造成9→10点伤害。
所有敌人失去2→3层力量。
木+土

木日符「苏生敕令」
2c先古技能
回复等同于最大生命12→20%的生命。
超出部分的回复将转化为最大生命。
消耗。
木+日。

木月符「低语之根」
1c先古技能
抽2→3张牌，将被抽到的卡牌在本回合内首次打出前的耗能置为0点。
木+月。

水日符「新星预言者」
1c先古技能
抽2→3张牌。
下个回合额外抽2→3张牌。
水+日。

水土符「沉疴泥沼」
2c先古技能
给予所有敌人2→3层缓慢，虚弱和易伤。
所有敌人在2→3回合内易伤和虚弱效率翻倍。
消耗。

水金符「密室之钫」
1c先古攻击
造成9→10点伤害。
敌人每有一种（+层）负面效果，就额外造成4点伤害。

你的元素不会在回合开始时减少这个效果放到遗物里面吧。
获得7->10点格挡。
本回合内受到的伤害减少你拥有的土元素层数点。
少女密室 这个弄个事件
给药水，或者卡牌，遗物好了。
每当你消耗能量时
你的元素的效果增加1点？打出卡牌时额外获得元素？
*/
/* 小恶魔
搜寻打击
1c 白攻击
预见3(4)。
造成7(9)点伤害。
抽1张牌。

速览定义 1c 蓝技能
预见7→10。
抽2张牌。
*/
