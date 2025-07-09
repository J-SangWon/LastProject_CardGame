using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CardEffectManager : MonoBehaviour
{
    private static CardEffectManager instance;
    public static CardEffectManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("CardEffectManager");
                instance = go.AddComponent<CardEffectManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }
    
    [Header("효과 시스템 설정")]
    public bool enableEffectSystem = true;
    public bool showEffectLogs = true;
    public bool autoLoadOnStart = true;
    
    // 효과 실행 이벤트
    public System.Action<CardEffect, GameObject, GameObject> OnEffectExecuted;
    public System.Action<EffectTrigger, GameObject> OnEffectTriggered;
    
    // 카드별 효과 데이터 저장
    private Dictionary<string, List<CardEffect>> cardEffects = new Dictionary<string, List<CardEffect>>();
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (autoLoadOnStart)
        {
            LoadAllCardEffects();
        }
    }
    
    // 모든 카드 효과 로드
    public void LoadAllCardEffects()
    {
        Debug.Log("=== 카드 효과 로딩 시작 ===");
        
        var allCards = LoadCardsFromResources();
        int totalCards = 0;
        int totalEffects = 0;
        
        foreach (var card in allCards)
        {
            if (RegisterCardEffects(card))
            {
                totalCards++;
                totalEffects += card.cardEffects != null ? card.cardEffects.Count : 0;
            }
        }
        
        Debug.Log($"=== 카드 효과 로딩 완료: {totalCards}개 카드, {totalEffects}개 효과 ===");
    }
    
    // Resources 폴더에서 카드 데이터 로드
    private List<BaseCardData> LoadCardsFromResources()
    {
        var allCards = new List<BaseCardData>();
        
        // 몬스터 카드 로드
        var monsterCards = Resources.LoadAll<MonsterCardData>("Cards");
        allCards.AddRange(monsterCards.Cast<BaseCardData>());
        
        // 스펠 카드 로드
        var spellCards = Resources.LoadAll<SpellCardData>("Cards");
        allCards.AddRange(spellCards.Cast<BaseCardData>());
        
        // 트랩 카드 로드
        var trapCards = Resources.LoadAll<TrapCardData>("Cards");
        allCards.AddRange(trapCards.Cast<BaseCardData>());
        
        Debug.Log($"Resources에서 {allCards.Count}개 카드 로드됨");
        return allCards;
    }
    
    // 카드 효과 등록
    public void RegisterCardEffects(string cardName, List<CardEffect> effects)
    {
        if (string.IsNullOrEmpty(cardName) || effects == null) return;
        
        cardEffects[cardName] = new List<CardEffect>(effects);
        
        if (showEffectLogs)
            Debug.Log($"카드 '{cardName}'의 {effects.Count}개 효과가 등록되었습니다.");
    }
    
    // 카드 데이터로부터 효과 등록
    private bool RegisterCardEffects(BaseCardData cardData)
    {
        if (cardData == null || string.IsNullOrEmpty(cardData.cardName))
        {
            return false;
        }
        
        if (cardData.cardEffects != null && cardData.cardEffects.Count > 0)
        {
            cardData.RegisterEffects();
            return true;
        }
        
        return false;
    }
    
    // 카드 효과 제거
    public void UnregisterCardEffects(string cardName)
    {
        if (cardEffects.ContainsKey(cardName))
        {
            cardEffects.Remove(cardName);
            
            if (showEffectLogs)
                Debug.Log($"카드 '{cardName}'의 효과가 제거되었습니다.");
        }
    }
    
    // 특정 타이밍의 효과 실행
    public void TriggerEffects(EffectTrigger trigger, GameObject source, GameObject target = null)
    {
        if (!enableEffectSystem) return;
        
        if (showEffectLogs)
            Debug.Log($"효과 타이밍 '{trigger}' 발동 - 소스: {source.name}");
        
        OnEffectTriggered?.Invoke(trigger, source);
        
        // 소스 카드의 효과 실행
        string sourceCardName = GetCardNameFromGameObject(source);
        if (!string.IsNullOrEmpty(sourceCardName) && cardEffects.ContainsKey(sourceCardName))
        {
            var effects = cardEffects[sourceCardName].Where(e => e.trigger == trigger).ToList();
            foreach (var effect in effects)
            {
                ExecuteEffect(effect, target ?? source, source);
            }
        }
        
        // 필드의 다른 카드들의 효과도 확인 (예: 지속 효과)
        if (trigger == EffectTrigger.OnSummon || trigger == EffectTrigger.OnActivate)
        {
            TriggerFieldEffects(trigger, source, target);
        }
    }
    
    // 필드의 다른 카드들의 효과 실행
    private void TriggerFieldEffects(EffectTrigger trigger, GameObject source, GameObject target)
    {
        // 필드의 모든 카드를 찾아서 지속 효과나 반응 효과 실행
        var fieldCards = FindObjectsOfType<CardDisplay>();
        
        foreach (var card in fieldCards)
        {
            if (card.gameObject == source) continue; // 자기 자신은 제외
            
            string cardName = GetCardNameFromGameObject(card.gameObject);
            if (!string.IsNullOrEmpty(cardName) && cardEffects.ContainsKey(cardName))
            {
                var effects = cardEffects[cardName].Where(e => 
                    e.trigger == EffectTrigger.Continuous || 
                    e.trigger == trigger).ToList();
                    
                foreach (var effect in effects)
                {
                    ExecuteEffect(effect, source, card.gameObject);
                }
            }
        }
    }
    
    // 개별 효과 실행
    private void ExecuteEffect(CardEffect effect, GameObject target, GameObject source)
    {
        if (effect == null) return;
        
        if (showEffectLogs)
            Debug.Log($"효과 실행: {effect.effectName} - {effect.trigger}");
        
        // EffectExecutor를 통해 효과 실행
        EffectExecutor.Instance.ExecuteEffect(effect, target, source);
        
        // 이벤트 발생
        OnEffectExecuted?.Invoke(effect, target, source);
    }
    
    // GameObject에서 카드 이름 추출
    private string GetCardNameFromGameObject(GameObject obj)
    {
        var cardDisplay = obj.GetComponent<CardDisplay>();
        if (cardDisplay != null && cardDisplay.cardData != null)
        {
            return cardDisplay.cardData.cardName;
        }
        return obj.name;
    }
    
    // 카드의 모든 효과 가져오기
    public List<CardEffect> GetCardEffects(string cardName)
    {
        if (cardEffects.ContainsKey(cardName))
        {
            return new List<CardEffect>(cardEffects[cardName]);
        }
        return new List<CardEffect>();
    }
    
    // 특정 타입의 효과를 가진 카드들 찾기
    public List<string> GetCardsWithEffectType(EffectType effectType)
    {
        var result = new List<string>();
        
        foreach (var kvp in cardEffects)
        {
            if (kvp.Value.Any(e => e.effectType == effectType))
            {
                result.Add(kvp.Key);
            }
        }
        
        return result;
    }
    
    // 카드 이름으로 카드 데이터 찾기
    public BaseCardData FindCardByName(string cardName)
    {
        var allCards = LoadCardsFromResources();
        return allCards.FirstOrDefault(c => c.cardName == cardName);
    }
    
    // 특정 카드의 효과 정보 출력
    public void PrintCardEffects(string cardName)
    {
        var card = FindCardByName(cardName);
        if (card == null)
        {
            Debug.LogWarning($"카드 '{cardName}'을 찾을 수 없습니다.");
            return;
        }
        
        Debug.Log($"=== 카드 '{cardName}' 효과 정보 ===");
        Debug.Log($"카드 타입: {card.cardType}");
        Debug.Log($"효과 수: {(card.cardEffects != null ? card.cardEffects.Count : 0)}개");
        
        if (card.cardEffects == null || card.cardEffects.Count == 0)
        {
            Debug.Log("등록된 효과가 없습니다.");
            return;
        }
        
        foreach (var effect in card.cardEffects)
        {
            Debug.Log($"- {effect.effectName}");
            Debug.Log($"  설명: {effect.effectDescription}");
            Debug.Log($"  타이밍: {effect.trigger}");
            Debug.Log($"  타입: {effect.effectType}");
            Debug.Log($"  메서드: {effect.effectMethodName}");
            
            if (effect.effectParameters.Count > 0)
            {
                Debug.Log($"  파라미터: {string.Join(", ", effect.effectParameters)}");
            }
            
            if (effect.hasCondition)
            {
                Debug.Log($"  조건: {effect.condition.conditionType} {effect.condition.comparison} {effect.condition.conditionValue}");
            }
        }
    }
    
    // 효과 통계
    [ContextMenu("효과 통계 출력")]
    public void PrintEffectStatistics()
    {
        var allCards = LoadCardsFromResources();
        
        var triggerStats = new Dictionary<EffectTrigger, int>();
        var typeStats = new Dictionary<EffectType, int>();
        var cardTypeStats = new Dictionary<CardType, int>();
        
        int totalEffects = 0;
        int totalCardsWithEffects = 0;
        
        foreach (var card in allCards)
        {
            if (card == null) continue;
            
            if (card.cardEffects != null && card.cardEffects.Count > 0)
            {
                totalCardsWithEffects++;
                totalEffects += card.cardEffects.Count;
                
                // 카드 타입 통계
                if (cardTypeStats.ContainsKey(card.cardType))
                    cardTypeStats[card.cardType]++;
                else
                    cardTypeStats[card.cardType] = 1;
                
                // 효과 타이밍 통계
                foreach (var effect in card.cardEffects)
                {
                    if (triggerStats.ContainsKey(effect.trigger))
                        triggerStats[effect.trigger]++;
                    else
                        triggerStats[effect.trigger] = 1;
                    
                    if (typeStats.ContainsKey(effect.effectType))
                        typeStats[effect.effectType]++;
                    else
                        typeStats[effect.effectType] = 1;
                }
            }
        }
        
        Debug.Log("=== 카드 효과 통계 ===");
        Debug.Log($"총 카드 수: {allCards.Count}");
        Debug.Log($"효과가 있는 카드 수: {totalCardsWithEffects}");
        Debug.Log($"총 효과 수: {totalEffects}");
        
        Debug.Log("--- 카드 타입별 분포 ---");
        foreach (var kvp in cardTypeStats.OrderByDescending(x => x.Value))
        {
            Debug.Log($"{kvp.Key}: {kvp.Value}개");
        }
        
        Debug.Log("--- 타이밍별 효과 수 ---");
        foreach (var kvp in triggerStats.OrderByDescending(x => x.Value))
        {
            Debug.Log($"{kvp.Key}: {kvp.Value}개");
        }
        
        Debug.Log("--- 타입별 효과 수 ---");
        foreach (var kvp in typeStats.OrderByDescending(x => x.Value))
        {
            Debug.Log($"{kvp.Key}: {kvp.Value}개");
        }
    }
}

// CardDisplay 클래스 (실제 구현에서는 별도 파일에 있어야 함)
public class CardDisplay : MonoBehaviour
{
    public BaseCardData cardData;
    
    public void SetCardData(BaseCardData data)
    {
        cardData = data;
    }
} 