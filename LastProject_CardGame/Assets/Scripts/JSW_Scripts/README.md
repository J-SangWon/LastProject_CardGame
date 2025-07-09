# 카드 효과 시스템 사용법 (CardEffectData 기반)

## 📁 파일 구조

### 핵심 파일들
- `CardEffectManager.cs` - 효과 관리 및 실행 (메인 매니저)
- `CardData/EffectSystem.cs` - 효과 시스템 정의 (열거형, 클래스)
- `CardData/CardEffectDataEditor.cs` - CardEffectData 편집기
- `CardData/CardDataEditor.cs` - 카드 데이터 편집기
- `CardData/CardData.cs` - 카드 데이터 (BaseCardData + CardEffectData)

### 삭제된 중복 파일들
- ~~EffectSystemExample.cs~~ (중복)
- ~~CardEffectUsageExample.cs~~ (중복)
- ~~CardEffectLoader.cs~~ (CardEffectManager로 통합)
- ~~CardDataEffectTest.cs~~ (테스트용)
- ~~CardEffectData.cs~~ (사용하지 않음)
- ~~CardDataEffectEditor.cs~~ (CardEffectDataEditor로 대체)

## 🚀 사용법

### 1. 카드 효과 ScriptableObject 생성
1. **Project 창에서 우클릭** → **Create** → **CardGame** → **CardEffectData**
2. 생성된 CardEffectData를 선택하여 Inspector에서 효과 정보 입력:
   - **효과명**
   - **설명**
   - **발동 타이밍** (OnSummon, OnAttack 등)
   - **효과 타입**
   - **실행 메서드** (DealDamage, Heal 등)
   - **파라미터** (필요시)
   - **조건** (필요시)

### 2. 카드에 효과 추가하기
1. **카드 ScriptableObject 선택** (MonsterCardData, SpellCardData, TrapCardData)
2. Inspector에서 **"카드 효과"** 리스트에 생성한 CardEffectData를 **드래그&드롭**
3. 여러 효과를 추가하려면 리스트에 계속 추가

### 3. 게임에서 효과 실행하기
```csharp
// 효과 발동
CardEffectManager.Instance.TriggerEffects(EffectTrigger.OnSummon, cardGameObject);

// 특정 카드의 효과 정보 가져오기
var effects = CardEffectManager.Instance.GetCardEffects("카드이름");
```

### 4. 자동 로딩
- `CardEffectManager`의 `autoLoadOnStart = true`로 설정하면 게임 시작 시 자동으로 모든 카드 효과를 로드합니다.

## 📋 지원하는 효과 타이밍
- `OnSummon` - 소환 시
- `OnAttack` - 공격 시
- `OnDeath` - 사망 시
- `OnTurnStart` - 턴 시작 시
- `OnTurnEnd` - 턴 종료 시
- `Continuous` - 지속 효과

## 🔧 지원하는 효과 메서드
- `DealDamage` - 데미지 처리
- `Heal` - 회복
- `DrawCard` - 카드 드로우
- `DestroyCard` - 카드 파괴
- `ChangeAttack` - 공격력 변경
- `ChangeDefense` - 방어력 변경

## 📊 디버깅
- `CardEffectManager`에서 우클릭 → "효과 통계 출력"
- 특정 카드 효과 확인: `PrintCardEffects("카드이름")`

## 🎯 예시
```csharp
// 카드가 소환될 때
CardEffectManager.Instance.TriggerEffects(EffectTrigger.OnSummon, monsterCard);

// 카드가 공격할 때
CardEffectManager.Instance.TriggerEffects(EffectTrigger.OnAttack, monsterCard, targetCard);
```

## 💡 팁
- **CardEffectData는 재사용 가능**: 같은 효과를 여러 카드에서 사용할 수 있습니다.
- **효과 템플릿**: CardEffectDataEditor에서 "빠른 설정" 버튼으로 템플릿 효과를 빠르게 설정할 수 있습니다.
- **효과 참조**: 카드에서 효과를 제거하려면 Inspector에서 리스트에서 해당 효과를 삭제하면 됩니다. 