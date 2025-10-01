using UnityEngine;

[ExecuteAlways, RequireComponent(typeof(Camera))]
public class FisheyeImageEffect : MonoBehaviour
{
    [Range(0,1)] public float strength = 0.3f;
    public Shader fisheyeShader;  // "Hidden/Freecam/Fisheye"
    Material _mat;

    void OnEnable(){ if (!fisheyeShader) fisheyeShader = Shader.Find("Hidden/Freecam/Fisheye"); if (fisheyeShader) _mat = new Material(fisheyeShader); }
    void OnDisable(){ if (_mat) DestroyImmediate(_mat); }
    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (_mat){ _mat.SetFloat("_Strength", strength); Graphics.Blit(src, dst, _mat); }
        else Graphics.Blit(src, dst);
    }
}
