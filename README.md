# Small Fix and Enhances

A [Unity Mod Manager](https://github.com/newman55/unity-mod-manager) mod for **Pathfinder: Wrath of the Righteous** (game 2.7.0, UMM 0.23.0). It patches two ability quirks with Harmony.

## Divine Weapon Bond while mounted

Divine Weapon Bond and Divine Hunter Bond play a weapon-enchant cast animation before the bond is applied. That clip is missing from the mounted animation set, so the command waits for an animation that never finishes and the bond never lands.

While the caster is in the saddle, this mod skips that animation and applies the bond immediately. On foot, the animation still plays as usual.

## Protective Luck from Cackle and Chant

Protective Luck normally lasts one round, plus one more at class levels 8 and 16. Witch Cackle and Shaman Chant extend an existing Protective Luck by one round each time they are used, so the same buff can be stacked for minutes before a fight.

When the caster knows Protective Luck, Cackle and Chant now grant it to the caster and to allies within 30 feet (line of sight). The buff lasts the hex's normal duration and is refreshed instead of stacked, so extra uses do not bank rounds. The action is still spent, and allies still have to be in range.

## Build

Requires the .NET SDK and a local Wrath of the Righteous install with Unity Mod Manager. `Directory.Build.props` points at the Steam game folder used to resolve the game and UMM assemblies.

```
dotnet build -c Release
```

Copy `bin/smallFixAndEnhances.dll` and `bin/Info.json` into `Mods/smallFixAndEnhances/` under the game folder, then enable the mod in Unity Mod Manager.
