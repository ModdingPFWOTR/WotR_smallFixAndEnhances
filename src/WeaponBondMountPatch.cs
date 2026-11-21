using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Visual.Animation.Kingmaker.Actions;

namespace smallFixAndEnhances
{
    // The enchant-weapon cast clip is missing from the mounted animation set.
    // UnitUseAbility waits for that clip to act, so the bond never applies.
    // Immediate is the game's own "no cast animation" style: TriggerAnimation
    // returns without starting a clip, and the command acts on the next tick.
    [HarmonyPatch(typeof(UnitUseAbility), "TriggerAnimation")]
    static class WeaponBondMountPatch
    {
        static readonly BlueprintGuid WeaponBond = BlueprintGuid.Parse("7ff088ab58c69854b82ea95c2b0e35b4");
        static readonly BlueprintGuid DivineHunterBond = BlueprintGuid.Parse("8b405b057dc5051448e20041bd366592");

        static void Prefix(
            UnitUseAbility __instance,
            ref UnitAnimationActionCastSpell.CastAnimationStyle ___m_CastAnimStyle,
            ref UnitAnimationActionCastSpell.SpecialBehaviourType ___m_Special,
            ref float ___m_CastTime)
        {
            if (!ShouldSkip(__instance))
                return;

            ___m_CastAnimStyle = UnitAnimationActionCastSpell.CastAnimationStyle.Immediate;
            ___m_Special = UnitAnimationActionCastSpell.SpecialBehaviourType.NoPrecast;
            ___m_CastTime = 0f;
            Main.Log("Skipped divine bond animation for mounted " + __instance.Executor.CharacterName + ".");
        }

        static bool ShouldSkip(UnitUseAbility command)
        {
            if (!IsDivineWeaponBond(command.Ability?.Blueprint))
                return false;

            var executor = command.Executor;
            if (executor == null)
                return false;

            return UnitHelper.GetSaddledUnit(executor) != null;
        }

        static bool IsDivineWeaponBond(BlueprintAbility ability)
        {
            if (ability == null)
                return false;

            if (ability.name == "WeaponBondSwitchAbility" || ability.name == "DivineHunterBondSwitchAbility")
                return true;

            var guid = ability.AssetGuid;
            return guid == WeaponBond || guid == DivineHunterBond;
        }
    }
}
