# Pipelined Commands

Unity package for **command pipelines** driven by Zenject signals (PostMVC-style).

Bind a signal type to a chain of commands. Run steps in **sequence** or **parallel**. Support **async** steps via `Retain` / `Release` / `Fail` / `Break`. Optional pooling and complete / break / fail follow-up signals.

| | |
|---|---|
| **UPM name** | `com.keybladerv.pipelined-commands` |
| **Assembly** | `PipelinedCommands` |
| **Namespace** | `PipelinedCommands` |
| **Unity** | 2021.3+ |
| **License** | MIT |
| **Latest** | [v0.1.2](https://github.com/KeybladerV/unity-pipelined-commands/releases/tag/v0.1.2) |

---

## Requirements

| | |
|---|---|
| **Zenject** (or Extenject / fork) | **Required.** Asmdef assembly name must be `Zenject`. This package does not ship or pin DI. |
| **UniTask** (`com.cysharp.unitask`) | **Optional.** Only for `DelaySecondsCommand` (assembly `PipelinedCommands.UniTask`). |

Zenject is a **peer** dependency (not listed in `package.json`):

- UPM cannot pull another git package as a dependency of a package.
- Projects use different Zenject / Extenject installs and forks.

The contract is the assembly name **`Zenject`** (standard for stock Zenject, Extenject, and most forks). Commands use `SignalBus` and `DiContainer`.

---

## Install

Package Manager → **+** → **Add package from git URL**:

```text
https://github.com/KeybladerV/unity-pipelined-commands.git#v0.1.2
```

Or in the project `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.keybladerv.pipelined-commands": "https://github.com/KeybladerV/unity-pipelined-commands.git#v0.1.2"
  }
}
```

Pin a **tag** (`#v0.1.2`), not a floating branch.

Host game assemblies that define commands must **reference** the asmdef **`PipelinedCommands`**.

> **Note:** This package commits Unity `.meta` files. Git UPM packages land in PackageCache as immutable; without metas, Unity ignores all scripts.

See [CHANGELOG.md](CHANGELOG.md) for release history.

---

## Setup

```csharp
using Zenject;
using PipelinedCommands;

SignalBusInstaller.Install(Container);

// Declare every signal type used with Bind / Fire / WaitSignalCommand
Container.DeclareSignal<LevelWonSignal>();
Container.DeclareSignal<LevelFlowDoneSignal>();

Container.BindInterfacesAndSelfTo<CommandMap>().AsSingle();
```

---

## Quick start

```csharp
using PipelinedCommands;
using PipelinedCommands.Utility;

// Signal → pipeline
_commands.Bind<LevelWonSignal>()
    .To0<SaveProgressCommand>()
    .To1<DelaySecondsCommand, float>(1f)   // requires UniTask in the project
    .To0<ShowInterstitialCommand>()          // your game command, not in this package
    .InSequence()
    .OnCompleteFire<LevelFlowDoneSignal>();

// Imperative pipeline (no trigger signal)
_commands.Flow()
    .To0<BootStepACommand>()
    .To0<BootStepBCommand>()
    .InSequence()
    .Execute();

// Trigger
_signalBus.Fire(new LevelWonSignal());
```

### Command skeleton

```csharp
using PipelinedCommands;

public sealed class SaveProgressCommand : Command
{
    // Zenject injects via constructor or [Inject]

    public override void Execute()
    {
        // Sync work — completes when Execute returns.

        // Async:
        // Retain();
        // ... later ...
        // Release();  // or Fail(ex) / Break();
    }
}

public sealed class HandleWonCommand : Command<LevelWonSignal>
{
    public override void Execute(LevelWonSignal signal) { /* ... */ }
}
```

### Binding API (essentials)

| API | Meaning |
|-----|---------|
| `To0<TCommand>()` | Parameterless command |
| `To1<TCommand>()` | `Command<TSignal>` — payload is the signal |
| `To1<TCommand, TArg>(arg)` | Fixed argument |
| `InSequence()` / `InParallel()` | Execution mode |
| `Once()` / `Once(OnceBehavior)` | Unbind after complete / fail / break |
| `When(...)` / `TriggerCondition(...)` | Gate the binding |
| `OnComplete` / `OnFail` / `OnBreak` | Callbacks |
| `OnCompleteFire<T>()` / `OnFailFire<T>()` / `OnBreakFire<T>()` | Fire follow-up signals |
| `And<TOtherSignal>()` | Same chain on another signal type |
| `Flow()` / `Flow<T1>()` / … | Imperative pipelines |

Game-specific commands (ads, UI, gameplay) stay in **your** assemblies.

---

## Utilities

| Type | Assembly | Notes |
|------|----------|--------|
| `InvokeActionCommand` | `PipelinedCommands` | `Command<Action>` — invoke a stored action |
| `WaitSignalCommand<T>` | `PipelinedCommands` | Retain until signal `T` fires once |
| `DelaySecondsCommand` | `PipelinedCommands.UniTask` | Needs UniTask in the project |

---

## License

MIT — see [LICENSE](LICENSE).
