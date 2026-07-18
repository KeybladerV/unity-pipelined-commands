## [0.1.2] - 2026-07-18

### Fixed
- `CommandBase.IsResolved` is `protected internal` so `CommandMap` (same assembly) can read it; subclasses in `PipelinedCommands.UniTask` still can too.

## [0.1.1] - 2026-07-18

### Fixed
- Commit Unity .meta files so git packages are not ignored in PackageCache (immutable).

# Changelog

## [0.1.0] - 2026-07-18

### Added
- Core command map: `Command` / `CommandMap` / bindings / flow / pool / `OnceBehavior`
- Zenject **`SignalBus`** integration (peer: host provides assembly `Zenject`)
- Utility: `InvokeActionCommand`, `WaitSignalCommand<T>`
- Optional `DelaySecondsCommand` assembly when UniTask is present (`PipelinedCommands.UniTask`)

### Package
- UPM: `com.keybladerv.pipelined-commands` (install via git URL)
- Display name: **Pipelined Commands**
- Root assembly / namespace: `PipelinedCommands`
- No hard UPM dependency on Zenject/Extenject Ã¢â‚¬â€ peer on assembly name `Zenject`
- Repo: `https://github.com/KeybladerV/unity-pipelined-commands`
