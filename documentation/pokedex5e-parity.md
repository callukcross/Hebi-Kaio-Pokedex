# Pokedex5E baseline parity inventory

This document treats Jerakin/Pokedex5E as the baseline product. HebiKaio additions extend this baseline; they do not replace it. Debug-only analytics controls are documented but are not a release requirement. Network and QR features require platform-specific implementations and are tracked explicitly rather than silently omitted.

| Area | Reference pages and controls | HebiKaio status |
|---|---|---|
| Shell | Splash/version, slide-out menu, active-screen highlighting, connection indicator | Partial: persistent reference-style navigation implemented; splash and live connection indicator remain |
| Profiles | Search, profile slots, create/name, activate, rename, isolated saves | Implemented in desktop shell |
| Party | Six configurable active slots, page switching, storage jump, edit | Implemented, including configurable active limit |
| Battle sheet | HP/current/temp controls and bar, EXP bar, loyalty, AC, saves, attributes, skills, type, SR, size, nature, STAB, proficiency, catch rate, hit die, speeds, senses, gender, held item, vulnerabilities/resistances/immunities | Partial: persistent battle meters, core information and controls implemented; type defenses and senses remain |
| Moves in battle | Move cards, PP current/max controls, attack/damage/type/range/duration/time, details/reset | Implemented except individual reset shortcut |
| Features/status | Abilities, feats, status effects (asleep, burned, confused, frozen, paralyzed, poisoned), full rest | Implemented |
| Pokémon creation | Species, nickname, level, gender, shiny, nature, variant, abilities, feats, skills, moves, held item, six ability scores, HP, ASI/custom ASI, collapsible sections | Implemented as tabbed searchable desktop editor; evolution workflow remains |
| Pokémon editing | All creation fields, evolution, max HP override, delete/release confirmation | Partial |
| Selectors | Searchable nature, move, ability, feat, skill, item, variant/fakemon lists; move filters for current/max level, TM/HM, egg, all | Missing |
| Storage | Search, sorting by index/name/level, rows, party indicator, add, transfer/move/release/share | Partial: search and drag ordering only |
| Encounter generator | Trainer level, Pokémon level, min/max SR, generation, habitat, type, trainer type, clear/reset, random encounter, add result | Partial: level/SR/generation/type/random/add/reset implemented; habitat and trainer-type filters remain |
| Pokédex list | Search, region tabs/counters, encountered-state filters, bulk mark menu | Implemented with composable filters, counters and bulk marking |
| Pokédex detail | Artwork, index/species/genus/flavor, height/weight/type, seen/caught toggles | Implemented |
| Trainer modifiers | Nine tabs; per-type attack/damage/STAB; always-STAB types; global attack/damage/STAB/move/ASI/evolution modifiers; six attributes; max active Pokémon; rename | Implemented in consolidated desktop tabs alongside HebiKaio classes/feats/inventory |
| Settings | Strict gender toggle, fakemon module selection/removal, help, import/paste behavior | Partial: settings and module management implemented; strict-gender enforcement remains |
| Import/share | Clipboard import/export, QR display/read, platform share, receive page | Partial: file profile and individual Pokémon transfer implemented; clipboard/QR remain |
| Local network | Host/join/direct/nearby, groups, members, connection status, Pokémon send/receive | Missing |
| About/version | Version, Android version, changelog/version dialog, support link, share log | Partial: about/version/source links implemented; changelog and share-log remain |
| Common overlays | Confirmation, info, text input, notification, searchable list, move info, transfer/swap, max-party tutorial | Partial: native dialogs only |

## Reference data required for parity

- 810 complete Pokémon records, including attributes, combat values, moves, abilities, skills, size, speed, senses, gender, variants and evolution metadata.
- 675 move records plus move index/machine mappings.
- Abilities, items, feats, natures, habitats, gender rules, Pokédex flavor/genus/height/weight, trainer classes, leveling and variant maps.

## Implementation order

1. Import and validate the complete reference rules data.
2. Expand persisted Pokémon and trainer state with explicit migrations.
3. Implement battle-sheet calculations and mutation services.
4. Replace the temporary main menu with a persistent reference-style navigation shell.
5. Implement screens in dependency order: profiles/settings, creation/edit selectors, storage, party/battle, generator, Pokédex detail, import/share, network/about.
6. Verify each row above with regression and UI interaction checks before declaring baseline parity.
