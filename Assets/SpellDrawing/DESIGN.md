# SpellDrawing 1단계 설계

> 상태: **제작 중** (2026-10-30 "제작 시작" 승인)
>
> 구현 중 바뀐 점
> - 네임스페이스: Debug는 `VoxelMagic.SpellDrawing.DebugUI`, Editor는 `VoxelMagic.SpellDrawing.EditorTools` (`UnityEngine.Debug`·`UnityEditor.Editor`와 이름 충돌 방지). asmdef 이름은 지침 그대로.
> - `JudgeSettings.distanceScoring` 추가: 거리 → 점수 변환을 "평균 거리 ÷ 허용 거리"(지침, 기본값)와 "점별로 0~1 자른 뒤 평균" 중 고를 수 있게 함. 평균 거리 방식은 절반만 그리거나 낙서한 경우 둘 다 0으로 붙어 순서를 가리기 어려울 수 있어서, 기준 비교표로 두 방식을 비교한 뒤 정한다.
> - 에디터 메뉴 `Tools/SpellDrawing/Create Default Assets` 추가 (설정·자리표시 도안 에셋 생성).
> - 테스트 추가: `DrawingSessionTests`, `StrokeRasterizerTests`, 공용 `SimilarityCases`.
> - **주 점수 = 비정규화 + 평균거리** (2026-10-30 사용자 승인). 기준 비교표에서 정규화 점수는 일부만 그린 그림을 과하게 깎아 "절반 < 낙서"로 순서가 뒤집혔다. 정규화 점수는 결과 화면·기록에 함께 남긴다. → `JudgeResult.Similarity/Coverage/Precision`은 비정규화 값.
> - 씬 빌더 `DrawingTestSceneBuilder` 추가 (`Tools/SpellDrawing/Build Test Scene`): 씬·`HistoryEntry` 프리팹·`RecordSettings` 에셋을 코드로 만든다. Debug 컴포넌트를 다뤄야 해서 Editor 어셈블리 대신 **Debug 어셈블리에 `#if UNITY_EDITOR`** 로 둔다 (`TemplateAssetSaver`와 같은 방식, 어셈블리 의존 방향은 지침 그대로).
> - 캔버스 스케일러는 `Expand` 모드: 게임 뷰 비율과 상관없이 1920×1080 배치가 모두 보인다.
> - "리스트 지우기"는 기록 리스트와 세션 요약을 함께 지운다 (파일은 남김).
> 기준 문서: 프로젝트 루트 `CLAUDE.md`, `DRAWING_PROTOTYPE_SPEC.md`

## 0. 확인한 환경 (읽기만 함)
| 항목 | 값 |
|---|---|
| 프로젝트 | `C:\Unity_Project\NCAI\Mahojin_Prototype` (지침의 `STT_Prototype`과 이름이 다름) |
| Unity / URP | 6000.0.38f1 / 17.0.3 |
| 입력 | Input System 1.13.0, Active Input Handling = Both |
| UI 패키지 | `com.unity.ugui` 2.0.0 (설치되어 있음, 추가 설치 없음) |
| 테스트 | `com.unity.test-framework` 1.4.6 |
| git | `spelldrawing-prototype` 브랜치 (main에서 분기) |
| VoiceSTT | 아직 폴더 없음. 참조하지 않음 |

**새 패키지 설치나 프로젝트 설정 변경은 필요 없게 설계했다.**

---

## 1. 좌표와 용어
- **그리기 공간(Drawing space)**: 그리기 영역을 `(0,0)` 왼쪽 아래 ~ `(1,1)` 오른쪽 위로 본 좌표. 화면 해상도와 상관없이 같은 값이 된다. 원본 도안도 이 좌표로 저장한다.
- **획(Stroke)**: 마우스를 누르고 뗄 때까지 이어진 점들.
- **리샘플링(Resampling)**: 선을 따라 같은 간격으로 점을 다시 뽑는 것. 빨리 그린 곳과 천천히 그린 곳의 점 밀도 차이를 없앤다.
- **정규화(Normalization)**: 위치와 크기를 맞춰서 "어디에, 얼마나 크게 그렸나"를 지우고 모양만 남기는 것.
- **챔퍼 거리(Chamfer distance)**: 한 점 집합의 각 점에서 다른 집합의 가장 가까운 곳까지 거리를 재서 평균 낸 값. 두 모양이 얼마나 겹치는지 나타낸다.
- **허용 거리(Tolerance)**: "이 정도 벗어나면 0점"인 거리. 그리기 영역 한 변을 1로 본 비율 (기본 0.08 = 영역 폭의 8%).
- **컴포지션 루트(Composition root)**: 객체들을 만들고 서로 연결하는 단 한 곳. 여기서는 `SpellDrawingBootstrap`.

---

## 2. 클래스 목록

### Runtime (`VoxelMagic.SpellDrawing`) — 게임으로 옮길 핵심
| 이름 | 종류 | 역할 |
|---|---|---|
| `StrokeSet` | 클래스 (불변) | 여러 획의 점 목록 + 획 수·점 수·전체 길이 |
| `StrokeRecorder` | 클래스 | 입력 점을 받아 일정 간격(4px)마다 기록, 영역 밖 시작 무시·영역 밖 점 잘라내기 → `StrokeSet` 생성 |
| `StrokeResampler` | 정적 클래스 | 모든 획을 합친 길이를 따라 등간격 점 N개로 리샘플링 (획 구분 유지) |
| `ShapeNormalizer` | 정적 클래스 | 무게중심을 원점으로, 가장 긴 변을 1로 맞춤 (비율 유지) |
| `ISimilarityMetric` | 인터페이스 | 두 점 집합 → `SimilarityScore` (교체 가능한 유사도 계산) |
| `ChamferSimilarityMetric` | 클래스 | 양방향 챔퍼 거리 → 커버리지·정밀도·유사도 |
| `SimilarityScore` | 구조체 | 유사도, 커버리지, 정밀도, 양방향 평균 거리 |
| `SpellJudge` | 클래스 | 리샘플링 → 정규화/비정규화 두 번 비교 → `JudgeResult`, 계산 시간 측정 |
| `JudgeResult` | 클래스 (불변) | 정규화 점수, 비정규화 점수, 계산 ms, 빈 그림 여부, 비교에 쓴 점 목록 |
| `DrawingSession` | 클래스 | 상태 기계(대기→카운트다운→그리기→판정→결과), 남은 시간, 이벤트 발행 |
| `AttemptRecord` | 클래스 (불변) | 시도 하나의 모든 정보 (번호, 시각, 도안 ID, 시간, 획, 판정 결과) |
| `SessionStats` | 클래스 | 시도 수, 평균·최고·최저 유사도 (이벤트로 갱신 알림) |
| `IAttemptResultHandler` | 인터페이스 | 시도가 끝났을 때 처리 (이번: 파일 기록. 나중: 결과 분류, 이펙트) |
| `IStrokeInput` | 인터페이스 | 입력 장치 추상화: 누름·이동·뗌을 화면 좌표로 알림 |
| `MouseStrokeInput` | MonoBehaviour | Input System의 `Mouse.current`로 `IStrokeInput` 구현 |
| `StrokeRasterizer` | 클래스 | 획을 `Color32[]` 픽셀 버퍼에 굵은 선으로 그림 (썸네일·PNG 공용, Unity 객체 없이 테스트 가능) |
| `SpellTemplate` | ScriptableObject | 도안: ID, 표시 이름, 획별 점 목록(그리기 공간), 가이드 색 |
| `SpellTemplateLibrary` | ScriptableObject | 도안 목록 (1단계는 1개, 3단계 대비) |
| `PlaceholderTemplateFactory` | 정적 클래스 | "원 안에 삼각형" 자리표시 도안 점 목록을 코드로 생성 |
| `DrawingSessionSettings` | ScriptableObject | 제한 시간 8초, 카운트다운 사용·초, 가이드 투명도, 점 기록 간격(px) |
| `JudgeSettings` | ScriptableObject | 리샘플링 점 개수 64, 허용 거리 0.08, 커버리지/정밀도 판정 표시 기준 |

### Debug (`VoxelMagic.SpellDrawing.Debug`) — 테스트 화면·기록
| 이름 | 종류 | 역할 |
|---|---|---|
| `SpellDrawingBootstrap` | MonoBehaviour | 컴포지션 루트: 설정 복제, 객체 생성·연결, 매 프레임 `Tick`, Space/Esc 처리 |
| `DrawingAreaView` | MonoBehaviour | 화면 좌표 → 그리기 공간 변환, 입력을 세션에 전달, 가이드·그린 선 표시 |
| `UIPolylineGraphic` | MonoBehaviour (`MaskableGraphic` 상속) | uGUI에서 굵은 선(획 목록)을 메시로 그림 |
| `TimerView` | MonoBehaviour | 카운트다운 숫자, 남은 시간 숫자 + 줄어드는 바 |
| `ResultPanelView` | MonoBehaviour | 유사도·세부 값·측정값·겹쳐 보기·세션 요약·설정 슬라이더·버튼 |
| `HistoryListView` | MonoBehaviour | 스크롤 기록 리스트, 최신이 맨 위, "모두 접기" |
| `HistoryEntryView` | MonoBehaviour | 토글 하나: 접힘(한 줄 요약) / 펼침(썸네일 3장 + 값) |
| `ThumbnailCache` | 클래스 | 썸네일 텍스처를 펼칠 때만 만들고, 개수 상한(LRU)을 넘으면 파괴 |
| `OverlayImageBuilder` | 클래스 | `StrokeRasterizer`로 원본/그린 것/겹쳐 보기 픽셀 생성 (색 규칙 담당) |
| `IAttemptRecorder` | 인터페이스 | 시도 하나를 저장하는 한 가지 형식 |
| `FileAttemptRecorder` | 추상 클래스 | 공통 흐름: 켜짐 확인 → 폴더 준비 → `Write` → 예외 시 경고 로그 |
| `CsvAttemptRecorder` | 클래스 | `attempts.csv` (UTF-8 BOM) 한 줄 추가 |
| `JsonlAttemptRecorder` | 클래스 | `attempts.jsonl` 한 줄 추가 |
| `StrokePointsRecorder` | 클래스 | `points/attempt_001.json` 그린 점 목록 |
| `ImageAttemptRecorder` | 클래스 | `images/attempt_001_{template,drawn,overlay}.png` |
| `AttemptFileLogger` | 클래스 | `IAttemptResultHandler` 구현: 등록된 레코더들을 차례로 실행 |
| `AttemptFileNames` | 정적 클래스 | 시도 번호 → 파일 이름 규칙 (CSV·JSONL·PNG가 같은 이름을 쓰도록) |
| `RecordSettings` | ScriptableObject | 기록 켜기/끄기, 형식별 켜기, 썸네일 크기 256, 선 굵기·색, 썸네일 캐시 상한 |
| `KoreanFontApplier` | MonoBehaviour | 실행 시 OS의 "맑은 고딕"으로 동적 폰트를 만들어 캔버스의 모든 `Text`에 적용 |
| `TemplateAssetSaver` | 정적 클래스 (`#if UNITY_EDITOR`) | "방금 그린 것을 도안으로 저장" → `Data/Templates/`에 새 `SpellTemplate` 에셋 |

### Editor (`VoxelMagic.SpellDrawing.Editor`)
| 이름 | 종류 | 역할 |
|---|---|---|
| `SpellTemplateEditor` | Editor (커스텀 인스펙터) | 도안 점 목록을 인스펙터에 그림으로 미리보기 |
| `SpellDrawingMenu` | 정적 클래스 | `Tools/SpellDrawing/Create Placeholder Template`, `Export Package` |

### Tests (`VoxelMagic.SpellDrawing.Tests`, EditMode)
| 이름 | 역할 |
|---|---|
| `StrokeRecorderTests` | 간격 기록, 영역 밖 시작 무시, 경계 잘라내기 |
| `StrokeResamplerTests` | 항상 N개, 등간격, 점 하나짜리 획·빈 입력 |
| `ShapeNormalizerTests` | 이동·크기 변경 후 같은 결과 |
| `ChamferSimilarityMetricTests` | 4번(지침)의 모든 경우 + 계산 시간 |
| `SimilarityBaselineTests` | 기준 비교표 생성 (콘솔 + `TESTING.md`에 붙일 표 출력) |
| `TestShapes` | 테스트용 모양 생성 (흔들림, 절반, 직선, 낙서, 거꾸로, 획 나누기) |

> 클래스 수가 많아 보이지만 대부분 30~100줄짜리 작은 클래스다. 묶어도 되는 것: `OverlayImageBuilder`를 `ImageAttemptRecorder`에 합치기, `AttemptFileNames`를 `FileAttemptRecorder`에 합치기. 원하면 줄인다.

---

## 3. 상속·구현 구조도
```
ISimilarityMetric  «interface»
└─ ChamferSimilarityMetric           양방향 챔퍼 거리(점→선분) → 0~1 유사도

IStrokeInput  «interface»
└─ MouseStrokeInput  (MonoBehaviour) 마우스 왼쪽 버튼으로 획 입력
   (나중) PenStrokeInput, TouchStrokeInput

IAttemptResultHandler  «interface»
└─ AttemptFileLogger                 파일 기록 (이번)
   (나중) SpellOutcomeHandler, SpellEffectHandler

IAttemptRecorder  «interface»
└─ FileAttemptRecorder  «abstract»   폴더 준비·켜짐 확인·예외 처리 공통 흐름
   ├─ CsvAttemptRecorder
   ├─ JsonlAttemptRecorder
   ├─ StrokePointsRecorder
   └─ ImageAttemptRecorder           원본·그린 것·겹친 것 PNG

MaskableGraphic  «Unity»
└─ UIPolylineGraphic                 uGUI 굵은 선 메시

ScriptableObject  «Unity»
├─ SpellTemplate / SpellTemplateLibrary
├─ DrawingSessionSettings / JudgeSettings      (Runtime)
└─ RecordSettings                               (Debug)
```
상속 깊이는 모두 2단계 이하.

---

## 4. 의존 관계도
```
SpellDrawingBootstrap  (만드는 쪽 — 모든 조립은 여기서만)
 ├─ 설정 SO들을 Instantiate로 복제 (슬라이더가 원본 에셋을 덮어쓰지 않게)
 ├─ new ChamferSimilarityMetric()
 ├─ new SpellJudge(metric, judgeSettings)
 ├─ new SessionStats()
 ├─ new AttemptFileLogger(recordSettings, [Csv, Jsonl, Points, Image 레코더])
 ├─ new DrawingSession(sessionSettings, judge, template, handlers: [fileLogger])
 └─ View들에 Bind(session, stats, ...) 호출
     ├─ DrawingAreaView.Bind(session, IStrokeInput=MouseStrokeInput, template)
     ├─ TimerView.Bind(session)
     ├─ ResultPanelView.Bind(session, stats, settings 복제본)
     └─ HistoryListView.Bind(session, thumbnailCache)

이벤트 구독 (→ 는 "구독한다")
 DrawingAreaView  → IStrokeInput.StrokeBegan / StrokeMoved / StrokeEnded
                     (변환 후 session.BeginStroke / AddPoint / EndStroke 호출)
 DrawingAreaView  → session.StrokesChanged, StateChanged   (선 다시 그리기, 새 시도 때 지우기)
 TimerView        → session.StateChanged, TimeChanged
 ResultPanelView  → session.AttemptCompleted, stats.Changed
 HistoryListView  → session.AttemptCompleted
 SessionStats     ← Bootstrap이 session.AttemptCompleted에 연결

DrawingSession 내부 (호출)
 StrokeRecorder → (시간 종료) → SpellJudge → AttemptRecord 생성
   → handlers[i].Handle(record) → AttemptCompleted 이벤트
```
- UI는 이벤트 구독과 공개 메서드 호출만 한다. 세션의 상태를 직접 바꾸지 않는다.
- Runtime은 Debug를 모른다. Runtime → Debug 방향 의존 없음.

---

## 5. 씬 구성 (`Scenes/DrawingTest.unity`)
```
DrawingTest
├─ Main Camera              Camera (단색 배경, UI만 보임)
├─ EventSystem              EventSystem, InputSystemUIInputModule
├─ SpellDrawingSystem       SpellDrawingBootstrap, MouseStrokeInput
└─ Canvas                   Canvas(Screen Space Overlay), CanvasScaler(1920×1080), KoreanFontApplier
   ├─ TopBar                  템플릿 이름 Text, [시작] Button, [방금 그린 것을 도안으로 저장] Button(에디터 전용)
   ├─ LeftColumn
   │  ├─ TimerPanel           TimerView — 남은 시간 Text(크게), 바 Image(Filled)
   │  └─ ResultPanel          ResultPanelView — 유사도 Text+바, 세부/측정 Text, 겹쳐 보기 RawImage,
   │                           세션 요약 Text, Slider×3(제한 시간·허용 거리·가이드 투명도),
   │                           [다시 하기][로그 폴더 열기][리스트 지우기] Button
   ├─ DrawingArea (가운데 640×640 정사각형)
   │                          DrawingAreaView, Image(배경), RectMask2D
   │  ├─ GuideLines           UIPolylineGraphic (반투명 원본)
   │  ├─ DrawnLines           UIPolylineGraphic (실시간 그린 선)
   │  └─ CountdownText        Text (3, 2, 1)
   ├─ HistoryPanel (오른쪽)   HistoryListView, ScrollRect
   │  ├─ Header               "기록" Text, [모두 접기] Button
   │  └─ Viewport/Content     VerticalLayoutGroup, ContentSizeFitter
   └─ ControlsHint (왼쪽 아래) Text — 조작 안내 표 (항상 표시)

Prefab: Debug/Prefabs/HistoryEntry.prefab
  HistoryEntry                HistoryEntryView, VerticalLayoutGroup
  ├─ Header (Toggle)          유사도 색 막대 Image, "#3  14:05:12  유사도 0.82" Text
  └─ Details (펼칠 때만 활성)  RawImage×3(원본·그린 것·겹쳐 보기), 값 Text
```

---

## 6. 동작 흐름
```
[대기] ──시작 버튼/Space──▶ [카운트다운 3,2,1] ──▶ [그리기: 제한 시간 8초]
  ▲                        (설정에서 끄면 바로 그리기)     │  마우스 누름 → 영역 안이면 획 시작
  │                                                       │  이동 → 4px마다 점 기록 (영역 밖은 경계로 잘라냄)
  │                                                       │  뗌 → 획 끝
  │   Esc: 카운트다운·그리기 중 취소 → [대기] (기록 안 함)   ▼
  │                                             시간 종료 → 입력 막기 (누르던 획은 그 자리에서 끝냄)
  │                                                       ▼
  │                                   [판정] (같은 프레임, 수 ms)
  │                                     1. 획 없음? → empty=true, 유사도 0
  │                                     2. 리샘플링: 그린 것·원본 각각 N=64점
  │                                     3-a. 비정규화: 그리기 공간 그대로 비교 → similarityRaw
  │                                     3-b. 정규화: 둘 다 정규화 후 비교 → similarity
  │                                     4. 계산 시간(ms) 기록
  │                                                       ▼
  │                                   AttemptRecord 생성 → 핸들러 실행(파일 저장)
  │                                                       ▼
  └──────────── 다시 하기/Space ───────── [결과] AttemptCompleted 이벤트
                                           → 결과 UI 표시, 세션 요약 갱신,
                                             리스트 맨 위에 토글 추가(접힌 상태)
```
- 판정은 메인 스레드에서 동기 실행한다. 64점 기준 1ms 안팎이라 스레드를 쓸 이유가 없다.
- 결과 화면의 "주요 점수"는 우선 **정규화 점수**로 두고, 비정규화 점수를 옆에 같이 표시한다. 사람 테스트 후 어느 쪽을 주 점수로 할지 정한다.

---

## 7. 유사도 계산 설명

### 7-1. 리샘플링: 왜, 어떻게
천천히 그린 곳은 점이 빽빽하고 빨리 그린 곳은 듬성하다. 그대로 비교하면 천천히 그린 곳만 점수에 크게 반영된다.
```
원래 점:   ●●●●●●●●     ●       ●       ●      (앞은 천천히, 뒤는 빠르게)
리샘플링:  ●    ●    ●    ●    ●    ●    ●     (선 길이를 따라 같은 간격)
```
- 모든 획의 길이를 더한 뒤(획 사이 빈 공간은 길이에 넣지 않음) 같은 간격으로 N개를 뽑는다. 긴 획엔 점이 많이, 짧은 획엔 적게 간다.
- 각 점이 어느 획에 속하는지 기억해 둔다 → 아래의 "점→선분 거리"에 쓴다.

### 7-2. 정규화
```
작게, 왼쪽 위에 그림            정규화 후
┌──────────┐                 ┌──────────┐
│ ○        │                 │   ◯     │   무게중심 → 원점
│          │       ──▶       │          │   가장 긴 변 → 1
│          │                 │          │   (가로세로 비율은 유지)
└──────────┘                 └──────────┘
```
가이드를 따라 그리는 1단계에서는 위치·크기가 이미 맞기 때문에 **비정규화 점수**도 의미가 있다. 둘을 같이 보여 주고 비교한다.
- 비정규화가 유리한 경우: 가이드와 다른 곳에 작게 그리면 점수가 낮아야 할 때
- 정규화가 유리한 경우: 3단계(가이드 없이 그리기) — 위치·크기는 사람마다 다르다

### 7-3. 양방향 챔퍼 거리
```
원본(○) 과 내가 그린 것(●)

  커버리지 (원본 → 나)                 정밀도 (나 → 원본)
  "원본의 각 부분을 내가 그렸나?"       "내가 그린 각 부분이 원본 위에 있나?"

   ○─○─○─○─○                           ○─○─○─○─○
   │ │ │                                         ●   ← 삐져나온 점: 멀다
   ●─●─●       ○ ○  ← 빠뜨린 부분:      ●─●─●
                      가까운 내 점이 멀다
```
1. **커버리지 거리** `dCov` = 원본의 각 점에서 내 선까지 가장 가까운 거리의 평균
2. **정밀도 거리** `dPre` = 내 각 점에서 원본 선까지 가장 가까운 거리의 평균
3. 점수로 바꾸기: `coverage = clamp01(1 - dCov / 허용거리)`, `precision = clamp01(1 - dPre / 허용거리)`
4. 합치기: **조화 평균** `similarity = 2·c·p / (c + p)` (둘 중 하나라도 0이면 0)

**"점→점"이 아니라 "점→선분" 거리를 쓴다.** 점→점으로 재면 똑같이 그려도 점의 위치가 반 칸 어긋나 있으면 거리가 0이 안 된다. 원 안 삼각형(전체 길이 약 4.6)을 64점으로 뽑으면 점 간격이 약 0.07이라, 완벽하게 그려도 평균 0.02쯤 오차가 생겨 0.75점 근처가 나온다. 점→선분은 이 오차가 없어 완벽한 그림이 1.0에 가깝게 나온다. 계산량은 64×64 = 4천 번 정도로 1ms 미만이다.

순서·방향·획 수와 상관없는 이유: 두 집합의 "가장 가까운 거리"만 보기 때문에 어떤 순서로 그었는지 정보가 쓰이지 않는다.

### 7-4. 숫자 예시 (허용 거리 0.08 = 640px 영역에서 약 51px)
| 경우 | dCov | dPre | 커버리지 | 정밀도 | 유사도(조화) | 산술평균이었다면 |
|---|---|---|---|---|---|---|
| 꼼꼼히 따라 그림 | 0.010 | 0.010 | 0.88 | 0.88 | **0.88** | 0.88 |
| 약간 삐뚤 | 0.024 | 0.020 | 0.70 | 0.75 | **0.72** | 0.73 |
| 절반만 그림 | 0.060 | 0.010 | 0.25 | 0.88 | **0.39** | 0.56 |
| 영역 전체에 낙서 | 0.010 | 0.070 | 0.88 | 0.13 | **0.22** | 0.50 |
| 가운데 직선 하나 | 0.150 | 0.090 | 0.00 | 0.00 | **0.00** | 0.00 |
(값은 설명용 추정치. 실제 값은 기준 비교표로 확인한다.)

산술평균이면 "절반만 그림"과 "낙서"가 0.5 근처라 너무 후하다. 조화 평균은 한쪽이 나쁘면 확실히 내려가서 사람 감각과 더 맞을 것으로 본다.

### 7-5. 겹쳐 보기 색 규칙
| 색 | 의미 |
|---|---|
| 회색 | 원본 중 내가 그린 부분 (허용 거리 절반 이내) |
| **빨강** | 원본 중 빠뜨린 부분 (커버리지 감점 원인) |
| 초록 | 내가 그린 선 중 원본 위에 있는 부분 |
| **주황** | 내가 그린 선 중 삐져나온 부분 (정밀도 감점 원인) |
겹쳐 보기는 가이드 좌표 그대로(비정규화) 그린다 — 화면에서 본 그대로가 납득하기 쉽다.

---

## 8. 기록 리스트와 파일 저장

### 리스트 성능
- 접힌 토글은 Text 몇 개뿐이라 수백 개도 문제없다.
- 썸네일 텍스처는 **펼칠 때 만든다.** `ThumbnailCache`가 최대 개수(기본 30장 = 10개 시도분)를 넘으면 오래 안 본 것부터 `Destroy`한다. 접는다고 바로 지우지는 않는다 (다시 펼칠 때 빠르게).
- 텍스처 원본 데이터는 `AttemptRecord`의 점 목록이라, 지워도 언제든 다시 만들 수 있다.
- "리스트 지우기"는 토글과 텍스처를 모두 정리한다 (파일은 지우지 않음).

### 이전 기록 불러오기 (제안)
세션 폴더가 플레이할 때마다 새로 생기므로 "같은 세션 폴더"는 다시 열리지 않는다. **1단계에서는 불러오지 않고**, 필요해지면 `JsonlAttemptLoader`(JSONL + 점 JSON → `AttemptRecord`)를 추가해 "이전 세션 불러오기" 버튼을 다는 방식을 제안한다.

### 폴더 구조
```
{persistentDataPath}/DrawingLogs/2026-10-30_14-05-12/
├─ attempts.csv             UTF-8 BOM
├─ attempts.jsonl
├─ points/attempt_001.json  획별 점 목록 (그리기 공간 좌표)
└─ images/attempt_001_template.png, _drawn.png, _overlay.png
```
기록 항목은 지침 3-6의 목록 그대로 + `coverageRaw`, `precisionRaw`.
"로그 폴더 열기"는 `Application.OpenURL("file:///...")`로 탐색기를 연다.

---

## 9. 확장 지점
| 단계 | 새로 만들 것 | 고칠 곳 |
|---|---|---|
| **2단계 결과 분류** | `SpellOutcome` enum(강한 성공/성공/엉뚱한 마법/불발), `IOutcomeClassifier` + `ThresholdOutcomeClassifier`, `OutcomeThresholds` SO | `AttemptRecord`에 `Outcome` 필드, Bootstrap에서 분류기 주입, 결과 UI·리스트·CSV에 열 추가. 판정 로직은 그대로 |
| **3단계 여러 도안·인식** | `ISpellRecognizer` + `BestMatchRecognizer`(라이브러리의 모든 도안과 정규화 점수 비교 → 순위), 필요하면 회전 보정 | `SpellTemplateLibrary`에 도안 추가, `DrawingSessionSettings.showGuide=false` 모드, `DrawingSession`이 고정 도안 대신 인식기 결과를 쓰도록 생성자 분기 |
| **4단계 월드 이펙트** | `SpellEffectHandler : IAttemptResultHandler`(그린 `StrokeSet`을 월드 평면에 매핑해 LineRenderer/VFX로 발광), `SpellEffectSettings` SO | Bootstrap(또는 게임 쪽 조립 코드)에서 핸들러 등록만. 그린 원본 획이 `AttemptRecord`에 그대로 남아 있어 코어 수정 없음 |
| 입력 장치 추가 | `PenStrokeInput`, `TouchStrokeInput` (`IStrokeInput`) | Bootstrap에서 어떤 입력을 쓸지 선택만 |
| 다른 유사도 | 예: `HitRatioSimilarityMetric`, `HausdorffSimilarityMetric` (`ISimilarityMetric`) | Bootstrap에서 교체. 결과 형식(`SimilarityScore`)은 같음 |

게임에 붙일 때 필요한 최소 코드는 Runtime만: `DrawingSession` 생성 → 입력 연결 → `AttemptCompleted` 구독. (SETUP.md에 예시 코드)

---

## 10. 고민한 선택지와 고른 이유
| 주제 | 선택지 | 고른 것 / 이유 |
|---|---|---|
| 거리 재는 방식 | 점→점 / **점→선분** | 점→선분. 리샘플링 간격 때문에 생기는 억울한 감점이 없다 (7-3) |
| 커버리지·정밀도 합치기 | 산술평균 / 최솟값 / **조화 평균** | 조화 평균. 한쪽만 좋은 경우(낙서, 절반)를 확실히 깎고, 최솟값보다는 부드럽다 |
| 거리 → 점수 | **평균 거리 ÷ 허용 거리 (지침)** / 허용 거리 안 점 비율 | 지침대로 평균 거리. 대신 `ISimilarityMetric`으로 교체 가능하게 두고, 사람 테스트 결과가 이상하면 비율 방식을 추가 비교 |
| 좌표 기준 | 화면 픽셀 / **그리기 공간 0~1** | 0~1. 해상도·영역 크기가 바뀌어도 허용 거리와 도안이 그대로 통한다 |
| UI 시스템 | **uGUI(Text)** / TextMeshPro / UI Toolkit | uGUI 기본 Text. TMP는 `Assets/TextMesh Pro`(모듈 밖)에 기본 리소스 가져오기가 필요하고, 맑은 고딕 파일을 프로젝트에 복사하면 폰트 라이선스 문제가 있다. 기본 Text는 `Font.CreateDynamicFontFromOSFont("Malgun Gothic")`로 OS 폰트를 바로 쓴다. 단점: 에디터에서 플레이 전에는 한글이 네모로 보일 수 있음 (플레이 중엔 정상) |
| 선 그리기 | LineRenderer / 텍스처에 칠하기 / **uGUI 메시(`UIPolylineGraphic`)** | uGUI 메시. 캔버스 안에서 마스크·정렬이 자연스럽고, 획이 늘어도 버텍스만 추가된다 |
| 썸네일 | 매번 새로 / 전부 유지 / **펼칠 때 생성 + LRU 상한** | 메모리 상한이 있어 리스트가 길어져도 안전 |
| 상태 관리 | 코루틴 / **일반 클래스 + `Tick(dt)`** | 테스트에서 시간을 직접 흘려 검증할 수 있다 |
| 슬라이더 설정 | 원본 에셋 수정 / **실행 시 복제본 수정** | 플레이 중 조절이 에셋 파일을 바꾸지 않는다. 쓴 값은 기록(`tolerance`, `timeLimitSec`)에 남는다. 좋은 값을 찾으면 에셋에 직접 반영 |
| 파일 저장 | 비동기 / **동기** | 시도당 PNG 3장(256px)·텍스트 몇 줄이라 수 ms. 문제가 되면 `FileAttemptRecorder`만 비동기로 바꾸면 된다 |
| 도안 저장 버튼 | 에디터 어셈블리 / **Debug 안 `#if UNITY_EDITOR`** | Debug는 Editor 어셈블리를 참조할 수 없어서. 빌드에서는 버튼이 숨겨진다 |

---

## 11. 구현 순서 (승인 후, 단계마다 보고)
1. 폴더·asmdef 4개, 설정 SO 클래스와 에셋, SETUP.md·CHANGELOG.md·TESTING.md 뼈대
2. Runtime 계산 로직 + 단위 테스트 + 기준 비교표 → **보고**
3. 씬, 그리기 영역, 가이드, 제한 시간 → 플레이 모드 확인 후 **보고**
4. 결과 UI → **보고**
5. 기록 리스트(토글) + 파일 저장 → **보고**
6. 콘솔 에러 0, 테스트 통과, 씬 저장, 커밋

## 12. 확인받고 싶은 것
1. 커버리지·정밀도를 **조화 평균**으로 합치는 것
2. 허용 거리 기본값 **0.08**(영역 폭의 8%), 리샘플링 **64점**
3. 결과의 주 점수를 일단 **정규화 점수**로 표시하는 것
4. UI를 **uGUI 기본 Text + OS 맑은 고딕**으로 만드는 것 (TMP 미사용)
5. 이전 세션 기록 불러오기는 **1단계에서 제외**
6. `HistoryEntry` 프리팹을 `Debug/Prefabs/`에 두는 것 (지침 폴더 구조에 없는 폴더 추가)
