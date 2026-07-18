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
- No hard UPM dependency on Zenject/Extenject — peer on assembly name `Zenject`
- Repo: `https://github.com/KeybladerV/unity-pipelined-commands`
