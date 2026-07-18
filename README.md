# Pipelined Commands

PostMVC-style **command map** for Unity: bind a signal to a pipeline of commands — sequence or parallel, `Retain` / `Release`, pooling, and optional complete / break / fail signals.

| | |
|---|---|
| **UPM name** | `com.keybladerv.pipelined-commands` |
| **Assembly** | `PipelinedCommands` |
| **Namespace** | `PipelinedCommands` |
| **Unity** | 2021.3+ |

---

## Peer dependency: Zenject

This package **does not ship or pin** Zenject / Extenject.

It only needs a host project that already provides the **`Zenject` assembly** (the usual asmdef name for stock Zenject, Extenject, and most forks). Commands talk to Zenject’s **`SignalBus`** and **`DiContainer`**.

| | Required | What matters |
|---|----------|----------------|
| **Zenject-compatible DI** | **Yes** | Assembly definition **name** = `Zenject` |
| **UniTask** | Optional | Only for `DelaySecondsCommand` (`com.cysharp.unitask`) |

**Why nothing is listed under `package.json` → `dependencies`:**

1. UPM **cannot** pull git packages as *dependencies of another package* — only as entries in the **project** `Packages/manifest.json`.
2. Projects disagree on *which* fork they use (classic Plugins drop, Extenject UPM, private fork). Pinning one would break the others.
3. The real contract is the **assembly name** `Zenject`, not a UPM package id.

If your DI already lives under `Assets/Plugins` (or any UPM path) with asmdef name **`Zenject`**, you’re done on that side.

<details>
<summary>Examples if you still need to install DI (optional — use what your team already uses)</summary>

**Classic:** import Zenject / Extenject into `Assets/Plugins` so the asmdef is named `Zenject`.

**UPM git (one common Extenject layout):**

```json
"com.svermeulen.extenject": "https://github.com/Mathijs-Bakker/Extenject.git?path=/UnityProject/Assets/Plugins/Zenject#9.2.0"
```

Any other source is fine as long as the compiled assembly is still called **`Zenject`**.

</details>

---

## Install this package

Package Manager → **+** → **Add package from git URL**:

```text
https://github.com/KeybladerV/unity-pipelined-commands.git#v0.1.0
```

Or in `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.keybladerv.pipelined-commands": "https://github.com/KeybladerV/unity-pipelined-commands.git#v0.1.0"
  }
}
```

Prefer a **tag** (`#v0.1.0`) over a floating branch. Host assemblies that define commands must reference asmdef **`PipelinedCommands`**.

---

## Setup

```csharp
using Zenject;
using PipelinedCommands;

// ProjectContext / SceneContext — once per container that runs commands
SignalBusInstaller.Install(Container);

// Every signal type you Bind / Fire / WaitSignal on
Container.DeclareSignal<LevelWonSignal>();
Container.DeclareSignal<LevelFlowDoneSignal>();

Container.BindInterfacesAndSelfTo<CommandMap>().AsSingle();
```

---

## Usage

```csharp
using PipelinedCommands;
using PipelinedCommands.Utility;

// Signal → pipeline
_commands.Bind<LevelWonSignal>()
    .To0<SaveCommand>()
    .To1<DelaySecondsCommand, float>(1f) // needs UniTask in the project
    .To0<ShowInterstitialCommand>()
    .InSequence()
    .OnCompleteFire<LevelFlowDoneSignal>();

// Imperative pipeline (no signal)
_commands.Flow()
    .To0<BootA>()
    .To0<BootB>()
    .InSequence()
    .Execute();

// Fire with the host SignalBus
_signalBus.Fire(new LevelWonSignal());
```

Game-specific commands (ads, UI, missions, …) stay in **your** assemblies; this package only provides the map, pooling, and utilities.

---

## Versioning

```bash
git tag v0.1.0
git push origin v0.1.0
```

Keep `package.json` → `"version"` aligned with the tag (`0.1.0` ↔ `v0.1.0`).

---

## License

MIT — see [LICENSE](LICENSE).
