using UnityEngine;
using UnityEditor;
using System.Collections;


public static class FSNEngineEditorMenu
{
	[MenuItem("FSN Engine/기본 레이어 이름 설정하기")]
	static void SetDefaultLayerNames()
	{
		if (EditorUtility.DisplayDialog("FSNEngine", "이 기능은 기본 설정된 FSNEngine 프리팹/씬을 사용할 시의 편의를 위해 프리팹에서 미리 참조중인 레이어(29~31번)의 이름을 지정해주는 기능이며, 꼭 실행하지 않더라도 작동에는 문제가 없습니다.\n\n해당 프로젝트에서 29~31번 레이어를 사용하지 않는 경우에만 '확인'을 눌러주세요.", "확인", "취소"))
		{
			var tagManager	= new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
			var layersProp	= tagManager.FindProperty("layers");

			layersProp.GetArrayElementAtIndex(29).stringValue	= "Text (FSNEngine)";
			layersProp.GetArrayElementAtIndex(30).stringValue	= "Image-back (FSNEngine)";
			layersProp.GetArrayElementAtIndex(31).stringValue	= "Image-forward (FSNEngine)";

			tagManager.ApplyModifiedProperties();
		}
	}

	[MenuItem("FSN Engine/조합 이미지 만들기")]
	static void BuildAllCombinedImages()
	{
		FSNCombinedImageGenerator.BuildCombinedImageSources();
	}
}
