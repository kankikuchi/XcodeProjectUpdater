using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.iOS.Xcode;
using System.IO;

/// <summary>
/// info.plistの設定を行うクラス
/// </summary>
public static class InfoPlistProcessor {

	/// <summary>
	/// URLスキームの設定。既に登録されていても重複しない
	/// </summary>
	public static void SetURLSchemes(string buildPath, string urlIdentifier, List<string> schemeList){

		//info.plistを取得
		string plistPath = Path.Combine(buildPath, XcodeProjectSetting.INFO_PLIST_NAME);
		PlistDocument plist = new PlistDocument();
		plist.ReadFromFile(plistPath);

		//URL typesを取得、設定されていなければ作成
		PlistElementArray urlTypes;
		if(plist.root.values.ContainsKey(XcodeProjectSetting.URL_TYPES_KEY)){
			urlTypes = plist.root[XcodeProjectSetting.URL_TYPES_KEY].AsArray();
		}
		else{
			urlTypes = plist.root.CreateArray (XcodeProjectSetting.URL_TYPES_KEY);
		}

		//URL types内のitemを取得、設定されていなければ作成
		PlistElementDict itmeDict;
		if(urlTypes.values.Count == 0){
			itmeDict = urlTypes.AddDict ();
		}
		else{
			itmeDict = urlTypes.values[0].AsDict();
		}

		//Document RoleとURL identifierを上書きで設定
		itmeDict.SetString (XcodeProjectSetting.URL_TYPE_ROLE_KEY,  "Editor");
		itmeDict.SetString (XcodeProjectSetting.URL_IDENTIFIER_KEY,  urlIdentifier);

		//URL Schemesを取得、設定されていなければ作成(上書きしたい場合はif無くして、CreateArrayのみでok)
		PlistElementArray schemesArray = itmeDict.CreateArray (XcodeProjectSetting.URL_SCHEMES_KEY);
		if(itmeDict.values.ContainsKey(XcodeProjectSetting.URL_SCHEMES_KEY)){
			schemesArray = itmeDict[XcodeProjectSetting.URL_SCHEMES_KEY].AsArray();
		}
		else{
			schemesArray = itmeDict.CreateArray (XcodeProjectSetting.URL_SCHEMES_KEY);
		}

		//既に設定されているものは一旦除外し、スキームを登録(多重登録防止用)
		for (int i = 0; i < schemesArray.values.Count; i++) {
			schemeList.Remove (schemesArray.values [i].AsString ());
		}

		foreach (string scheme in schemeList) {
			schemesArray.AddString (scheme);
		}

		//plist保存
		plist.WriteToFile(plistPath);
	}

	/// <summary>
	/// デフォルトで設定されているスプラッシュ画像の設定を消す
	/// </summary>
	public static void DeleteLaunchiImagesKey(string buildPath){
		//info.plistを取得
		string plistPath = Path.Combine(buildPath, XcodeProjectSetting.INFO_PLIST_NAME);
		PlistDocument plist = new PlistDocument();
		plist.ReadFromFile(plistPath);

		//keyが存在していれば削除
		if(plist.root.values.ContainsKey(XcodeProjectSetting.UI_LAUNCHI_IMAGES_KEY)){
			plist.root.values.Remove (XcodeProjectSetting.UI_LAUNCHI_IMAGES_KEY);
		}

		//plist保存
		plist.WriteToFile(plistPath);
	}

}
