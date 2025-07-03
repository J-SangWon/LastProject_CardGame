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
    SerializedProperty effectIdsProp;
    SerializedProperty effectTimingsProp;
    SerializedProperty tagsProp;

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
        effectIdsProp = serializedObject.FindProperty("effectIds");
        effectTimingsProp = serializedObject.FindProperty("effectTimings");
        tagsProp = serializedObject.FindProperty("tags");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("카드 기본 정보", EditorStyles.boldLabel);
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
        EditorGUILayout.LabelField("효과 정보", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(effectIdsProp, new GUIContent("효과 ID"), true);
        EditorGUILayout.PropertyField(effectTimingsProp, new GUIContent("발동 조건"), true);
        EditorGUILayout.PropertyField(tagsProp, new GUIContent("태그"), true);

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

            monster.monsterType = (MonsterType)EditorGUILayout.EnumPopup("몬스터 타입", monster.monsterType);
            monster.attack = EditorGUILayout.IntField("공격력", monster.attack);
            monster.health = EditorGUILayout.IntField("체력", monster.health);
            monster.race = (Race)EditorGUILayout.EnumPopup("종족", monster.race);
        }
        else if (baseCard is SpellCardData spell)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("스펠 정보", EditorStyles.boldLabel);
            spell.spellType = (SpellType)EditorGUILayout.EnumPopup("스펠 타입", spell.spellType);
        }
        else if (baseCard is TrapCardData trap)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("트랩 정보", EditorStyles.boldLabel);
            trap.trapType = (TrapType)EditorGUILayout.EnumPopup("트랩 타입", trap.trapType);
        }
    }
}
#endif
