using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "kmty/CreateFilter/Demo")]
public class Filter : ScriptableObject {
    [SerializeField] protected Material invertColor;
    [SerializeField] protected Material copyDepth;

    public void Render(CommandBuffer cmd, int colerTexID, int depthTexID) {
        cmd.Blit(colerTexID, BuiltinRenderTextureType.CameraTarget, invertColor);
        cmd.Blit(depthTexID, BuiltinRenderTextureType.CameraTarget, copyDepth);
    }
}