# RealtVJ - Claude Context

## Sistem Özeti

Event-driven visual programming sistemi. Unity GraphView tabanlı node-graph editörü ile real-time VJ performans kontrolü sağlar. **Trigger → Result** paradigması üzerine kurulu. Boolean logic (AND/OR) graph topolojisi ile ifade edilir.

**Runtime yok henüz** — sadece editor-side graph oluşturma ve ScriptableObject'e serialize etme mevcut.

---

## Mimari

```
[TriggerBox] ──→ [ResultBox] ──→ [DefaultStateBox]
   (koşul)         (aksiyon)       (reset aksiyonu)
```

- **Dikey bağlantı (chain):** AND logic
- **Fan-in (birden fazla trigger → aynı result):** OR logic
- **Result → Result:** Sıralı execution (delay destekli)
- **Result → DefaultState:** Yatay bağlantı, trigger false olunca çalışır

---

## Dosya Haritası

### Data Layer (`Scripts/Data/`)

| Dosya | Sınıf | Rol |
|-------|-------|-----|
| `Boxes/BoxData.cs` | `BoxData` | Base: GUID + position |
| `Boxes/TriggerBoxData.cs` | `TriggerBoxData` | Trigger listesi + cooldown ayarları |
| `Boxes/ResultBoxData.cs` | `ResultBoxData` | Result listesi + execution mode (ATST/InOrder) + loop |
| `Boxes/DefaultStateBoxData.cs` | `DefaultStateBoxData` | Reset result listesi |
| `Edges/EdgeData.cs` | `EdgeData` | Bağlantı: source/target GUID, EdgeType, delay |
| `Groups/RuleGroupData.cs` | `RuleGroupData` | Görsel gruplama (logic'i etkilemez) |
| `RuleGraphContainer.cs` | `RuleGraphContainer` | **Ana ScriptableObject.** Tüm box, edge, group listelerini tutar. Runtime cache'ler (dict by GUID) içerir. |
| `Enums/VJEnums.cs` | — | Tüm enum tanımları tek dosyada |

### Trigger Tipleri (`Scripts/Data/Triggers/`)

| Dosya | Sınıf | Alanlar |
|-------|-------|---------|
| `Trigger.cs` | `Trigger` (abstract) | `TriggerType Type`, `Clone()` |
| `HandTrigger.cs` | `HandTrigger` | `WhichHand`, `HandPosition` (L/R), `HandGesture` |
| `MusicTrigger.cs` | `MusicTrigger` | `MusicTriggerType`, `float Value` |
| `TimeTrigger.cs` | `TimeTrigger` | `TimeTriggerType`, `float Duration`, `int RepeatCount` |

### Result Tipleri (`Scripts/Data/Results/`)

| Dosya | Sınıf | Alanlar |
|-------|-------|---------|
| `Result.cs` | `Result` (abstract) | `ResultType Type`, `Clone()` |
| `ObjectResult.cs` | `ObjectResult` | `GameObject`, `SetActive`, `Scale/Rotation/Position` |
| `AnimationResult.cs` | `AnimationResult` | `GameObject`, `TriggerName` |
| `SoundResult.cs` | `SoundResult` | `AudioClip`, `Volume` |
| `VolumeSettingResult.cs` | `VolumeSettingResult` | `GameObject`, `PropertyName`, `Value`, `ColorValue` |
| `CameraSwitchResult.cs` | `CameraSwitchResult` | `GameObject`, `CameraViewMode` |
| `CameraMoveResult.cs` | `CameraMoveResult` | `TargetPosition/Rotation`, `Duration` |

### Editor Layer (`Scripts/Editor/`)

| Dosya | Sınıf | Rol |
|-------|-------|-----|
| `VJGraph.cs` | `VJGraph : EditorWindow` | Ana pencere. Toolbar (save/load/minimap/group). Menü: `Graph → VJ Rule Graph` |
| `VJGraphView.cs` | `VJGraphView : GraphView` | Grid, zoom/pan, port uyumluluk kontrolü, node factory |
| `VJGraphSaveUtility.cs` | `VJGraphSaveUtility` | Save/Load → `Assets/Resources/VJGraphs/{name}.asset` |
| `VJNodeSearchWindow.cs` | `VJNodeSearchWindow` | Sağ-tık node oluşturma menüsü |
| `Nodes/VJNode.cs` | `VJNode : Node` | Abstract base. Dikey portlar (Top=Input, Bottom=Output), yan portlar (DefaultState) |
| `Nodes/TriggerBoxNode.cs` | `TriggerBoxNode : VJNode` | Mavi (#00D4FF). Trigger ekleme/silme, tip değiştirme dropdown, dinamik alanlar |
| `Nodes/ResultBoxNode.cs` | `ResultBoxNode : VJNode` | Mor (#9B59B6). Result ekleme/silme, DefaultStatePort (sağ yan) |
| `Nodes/DefaultStateNode.cs` | `DefaultStateNode : VJNode` | Lavanta (#7C3AED). Sol yan input portu |
| `Resources/VJGraph.uss` | — | Dark theme stylesheet |

---

## Enum Referansı (VJEnums.cs)

```
TriggerType:        Hand, Music, Time
ResultType:         Object, Animation, VolumeSetting, Sound, CameraSwitch, CameraMove
EdgeType:           TriggerToResult, TriggerToTrigger, ResultToResult, ResultToDefault
CooldownMode:       Full, ForDuration, EveryInterval
ResultExecutionMode: ATST, InOrder
WhichHand:          Left, Right, LeftOrRight, LeftAndRight
HandPosition:       Anywhere, LeftScreen, RightScreen, MiddleScreen, NotOnScreen
HandGesture:        Any, ThumbsUp, ThumbsDown, One, Two, Three, Four, Five,
                    Pinching, FingerGun, Fist, Peace, RockAndRoll, HandsTogether, HandsApart
MusicTriggerType:   EveryNBeats, EnergyAbove, FrequencyBandEnergy, BPMChange, SilenceDetection, BeatDrop
TimeTriggerType:    AfterDuration, EveryInterval, RepeatN, DelayedStart
CameraViewMode:     Full, LeftHalf, RightHalf, TopHalf, BottomHalf
```

---

## Serialization

- **Polymorphism:** `[SerializeReference]` ile Trigger/Result listeleri serialize edilir
- **Vector2:** `ISerializationCallbackReceiver` ile float'lara ayrıştırılır
- **Referanslar:** GUID tabanlı — BoxData'lar arası bağlantılar EdgeData üzerinden
- **Save path:** `Assets/Resources/VJGraphs/{fileName}.asset`

---

## Bağımlılıklar

- **Cadi Library** (`Assets/Cadi/`): EventSystem (priority-based), CacherSystem ([CachedField] auto-ref), CustomAttributes (Button, ShowIf, DynamicRange)
- **Odin Inspector** (`Assets/Plugins/Sirenix/`): Opsiyonel, `ODIN_INSPECTOR` define ile kontrol edilir
- **Unity:** UIElements, GraphView (Experimental), Editor, Serialization

---

## Kurallar & Dikkat Edilecekler

1. **Yeni Trigger/Result tipi eklerken:**
   - `Trigger.cs` veya `Result.cs`'den türet
   - İlgili enum'a ekle (`VJEnums.cs`)
   - Editor node'unda (`TriggerBoxNode.cs` veya `ResultBoxNode.cs`) UI alanlarını ekle
   - `Clone()` metodunu implement et
   - `VJGraphSaveUtility` otomatik handle eder (`[SerializeReference]` sayesinde)

2. **Port sistemi dikey:**
   - Top = Input (gelen bağlantı)
   - Bottom = Output (giden bağlantı)
   - Yan portlar sadece DefaultState bağlantısı için

3. **Edge tipleri otomatik belirlenir:** Bağlanan node tiplerine göre `EdgeType` set edilir (save sırasında)

4. **Runtime henüz yok:** Graph'ı runtime'da evaluate edecek bir RuleEngine/Executor yazılması gerekiyor

5. **Namespace yok:** Dosyalar namespace kullanmıyor, dikkatli olunmalı

6. **Assembly Definition yok:** Tüm scriptler default assembly'de

---

## Eksik / Yapılacaklar

- [ ] Runtime evaluation engine (graph traversal + trigger polling + result execution)
- [ ] Hand tracking entegrasyonu (input source)
- [ ] Music analysis entegrasyonu (audio input)
- [ ] Undo/Redo desteği (GraphView native undo ile)
- [ ] Copy/Paste node desteği
- [ ] Graph validation (orphan node, cycle detection)
- [ ] Assembly definitions eklenmesi
- [ ] Namespace organizasyonu