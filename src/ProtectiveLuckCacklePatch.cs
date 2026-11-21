using System;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;

namespace smallFixAndEnhances
{
    // Protective Luck normally lasts 1 round, +1 at class level 8 and 16.
    // Cackle and Chant add a round each use, so the same buff can be stacked
    // for minutes before a fight. Sharing it from the action itself keeps the
    // 30-foot placement requirement and spends the action, without banking rounds.
    [HarmonyPatch(typeof(AbilityEffectRunAction), nameof(AbilityEffectRunAction.Apply))]
    static class ProtectiveLuckCacklePatch
    {
        static readonly BlueprintGuid WitchCackle = BlueprintGuid.Parse("4bd01292a9bc4304f861a6a07f03b855");
        static readonly BlueprintGuid WitchProtectiveLuck = BlueprintGuid.Parse("e7ecd11651b4df34897f33271a8d1cfc");
        static readonly BlueprintGuid WitchProtectiveLuckFeature = BlueprintGuid.Parse("ddfc51eaa495acb40a01b92a2dd4ab73");
        static readonly BlueprintGuid WitchProtectiveLuckBuff = BlueprintGuid.Parse("1ba3d3a0942d080448ce6aa865bbbd65");

        static readonly BlueprintGuid ShamanChant = BlueprintGuid.Parse("6cd07c80aabf2b248a11921090de9c17");
        static readonly BlueprintGuid ShamanProtectiveLuck = BlueprintGuid.Parse("e3eddc31b2430b7408c62f313efea4bc");
        static readonly BlueprintGuid ShamanProtectiveLuckFeature = BlueprintGuid.Parse("f0db7409313e50c41ab1bfe36584a4e1");
        static readonly BlueprintGuid ShamanProtectiveLuckBuff = BlueprintGuid.Parse("9e7a13c71f6be1749af81afc57640ccf");

        static AbilityExecutionContext handled;
        static bool missingLogged;

        static void Postfix(AbilityExecutionContext context, TargetWrapper target)
        {
            if (context == null)
                return;

            var ability = context.AbilityBlueprint;
            if (ability == null)
                return;

            var guid = ability.AssetGuid;
            var witch = guid == WitchCackle || ability.name == "WitchHexCackleAbility";
            var shaman = guid == ShamanChant || ability.name == "ShamanHexChantAbility";
            if (!witch && !shaman)
                return;

            try
            {
                Share(context, target, witch);
            }
            catch (Exception ex)
            {
                Main.Log("Protective Luck share failed: " + ex);
            }
        }

        static void Share(AbilityExecutionContext context, TargetWrapper target, bool witch)
        {
            var caster = context.MaybeCaster;
            if (caster == null)
                return;

            var feature = Load<BlueprintFeature>(witch ? WitchProtectiveLuckFeature : ShamanProtectiveLuckFeature);
            var hex = Load<BlueprintAbility>(witch ? WitchProtectiveLuck : ShamanProtectiveLuck);
            var buff = Load<BlueprintBuff>(witch ? WitchProtectiveLuckBuff : ShamanProtectiveLuckBuff);
            if (feature == null || hex == null || buff == null)
            {
                if (!missingLogged)
                {
                    missingLogged = true;
                    Main.Log("Protective Luck blueprints were not found. Cackle and Chant were left unchanged.");
                }
                return;
            }

            if (!caster.HasFact(feature) && !caster.HasFact(hex))
                return;

            var rank = hex.GetComponent<ContextRankConfig>();
            var rounds = rank != null ? rank.GetValue(context) : 1;
            if (rounds < 1)
                rounds = 1;

            var duration = new Rounds(rounds).Seconds;
            var firstTarget = !ReferenceEquals(context, handled);
            if (firstTarget)
            {
                handled = context;
                var radius = AbilityRadius(context);
                var shared = 0;

                Grant(caster, buff, context, duration);
                shared++;

                foreach (var unit in GameHelper.GetTargetsAround(caster.EyePosition, radius, checkLOS: true))
                {
                    if (unit == null || unit == caster || !caster.IsAlly(unit))
                        continue;

                    Grant(unit, buff, context, duration);
                    shared++;
                }

                Main.Log(caster.CharacterName + " shared Protective Luck with " + shared + " allies for " + rounds + " rounds.");
            }

            // Cackle still extends an existing Protective Luck by 1 round while it
            // walks each target. Put this target back to the hex duration.
            var current = target.Unit;
            if (current != null && (current == caster || caster.IsAlly(current)))
                Grant(current, buff, context, duration);
        }

        static Feet AbilityRadius(AbilityExecutionContext context)
        {
            return context.AbilityBlueprint.GetComponent<AbilityTargetsAround>()?.AoERadius ?? 30.Feet();
        }

        static void Grant(UnitEntityData unit, BlueprintBuff buff, MechanicsContext context, TimeSpan duration)
        {
            if (unit?.Descriptor == null)
                return;

            var applied = unit.Descriptor.Buffs.AddBuff(buff, context, duration) ?? unit.Descriptor.Buffs.GetBuff(buff);
            applied?.SetDuration(duration);
        }

        static T Load<T>(BlueprintGuid guid) where T : BlueprintScriptableObject
        {
            return ResourcesLibrary.TryGetBlueprint<T>(guid);
        }
    }
}
