#if UNIVERSAL_RENDERER
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace WaterCausticsForURP
{
    public enum CausticsDirection
    {
        DirectionalLight,
        Fixed,
    }

    public class CausticsFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class CausticsSettings
        {
       
        }

        CausticsPass pass;
        public CausticsSettings settings = new CausticsSettings();

        public override void Create()
        {
            pass = new CausticsPass(settings);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            pass.Setup();
            renderer.EnqueuePass(pass);
        }
    }

    public class CausticsPass : ScriptableRenderPass
    {
        private readonly CausticsFeature.CausticsSettings settings;
        private static readonly int MainLightDirection = Shader.PropertyToID("_MainLightDirection");

        public CausticsPass(CausticsFeature.CausticsSettings settings)
        {
            this.settings = settings;
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        public void Setup()
        {
            // require normals to be generated
            // note: this does not play nicely with terrain
            // ConfigureInput(ScriptableRenderPassInput.Normal);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cam = renderingData.cameraData.camera;
            if (cam.cameraType == CameraType.Preview) return;

            var sunMatrix = RenderSettings.sun != null
                ? RenderSettings.sun.transform.localToWorldMatrix
                : Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90.0f, 0.0f, 0.0f), Vector3.one);

            Shader.SetGlobalMatrix(MainLightDirection, sunMatrix);
        }
    }
}
#endif