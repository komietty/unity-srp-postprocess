using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace kmty.srp.test.postprocess {
    public class PostProcessingPipe : RenderPipelineAsset {
        [SerializeField] protected Filter filter;
        #if UNITY_EDITOR
        [UnityEditor.MenuItem("/kmty/CreatePipeline/PostProcess")]
        static void CreateBasicAssetPipeline() {
            var instance = CreateInstance<PostProcessingPipe>();
            UnityEditor.AssetDatabase.CreateAsset(instance, "Assets/PostProcess/PostProcessPipe.asset");
        }
        #endif

        protected override RenderPipeline CreatePipeline() => new PostProcessRP(filter);
    }

    public class PostProcessRP : RenderPipeline {
        protected CommandBuffer cmd;
        protected CullingResults cull;
        protected Filter filter;

        static int colorTexID = Shader.PropertyToID("_ColorTex");
        static int depthTexID = Shader.PropertyToID("_DepthTex");

        public PostProcessRP(Filter filter) {
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
                    PreparePostProcess(ctx, cam);
                    SrpUtil.DrawOpaque(ctx, cam, cull, "SRPDefaultUnlit");
                    ExecutePostProcess(ctx, cam);
                    SrpUtil.DrawOpaque(ctx, cam, cull, "BasicPass");
                    SrpUtil.DrawSkybox(ctx, cam);
                    SrpUtil.DrawTransp(ctx, cam, cull, "SRPDefaultUnlit");
                    SrpUtil.DrawErrors(ctx, cam, cull);
                    SrpUtil.DrawGizmos(ctx, cam, GizmoSubset.PreImageEffects);
                    SrpUtil.DrawGizmos(ctx, cam, GizmoSubset.PostImageEffects);
                    SrpUtil.Execute(ctx, cmd);
                }
                ctx.Submit();
            }
        }

        void PreparePostProcess(ScriptableRenderContext ctx, Camera cam) {
            if (filter == null) return;

            var l = RenderBufferLoadAction.DontCare;
            var s = RenderBufferStoreAction.Store;
            var w = cam.pixelWidth;
            var h = cam.pixelHeight;
            cmd.GetTemporaryRT(colorTexID, w, h, 0,  FilterMode.Bilinear, RenderTextureFormat.Default);
            cmd.GetTemporaryRT(depthTexID, w, h, 24, FilterMode.Point,    RenderTextureFormat.Depth);
            cmd.SetRenderTarget(colorTexID, l, s, depthTexID, l, s);
            cmd.ClearRenderTarget(true, true, Color.clear);
            SrpUtil.Execute(ctx, cmd);
        }

        void ExecutePostProcess(ScriptableRenderContext ctx, Camera cam) {
            if (filter == null) return;
            filter.Render(cmd, colorTexID, depthTexID);
            SrpUtil.Execute(ctx, cmd);
            cmd.ReleaseTemporaryRT(colorTexID);
            cmd.ReleaseTemporaryRT(depthTexID);
            SrpUtil.Execute(ctx, cmd);

        }
    }
}
