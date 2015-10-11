using UnityEngine;
using System.Collections;
using UnityEditor.iOS.Xcode;
using System.IO;

/// <summary>
/// ディレクトリを操作するクラス
/// </summary>
public static class DirectoryProcessor {

	/// <summary>
	/// 指定ディレクトリをXcodeにコピーして追加する
	/// </summary>
	public static void CopyAndAddBuildToXcode(
		PBXProject pbxProject, string targetGuid, 
		string copyDirectoryPath, string buildPath, string currentDirectoryPath,
		bool needToAddBuild = true
	){
		//コピー元(Unity)のディレクトリとコピー先(Xcode)のディレクトリのパスを作成
		string unityDirectoryPath = copyDirectoryPath;
		string xcodeDirectoryPath = buildPath;

		//ディレクトリ内のディレクトリの中身をコピーしている場合
		if(!string.IsNullOrEmpty(currentDirectoryPath)){
			unityDirectoryPath = Path.Combine(unityDirectoryPath, currentDirectoryPath);
			xcodeDirectoryPath = Path.Combine(xcodeDirectoryPath, currentDirectoryPath);

			//既にディクショナリーがある場合は削除し、新たにディクショナリー作成
			Delete (xcodeDirectoryPath);
			Directory.CreateDirectory(xcodeDirectoryPath);
		}

		//ファイルをコピーし、プロジェクトへの追加も行う
		foreach (string filePath in Directory.GetFiles(unityDirectoryPath)){

			//metaファイルはコピーしない
			string extension = Path.GetExtension (filePath);
			if(extension == ExtensionName.META){
				continue;
			}
			//アーカイブファイルの場合は、それが入っているディレクトリにパスを通す
			else if(extension == ExtensionName.ARCHIVE){
				pbxProject.AddBuildProperty(
					targetGuid, 
					XcodeProjectSetting.LIBRARY_SEARCH_PATHS_KEY, 
					XcodeProjectSetting.PROJECT_ROOT + currentDirectoryPath
				);
			}

			//ファイルパスからファイル名を取得し、コピー先のパスを作成
			string fileName = Path.GetFileName (filePath);
			string copyPath = Path.Combine (xcodeDirectoryPath, fileName);


			//隠しファイルはコピーしない .DS_Storeとか
			if(fileName[0] == '.'){
				continue;
			}

			//既に同名ファイルがある場合は削除、その後コピー
			File.Delete(copyPath);
			File.Copy(filePath, copyPath);

			if(needToAddBuild){
				//プロジェクト内へ追加する時のパスは、ビルドしたディレクトリからの相対パス
				string relativePath = Path.Combine(currentDirectoryPath, fileName);
				pbxProject.AddFileToBuild(targetGuid, pbxProject.AddFile(relativePath, relativePath, PBXSourceTree.Source));
			}

		}

		//ディレクトリの中にあるディレクトリの中もコピー
		foreach (string directoryPath in Directory.GetDirectories(unityDirectoryPath)){
			string directoryName = Path.GetFileName (directoryPath);
			bool nextNeedToAddBuild = needToAddBuild;

			//フレームワークやImages.xcassetsがが入っているディレクトリはコピーするだけ
			if(directoryName.Contains(ExtensionName.FRAMEWORK) || directoryName.Contains(ExtensionName.BUNDLE) || 
				directoryName == XcodeProjectSetting.IMAGE_XCASSETS_DIRECTORY_NAME){
				nextNeedToAddBuild = false;
			}

			CopyAndAddBuildToXcode (
				pbxProject, targetGuid, 
				copyDirectoryPath, buildPath, Path.Combine(currentDirectoryPath, directoryName), 
				nextNeedToAddBuild
			);

			//フレームワークはディレクトリ内を全てコピーしてから、フレームワークごとプロジェクトに追加し、フレームワーク検索パスを通す
			if(directoryName.Contains(ExtensionName.FRAMEWORK) || directoryName.Contains(ExtensionName.BUNDLE)){
				string relativePath = Path.Combine(currentDirectoryPath, directoryName);
				pbxProject.AddFileToBuild(targetGuid, pbxProject.AddFile(relativePath, relativePath, PBXSourceTree.Source));
				pbxProject.AddBuildProperty(
					targetGuid, 
					XcodeProjectSetting.FRAMEWORK_SEARCH_PATHS_KEY, 
					XcodeProjectSetting.PROJECT_ROOT + currentDirectoryPath
				);
			}
		}

	}

	/// <summary>
	/// ディレクトリとその中身を上書きコピー
	/// </summary>
	public static void CopyAndReplace(string sourcePath, string copyPath)
	{
		//既にディクショナリーがある場合は削除し、新たにディクショナリー作成
		Delete (copyPath);
		Directory.CreateDirectory(copyPath);

		//ファイルをコピー
		foreach (var file in Directory.GetFiles(sourcePath)){
			File.Copy(file, Path.Combine(copyPath, Path.GetFileName(file)));
		}

		//ディレクトリの中のディレクトリも再帰的にコピー
		foreach (var dir in Directory.GetDirectories(sourcePath)){
			CopyAndReplace(dir, Path.Combine(copyPath, Path.GetFileName(dir)));
		}
	}

	/// <summary>
	/// 指定したディレクトリとその中身を全て削除する
	/// </summary>
	public static void Delete(string targetDirectoryPath){
		if (!Directory.Exists (targetDirectoryPath)) {
			return;
		}

		//ディレクトリを覗く全ファイルを削除
		string[] filePaths = Directory.GetFiles(targetDirectoryPath);
		foreach (string filePath in filePaths){
			File.SetAttributes(filePath, FileAttributes.Normal);
			File.Delete(filePath);
		}

		//ディレクトリの中のディレクトリも再帰的に削除
		string[] directoryPaths = Directory.GetDirectories(targetDirectoryPath);
		foreach (string directoryPath in directoryPaths){
			Delete(directoryPath);
		}

		Directory.Delete(targetDirectoryPath, false);
	}
}
