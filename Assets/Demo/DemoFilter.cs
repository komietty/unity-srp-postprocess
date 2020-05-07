using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "kmty/CreateFilter/Demo")]
public class DemoFilter : ScriptableObject {
    [SerializeField] protected Material copyDepth;
    [SerializeField] protected Material effectMat;

    public void Render(CommandBuffer cmd, int colerTexID, int depthTexID, int w, int h) {
        cmd.Blit(colerTexID, BuiltinRenderTextureType.CameraTarget, effectMat);
        cmd.Blit(depthTexID, BuiltinRenderTextureType.CameraTarget, copyDepth);
    }
}