# Pokedex5E baseline parity inventory

This document treats Jerakin/Pokedex5E as the baseline product. HebiKaio additions extend this baseline; they do not replace it. Debug-only analytics controls are documented but are not a release requirement. Network and QR features require platform-specific implementations and are tracked explicitly rather than silently omitted.

| Area | Reference pages and controls | HebiKaio status |
|---|---|---|
| Shell | Splash/version, slide-out menu, active-screen highlighting, connection indicator | Partial: basic main menu only |
| Profiles | Search, profile slots, create/name, activate, rename, isolated saves | Partial: create/active; search, slots, rename missing |
| Party | Six configurable active slots, page switching, storage jump, edit | Partial: membership/order only |
| Battle sheet | HP/current/temp controls and bar, EXP bar, loyalty, AC, saves, attributes, skills, type, SR, size, nature, STAB, proficiency, catch rate, hit die, speeds, senses, gender, held item, vulnerabilities/resistances/immunities | Missing |
| Moves in battle | Move cards, PP current/max controls, attack/damage/type/range/duration/time, details/reset | Missing |
| Features/status | Abilities, feats, status effects (asleep, burned, confused, frozen, paralyzed, poisoned), full rest | Missing |
| Pokémon creation | Species, nickname, level, gender, shiny, nature, variant, abilities, feats, skills, moves, held item, six ability scores, HP, ASI/custom ASI, collapsible sections | Partial: species/nickname/form/level/image only |
| Pokémon editing | All creation fields, evolution, max HP override, delete/release confirmation | Partial |
| Selectors | Searchable nature, move, ability, feat, skill, item, variant/fakemon lists; move filters for current/max level, TM/HM, egg, all | Missing |
| Storage | Search, sorting by index/name/level, rows, party indicator, add, transfer/move/release/share | Partial: search and drag ordering only |
| Encounter generator | Trainer level, Pokémon level, min/max SR, generation, habitat, type, trainer type, clear/reset, random encounter, add result | Missing |
| Pokédex list | Search, region tabs/counters, encountered-state filters, bulk mark menu | Partial: stronger composable filters, but counters/bulk marking missing |
| Pokédex detail | Artwork, index/species/genus/flavor, height/weight/type, seen/caught toggles | Partial: rules summary only |
| Trainer modifiers | Nine tabs; per-type attack/damage/STAB; always-STAB types; global attack/damage/STAB/move/ASI/evolution modifiers; six attributes; max active Pokémon; rename | Partial: class-derived effects, feats and inventory; manual baseline controls missing |
| Settings | Strict gender toggle, fakemon module selection/removal, help, import/paste behavior | Partial: custom modules exist; settings page missing |
| Import/share | Clipboard import/export, QR display/read, platform share, receive page | Partial: file profile transfer only |
| Local network | Host/join/direct/nearby, groups, members, connection status, Pokémon send/receive | Missing |
| About/version | Version, Android version, changelog/version dialog, support link, share log | Missing |
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
