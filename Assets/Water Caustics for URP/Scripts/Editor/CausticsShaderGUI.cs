using UnityEngine;
using UnityEditor;
using UnityEditor.Rendering;

namespace WaterCausticsForURP
{
    public class CausticsShaderGUI : ShaderGUI
    {
        public enum LightDirection
        {
            DirectionalLight,
            Fixed,
        }

        private LightDirection lightDirection;
        private Vector3 fixedDirection;
        private SerializedProperty lightDirectionProperty;

        private bool showTextureSettings, showVisualsSettings, showLightInfluenceSettings, showEdgeFadingSettings;

        private delegate void DrawSettingsMethod(MaterialEditor materialEditor, MaterialProperty[] properties);

        Material targetMaterial;

        static GUIStyle _centeredGreyMiniLabel;

        private static GUIStyle CenteredGreyMiniLabel =>
            _centeredGreyMiniLabel ??= new GUIStyle(
                GUI.skin.FindStyle("MiniLabel") ??
                EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector)
                    .FindStyle("MiniLabel"))
            {
                alignment = TextAnchor.MiddleLeft,
                normal = {textColor = Color.gray}
            };

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            targetMaterial = materialEditor.target as Material;

            EditorGUILayout.LabelField("Water Caustics for URP v1.0.0", EditorStyles.boldLabel);
            if (GUILayout.Button("@alexanderameye", CenteredGreyMiniLabel))
                Application.OpenURL("https://twitter.com/alexanderameye");

            EditorGUILayout.Space();
            CoreEditorUtils.DrawSplitter();
            showTextureSettings = CoreEditorUtils.DrawHeaderFoldout("Texture", showTextureSettings);
            DrawPropertiesInspector(showTextureSettings, materialEditor, properties, DrawTextureSettings);

            showVisualsSettings = CoreEditorUtils.DrawHeaderFoldout("Visuals", showVisualsSettings);
            DrawPropertiesInspector(showVisualsSettings, materialEditor, properties, DrawVisualsSettings);

            showLightInfluenceSettings =
                CoreEditorUtils.DrawHeaderFoldout("Light influence", showLightInfluenceSettings);
            DrawPropertiesInspector(showLightInfluenceSettings, materialEditor, properties, DrawLightInfluenceSettings);

            showEdgeFadingSettings = CoreEditorUtils.DrawHeaderFoldout("Edge fading", showEdgeFadingSettings);
            DrawPropertiesInspector(showEdgeFadingSettings, materialEditor, properties, DrawEdgeFadingSettings);
        }

        void DrawTextureSettings(MaterialEditor editor, MaterialProperty[] properties)
        {
            editor.ShaderProperty(FindProperty("_CausticsTexture", properties),
                EditorGUIUtility.TrTextContent("Texture"));
            editor.ShaderProperty(FindProperty("_CausticsScale", properties), "Scale");
            editor.ShaderProperty(FindProperty("_CausticsSpeed", properties), "Speed");
        }

        void DrawVisualsSettings(MaterialEditor editor, MaterialProperty[] properties)
        {
            editor.ShaderProperty(FindProperty("_CausticsStrength", properties), "Intensity");
            editor.ShaderProperty(FindProperty("_CausticsSplit", properties),
                EditorGUIUtility.TrTextContent("RGB Split",
                    "How much the light should fall out into the color spectrum."));
        }

        void DrawLightInfluenceSettings(MaterialEditor editor, MaterialProperty[] properties)
        {
            EditorGUI.BeginChangeCheck();
            lightDirection = (LightDirection) EditorGUILayout.EnumPopup("Light direction", lightDirection);
            if (EditorGUI.EndChangeCheck())
            {
                if (lightDirection == LightDirection.Fixed) targetMaterial.EnableKeyword("FIXED_LIGHT_DIRECTION");
                else targetMaterial.DisableKeyword("FIXED_LIGHT_DIRECTION");
            }

            if (lightDirection == LightDirection.Fixed)
            {
                EditorGUI.BeginChangeCheck();
                fixedDirection = EditorGUILayout.Vector3Field("Direction", fixedDirection);
                if (EditorGUI.EndChangeCheck())
                {
                    Matrix4x4 fixedDirectionMatrix = Matrix4x4.TRS(Vector3.zero,
                        Quaternion.Euler(fixedDirection.x, fixedDirection.y, fixedDirection.z),
                        Vector3.one);
                    targetMaterial.SetMatrix("_FixedLightDirection", fixedDirectionMatrix);
                }
            }

            // editor.ShaderProperty(FindProperty("_CausticsNdotLMaskStrength", properties), "NdotL Mask");
            editor.ShaderProperty(FindProperty("_CausticsSceneLuminanceMaskStrength", properties),
                EditorGUIUtility.TrTextContent("Luminance Mask",
                    "How much to mask the light based on the scene luminance."));
        }

        void DrawEdgeFadingSettings(MaterialEditor editor, MaterialProperty[] properties)
        {
            editor.ShaderProperty(FindProperty("_CausticsFadeAmount", properties),
                EditorGUIUtility.TrTextContent("Fade Amount",
                    "How much to fade the caustics effect at the edges of the volume."));
            editor.ShaderProperty(FindProperty("_CausticsFadeHardness", properties),
                EditorGUIUtility.TrTextContent("Fade Hardness", "How harsh the border of the fade should be."));
        }

        private static void DrawPropertiesInspector(bool active, MaterialEditor editor, MaterialProperty[] properties,
            DrawSettingsMethod Drawer)
        {
            if (active)
            {
                EditorGUI.indentLevel++;
                Drawer(editor, properties);
                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }

            CoreEditorUtils.DrawSplitter();
        }
    }
}