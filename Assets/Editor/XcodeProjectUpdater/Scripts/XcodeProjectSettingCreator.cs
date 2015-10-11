using UnityEngine;
using UnityEditor;
using System.Collections;

/// <summary>
/// XcodeProjectSettingを作るクラス
/// </summary>
public class XcodeProjectSettingCreator : MonoBehaviour {
	[MenuItem("Assets/Create/XcodeProjectSetting")]
	public static void CreateAsset()
	{
		string path = AssetDatabase.GenerateUniqueAssetPath("Assets/XcodeProjectSetting.asset");
		XcodeProjectSetting data = ScriptableObject.CreateInstance<XcodeProjectSetting> ();
		AssetDatabase.CreateAsset(data, path);
		AssetDatabase.SaveAssets();
	}
}