#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Effect.Area;
using Domain.Model.Effect.Position;
using Domain.Model.Evaluation;
using Domain.Model.Item;
using Domain.Model.Memento;
using Domain.Service.Effect;

namespace Domain.Service.Items
{
    /// <summary>
    /// アイテム説明の要約表示と、エディタ向けテンプレート整合性検証。
    /// </summary>
    public static class ItemDescriptionTemplate
    {
        public static IReadOnlyList<string> ValidateDirectWeapon(DirectWeaponData data)
        {
            var errors = new List<string>();
            try
            {
                var memento = DirectWeapon.Build(data);
                errors.AddRange(ValidateDirectWeaponMemento(memento));
            }
            catch (Exception ex)
            {
                errors.Add($"近接武器データの生成に失敗しました: {ex.Message}");
            }

            return errors;
        }

        public static IReadOnlyList<string> ValidateRangedWeapon(RangedWeaponData data)
        {
            var errors = new List<string>();
            try
            {
                var memento = RangedWeapon.Build(data);
                errors.AddRange(ValidateRangedWeaponMemento(memento));
            }
            catch (Exception ex)
            {
                errors.Add($"射撃武器データの生成に失敗しました: {ex.Message}");
            }

            return errors;
        }

        public static IReadOnlyList<string> ValidatePotionItemData(ItemData data)
        {
            var errors = new List<string>();
            if (data.Category != ItemCategory.Potions)
                return errors;

            if (data.EffectType != ItemEffectType.SpawnEffect)
            {
                errors.Add("ポーションテンプレート: 効果タイプは「SpawnEffect」である必要があります。");
                return errors;
            }

            if (!data.SpawnEffectsOnUse)
                errors.Add("ポーションテンプレート: 「使用時にSpawnEffect」をオンにしてください。");

            if (!data.SpawnEffectsOnThrow)
                errors.Add("ポーションテンプレート: 「投擲時にSpawnEffect」をオンにしてください。");

            if (data.SkillOnUse == null)
            {
                errors.Add("ポーションテンプレート: 使用時スキルが設定されていません。");
                return errors;
            }

            if (data.SkillOnUse.Position is not AtFeet)
                errors.Add("ポーションテンプレート: 使用時の発動位置は「発動場所」(AtFeet)である必要があります。");

            if (data.SkillOnUse.Area is not SelfArea)
                errors.Add("ポーションテンプレート: 使用時の範囲は「その場」(SelfArea)である必要があります。");

            return errors;
        }

        internal static string FormatDirectWeapon(SkillWithCost skillOnUse, SkillWithCost skillOnThrow,
            bool hasSameEffect)
        {
            var useSpawn = (SpawnEffectSkill)skillOnUse.Skill;
            var throwSpawn = (SpawnEffectSkill)skillOnThrow.Skill;

            var sb = new StringBuilder();
            sb.Append('\n');
            sb.AppendLine(ItemDescriptionRichText.HeaderLine("使用したときの効果..."));
            AppendCompactSkillBody(sb, skillOnUse, useSpawn, "使用");
            sb.Append('\n');
            sb.AppendLine(ItemDescriptionRichText.HeaderLine("投擲したときの効果..."));
            AppendCompactSkillBody(sb, skillOnThrow, throwSpawn, "投擲");

            return sb.ToString();
        }

        internal static string FormatRangedWeapon(SkillWithCost skillOnUse)
        {
            var spawn = (SpawnEffectSkill)skillOnUse.Skill;

            var sb = new StringBuilder();
            sb.Append('\n');
            sb.AppendLine(ItemDescriptionRichText.HeaderLine("使用したときの効果..."));
            sb.AppendLine(BuildRangedLaunchLine(spawn.EffectPosition));
            AppendCompactSkillBody(sb, skillOnUse, spawn, "使用");
            return sb.ToString();
        }

        internal static string FormatPotion(SkillWithCost? skillOnUse, SkillWithCost? skillOnThrow,
            bool hasSameEffect, bool hasSameSkill)
        {
            if (skillOnUse == null)
                return "";
            var useSpawn = (SpawnEffectSkill)skillOnUse.Skill;

            var sb = new StringBuilder();
            if (hasSameSkill)
            {
                sb.Append('\n');
                sb.AppendLine(ItemDescriptionRichText.HeaderLine("使用または投擲したときの効果..."));
                AppendCompactSkillBody(sb, skillOnUse, useSpawn, "使用", includeSuccessRate: false);
                if (skillOnThrow != null && skillOnThrow.Skill is SpawnEffectSkill throwSpawnOnSameSkill)
                    sb.AppendLine(ItemDescriptionRichText.ColorPercentagesInPlainText(
                        $"成功率：使用{useSpawn.ProbabilityOfSuccess:P0}／投擲{throwSpawnOnSameSkill.ProbabilityOfSuccess:P0}"));
                else
                    sb.AppendLine(ItemDescriptionRichText.ColorPercentagesInPlainText(
                        $"成功率：使用{useSpawn.ProbabilityOfSuccess:P0}"));
                return sb.ToString();
            }

            sb.Append('\n');
            sb.AppendLine(ItemDescriptionRichText.HeaderLine("使用したときの効果..."));
            AppendCompactSkillBody(sb, skillOnUse, useSpawn, "使用");

            if (skillOnThrow != null && skillOnThrow.Skill is SpawnEffectSkill throwSpawn)
            {
                sb.Append('\n');
                sb.AppendLine(ItemDescriptionRichText.HeaderLine("投擲したときの効果..."));
                AppendCompactSkillBody(sb, skillOnThrow, throwSpawn, "投擲");
            }

            return sb.ToString();
        }

        private static void AppendCompactSkillBody(StringBuilder sb, SkillWithCost skillCost, SpawnEffectSkill spawn,
            string context, bool includeSuccessRate = true)
        {
            if (skillCost.Cost > 0)
                sb.AppendLine($"消費HP: {ItemDescriptionRichText.RichHpCost(skillCost.Cost)}");
            if (spawn.RushDistance > 0)
                sb.AppendLine($"攻撃前に{ItemDescriptionRichText.RichSpatialCells(spawn.RushDistance)}前進する");

            var main = BuildMainCombatLine(spawn.EffectPosition, spawn.EffectArea, spawn.EffectList, context);
            if (!string.IsNullOrEmpty(main))
                sb.AppendLine(main);

            var primaryAttack = spawn.EffectList.OfType<AttackEffect>().FirstOrDefault();
            if (primaryAttack != null)
            {
                foreach (var line in SplitLines(primaryAttack.Description()).Skip(1))
                    sb.AppendLine(line);
            }

            foreach (var line in FormatExtraEffectLines(spawn.EffectList))
                sb.AppendLine(line);

            if (spawn.Repeats > 1)
                sb.AppendLine($"効果は{ItemDescriptionRichText.RichMeta(spawn.Repeats)}回発動する");

            if (includeSuccessRate)
                sb.AppendLine(ItemDescriptionRichText.ColorPercentagesInPlainText($"成功率：{spawn.ProbabilityOfSuccess:P0}"));

            if (spawn.BackStepDistance > 0)
                sb.AppendLine($"攻撃後に{ItemDescriptionRichText.RichSpatialCells(spawn.BackStepDistance)}後退する");
            if (skillCost.ChargeTurn > 0)
                sb.AppendLine($"発動には{ItemDescriptionRichText.RichTurns(skillCost.ChargeTurn + 1)}ターンかかる");
            if (skillCost.CoolTime > 0)
                sb.AppendLine($"発動後に{ItemDescriptionRichText.RichTurns(skillCost.CoolTime)}ターンは再使用不能");
        }

        private static string BuildMainCombatLine(IEffectPosition position, IArea area, IReadOnlyList<IEffect> effects,
            string context)
        {
            var attack = effects.OfType<AttackEffect>().FirstOrDefault();
            if (attack != null)
                return BuildAttackLine(position, area, FirstLine(attack.Description()), context);
            var absorb = effects.OfType<AbsorbsEffect>().FirstOrDefault();
            if (absorb != null)
                return BuildAttackLine(position, area, FirstLine(absorb.Description()), context);
            return "";
        }

        private static string BuildAttackLine(IEffectPosition position, IArea area, string attackInfoLine, string context)
        {
            var target = ResolveAttackTargetText(position, area, context);
            var plain = ItemDescriptionRichText.StripColorTags(attackInfoLine);
            var compact = AttackInfoToCompactPower(plain);
            var powerRich = ItemDescriptionRichText.RichAttackPowerSummary(compact);
            return $"{target}に攻撃［{powerRich}］";
        }

        private static string ResolveAttackTargetText(IEffectPosition position, IArea area, string context)
        {
            if (position is NearByCharacter)
                return "近くの敵";

            if (position is ProjectileImpact projectile)
            {
                if (projectile.IsPiercing)
                {
                    if (area is CircleArea circleOnPierce)
                        return $"射線上の各対象とその周囲{ItemDescriptionRichText.RichSpatialCells(circleOnPierce.Radius)}";
                    return "射線上の対象すべて";
                }

                if (area is CircleArea circle)
                    return $"命中地点とその周囲{ItemDescriptionRichText.RichSpatialCells(circle.Radius)}";
                return "命中地点";
            }

            if (context == "投擲")
                return "投擲先";
            return AreaToTargetText(area);
        }

        private static string BuildRangedLaunchLine(IEffectPosition position)
        {
            return position switch
            {
                ProjectileImpact projectile => projectile.IsPiercing
                    ? $"射程{ItemDescriptionRichText.RichSpatialCells(CommonSenseParameters.ThrowDistance)}の貫通攻撃を放つ"
                    : $"射程{ItemDescriptionRichText.RichSpatialCells(CommonSenseParameters.ThrowDistance)}の攻撃を放つ",
                NearByCharacter => "曲射で近くの敵を狙う",
                _ => "射撃攻撃を放つ"
            };
        }

        private static string AreaToTargetText(IArea area)
        {
            return area switch
            {
                LineArea line => $"前方{ItemDescriptionRichText.RichSpatialCells(line.Length)}",
                FanArea fan => $"前{ItemDescriptionRichText.RichSpatialCells(fan.Radius)}（扇形）",
                CircleArea circle => $"周囲{ItemDescriptionRichText.RichSpatialCells(circle.Radius)}",
                SelfArea => "その場",
                _ => area.Description()
            };
        }

        private static string AttackInfoToCompactPower(string attackInfoLine)
        {
            var text = attackInfoLine;
            if (text.StartsWith("攻撃[") && text.EndsWith("]"))
            {
                text = text.Substring(3, text.Length - 4);
            }
            text = text.Replace("の攻撃を行う", "");
            text = text.Replace("属性、威力", "");
            text = text.Replace(" ", "/");
            return text.Trim();
        }

        private static IEnumerable<string> FormatExtraEffectLines(IReadOnlyList<IEffect> effects)
        {
            foreach (var e in effects)
            {
                if (e is AttackEffect)
                    continue;
                var lines = SplitLines(e.Description()).ToList();
                if (e is AbsorbsEffect)
                {
                    foreach (var extra in lines.Skip(1))
                        yield return extra;
                    continue;
                }

                foreach (var line in lines)
                {
                    yield return line.Contains("<color") ? line : CompactPhrase(line);
                }
            }
        }

        private static string FirstLine(string multiLine)
        {
            return SplitLines(multiLine).FirstOrDefault() ?? "";
        }

        private static IEnumerable<string> SplitLines(string multiLine)
        {
            return multiLine
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s));
        }

        private static string CompactPhrase(string line)
        {
            if (line.Contains("の確率で") && line.Contains("状態を付与"))
            {
                var probability = line.Split("の確率で")[0].Trim();
                var condition = line
                    .Split("の確率で")[1]
                    .Replace("状態を付与する", "")
                    .Replace("状態を付与", "")
                    .Trim();
                return $"[{condition}]を付与（{probability}）";
            }

            if (line.StartsWith("威力") && line.Contains("の回復"))
            {
                var hp = line
                    .Replace("威力", "")
                    .Replace("の回復を行う", "")
                    .Replace("の回復", "")
                    .Trim();
                return $"{hp}回復";
            }

            return line
                .Replace("最初に", "攻撃前に")
                .Replace("最後に", "攻撃後に")
                .Replace("マス前に進む", "マス前進する")
                .Replace("マス後ろに下がる", "マス後退する")
                .Replace("を行う", "")
                .Replace("する", "");
        }

        private static bool IsHarmfulCombatEffect(IEffect effect) =>
            effect is AttackEffect or AbsorbsEffect or PercentageDamageEffect;

        private static bool HasAttackLike(IReadOnlyList<IEffect> effects) =>
            effects.Any(e => e is AttackEffect or AbsorbsEffect);

        private static bool IsDirectWeaponArea(IArea area) =>
            area is LineArea or FanArea or CircleArea;

        private static IReadOnlyList<string> ValidateDirectWeaponMemento(DirectWeaponMemento memento)
        {
            var errors = new List<string>();
            if (!TryGetSpawn(memento.SkillOnUse, out var use))
            {
                errors.Add("近接武器テンプレート: 使用時スキルがSpawnEffectSkillではありません。");
                return errors;
            }

            if (use.Position is not AtFeet)
                errors.Add("近接武器テンプレート: 使用時の発動位置は「発動場所」(AtFeet)である必要があります。");

            if (!IsDirectWeaponArea(use.Area))
                errors.Add("近接武器テンプレート: 使用時の範囲は直線・扇・周囲のいずれかである必要があります。");

            if (!HasAttackLike(use.Effects))
                errors.Add("近接武器テンプレート: 使用時に攻撃または吸血効果を含める必要があります。");

            if (!TryGetSpawn(memento.SkillOnThrow, out var thr))
            {
                errors.Add("近接武器テンプレート: 投擲時スキルがSpawnEffectSkillではありません。");
                return errors;
            }

            if (thr.Position is not AtFeet)
                errors.Add("近接武器テンプレート: 投擲時の発動位置はAtFeetである必要があります。");

            if (thr.Area is not SelfArea)
                errors.Add("近接武器テンプレート: 投擲時の範囲は「その場」(SelfArea)である必要があります。");

            if (!HasAttackLike(thr.Effects))
                errors.Add("近接武器テンプレート: 投擲時にも攻撃または吸血効果が必要です。");

            return errors;
        }

        private static IReadOnlyList<string> ValidateRangedWeaponMemento(RangedWeaponMemento memento)
        {
            var errors = new List<string>();
            if (!TryGetSpawn(memento.SkillOnUse, out var use))
            {
                errors.Add("射撃武器テンプレート: 使用時スキルがSpawnEffectSkillではありません。");
                return errors;
            }

            if (use.Position is not ProjectileImpact && use.Position is not NearByCharacter)
                errors.Add("射撃武器テンプレート: 使用時の発動位置は弾道（ProjectileImpact）またはNearByCharacterである必要があります。");

            if (use.Area is not SelfArea && use.Area is not CircleArea)
                errors.Add("射撃武器テンプレート: 使用時の範囲は「その場」または円範囲である必要があります。");

            if (!HasAttackLike(use.Effects))
                errors.Add("射撃武器テンプレート: 攻撃または吸血効果を含める必要があります。");

            return errors;
        }

        private static bool TryGetSpawn(SkillWithCostMemento skill, out SpawnEffectSkillMemento spawn)
        {
            if (skill.Skill is SpawnEffectSkillMemento s)
            {
                spawn = s;
                return true;
            }

            spawn = null!;
            return false;
        }
    }
}
