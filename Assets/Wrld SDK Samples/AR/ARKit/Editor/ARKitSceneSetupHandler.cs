using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Reflection;

namespace Wrld.AR.EditorScripts
{
    [InitializeOnLoad]
    [CustomEditor(typeof(WRLDARKitSetupHelper))]
    public class ARKitSceneSetupHandler : UnityEditor.Editor
    {
        const string packagePath = "Assets/Wrld SDK Samples/AR/ARKit/Package/ARKitDependentScripts.unitypackage";


        private static bool m_editorEventsSubscribed = false;
        private const string WaitingForScriptCompilationKey = "WaitingForScriptCompilationKey";

        static ARKitSceneSetupHandler()
        {
            if (!m_editorEventsSubscribed) 
            {
                EditorSceneManager.sceneOpened += EditorSceneOpened;
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

            if (GUILayout.Button("Setup ARKit"))
            {
                CheckSceneStatus ();
            }
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsCompiled() 
        {
            if (EditorPrefs.GetBool(WaitingForScriptCompilationKey, false)) 
            {
                EditorPrefs.DeleteKey (WaitingForScriptCompilationKey);
                SetupARKitScene ();
            }
        }

        private static void CheckARKitDependentScripts ()
        {
            if (GetType ("WRLDARKitAnchorHandler") == null) 
            {
                AssetDatabase.ImportPackage (packagePath, false);
                EditorPrefs.SetBool (WaitingForScriptCompilationKey, true);
            } 
            else 
            {
                SetupARKitScene ();
            }
        }

        private static void CheckSceneStatus()
        {
            WRLDARKitSetupHelper wrldARKitSetupHelper = GameObject.FindObjectOfType<WRLDARKitSetupHelper>();

            if (wrldARKitSetupHelper != null) 
            {
                DisplaySetupDialog ();
            }
        }

        private static void DisplaySetupDialog()
        {
            if (GetType ("UnityARCameraManager") == null) 
            {
                string messageWithoutARKit = "Would you like to download ARKit plugin?";

                if (EditorUtility.DisplayDialog ("WRLD ARKit Sample", messageWithoutARKit, "Yes", "No")) 
                {
                    Application.OpenURL ("https://www.assetstore.unity3d.com/en/#!/content/92515");
                }
            } 
            else 
            {
                string messageWithARKit = "Would you like to setup the scene for ARKit?";

                if (EditorUtility.DisplayDialog ("WRLD ARKit Sample", messageWithARKit, "Setup", "Later")) 
                {
                    CheckARKitDependentScripts ();
                }
            }
        }

        private static void SetupARKitScene() 
        {
            WRLDARKitSetupHelper wrldARKitSetupHelper = GameObject.FindObjectOfType<WRLDARKitSetupHelper>();
            EditorSceneManager.MarkAllScenesDirty ();

            SetupUnityARCameraManager (wrldARKitSetupHelper);

            SetupUnityARCameraNearFar (wrldARKitSetupHelper);

            SetupUnityARVideo (wrldARKitSetupHelper);

            SetupWRLDARKitAnchorHandler (wrldARKitSetupHelper);

            GameObject.DestroyImmediate (wrldARKitSetupHelper.gameObject);
        }

        private static void SetupUnityARCameraManager(WRLDARKitSetupHelper wrldARKitSetupHelper)
        {
            System.Type arCameraManagerType = GetType ("UnityARCameraManager");

            if (arCameraManagerType != null) {
                GameObject arCameraManagerObject = new GameObject ("ARCameraManager");
                var comp = System.Convert.ChangeType (arCameraManagerObject.AddComponent (arCameraManagerType), arCameraManagerType);
                FieldInfo field = comp.GetType ().GetField ("m_camera");
                field.SetValue (comp, wrldARKitSetupHelper.MainCamera);
            }
            else 
            {
                Debug.LogError ("UnityARCameraManager not found in ARKit plugin.");
            }
        }

        private static void SetupUnityARCameraNearFar(WRLDARKitSetupHelper wrldARKitSetupHelper)
        {
            System.Type arCameraNearFarType = GetType ("UnityARCameraNearFar");

            if (arCameraNearFarType != null) 
            {
                wrldARKitSetupHelper.MainCamera.gameObject.AddComponent (arCameraNearFarType);
            } 
            else 
            {
                Debug.LogError ("UnityARCameraNearFar not found in ARKit plugin.");
            }
        }

        private static void SetupUnityARVideo(WRLDARKitSetupHelper wrldARKitSetupHelper)
        {
            System.Type arVideoType = GetType ("UnityARVideo");

            if (arVideoType != null) 
            {
                var comp = System.Convert.ChangeType(wrldARKitSetupHelper.MainCamera.gameObject.AddComponent (arVideoType), arVideoType);
                FieldInfo field = comp.GetType().GetField("m_ClearMaterial");
                string[] guids = AssetDatabase.FindAssets ("YUVMaterial");

                if (guids != null && guids.Length > 0) 
                {
                    string path = AssetDatabase.GUIDToAssetPath (guids [0]);
                    field.SetValue (comp, AssetDatabase.LoadAssetAtPath(path, typeof(Material)));
                }
                else 
                {
                    Debug.LogError ("YUVMaterial not found in ARKit plugin.");
                }
            }
            else 
            {
                Debug.LogError ("UnityARVideo not found in ARKit plugin.");
            }
        }

        private static void SetupWRLDARKitAnchorHandler(WRLDARKitSetupHelper wrldARKitSetupHelper)
        {
            System.Type arAnchorHandler = GetType ("WRLDARKitAnchorHandler");

            if (arAnchorHandler != null) 
            {
                GameObject arAnchorObject = new GameObject ("WRLDARKitAnchorHandler");
                var comp = System.Convert.ChangeType (arAnchorObject.AddComponent (arAnchorHandler), arAnchorHandler);
                FieldInfo mapParentField = comp.GetType ().GetField ("wrldMapParent");
                mapParentField.SetValue (comp, wrldARKitSetupHelper.WrldMapParent);
                FieldInfo mapMaskField = comp.GetType ().GetField ("wrldMapMask");
                mapMaskField.SetValue (comp, wrldARKitSetupHelper.WrldMapMask);
            }
            else 
            {
                Debug.LogError ("WRLDARKitAnchorHandler is missing.");
            }
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


