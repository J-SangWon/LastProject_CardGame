using System;
using System.Collections.Generic;
using UnityEngine;

// 유희왕식 효과 타이밍
public enum EffectTrigger
{
    // 소환 관련
    OnSummon,           // 소환 시
    OnSpecialSummon,    // 특수 소환 시
    OnNormalSummon,     // 일반 소환 시
    
    // 발동 관련
    OnActivate,         // 직접 발동 시
    OnQuickActivate,    // 퀵 발동 시
    
    // 지속 효과
    Continuous,         // 지속적으로 발동
    
    // 전투 관련
    OnBattleStart,      // 전투 시작 시
    OnAttack,           // 공격 시
    OnDefend,           // 방어 시
    OnBattleEnd,        // 전투 종료 시
    OnDestroy,          // 파괴 시
    
    // 턴 관련
    OnTurnStart,        // 턴 시작 시
    OnTurnEnd,          // 턴 종료 시
    OnOpponentTurnStart, // 상대 턴 시작 시
    OnOpponentTurnEnd,   // 상대 턴 종료 시
    
    // 특수 조건
    OnDeath,            // 사망 시
    OnDraw,             // 드로우 시
    OnDiscard,          // 버림 시
    OnField,            // 필드에 있을 때
    OnHand,             // 손에 있을 때
    OnGraveyard         // 묘지에 있을 때
}

// 효과 타입 (간단한 분류)
public enum EffectType
{
    Normal,             // 일반 효과
    Continuous,         // 지속 효과
    Trigger,            // 발동 효과
    Counter,            // 카운터 효과
    Custom              // 사용자 정의
}

// 효과 조건
[System.Serializable]
public class EffectCondition
{
    public enum ConditionType
    {
        None,           // 조건 없음
        ManaCost,       // 마나 코스트
        CardCount,      // 카드 수
        Health,         // 체력
        Attack,         // 공격력
        Race,           // 종족
        CardType,       // 카드 타입
        Custom          // 사용자 정의
    }
    
    public ConditionType conditionType;
    public string conditionValue;
    public string comparison; // ">", "<", "==", ">=", "<="
}

// 개별 효과 데이터
[System.Serializable]
public class CardEffect
{
    [Header("효과 기본 정보")]
    public string effectName;
    [TextArea(3, 6)]
    public string effectDescription;
    public EffectTrigger trigger;
    public EffectType effectType;
    
    [Header("효과 조건")]
    public EffectCondition condition;
    public bool hasCondition => condition.conditionType != EffectCondition.ConditionType.None;
    
    [Header("효과 실행")]
    public string effectMethodName; // 실행할 메서드 이름
    public List<string> effectParameters; // 효과 파라미터
    
    [Header("시각적 표현")]
    public Color effectColor = Color.white;
    public bool isHighlighted = false;
    
    [Header("효과 설정")]
    public bool isOncePerTurn = false; // 턴당 한 번만
    public bool isOncePerGame = false; // 게임당 한 번만
    public int activationCount = 0; // 발동 횟수 추적
}

// 효과 실행기
public class EffectExecutor : MonoBehaviour
{
    private static EffectExecutor instance;
    public static EffectExecutor Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("EffectExecutor");
                instance = go.AddComponent<EffectExecutor>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }
    
    // 효과 실행 메서드들
    public void ExecuteEffect(CardEffect effect, GameObject target, GameObject source)
    {
        if (effect == null) return;
        
        // 조건 확인
        if (effect.hasCondition && !CheckCondition(effect.condition, target, source))
            return;
        
        // 발동 제한 확인
        if (effect.isOncePerTurn && effect.activationCount > 0)
            return;
        if (effect.isOncePerGame && effect.activationCount > 0)
            return;
        
        // 효과 실행
        InvokeEffect(effect, target, source);
        
        // 발동 횟수 증가
        effect.activationCount++;
    }
    
    private bool CheckCondition(EffectCondition condition, GameObject target, GameObject source)
    {
        // 조건 확인 로직 구현
        switch (condition.conditionType)
        {
            case EffectCondition.ConditionType.None:
                return true;
            case EffectCondition.ConditionType.ManaCost:
                return CheckManaCondition(condition, target);
            case EffectCondition.ConditionType.CardCount:
                return CheckCardCountCondition(condition, target);
            case EffectCondition.ConditionType.Health:
                return CheckHealthCondition(condition, target);
            case EffectCondition.ConditionType.Attack:
                return CheckAttackCondition(condition, target);
            default:
                return true;
        }
    }
    
    private bool CheckManaCondition(EffectCondition condition, GameObject target)
    {
        // 마나 코스트 조건 확인 로직
        return true; // 임시 구현
    }
    
    private bool CheckCardCountCondition(EffectCondition condition, GameObject target)
    {
        // 카드 수 조건 확인 로직
        return true; // 임시 구현
    }
    
    private bool CheckHealthCondition(EffectCondition condition, GameObject target)
    {
        // 체력 조건 확인 로직
        return true; // 임시 구현
    }
    
    private bool CheckAttackCondition(EffectCondition condition, GameObject target)
    {
        // 공격력 조건 확인 로직
        return true; // 임시 구현
    }
    
    private void InvokeEffect(CardEffect effect, GameObject target, GameObject source)
    {
        // 리플렉션을 사용하여 효과 메서드 호출
        try
        {
            var method = this.GetType().GetMethod(effect.effectMethodName);
            if (method != null)
            {
                var parameters = new object[] { effect, target, source };
                method.Invoke(this, parameters);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"효과 실행 오류: {effect.effectName} - {e.Message}");
        }
    }
    
    // 실제 효과 구현 메서드들
    public void DealDamage(CardEffect effect, GameObject target, GameObject source)
    {
        int damage = 0;
        if (effect.effectParameters.Count > 0)
            int.TryParse(effect.effectParameters[0], out damage);
        
        // 데미지 처리 로직
        Debug.Log($"{source.name}이(가) {target.name}에게 {damage} 데미지를 입혔습니다.");
    }
    
    public void Heal(CardEffect effect, GameObject target, GameObject source)
    {
        int heal = 0;
        if (effect.effectParameters.Count > 0)
            int.TryParse(effect.effectParameters[0], out heal);
        
        // 힐 처리 로직
        Debug.Log($"{source.name}이(가) {target.name}을(를) {heal}만큼 회복시켰습니다.");
    }
    
    public void DrawCard(CardEffect effect, GameObject target, GameObject source)
    {
        int drawCount = 1;
        if (effect.effectParameters.Count > 0)
            int.TryParse(effect.effectParameters[0], out drawCount);
        
        // 카드 드로우 로직
        Debug.Log($"{source.name}이(가) 카드를 {drawCount}장 드로우했습니다.");
    }
    
    public void AddKeyword(CardEffect effect, GameObject target, GameObject source)
    {
        if (effect.effectParameters.Count > 0)
        {
            string keywordName = effect.effectParameters[0];
            // 키워드 추가 로직
            Debug.Log($"{target.name}에게 {keywordName} 키워드가 추가되었습니다.");
        }
    }
} 