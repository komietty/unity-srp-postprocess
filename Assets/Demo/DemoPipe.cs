using UnityEngine;
using UnityEngine.Rendering;

namespace kmty.srp.test.postprocess {
    public class DemoPipe : RenderPipelineAsset {
        [SerializeField] protected DemoFilter filter;
        #if UNITY_EDITOR
        [UnityEditor.MenuItem("/kmty/CreatePipeline/Demo")]
        static void CreateBasicAssetPipeline() {
            var instance = CreateInstance<DemoPipe>();
            UnityEditor.AssetDatabase.CreateAsset(instance, "Assets/Demo/DemoPipe.asset");
        }
        #endif

        protected override RenderPipeline CreatePipeline() => new DemoRP(filter);
    }

    public class DemoRP : RenderPipeline {
        protected CommandBuffer cmd;
        protected CullingResults cull;
        protected DemoFilter filter;

        static int colorTexID = Shader.PropertyToID("_MainTex");
        static int depthTexID = Shader.PropertyToID("_DepthTex");

        public DemoRP(DemoFilter filter) {
            this.cmd = new CommandBuffer() { name = "default" };
            this.filter = filter;
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            cmd.Release();
        }

        protected override void Render(ScriptableRenderContext ctx, Camera[] cams) {
            foreach (var cam in cams) {
                ctx.SetupCameraProperties(cam);
                if (Application.isEditor) cmd.name = cam.name;
                SrpUtil.PrepareForSceneWindow(cam);
                if (!SrpUtil.Culling(ctx, cam, ref cull)) continue;
                {
                    PreparePostProcess(ctx, cam, colorTexID, depthTexID);
                    SrpUtil.DrawOpaque(ctx, cam, cull);
                    SrpUtil.DrawOpaque(ctx, cam, cull, "UnlitPreFilter");
                    SrpUtil.DrawSkybox(ctx, cam);
                    ExecutePostProcess(ctx, cam, colorTexID, depthTexID);
                    SrpUtil.DrawOpaque(ctx, cam, cull, "UnlitPostFilter");
                    SrpUtil.DrawTransp(ctx, cam, cull);
                    SrpUtil.DrawErrors(ctx, cam, cull);
                    SrpUtil.DrawGizmos(ctx, cam, GizmoSubset.PreImageEffects);
                    SrpUtil.DrawGizmos(ctx, cam, GizmoSubset.PostImageEffects);
                    SrpUtil.Execute(ctx, cmd);
                }
                ctx.Submit();
            }
        }

        void PreparePostProcess(ScriptableRenderContext ctx, Camera cam, int color, int depth) {
            if (filter == null) return;

            var l = RenderBufferLoadAction.DontCare;
            var s = RenderBufferStoreAction.Store;
            var w = cam.pixelWidth;
            var h = cam.pixelHeight;
            cmd.GetTemporaryRT(color, w, h, 0,  FilterMode.Bilinear, RenderTextureFormat.Default);
            cmd.GetTemporaryRT(depth, w, h, 24, FilterMode.Point,    RenderTextureFormat.Depth);
            cmd.SetRenderTarget(color, l, s, depth, l, s);
            cmd.ClearRenderTarget(true, true, Color.clear);
            SrpUtil.Execute(ctx, cmd);
        }

        void ExecutePostProcess(ScriptableRenderContext ctx, Camera cam, int color, int depth) {
            if (filter == null) return;
            filter.Render(cmd, color, depth, cam.pixelWidth, cam.pixelHeight);
            SrpUtil.Execute(ctx, cmd);
            cmd.ReleaseTemporaryRT(color);
            cmd.ReleaseTemporaryRT(depth);
            SrpUtil.Execute(ctx, cmd);
        }
    }
}
