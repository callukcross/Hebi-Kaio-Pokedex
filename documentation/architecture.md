# HebiKaio Pokédex architecture

## Audit summary

The repository began as three separate .NET Framework 4.8 Windows Forms experiments. The main project contained screen layouts but no domain model, save format, profile persistence, Pokédex data loader, or automated verification. Pokémon artwork was also committed twice under two nearly identical directories.

The reference Pokedex5E application is a mature Defold/Lua project. Its reusable product concepts are profiles, trainer state, party and storage, Pokédex discovery state, Pokémon creation/editing, items, feats, filters, backups, and data-driven content. Its engine-specific GUI and Lua modules should be treated as behavioral reference rather than copied into form event handlers.

## Direction

All game and application rules live in `src/HebiKaio.Core`, which has no dependency on Windows Forms. Platform front ends call this core. The existing Windows Forms project remains a temporary desktop host while the team validates workflows. A later UI milestone can use .NET MAUI or another cross-platform shell without migrating save rules or domain logic again.

Save data uses versioned JSON and an adjacent backup file. Custom modules should eventually use the same versioned, data-driven approach and must not require recompiling the UI.

## Delivery phases

1. Foundation: persistent profiles, versioned saves/backups, domain models, filtering, and repeatable checks.
2. Pokédex: canonical species dataset, searchable/filterable list, detail view, and seen/caught state. **Implemented for the 810-entry Pokémon 5e reference index.**
3. Pokémon management: creation/editing, storage, party limits, drag-and-drop ordering, and custom images.
4. Trainer rules: classes, feats, items, inventory, derived modifiers, and milestone leveling.
5. Extensibility: import/export and validated custom content modules.
6. Distribution: responsive cross-platform UI, migration from the Windows prototype, and PC/mobile packaging.

## Save compatibility rules

- Increment `schemaVersion` only when the persisted shape changes.
- Add an explicit migration before removing or renaming a persisted field.
- Never overwrite the only known-good save; preserve the previous file as `.bak`.
- Store user-selected image paths or copied user assets, not image bytes inside the profile JSON.
