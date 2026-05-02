# CupkekGames VFX

Runtime VFX layer. Holds the pooled VFX spawner, render-feature darken/fade controller, dissolve helpers, abstract outline manager, and shader-property controllers. Concrete shader backends (MKToon, PotaToon) live in sibling bridge packages so this core stays renderer-agnostic.

## What's inside

**Runtime** (`CupkekGames.VFX.asmdef`)

- `VFXBundle` — serializable pooled VFX spawner with optional SFX + tween + time-scaling.
- `RenderFeatureManager` (`ServiceProvider`) — manages a darken layer for an "everything except X" effect with material fades.
- `Dissolvable` + `DissolveGroup` + `FadeableOutline` — soft fade-in/out helpers driven by `Fadeable`.
- `OutlineManager` (abstract) — base for shader-specific outline backends; consumers call `AddOutline`/`RemoveOutline`/`SetWidth`/`SetColor`.
- `ShaderColorController`, `ShaderEmissionController` — weighted-source overrides for material color/emission, returning `Guid` handles.

## Dependencies

Asmdef references resolve via the CupkekGames scoped registry: `services`, `pool`, `fadeables`, `addressableassets`, `scenemanagement`, `sequencer`, `settings`, `gamesave`, `transforms`, `audio`, `timesystem`, `keyvaluedatabases`. Bring your own copy via the registry.

Shader backends are NOT bundled here — see `com.cupkekgames.vfx.mktoon` / `com.cupkekgames.vfx.potatoon`.
