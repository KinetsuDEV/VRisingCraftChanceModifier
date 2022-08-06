# CraftChanceModifier
`Server side only` mod to change craft chance.
`Hot reload` enabled.

## Installation
* Install [BepInEx](https://v-rising.thunderstore.io/package/BepInEx/BepInExPack_V_Rising/)
* Install [Wetstone](https://v-rising.thunderstore.io/package/molenzwiebel/Wetstone/)
* Extract _CraftChanceModifier.dll_ into _(VRising server folder)/BepInEx/plugins_

## Configurable Values

Set `CraftChanceModifier` value between 0 and 1 to change craft success rate.

Example:
- 0.1 = 10% success rate
- 0.5 = 50% success rate
- 1 = 100% success rate

```ini
[CraftChanceConfig]

## Craft chance modifier value
# Setting type: Single
# Default value: 1
CraftChanceModifier = 1
```