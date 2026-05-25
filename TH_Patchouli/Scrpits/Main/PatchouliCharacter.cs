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

冻结 造成的伤害减少层数点，在你的回合结束时减少1点。如果在你的回合开始时冻结层数不小于你当前生命，你将被击晕。

不得不都会自然减少，因为这个元素这么个获得法，等于不消耗，变牌等于存起来，多打总会多。
金 在你的回合开始时，本回合内获得层数的临时力量。
木 在你的回合结束时，恢复层数点生命。
水 在你的回合开始时，给予所有敌人层数层冻结。
火 每当你给予点燃时，额外给予层数层。
土 在你的回合结束时，获得层数点格挡。
日 在你的回合开始时，每有2层日就额外获得1点能量。
月 在你的回合开始时，每有2层月就额外抽1张牌。






三重伟大者 金能力 2->1c




嬗变 1c蓝能力
（+固有）
在你的回合开始时，消耗你的所有元素，然后随机获得等量元素。

一周少女 1c蓝能力
在你的回合开始时，按顺序获得1→2点指定元素。
（从周日开始计，星期天、星期一、星期二、星期三、星期四、星期五和星期六分别是日曜日、月曜日、火曜日、水曜日、木曜日、金曜日和土曜日）

元素周期律2→1c金能力
每当你的元素层数减少时，按顺序获得1点对应元素。（金→木→水→火→土→日→月→金）

变幻莫测
2→1c金能力
每当你变化卡牌时，将变化后的卡牌在本回合内首次打出前的耗能置为0点。

贫弱魔女 1→0c蓝能力
固有
获得1层虚弱和脆弱。
在你的回合开始时，获得1点能量。

浩瀚书海 1c蓝技能
从你的弃牌堆中选择2->3张牌放回手中。
如果你的抽牌堆中的牌数多于弃牌堆中的牌数，则将这些牌在本回合内首次打出的耗能置为0点。

元素操纵2c蓝能力
每当你消耗或获得元素时，对所有敌人造成4→6点伤害。

幻想练成 2→1c金技能
随机获得1件遗物。
每次打出时，将这张牌在本场战斗中的耗能翻倍。

元素占卜1c白技能
下个回合额外获得2→3点能量并获得2→3点随机元素。

二重施术 1c金技能
本回合内打出卡牌时额外打出1次。
消耗(-)。

七曜的魔女7c金能力
(+固有)
在你的回合开始时，每有一种元素就抽1张牌并获得1点能量。
每当你获得元素时，将这张牌的耗能减少1点。
保留。

元素魔法使 2→1c金能力
带有元素的卡牌的耗能将减少你拥有的对应元素点。

书虫魔女 0c金技能
消耗一张牌。
获得1→2层力量和敏捷。
抽1张牌。

万物流转1→0c蓝能力
每当你变化卡牌时，抽2张牌。

载体创造 1c蓝技能
选择至多2→3张手牌变化为基底。

基底 -1c状态 
不能被打出。
保留。
金+木+水+火+土+日+月

元素飞弹1c金攻击
造成7→10点伤害。
你每有一种元素，这张牌就额外造成一次伤害。

剿灭老鼠 2c金技能
造成14点伤害。
所有敌人在本回合内受到的伤害翻2→3倍。

使役恶魔 1->0c白技能
选择一张手牌，将其打出并消耗。
保留。

碧绿闪光 1c白攻击
随机造成4->5点伤害3次。
木。


红馆之魔女 1c金攻击
造成7点伤害。
回复3点生命。

获得7->10点格挡。
本回合内受到的伤害减少你拥有的土元素层数点。

你的元素不会在回合开始时减少。

你每有一种元素，就将这张牌的耗能减少1点。

知识与避世的少女 金能力

少女密室 金技能
获得格挡。

不明的魔法之元 金能力

红魔馆的头脑 蓝能力

无尽炼成

双七组合 1→0c事件
造成7点伤害。
获得7点格挡。
如果这张牌上的数值为7的倍数，将效果翻倍。


魔女聚会

技能的百货中心 1→0c金技能
其他玩家失去50金币，并从这张牌之外的所有（+升级过的）技能牌中选择一张放入手中，然后你获得等量的金币。
消耗。

睡衣派对 1c蓝技能
所有玩家获得5点格挡。
每有一名存活的玩家，额外获得一次。



火土符「Lava Cromlech」（熔岩环石阵）
火水符「Phlogistic Pillar」（燃素之柱）
水火符「Phlogistic Rain」（燃素之雨）
水木符「Water Elf」（水精灵）
金土符「Emerald Megalith」（翡翠巨石）
木火符「Forest Blaze」（森林大火）
火土符「Lava Cromlech」（环状熔岩带）
金土符「Ginger Gust」（淡黄色的阵风）
日水符「Hydrogenous Prominence」（氢化日珥）
火金符「St. Elmo Pillar」（圣爱尔摩火柱）
土金符「Emerald Megalopolis」（翡翠巨城）
金水符「Mercury Poison」（水银之毒）
日月符「Royal Diamond Ring」（皇家钻戒）
日木符「Photosynthesis」（光合作用）
月木符「Satellite Himawari」（卫星向日葵）
金木符「Elemental Harvester」（元素收获者）
土水符「Noachian Deluge」（诺亚的大洪水）
月金符「Sunshine Reflector」（日光反射器）
木金符「金缕银枝」
水金符「密室之钫」
金火符「赫菲斯托斯之锤」
在本场战斗中升级你的所有卡牌。
金日符「鎏金蚀日」
金月符「残月收割」
木水符「根源活化」
木土符「地脉天顶」
木日符「苏生敕令」
木月符「低语之根」
水土符「沉疴泥沼」
水日符「新星预言者」
水月符「凝冰玉」
火木符「炭辙蔓生」
火水符「焚风」
土火符「幻重火场」
火日符「羽化黎明」
火月符「望月焚诏」消耗
土木符「打灰」
土日符「地幔喉舌」
土月符「宿月魔典」
日金符「曜冠悬剑」
日火符「琉璃日径」
月日符「黯月晨光」
日土符「塑方环刃」
月火符「胧月流火」
月土符「明灯尘世」
月水符「引力潮汐」

火水木金土符「贤者之石」

新创符卡太复杂，去掉，改成也能右键，不过额外支付元素就行。
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
