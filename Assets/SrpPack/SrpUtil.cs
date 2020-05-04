using UnityEngine;
using UnityEngine.Rendering;

namespace kmty.srp {
    using CTX = ScriptableRenderContext;
    using CMD = CommandBuffer;
    using CRS = CullingResults;

    public static class SrpUtil {

        public static void CreateSetRT(CTX ctx, CMD cmd, int w, int h, int colID, int dptID) {
            var l = RenderBufferLoadAction.DontCare;
            var s = RenderBufferStoreAction.Store;
            cmd.GetTemporaryRT(colID, w, h, 0,  FilterMode.Bilinear, RenderTextureFormat.Default);
            cmd.GetTemporaryRT(dptID, w, h, 24, FilterMode.Point,    RenderTextureFormat.Depth);
            cmd.SetRenderTarget(colID, l, s, dptID, l, s);
            cmd.ClearRenderTarget(true, true, Color.clear);
            Execute(ctx, cmd);
        }

        public static void Execute(CTX ctx, CMD cmd) {
            ctx.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }

        public static bool Culling(CTX ctx, Camera cam, ref CRS cull) {
            if (!cam.TryGetCullingParameters(out ScriptableCullingParameters p)) return false;
            #if UNITY_EDITOR
            if (cam.cameraType == CameraType.SceneView) CTX.EmitWorldGeometryForSceneView(cam);
            #endif
            cull = ctx.Cull(ref p);
            return true;
        }

        public static void Clear(CTX ctx, Camera cam, CMD cmd) {
            var f = cam.clearFlags;
            cmd.ClearRenderTarget(
                f <= CameraClearFlags.Depth,
                f == CameraClearFlags.Color,
                f == CameraClearFlags.Color ?  cam.backgroundColor.linear : Color.clear
            );
            Execute(ctx, cmd);
        }

        public static void DrawOpaque(CTX ctx, Camera cam, CRS cull, string tag = "SRPDefaultUnlit") {
            DrawBase(ctx, cam, cull, tag, SortingCriteria.CommonOpaque, RenderQueueRange.opaque);
        }

        public static void DrawTransp(CTX ctx, Camera cam, CRS cull, string tag = "SRPDefaultUnlit") {
            DrawBase(ctx, cam, cull, tag, SortingCriteria.CommonTransparent, RenderQueueRange.transparent);
        }

        static void DrawBase(CTX ctx, Camera cam, CRS cull, string tag, SortingCriteria c, RenderQueueRange r) {
            var sort = new SortingSettings(cam) { criteria = c };
            var draw = new DrawingSettings(new ShaderTagId(tag), sort);
            var filt = new FilteringSettings(r, cam.cullingMask);
            ctx.DrawRenderers(cull, ref draw, ref filt);
        }

        #region editor only
        public static void DrawSkybox(CTX ctx, Camera cam) {
            if (cam.clearFlags == CameraClearFlags.Skybox) ctx.DrawSkybox(cam);
        }

        public static void DrawGizmos(CTX ctx, Camera cam, GizmoSubset subset) {
            #if UNITY_EDITOR
            if (UnityEditor.Handles.ShouldRenderGizmos()) ctx.DrawGizmos(cam, subset);
            #endif
        }

        public static void PrepareForSceneWindow(Camera cam) {
            #if UNITY_EDITOR
            if (cam.cameraType == CameraType.SceneView) CTX.EmitWorldGeometryForSceneView(cam);
            #endif
        }

        static Material err;

        public static void DrawErrors(CTX ctx, Camera cam, CRS cull) {
            #if UNITY_EDITOR
            if (err == null) 
                err = new Material(Shader.Find("Hidden/InternalErrorShader"));

            var sort = new SortingSettings(cam);
            var filt = FilteringSettings.defaultValue;
            var draw = new DrawingSettings(new ShaderTagId("ForwardBase"), sort) { overrideMaterial = err };
            draw.SetShaderPassName(1, new ShaderTagId("PrepassBase"));
            draw.SetShaderPassName(2, new ShaderTagId("Always"));
            draw.SetShaderPassName(3, new ShaderTagId("Vertex"));
            draw.SetShaderPassName(4, new ShaderTagId("VertexLMRGBM"));
            draw.SetShaderPassName(5, new ShaderTagId("VertexLM"));
            ctx.DrawRenderers(cull, ref draw, ref filt);
            #endif
        }
        #endregion

    }
}
