#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(CardEffectData))]
public class CardEffectDataEditor : Editor
{
    private CardEffectData effectData;
    private bool showParameters = true;
    private bool showCondition = true;
    
    private void OnEnable()
    {
        effectData = (CardEffectData)target;
        
        // 초기화
        if (effectData.effectParameters == null)
            effectData.effectParameters = new List<string>();
        
        if (effectData.condition == null)
            effectData.condition = new EffectCondition();
    }
    
    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("카드 효과 데이터", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // 기본 정보
        EditorGUILayout.LabelField("기본 정보", EditorStyles.boldLabel);
        effectData.effectName = EditorGUILayout.TextField("효과명", effectData.effectName);
        effectData.effectDescription = EditorGUILayout.TextArea(effectData.effectDescription, GUILayout.Height(60));
        
        EditorGUILayout.BeginHorizontal();
        effectData.trigger = (EffectTrigger)EditorGUILayout.EnumPopup("발동 타이밍", effectData.trigger);
        effectData.effectType = (EffectType)EditorGUILayout.EnumPopup("효과 타입", effectData.effectType);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // 실행 정보
        EditorGUILayout.LabelField("실행 정보", EditorStyles.boldLabel);
        effectData.effectMethodName = EditorGUILayout.TextField("실행 메서드", effectData.effectMethodName);
        
        // 파라미터 편집
        showParameters = EditorGUILayout.Foldout(showParameters, "파라미터");
        if (showParameters)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < effectData.effectParameters.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                effectData.effectParameters[i] = EditorGUILayout.TextField($"파라미터 {i + 1}", effectData.effectParameters[i]);
                if (GUILayout.Button("삭제", GUILayout.Width(60)))
                {
                    effectData.effectParameters.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("파라미터 추가"))
            {
                effectData.effectParameters.Add("");
            }
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space();
        
        // 조건 편집
        showCondition = EditorGUILayout.Foldout(showCondition, "조건");
        if (showCondition)
        {
            EditorGUI.indentLevel++;
            effectData.condition.conditionType = (EffectCondition.ConditionType)EditorGUILayout.EnumPopup("조건 타입", effectData.condition.conditionType);
            
            if (effectData.condition.conditionType != EffectCondition.ConditionType.None)
            {
                effectData.condition.comparison = EditorGUILayout.TextField("비교 연산자", effectData.condition.comparison);
                effectData.condition.conditionValue = EditorGUILayout.TextField("조건 값", effectData.condition.conditionValue);
            }
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space();
        
        // 효과 설정
        EditorGUILayout.LabelField("효과 설정", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        effectData.isOncePerTurn = EditorGUILayout.Toggle("턴당 한 번", effectData.isOncePerTurn);
        effectData.isOncePerGame = EditorGUILayout.Toggle("게임당 한 번", effectData.isOncePerGame);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // 템플릿 버튼들
        EditorGUILayout.LabelField("빠른 설정", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("데미지 효과"))
        {
            SetTemplateEffect("데미지 효과", EffectTrigger.OnActivate, EffectType.Normal, "대상에게 데미지를 입힙니다.", "DealDamage", "2");
        }
        if (GUILayout.Button("회복 효과"))
        {
            SetTemplateEffect("회복 효과", EffectTrigger.OnActivate, EffectType.Normal, "대상을 회복시킵니다.", "Heal", "3");
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("드로우 효과"))
        {
            SetTemplateEffect("드로우 효과", EffectTrigger.OnActivate, EffectType.Normal, "카드를 드로우합니다.", "DrawCard", "1");
        }
        if (GUILayout.Button("지속 효과"))
        {
            SetTemplateEffect("지속 효과", EffectTrigger.Continuous, EffectType.Continuous, "지속적으로 효과가 발동됩니다.", "Heal", "1");
        }
        EditorGUILayout.EndHorizontal();
        
        // 변경사항 적용
        if (GUI.changed)
        {
            EditorUtility.SetDirty(effectData);
        }
    }
    
    private void SetTemplateEffect(string name, EffectTrigger trigger, EffectType type, string description, string methodName, string parameter)
    {
        effectData.effectName = name;
        effectData.effectDescription = description;
        effectData.trigger = trigger;
        effectData.effectType = type;
        effectData.effectMethodName = methodName;
        
        effectData.effectParameters.Clear();
        if (!string.IsNullOrEmpty(parameter))
        {
            effectData.effectParameters.Add(parameter);
        }
        
        EditorUtility.SetDirty(effectData);
    }
}
#endif 