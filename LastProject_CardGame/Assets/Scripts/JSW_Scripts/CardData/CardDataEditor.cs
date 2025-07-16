#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(BaseCardData), true)]
public class CardDataEditor : Editor
{
    SerializedProperty cardNameProp;
    SerializedProperty descriptionProp;
    SerializedProperty artworkProp;
    SerializedProperty cardTypeProp;
    SerializedProperty rarityProp;
    SerializedProperty costProp;
    SerializedProperty haveLive2DProp;
    SerializedProperty live2DPathProp;
    SerializedProperty tagsProp;
    SerializedProperty cardEffectsProp;
    SerializedProperty cardIdProp;

    void OnEnable()
    {
        cardNameProp = serializedObject.FindProperty("cardName");
        descriptionProp = serializedObject.FindProperty("description");
        artworkProp = serializedObject.FindProperty("artwork");
        cardTypeProp = serializedObject.FindProperty("cardType");
        rarityProp = serializedObject.FindProperty("rarity");
        costProp = serializedObject.FindProperty("cost");
        haveLive2DProp = serializedObject.FindProperty("haveLive2D");
        live2DPathProp = serializedObject.FindProperty("live2DPath");
        tagsProp = serializedObject.FindProperty("tags");
        cardEffectsProp = serializedObject.FindProperty("cardEffects");
        cardIdProp = serializedObject.FindProperty("cardId");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("카드 기본 정보", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(cardIdProp, new GUIContent("카드 ID"));
        EditorGUILayout.PropertyField(cardNameProp, new GUIContent("카드 이름"));
        EditorGUILayout.PropertyField(descriptionProp, new GUIContent("설명"));
        EditorGUILayout.PropertyField(artworkProp, new GUIContent("일러스트"));
        EditorGUILayout.PropertyField(cardTypeProp, new GUIContent("카드 타입"));
        EditorGUILayout.PropertyField(rarityProp, new GUIContent("등급"));
        EditorGUILayout.PropertyField(costProp, new GUIContent("코스트"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Live2D", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(haveLive2DProp, new GUIContent("Live2D 사용"));
        if (haveLive2DProp.boolValue)
        {
            EditorGUILayout.PropertyField(live2DPathProp, new GUIContent("Live2D 경로"));
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("기타 정보", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(tagsProp, new GUIContent("태그"), true);
        EditorGUILayout.PropertyField(cardEffectsProp, new GUIContent("카드 효과"), true);

        EditorGUILayout.Space();
        DrawTypeSpecificFields();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawTypeSpecificFields()
    {
        BaseCardData baseCard = (BaseCardData)target;

        if (baseCard is MonsterCardData monster)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("몬스터 정보", EditorStyles.boldLabel);

            SerializedProperty monsterTypeProp = serializedObject.FindProperty("monsterType");
            SerializedProperty attackProp = serializedObject.FindProperty("attack");
            SerializedProperty healthProp = serializedObject.FindProperty("health");
            SerializedProperty raceProp = serializedObject.FindProperty("race");

            EditorGUILayout.PropertyField(monsterTypeProp, new GUIContent("몬스터 타입"));
            EditorGUILayout.PropertyField(attackProp, new GUIContent("공격력"));
            EditorGUILayout.PropertyField(healthProp, new GUIContent("체력"));
            EditorGUILayout.PropertyField(raceProp, new GUIContent("종족"));
        }
        else if (baseCard is SpellCardData)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("스펠 정보", EditorStyles.boldLabel);

            SerializedProperty spellTypeProp = serializedObject.FindProperty("spellType");
            EditorGUILayout.PropertyField(spellTypeProp, new GUIContent("스펠 타입"));
        }
        else if (baseCard is TrapCardData)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("트랩 정보", EditorStyles.boldLabel);

            SerializedProperty trapTypeProp = serializedObject.FindProperty("trapType");
            EditorGUILayout.PropertyField(trapTypeProp, new GUIContent("트랩 타입"));
        }
    }
}
#endif
