# unity-arbitrary-postprocess

Scriptable rendering pipeline(SRP) sample aming to test execute postprocess at arbitrary timing.
With SRP it is now enabled to add postprocess even inside Opaque/Transparent rendering.

## Usage
```cs
 protected override void Render(ScriptableRenderContext ctx, Camera[] cams) {
            foreach (var cam in cams) {
                ctx.SetupCameraProperties(cam);
                SrpUtil.PrepareForSceneWindow(cam);
                if (!SrpUtil.Culling(ctx, cam, ref cull)) continue;
                {
                    PreparePostProcess(ctx, cam, colorTexID, depthTexID);
                    SrpUtil.DrawOpaque(ctx, cam, cull);
                    SrpUtil.DrawOpaque(ctx, cam, cull, "UnlitPreFilter"); // postprocess affected
                    SrpUtil.DrawSkybox(ctx, cam);
                    ExecutePostProcess(ctx, cam, colorTexID, depthTexID); // execute
                    SrpUtil.DrawOpaque(ctx, cam, cull, "UnlitPostFilter"); // postprocess not affected
                    SrpUtil.DrawTransp(ctx, cam, cull);
                    SrpUtil.DrawErrors(ctx, cam, cull);
                    SrpUtil.DrawGizmos(ctx, cam, GizmoSubset.PreImageEffects);
                    SrpUtil.DrawGizmos(ctx, cam, GizmoSubset.PostImageEffects);
                    SrpUtil.Execute(ctx, cmd);
                }
                ctx.Submit();
            }
        }
```

## Compability

Tested on Unity 2019.3.11f1, windows10 (RTX 2080 max-q).

## License
[MIT](LICENSE)
