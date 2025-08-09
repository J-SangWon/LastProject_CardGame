#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Ability_Search))]
public class Ability_SearchEditor : Editor
{
	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		var searchTypeProp = serializedObject.FindProperty("searchType");
		EditorGUILayout.PropertyField(searchTypeProp);

		var searchType = (SearchType)searchTypeProp.enumValueIndex;

		switch (searchType)
		{
			case SearchType.CardType:
				EditorGUILayout.PropertyField(serializedObject.FindProperty("cardType"));
				break;
			case SearchType.Cost:
				EditorGUILayout.PropertyField(serializedObject.FindProperty("cost"));
				break;
			case SearchType.Race:
				EditorGUILayout.PropertyField(serializedObject.FindProperty("race"));
				break;
			case SearchType.CardID:
				EditorGUILayout.PropertyField(serializedObject.FindProperty("cardID"));
				break;
		}

		serializedObject.ApplyModifiedProperties();
	}
}
#endif