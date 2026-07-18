# Changelog

## [0.1.2] - 2026-07-18

### Fixed
- `CommandBase.IsResolved` is `protected internal` so `CommandMap` can access it and UniTask utility subclasses still can.

## [0.1.1] - 2026-07-18

### Fixed
- Commit Unity `.meta` files so git packages are not ignored in PackageCache (immutable).

## [0.1.0] - 2026-07-18

### Added
- Core: `Command` / `CommandMap` / bindings / flow / pool / `OnceBehavior`
- Zenject `SignalBus` integration (peer assembly `Zenject`)
- Utilities: `InvokeActionCommand`, `WaitSignalCommand<T>`
- Optional `DelaySecondsCommand` (`PipelinedCommands.UniTask`) when UniTask is present
- UPM package `com.keybladerv.pipelined-commands`
