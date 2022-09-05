using System;
using UnityEngine;
using UnityStandardAssets.ImageEffects.UnityStandardAssets.ImageEffects;

namespace UnityStandardAssets.ImageEffects
{
     namespace UnityStandardAssets.ImageEffects
    {
        [ExecuteInEditMode]
         [RequireComponent (typeof(Camera))]
        public class PostEffectsBase : MonoBehaviour
        {
            protected bool  supportHDRTextures = true;
            protected bool  supportDX11 = false;
            protected bool  isSupported = true;
    
            protected Material CheckShaderAndCreateMaterial ( Shader s, Material m2Create)
            {
                if (!s)
                {
                    Debug.Log("Missing shader in " + ToString ());
                    enabled = false;
                }
    
                if (s.isSupported && m2Create && m2Create.shader == s)
                    return m2Create;
  
              if (!s.isSupported)
              {
                  NotSupported ();
                  Debug.Log("The shader " + s.ToString() + " on effect "+ToString()+" is not supported on this platform!");
                  return null;
              }
              else
              {
                  m2Create = new Material (s);
                  m2Create.hideFlags = HideFlags.DontSave;
                  if (m2Create)
                      return m2Create;
                  else return null;
              }
          }
  
  
          protected Material CreateMaterial (Shader s, Material m2Create)
          {
             if (!s)
               {
                   Debug.Log ("Missing shader in " + ToString ());
                   return null;
               }
   
               if (m2Create && (m2Create.shader == s) && (s.isSupported))
                   return m2Create;
   
               if (!s.isSupported)
               {
                   return null;
               }
               else
               {
                   m2Create = new Material (s);
                   m2Create.hideFlags = HideFlags.DontSave;
                   if (m2Create)
                       return m2Create;
                   else return null;
               }
          }
             void OnEnable ()
          {
              isSupported = true;
          }
  
          protected bool CheckSupport ()
          {
              return CheckSupport (false);
          }
  
  
          public virtual bool CheckResources ()
          {
              Debug.LogWarning ("CheckResources () for " + ToString() + " should be overwritten.");
              return isSupported;
          }
  
  
          protected void Start ()
           {
               CheckResources ();
           }
   
           protected bool CheckSupport (bool needDepth)
           {
               isSupported = true;
               supportHDRTextures = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf);
               supportDX11 = SystemInfo.graphicsShaderLevel >= 50 && SystemInfo.supportsComputeShaders;
   
               if (!SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures)
               {
                   NotSupported ();
                   return false;
               }
   
               if (needDepth && !SystemInfo.SupportsRenderTextureFormat (RenderTextureFormat.Depth))
               {
                   NotSupported ();
                   return false;
              }
  
              if (needDepth)
                  GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
  
              return true;
          }
  
          protected bool CheckSupport (bool needDepth,  bool needHdr)
          {
              if (!CheckSupport(needDepth))
                  return false;
  
              if (needHdr && !supportHDRTextures)
              {
                  NotSupported ();
                  return false;
              }
  
              return true;
          }
  
  
          public bool Dx11Support ()
          {
              return supportDX11;
          }
  
  
          protected void ReportAutoDisable ()
          {
              Debug.LogWarning ("The image effect " + ToString() + " has been disabled as it's not supported on the current platform.");
          }
  
          // deprecated but needed for old effects to survive upgrading
          bool CheckShader (Shader s)
          {
              Debug.Log("The shader " + s.ToString () + " on effect "+ ToString () + " is not part of the Unity 3.2+ effects suite anymore. For best performance and quality, please ensure you are using the latest Standard Assets Image Effects (Pro only) package.");
              if (!s.isSupported)
              {
                 NotSupported ();
                 return false;
             }
             else
             {
                 return false;
             }
         }
 
 
         protected void NotSupported ()
         {
             enabled = false;
             isSupported = false;
             return;
         }
 
 
         protected void DrawBorder (RenderTexture dest, Material material)
         {
             float x1;
             float x2;
             float y1;
             float y2;
               RenderTexture.active = dest;
             bool  invertY = true; // source.texelSize.y < 0.0ff;
             // Set up the simple Matrix
             GL.PushMatrix();
             GL.LoadOrtho();
 
             for (int i = 0; i < material.passCount; i++)
             {
                 material.SetPass(i);
 
                 float y1_; float y2_;
                 if (invertY)
                 {
                     y1_ = 1.0f; y2_ = 0.0f;
                 }
                 else
                 {
                     y1_ = 0.0f; y2_ = 1.0f;
                 }
                   // left
                 x1 = 0.0f;
                 x2 = 0.0f + 1.0f/(dest.width*1.0f);
                 y1 = 0.0f;
                 y2 = 1.0f;
                 GL.Begin(GL.QUADS);
 
                 GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
                 GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
                 GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
                 GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);
 
                 // right
                 x1 = 1.0f - 1.0f/(dest.width*1.0f);
                  x2 = 1.0f;
                  y1 = 0.0f;
                  y2 = 1.0f;
  
                  GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
                  GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
                  GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
                  GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);
  
                  // top
                  x1 = 0.0f;
                  x2 = 1.0f;
                  y1 = 0.0f;
                  y2 = 0.0f + 1.0f/(dest.height*1.0f);
  
                  GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
                  GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
                  GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
                  GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);
  
                  // bottom
                  x1 = 0.0f;
                  x2 = 1.0f;
                  y1 = 1.0f - 1.0f/(dest.height*1.0f);
                  y2 = 1.0f;
  
                  GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
                  GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
                  GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
                  GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);
  
                  GL.End();
              }
  
              GL.PopMatrix();
          }
       }
        
   } 
     
    [ExecuteInEditMode]
    [AddComponentMenu ("Image Effects/Color Adjustments/Color Correction (Curves, Saturation)")]
    public class ColorCorrectionCurves : PostEffectsBase
    {
        public enum ColorCorrectionMode
		{
            Simple = 0,
            Advanced = 1
        }

        public AnimationCurve redChannel = new AnimationCurve(new Keyframe(0f,0f), new Keyframe(1f,1f));
        public AnimationCurve greenChannel = new AnimationCurve(new Keyframe(0f,0f), new Keyframe(1f,1f));
        public AnimationCurve blueChannel = new AnimationCurve(new Keyframe(0f,0f), new Keyframe(1f,1f));

        public bool  useDepthCorrection = false;

        public AnimationCurve zCurve = new AnimationCurve(new Keyframe(0f,0f), new Keyframe(1f,1f));
        public AnimationCurve depthRedChannel = new AnimationCurve(new Keyframe(0f,0f), new Keyframe(1f,1f));
        public AnimationCurve depthGreenChannel = new AnimationCurve(new Keyframe(0f,0f), new Keyframe(1f,1f));
        public AnimationCurve depthBlueChannel = new AnimationCurve(new Keyframe(0f,0f), new Keyframe(1f,1f));

        private Material ccMaterial;
        private Material ccDepthMaterial;
        private Material selectiveCcMaterial;

        private Texture2D rgbChannelTex;
        private Texture2D rgbDepthChannelTex;
        private Texture2D zCurveTex;

        public float saturation = 1.0f;

        public bool  selectiveCc = false;

        public Color selectiveFromColor = Color.white;
        public Color selectiveToColor = Color.white;

        public ColorCorrectionMode mode;

        public bool  updateTextures = true;

        public Shader colorCorrectionCurvesShader = null;
        public Shader simpleColorCorrectionCurvesShader = null;
        public Shader colorCorrectionSelectiveShader = null;

        private bool  updateTexturesOnStartup = true;


        new void Start ()
		{
            base.Start ();
            updateTexturesOnStartup = true;
        }

        void Awake () {	}


        public override bool CheckResources ()
		{
            CheckSupport (mode == ColorCorrectionMode.Advanced);

            ccMaterial = CheckShaderAndCreateMaterial (simpleColorCorrectionCurvesShader, ccMaterial);
            ccDepthMaterial = CheckShaderAndCreateMaterial (colorCorrectionCurvesShader, ccDepthMaterial);
            selectiveCcMaterial = CheckShaderAndCreateMaterial (colorCorrectionSelectiveShader, selectiveCcMaterial);

            if (!rgbChannelTex)
                rgbChannelTex = new Texture2D (256, 4, TextureFormat.ARGB32, false, true);
            if (!rgbDepthChannelTex)
                rgbDepthChannelTex = new Texture2D (256, 4, TextureFormat.ARGB32, false, true);
            if (!zCurveTex)
                zCurveTex = new Texture2D (256, 1, TextureFormat.ARGB32, false, true);

            rgbChannelTex.hideFlags = HideFlags.DontSave;
            rgbDepthChannelTex.hideFlags = HideFlags.DontSave;
            zCurveTex.hideFlags = HideFlags.DontSave;

            rgbChannelTex.wrapMode = TextureWrapMode.Clamp;
            rgbDepthChannelTex.wrapMode = TextureWrapMode.Clamp;
            zCurveTex.wrapMode = TextureWrapMode.Clamp;

            if (!isSupported)
                ReportAutoDisable ();
            return isSupported;
        }

        public void UpdateParameters ()
		{
            CheckResources(); // textures might not be created if we're tweaking UI while disabled

            if (redChannel != null && greenChannel != null && blueChannel != null)
			{
                for (float i = 0.0f; i <= 1.0f; i += 1.0f / 255.0f)
				{
                    float rCh = Mathf.Clamp (redChannel.Evaluate(i), 0.0f, 1.0f);
                    float gCh = Mathf.Clamp (greenChannel.Evaluate(i), 0.0f, 1.0f);
                    float bCh = Mathf.Clamp (blueChannel.Evaluate(i), 0.0f, 1.0f);

                    rgbChannelTex.SetPixel ((int) Mathf.Floor(i*255.0f), 0, new Color(rCh,rCh,rCh) );
                    rgbChannelTex.SetPixel ((int) Mathf.Floor(i*255.0f), 1, new Color(gCh,gCh,gCh) );
                    rgbChannelTex.SetPixel ((int) Mathf.Floor(i*255.0f), 2, new Color(bCh,bCh,bCh) );

                    float zC = Mathf.Clamp (zCurve.Evaluate(i), 0.0f,1.0f);

                    zCurveTex.SetPixel ((int) Mathf.Floor(i*255.0f), 0, new Color(zC,zC,zC) );

                    rCh = Mathf.Clamp (depthRedChannel.Evaluate(i), 0.0f,1.0f);
                    gCh = Mathf.Clamp (depthGreenChannel.Evaluate(i), 0.0f,1.0f);
                    bCh = Mathf.Clamp (depthBlueChannel.Evaluate(i), 0.0f,1.0f);

                    rgbDepthChannelTex.SetPixel ((int) Mathf.Floor(i*255.0f), 0, new Color(rCh,rCh,rCh) );
                    rgbDepthChannelTex.SetPixel ((int) Mathf.Floor(i*255.0f), 1, new Color(gCh,gCh,gCh) );
                    rgbDepthChannelTex.SetPixel ((int) Mathf.Floor(i*255.0f), 2, new Color(bCh,bCh,bCh) );
                }

                rgbChannelTex.Apply ();
                rgbDepthChannelTex.Apply ();
                zCurveTex.Apply ();
            }
        }

        void UpdateTextures ()
		{
            UpdateParameters ();
        }

        void OnRenderImage (RenderTexture source, RenderTexture destination)
		{
            if (CheckResources()==false)
			{
                Graphics.Blit (source, destination);
                return;
            }

            if (updateTexturesOnStartup)
			{
                UpdateParameters ();
                updateTexturesOnStartup = false;
            }

            if (useDepthCorrection)
                GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;

            RenderTexture renderTarget2Use = destination;

            if (selectiveCc)
			{
                renderTarget2Use = RenderTexture.GetTemporary (source.width, source.height);
            }

            if (useDepthCorrection)
			{
                ccDepthMaterial.SetTexture ("_RgbTex", rgbChannelTex);
                ccDepthMaterial.SetTexture ("_ZCurve", zCurveTex);
                ccDepthMaterial.SetTexture ("_RgbDepthTex", rgbDepthChannelTex);
                ccDepthMaterial.SetFloat ("_Saturation", saturation);

                Graphics.Blit (source, renderTarget2Use, ccDepthMaterial);
            }
            else
			{
                ccMaterial.SetTexture ("_RgbTex", rgbChannelTex);
                ccMaterial.SetFloat ("_Saturation", saturation);

                Graphics.Blit (source, renderTarget2Use, ccMaterial);
            }

            if (selectiveCc)
			{
                selectiveCcMaterial.SetColor ("selColor", selectiveFromColor);
                selectiveCcMaterial.SetColor ("targetColor", selectiveToColor);
                Graphics.Blit (renderTarget2Use, destination, selectiveCcMaterial);

                RenderTexture.ReleaseTemporary (renderTarget2Use);
            }
        }
    }
}
