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


炼金术
占星学

星之魔女

神秘学

三重伟大者 先古能力 2->1c
固有
在本场战斗中[gold]升级[/gold]你的所有卡牌。\n在你的回合开始时，额外获得1点能量，额外抽1张牌。

嬗变

元素提纯 1(0)c初始技能
消耗一张牌，获得被消耗的卡牌耗能3倍的对应元素，如果其没有对应元素则随机获得等量元素。

至高炼金 1c 先古技能
消耗任意张牌，然后将等量元素炼成(+)置入手中。
保留。
消耗。

元素练成。0c无色技能。
获得3->5点指定元素。
抽1张牌。

元素赋予 1c白技能
获得5点格挡。
选择一张手牌添加元素，如果其有元素，获得1点其相同元素。
（+为所有手牌添加元素，如果其有元素，获得1点其相同元素。）

书籍装帧 1c白技能
获得7(10)点格挡。
选择一张手牌，为其添加保留。

夏日之红(Summer Red)0c蓝攻击
造成2->3点伤害。
给予2->3层点燃。
每当你打出拥有火元素的卡牌时，将这张牌放入你的手中，同名卡牌除外。
火

春日之风(Spring Wind) 0c蓝技能
给予所有敌人1层虚弱。
获得5->7层再生。
消耗。
木

秋之边缘(Autumn Edge) 1c白攻击
对所有敌人造成13->17点伤害。
结束你的回合。
金

土用之矛(Doyou Spear) 1c白攻击
造成5->8点伤害。
获得造成的未被格挡的伤害点格挡。
土

冬之元素(Winter Element) 2c白技能
获得8->10点格挡。
给予4->5层冻结。
水

夏炎(Summer Flame)1c 白技能
给予4->6层点燃。
如果敌人有点燃，再给予4->6层点燃。
火

高压水泡(Condensed Bubble) 2c 白技能
获得15->20层水泡护盾。(被攻击时减少伤害层数并减少等量伤害(即使被格挡)，被移除时对所有敌人造成15->20点伤害。)
水

粘性水泡(Sticky Bubbles) 1c蓝技能
给予所有敌人3层粘性水泡。
（每当敌人攻击时或其回合结束时，受到10点伤害并减少1层。）
水

春斩(Flash Of Spring)1c白攻击
对所有敌人造成7->8点伤害。
抽1->2张牌。
木

秋刃(Autumn Blades)1c白攻击
造成1点伤害5->7次。
这张牌造成的伤害受金元素的加成。
金

Every-Angle Shot（全角度射击）1c蓝攻击
对所有敌人造成5->7点伤害。
你每有一种元素，就额外造成一次伤害。

「Five Season」（五个季节）1c蓝技能
你每有一种元素，就抽1张牌并获得1点能量。
消耗(-)。

悬剑(Fall Slasher)1c蓝技能
在本回合内，你每打出一张牌，就对随机敌人造成7->9点伤害。
金

静态之绿(Static Green) 1c蓝能力
在你的回合结束时，如果你在本回合内打出的牌不超过5张，获得1->2点敏捷。
木

日符「Royal Flare」（皇家圣焰）2c金技能
消耗你的所有状态和诅咒牌。
每消耗一张牌，就给予所有敌人11->13层点燃。
消耗。

火符「Ring of Agni」（火天神印）1c金技能
给予3层点燃印记。
给予6->9层点燃。

翡翠之城(Emerald City) 2费蓝技能
获得13->18点格挡。
在本回合被攻击时，对所有敌人造成等量伤害。
保留。
土

钻石之坚(Diamond Hardness) 1c蓝能力
(+获得3层覆甲。）
在你的回合开始时，获得3层覆甲。
土

灼燃之雾(Wipe Moisture) 2->1c蓝能力
在你的回合开始时，给予所有敌人5层点燃1次。
火

土符「Trilithon Shake」（三石塔的震动）1c蓝攻击
对所有敌人造成等同于你当前格挡值2->3倍的伤害。
移除你的所有格挡。
土

木符「Green Storm」（翠绿风暴）Xc蓝攻击
对所有敌人造成5点伤害X(+2)次。
获得X(+2)层[gold]再生[gold]。
木

月符「Silent Selene」（沉默的月神）0c金技能
抽1->2张牌。
获得2点能量。
如果本回合内你没有打出过攻击牌，这张牌的效果翻倍。
消耗。
月

金符「Metal Fatigue」（金属疲劳）1c蓝技能
获得等同于你金元素层数的力量。
获得2->1层虚弱。
金

金符「Silver Dragon」（银龙）2c白攻击
造成14点伤害。
力量在这张牌上发挥3->5倍作用。
金

火符「Akiba Summer」（秋叶之夏）1c白攻击
造成6->8点伤害。
给予3->4层点燃。
火

水符「Jellyfish Princess」（水母公主）1c蓝能力
每当造成以你为来源的伤害时，给予1->2层中毒。
水

符之二「Deluge Forty Day」（大洪水四十日）2c金能力
在你的回合开始时，给予所有敌人6->8层冻结。
水。

水符「Princess Undine」（水精公主）1c蓝能力
+固有
在你的回合开始时，将一张水之精放入手中。
水

水之精0c状态
造成5->8点伤害。
被消耗时获得1->2点能量。
保留。
水。

火符「Agni Radiance」（火神的光辉）2c蓝攻击
对所有敌人造成10->13点伤害。
这张牌造成的伤害受所有敌人点燃层数之和的加成。
火

火符「Agni Shine」（火神之光）1c蓝技能
给予所有敌人4->7层点燃。
升级你的所有手牌。
火

水符「Bury in Lake」（湖葬）1c蓝攻击
造成10->15点伤害。
消耗一张牌，如果被消耗的牌拥有水元素/火元素，获得1点能量/再造成一次伤害。
水

木符「Sylphy Horn」（风灵的角笛）1c蓝技能
抽2->3张牌。
每抽到一张技能牌，就再抽2->3张牌。
木

土符「Lazy Trilithon」（慵懒三石塔）2c蓝技能
给予所有敌人1层缓慢。
将你的格挡翻2->3倍。
土

符之一「St. Elmo Explosion」（圣爱尔摩爆炸）3c金攻击
对所有敌人造成20->22点伤害。
你每有一张拥有火元素的牌，就额外造成5->8点伤害。不论其在何处。
火。

书页风暴 1c蓝能力
每当有卡牌改变所处的牌堆时，对所有敌人造成1(2)点伤害。

魔力储备 1c白技能
获得2->3点能量。
每次打出后，本回合内少获得1点能量。

广霍香
1c蓝技能
你的抽牌堆中每有3→2张牌，获得1点能量。
本回合内你不能再获得能量。

书卷打击
2→1c蓝攻击
造成等同于你卡组中牌数的伤害。

知识壁垒 2→1c金能力
在你的回合结束时，每有一张牌在手中，直到你的下个回合开始前，受到的伤害就减少1点。

书山 1c蓝攻击
造成8→10点伤害。
你的手中每有一张牌，造成的伤害增加3→4点。

书咬 2c白攻击
造成7→10点伤害。
这张牌每次被保留时伤害增加7→10点。
敌人失去造成的未被格挡的伤害点最大生命。
保留。

不动的大图书馆  1c蓝能力
每当你保留卡牌时，获得4→6点格挡。

休整2c蓝技能
获得10点格挡。
本回合内保留你的手牌。
下个回合，抽2张牌并获得2点能量。
消耗（-）。

渊博学识 1→0c蓝能力
获得1层力量和敏捷。
你每有一种正面效果，额外获得1层。

花昙的魔女 2→1c金能力
每当你被攻击时，如果你的抽牌堆不为空，丢弃抽牌堆顶部的卡牌并免受这次伤害。

从容不迫1c金能力
+固有
每当你的抽牌堆为空时，抽2->3张牌并获得2->3点能量。

滴水不漏 1c蓝攻击
造成9→12点伤害。
本回合保留至多你造成的未被格挡的伤害/3张牌。

蓄势待发 1->0c蓝技能
你的下一张攻击牌造成的伤害翻3倍。
每当这张牌被保留时，获得5->8层活力。

知识攫取 1c金攻击
造成6→9点伤害。
敌人失去2→3点力量和敏捷。
你获得2→3点力量和敏捷。

学无止境 2c金技能
为你的所有手牌添加重放1→2。
消耗。

硬撑 1c 蓝技能
获得17→22点格挡。
将2张体力透支放入你的手中。

体力透支 1c状态
每当你抽到这张牌时，获得一层虚弱和脆弱。
消耗。

文件备份 1c蓝技能
选择一张手牌，将其的2→3张复制品放入你的抽牌堆中。

图书管理员 1c蓝能力
+固有
在你的回合开始时，选择至多1张手牌获得保留。





获得8->10点格挡。
1->2回合内你的元素不会因回合结束而减少。

获得7->10点格挡。
本回合内受到的伤害减少你拥有的土元素层数点。

从你的弃牌堆中选择一张牌放回手中。
将这张牌在本回合内首次打出前的耗能置为0点

现在你的卡牌进入弃牌堆或消耗牌堆时，将其放回抽牌堆中。

每当你变化卡牌时，将其升级。

每当你变化卡牌时，将变化后的卡牌在本回合内首次打出前的耗能置为0点。

在你的回合结束时，保留至多1→2张牌。

获得2→3点能量。

目录索引

抽一张牌。
从你的弃牌堆中选择一张牌放回手中。

保留卡牌。
保留至多张牌。
0c
获得ee。
消耗。

使役恶魔 1c蓝能力
+固有
在你的回合开始时，失去1点生命，获得1点能量。

魔女聚会
不明的魔法之元
知识与避世的少女
红馆的魔女
红魔馆之头脑
元素周期表

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
