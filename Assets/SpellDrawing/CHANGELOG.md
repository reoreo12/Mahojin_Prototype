# SpellDrawing 변경 기록

## 0.1.0 (진행 중) — 1단계
### 2026-11-02
- 주 점수를 비정규화 + 평균거리로 결정 (사용자 승인), 테스트 정리 → EditMode 34개 통과
- Debug: 그리기 영역·선 그래픽·타이머·결과 패널·기록 리스트(토글)·썸네일 캐시·겹쳐 보기 이미지·한글 OS 폰트
- 기록: CSV(UTF-8 BOM)·JSONL·점 JSON·PNG 3장, `RecordSettings` 에셋
- 부트스트랩(`SpellDrawingBootstrap`), "방금 그린 것을 도안으로 저장"(에디터 전용)
- 씬 빌더 `Tools/SpellDrawing/Build Test Scene` → `Scenes/DrawingTest.unity`, `Debug/Prefabs/HistoryEntry.prefab`
- 플레이 모드 확인: 모의 시도 2회 → 판정·결과 UI·리스트 토글·파일 저장 동작, 한글 표시 정상, 콘솔 에러 0
- 프로젝트 설정 변경 없음

### 2026-10-30
- 설계 문서 `DESIGN.md` 작성, "제작 시작" 승인
- 폴더·asmdef 4개 생성 (Runtime / Debug / Editor / Tests)
- Runtime 계산 로직: 획 기록, 리샘플링, 정규화, 양방향 챔퍼 거리(점→선분), 판정, 세션 상태 기계, 썸네일 래스터라이저
- 설정 SO: `DrawingSessionSettings`, `JudgeSettings`, 도안 SO: `SpellTemplate`, `SpellTemplateLibrary`
- 에디터 메뉴: `Tools/SpellDrawing/Create Default Assets`, `Export Package`, 도안 미리보기 인스펙터
- 단위 테스트, 기준 비교표 테스트
- 설계 보완: `JudgeSettings.distanceScoring` 추가 (거리 → 점수 변환 두 방식 비교용, 기본값은 지침 방식)
- 패키지 추가 (사용자 승인): `com.coplaydev.unity-mcp` v10.3.0 — 개발 도구, 모듈 코드는 참조하지 않음
- 프로젝트 설정 변경 없음
