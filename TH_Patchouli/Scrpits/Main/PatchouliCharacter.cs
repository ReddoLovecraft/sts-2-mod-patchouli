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
帕秋莉事件：
三重伟大者
阅读玉石板 获得《翠玉录》。
镶嵌权杖 失去贤者之石。获得贤者之杖。（锁定：需要[red]贤者之石[/red]）
搜罗材料 获得艾哲红石。

偶遇爱丽丝
结伴同行。获得双七组合。
讨论魔法。升级2张牌。

命运的邂逅（偶遇蕾米）
操纵命运。失去4+选择次数点生命。变化一张牌。
带走石头。获得命运之石。

全知头骨
知识？失去5+选择该选项次数点生命。从三张随机无色牌中选择一张获得。
财富？失去5+选择该选项次数点生命。获得100金币。
健康？失去5+选择该选项次数点生命。获得5点最大生命。
我该如何离开。失去5生命。

被诅咒的书籍
阅读。移除你的所有卡牌，从每个角色的牌库中依次选择一张牌加入你的牌组。将一张狂乱加入你的牌组。
无视。

少女密室 
无意中走到了一处密室中。
搜寻四周。获得一件随机稀有遗物。
仔细思考。升级3张牌。
睡个觉吧。回复全部生命。


*/
