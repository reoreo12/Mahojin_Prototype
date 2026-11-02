# SpellDrawing 설정·이식 안내

## 환경
| 항목 | 값 |
|---|---|
| Unity | 6000.0.38f1 |
| 렌더 파이프라인 | URP 17.0.3 |
| 입력 | Input System 1.13.0 (Active Input Handling = Both) |
| UI | `com.unity.ugui` 2.0.0 (기본 Text 사용, TextMeshPro 미사용) |
| 테스트 | `com.unity.test-framework` 1.4.6 |

## 추가로 설치한 패키지
| 패키지 | 버전 | 용도 | 게임 이식 시 |
|---|---|---|---|
| `com.coplaydev.unity-mcp` (MCP for Unity) | `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#v10.3.0` | Claude Code가 Unity를 다루는 개발 도구 | **필요 없음** (모듈 코드는 참조하지 않음) |

## 프로젝트 설정 변경
없음. (변경하면 여기에 변경 전후 값을 적는다)

## 어셈블리
| asmdef | 폴더 | 참조 |
|---|---|---|
| `VoxelMagic.SpellDrawing` | `Runtime/` | Unity.InputSystem |
| `VoxelMagic.SpellDrawing.Debug` | `Debug/` | Runtime, Unity.InputSystem, UnityEngine.UI |
| `VoxelMagic.SpellDrawing.Editor` | `Editor/` | Runtime (에디터 전용) |
| `VoxelMagic.SpellDrawing.Tests` | `Tests/` | Runtime, Test Runner (에디터 전용) |

네임스페이스: Runtime `VoxelMagic.SpellDrawing`, Debug `VoxelMagic.SpellDrawing.DebugUI`, Editor `VoxelMagic.SpellDrawing.EditorTools`.
(`...Debug`, `...Editor`로 하면 `UnityEngine.Debug`, `UnityEditor.Editor`와 이름이 겹쳐서 바꿨다.)

## 처음 설정
1. `Tools/SpellDrawing/Create Default Assets` 실행 → `Data/Templates/`, `Data/Settings/`에 기본 에셋이 생긴다. (이미 있으면 그대로 둔다)
2. `Tools/SpellDrawing/Build Test Scene` 실행 → `Scenes/DrawingTest.unity`, `Debug/Prefabs/HistoryEntry.prefab`, `Data/Settings/RecordSettings.asset`을 만든다. 씬은 매번 새로 만들어 덮어쓰고, 설정 에셋은 있으면 그대로 둔다.

## 설정 에셋
| 에셋 | 주요 값 |
|---|---|
| `DrawingSessionSettings` | 제한 시간 8초, 카운트다운 3초(끌 수 있음), 가이드 투명도 0.35, 점 기록 간격 4px |
| `JudgeSettings` | 리샘플링 64점, 허용 거리 0.08(영역 폭의 8%), 점수 방식 MeanThenScale |
| `RecordSettings` | 기록 켜기/형식별 켜기, 폴더 이름 `DrawingLogs`, 썸네일 256px·선 3px, 캐시 10시도, 겹쳐 보기·점수 색 |
슬라이더(제한 시간·허용 거리·가이드 투명도)는 실행 중 복제본만 바꾼다. 좋은 값을 찾으면 에셋에 직접 적는다.

## 기록 파일
`{persistentDataPath}/DrawingLogs/{세션 시작 시각}/` — 이 PC에서는 `C:\Users\reo36\AppData\LocalLow\DefaultCompany\Mahojin_Prototype\DrawingLogs\`
- `attempts.csv`(UTF-8 BOM), `attempts.jsonl`, `points/attempt_001.json`, `images/attempt_001_{template,drawn,overlay}.png`
- 항목: `similarity·coverage·precision` = 주 점수(비정규화), `similarityRaw` = 비정규화(주 점수와 같음), `similarityNorm·coverageNorm·precisionNorm` = 정규화 점수. 그 밖에 지침 3-6 항목 그대로.

## 이식 절차
1. 같은 Unity 버전·URP의 게임 프로젝트에 Input System, uGUI 패키지가 있는지 확인한다.
2. `Assets/SpellDrawing/` 폴더를 통째로 복사하거나 `SpellDrawing_v{버전}.unitypackage`를 가져온다.
3. 아래 체크리스트를 확인한다.

### 이식 체크리스트
- [ ] 같은 Unity 버전·URP의 게임 프로젝트에 `Assets/SpellDrawing/`를 가져옴
- [ ] 콘솔 에러 0개
- [ ] DrawingTest 씬에서 그리기 → 유사도 → 리스트 → 파일 저장 확인

## 게임에 붙이는 최소 예시
```csharp
// Runtime 어셈블리만 참조하면 된다.
var judge = new SpellJudge(new ChamferSimilarityMetric(judgeSettings.DistanceScoring));
var session = new DrawingSession(sessionSettings, judgeSettings, judge, captureTemplate);
session.AttemptCompleted += record => Debug.Log($"유사도 {record.Similarity:F2}");

// 입력: 화면 좌표를 그리기 공간(0~1)으로 바꿔 넘긴다.
session.BeginStroke(areaPoint); session.AddPoint(areaPoint); session.EndStroke(areaPoint);
// 매 프레임
session.Tick(Time.deltaTime);
```

## 패키지 내보내기
`Tools/SpellDrawing/Export Package` → 프로젝트 폴더에 `SpellDrawing_v0.1.0.unitypackage`.
