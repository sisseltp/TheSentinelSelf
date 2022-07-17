using UnityEngine;

namespace WaterCausticsForURP
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer)), ExecuteAlways,
     AddComponentMenu("Effects/Caustics Volume")]
    [HelpURL("https://alexander-ameye.gitbook.io/water-caustics-for-urp/")]
    public class CausticsVolume : MonoBehaviour
    {
        private MeshRenderer meshRenderer;

        [SerializeField] private bool displayDebugOverlay;

#if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/Effects/Caustics Volume", priority = 7)]
        static void CreateCausticsVolume()
        {
            var guids = UnityEditor.AssetDatabase.FindAssets($"t:Prefab " + "Caustics Volume");
            if (guids.Length == 0) Debug.Log("Error: caustics volume not found");
            else
            {
                GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]));
                GameObject instance = (GameObject) UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
                UnityEditor.PrefabUtility.UnpackPrefabInstance(instance, UnityEditor.PrefabUnpackMode.Completely,
                    UnityEditor.InteractionMode.AutomatedAction);
                UnityEditor.Undo.RegisterCreatedObjectUndo(instance, "Create Caustics Volume");
                UnityEditor.Selection.activeObject = instance;
                UnityEditor.SceneView.FrameLastActiveSceneView();
            }
        }
#endif
        private void OnEnable()
        {
            if (!meshRenderer) meshRenderer = GetComponent<MeshRenderer>();
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (!displayDebugOverlay || meshRenderer == null) return;

            Color visible = Color.green;
            Color occluded = Color.white;

            var v1 = new Vector3(-0.5f, -0.5f, -0.5f);
            var v2 = new Vector3(0.5f, 0.5f, 0.5f);
            var v3 = new Vector3(v1.x, v1.y, v2.z);
            var v4 = new Vector3(v1.x, v2.y, v1.z);
            var v5 = new Vector3(v2.x, v1.y, v1.z);
            var v6 = new Vector3(v1.x, v2.y, v2.z);
            var v7 = new Vector3(v2.x, v1.y, v2.z);
            var v8 = new Vector3(v2.x, v2.y, v1.z);

            UnityEditor.Handles.matrix = transform.localToWorldMatrix;

            // draw border lines
            UnityEditor.Handles.color = GetVisibleOutlineColor(visible);
            UnityEditor.Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
            UnityEditor.Handles.DrawAAPolyLine(Texture2D.whiteTexture, 5f, v1, v3, v7, v5, v1);
            UnityEditor.Handles.DrawAAPolyLine(Texture2D.whiteTexture, 5f, v4, v6, v2, v8, v4);
            UnityEditor.Handles.DrawAAPolyLine(Texture2D.whiteTexture, 5f, v7, v2, v6, v3, v7);
            UnityEditor.Handles.DrawAAPolyLine(Texture2D.whiteTexture, 5f, v1, v4, v8, v5, v1);
            UnityEditor.Handles.DrawSolidRectangleWithOutline(new[] {v7, v2, v6, v3}, GetVisibleFaceColor(visible),
                Color.clear);
            UnityEditor.Handles.DrawSolidRectangleWithOutline(new[] {v1, v4, v8, v5}, GetVisibleFaceColor(visible),
                Color.clear);
            UnityEditor.Handles.DrawSolidRectangleWithOutline(new[] {v1, v3, v7, v5}, GetVisibleFaceColor(visible),
                Color.clear);
            UnityEditor.Handles.DrawSolidRectangleWithOutline(new[] {v4, v6, v2, v8}, GetVisibleFaceColor(visible),
                Color.clear);
            UnityEditor.Handles.DrawSolidRectangleWithOutline(new[] {v5, v8, v2, v7}, GetVisibleFaceColor(visible),
                Color.clear);
            UnityEditor.Handles.DrawSolidRectangleWithOutline(new[] {v3, v6, v4, v1}, GetVisibleFaceColor(visible),
                Color.clear);

            UnityEditor.Handles.color = GetOccludedOutlineColor(occluded);
            UnityEditor.Handles.zTest = UnityEngine.Rendering.CompareFunction.Greater;
            UnityEditor.Handles.DrawAAPolyLine(Texture2D.whiteTexture, 5f, v1, v3, v7, v5, v1);
            UnityEditor.Handles.DrawAAPolyLine(Texture2D.whiteTexture, 5f, v4, v6, v2, v8, v4);
            UnityEditor.Handles.DrawAAPolyLine(Texture2D.whiteTexture, 5f, v7, v2, v6, v3, v7);
            UnityEditor.Handles.DrawAAPolyLine(Texture2D.whiteTexture, 5f, v1, v4, v8, v5, v1);
            UnityEditor.Handles.DrawSolidRectangleWithOutline(new[] {v7, v2, v6, v3}, GetOccludedFaceColor(occluded),
                Color.clear);
            UnityEditor.Handles.DrawSolidRectangleWithOutline(new[] {v1, v4, v8, v5}, GetOccludedFaceColor(occluded),
                Color.clear);
            UnityEditor.Handles.DrawSolidRectangleWithOutline(new[] {v1, v3, v7, v5}, GetOccludedFaceColor(occluded),
                Color.clear);
            UnityEditor.Handles.DrawSolidRectangleWithOutline(new[] {v4, v6, v2, v8}, GetOccludedFaceColor(occluded),
                Color.clear);
            UnityEditor.Handles.DrawSolidRectangleWithOutline(new[] {v5, v8, v2, v7}, GetOccludedFaceColor(occluded),
                Color.clear);
            UnityEditor.Handles.DrawSolidRectangleWithOutline(new[] {v3, v6, v4, v1}, GetOccludedFaceColor(occluded),
                Color.clear);

#endif
        }

        private static Color GetVisibleOutlineColor(Color baseColor)
        {
            baseColor.a = 0.7f;
            return baseColor;
        }

        private static Color GetVisibleFaceColor(Color baseColor)
        {
            baseColor.a = 0.0f;
            return baseColor;
        }

        private static Color GetOccludedOutlineColor(Color baseColor)
        {
            baseColor.a = 0.1f;
            return baseColor;
        }

        private static Color GetOccludedFaceColor(Color baseColor)
        {
            baseColor.a = .15f;
            return baseColor;
        }
    }
}