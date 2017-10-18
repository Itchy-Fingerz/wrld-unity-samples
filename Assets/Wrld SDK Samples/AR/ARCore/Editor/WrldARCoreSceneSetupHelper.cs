using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Reflection;

namespace WRLD.ARCore.Editor
{
	[InitializeOnLoad]
	[CustomEditor(typeof(WRLDARCoreSetupHelper))]
	public class WRLDARCoreSceneSetupHelper : UnityEditor.Editor 
	{

		const string PackageName = "WRLDARCore";
		const string PackagePath = "Assets/Wrld SDK Samples/AR/ARCore/Package/"+PackageName+".unitypackage";

		private static bool m_editorEventsSubscribed = false;

		static WRLDARCoreSceneSetupHelper()
		{
			if (!m_editorEventsSubscribed) 
			{
				EditorSceneManager.sceneOpened += EditorSceneOpened;
				AssetDatabase.importPackageCompleted += OnImportPackageCompleted;
				m_editorEventsSubscribed = true;
			}
		}

		static void EditorSceneOpened (UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
		{
			CheckSceneStatus ();
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI ();

			if (GUILayout.Button("Setup ARCore"))
			{
				CheckSceneStatus ();
			}
		}

		private static void OnImportPackageCompleted(string packageName) 
		{

			WRLDARCoreSetupHelper wrldARCoreSetupHelper = GameObject.FindObjectOfType<WRLDARCoreSetupHelper>();
			if (packageName.Equals(PackageName) && wrldARCoreSetupHelper!=null) 
			{
				SetupARCoreScene ();
			}
		}

		private static void CheckForWRLDARCoreScripts ()
		{
			if (GetType ("WRLDARCoreManager") == null) 
			{
				AssetDatabase.ImportPackage (PackagePath, false);
			} 
			else 
			{
				SetupARCoreScene ();
			}
		}

		private static void CheckSceneStatus()
		{
			WRLDARCoreSetupHelper wrldARCoreSetupHelper = GameObject.FindObjectOfType<WRLDARCoreSetupHelper>();

			if (wrldARCoreSetupHelper != null) 
			{
				DisplaySetupDialog ();
			}
		}

		private static void DisplaySetupDialog()
		{
			if (GetType ("SessionComponent") == null) 
			{
				string messageWithoutARCore = "Would you like to download ARCore Preview plugin?";

				if (EditorUtility.DisplayDialog ("WRLD ARCore Sample", messageWithoutARCore, "Yes", "No")) 
				{
					Application.OpenURL ("https://github.com/google-ar/arcore-unity-sdk/releases");
				}
			} 
			else 
			{
				string messageWithoutARCore = "Would you like to setup the scene for ARCore?";

				if (EditorUtility.DisplayDialog ("WRLD ARCore Sample", messageWithoutARCore, "Setup", "Later")) 
				{
					CheckForWRLDARCoreScripts ();
				}
			}
		}

		private static void SetupARCoreScene() 
		{

			EditorSceneManager.MarkAllScenesDirty ();

			WRLDARCoreSetupHelper wrldARCoreSetupHelper = GameObject.FindObjectOfType<WRLDARCoreSetupHelper>();

			SetupARCoreSessionComponent (wrldARCoreSetupHelper);
			SetupWRLDARCoreManager (wrldARCoreSetupHelper);
			SetupWRLDARMapPositioner (wrldARCoreSetupHelper);

			GameObject.DestroyImmediate (wrldARCoreSetupHelper.gameObject);

		}

		private static void SetupARCoreSessionComponent(WRLDARCoreSetupHelper wrldARCoreSetupHelper)
		{
			
			System.Type sessionComponentType = GetType ("SessionComponent");
			System.Type sessionConfigType = GetType ("SessionConfig");

			if(sessionComponentType==null)
			{
				Debug.LogError ("SessionComponent not found in ARCore package.");
				return;
			}

			if(sessionConfigType==null)
			{
				Debug.LogError ("SessionConfig not found in ARCore package.");
				return;
			}

			var sessionComponent = System.Convert.ChangeType (wrldARCoreSetupHelper.ARCoreDevice.AddComponent (sessionComponentType), sessionComponentType);	

			FieldInfo cameraField = sessionComponent.GetType ().GetField ("m_firstPersonCamera");
			cameraField.SetValue (sessionComponent, wrldARCoreSetupHelper.MainCamera);

			FieldInfo sessionConfigField = sessionComponent.GetType ().GetField ("m_arSessionConfig");
			string[] guids = AssetDatabase.FindAssets ("WRLDARCoreSessionConfig");
			if (guids != null && guids.Length > 0) 
			{
				string path = AssetDatabase.GUIDToAssetPath (guids [0]);
				sessionConfigField.SetValue (sessionComponent, AssetDatabase.LoadAssetAtPath(path, sessionConfigType));
			}
			else 
			{
				Debug.LogError ("WRLDARCoreSessionConfig.asset not found. Have you loaded ARCoreDependency package.");
			}

		}


		private static void SetupWRLDARCoreManager(WRLDARCoreSetupHelper wrldARCoreSetupHelper)
		{


			System.Type wrldARCoreManagerType = GetType ("WRLDARCoreManager");

			if(wrldARCoreManagerType==null)
			{
				Debug.LogError ("WRLDARCoreManager not found in ARCoreDependency package.");
				return;
			}

			GameObject wrldARCoreManagerGO = new GameObject ("WRLDARCoreManager");
			var wrldARCoreManagerComponent = System.Convert.ChangeType (wrldARCoreManagerGO.AddComponent (wrldARCoreManagerType), wrldARCoreManagerType);

			FieldInfo wrldMapFiled = wrldARCoreManagerComponent.GetType ().GetField ("wrldMap");
			wrldMapFiled.SetValue (wrldARCoreManagerComponent, wrldARCoreSetupHelper.WrldMapParent);

			FieldInfo wrldMapMaskFiled = wrldARCoreManagerComponent.GetType ().GetField ("wrldMapMask");
			wrldMapMaskFiled.SetValue (wrldARCoreManagerComponent, wrldARCoreSetupHelper.WrldMapMask);
		}

		private static void SetupWRLDARMapPositioner (WRLDARCoreSetupHelper wrldARCoreSetupHelper)
		{
			System.Type wrldARCorePositionerType = GetType ("WRLDARCorePositioner");

			if(wrldARCorePositionerType==null)
			{
				Debug.LogError ("WRLDARCorePositioner not found in ARCoreDependency package.");
				return;
			}

			var wrldARCorePositionerComponent = System.Convert.ChangeType (wrldARCoreSetupHelper.WrldMapParent.gameObject.AddComponent (wrldARCorePositionerType), wrldARCorePositionerType);

			FieldInfo wrldMapMaskFiled = wrldARCorePositionerComponent.GetType ().GetField ("wrldMapMask");
			wrldMapMaskFiled.SetValue (wrldARCorePositionerComponent, wrldARCoreSetupHelper.WrldMapMask);
		}


		public static System.Type GetType(string typeName)
		{
			System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
			foreach (var a in assemblies)
			{
				System.Type[] types = a.GetTypes();
				foreach (var t in types)
				{
					if (t.Name.Equals(typeName))
						return t;
				}
			}
			return null;
		}
	}
}