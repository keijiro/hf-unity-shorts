// Regenerates assets/bunny-render.png — the hero cutout used by #bunny in index.html.
//
// Run against a live Unity Editor holding the Bunny Blitz project:
//
//   cd /Users/keijiro/Projects/bunny-blitz/worktree-pipeline-eval
//   unity open . --args -automated
//   until unity status --json | grep -q '"state": *"ready"'; do sleep 5; done
//   unity command eval_file \
//     --file ../../hf-test/videos/eval-code-to-coins/tools/render-bunny.cs --timeout 180
//
// That readiness loop is not optional. Until `unity status` reports "ready",
// `unity command` cannot connect at all and every call fails with
// STATUS_NO_INSTANCES — which reads like "no Editor is running" rather than
// "still importing". A cold first open of this project takes several minutes.
//
// Notes that cost time to rediscover:
//   * Render Prefab_Rabbit, NOT Player.prefab. Both wrap the same rig, but the
//     Player instance overrides the materials to MV_Rabbit_A/B (SG_3d_Rabbit),
//     which is the in-game variant and expects the scene's 2D lighting. Rendered
//     in isolation it comes out wrong-looking with no error to tell you so.
//     Prefab_Rabbit keeps MAT_rabbitA/B, which stand on their own (see below).
//   * MAT_rabbitA/B use ShaderGraph_MatCap_Lighting, so the character needs no scene
//     lighting to look right — an isolated render matches the game.
//   * The camera is ORTHOGRAPHIC on purpose: Bunny Blitz is a URP 2D project, so ortho
//     is the faithful projection, and it keeps the cutout undistorted.
//   * RenderTexture.antiAliasing must stay 1. Anything above the URP asset's MSAA count
//     makes URP log "Attachment 0 was created with 4 samples but 8 samples were requested"
//     and hand back a fully transparent frame.
//   * The frame is fit tightly to the renderers' world bounds, so OUT_H and the model's
//     own aspect decide the PNG size. index.html hard-codes that aspect (0.5525) as a
//     116x210 box; re-check #bunny if the rig or pose ever changes.
//   * `eval` compiles the file as a method body: no `using` directives, and `Object` is
//     ambiguous — spell out UnityEngine.Object.
//   * This script adds nothing to the Unity project: it saves no scene and imports no
//     asset. If that worktree shows uncommitted changes to
//     ProjectSettings.asset (resizableWindow), Assets/Editor/DevBuilder.cs or
//     Assets/Settings/Pipeline/, those are the com.unity.pipeline package's own
//     doing and have nothing to do with this video — don't go hunting for what
//     the capture broke.

const string PREFAB = "Assets/BunnyBlitz/Game/Prefabs/Prefab_Rabbit.prefab";
const string OUT = "/Users/keijiro/Projects/hf-test/videos/eval-code-to-coins/assets/bunny-render.png";
const int OUT_H = 800;      // pixel height; width follows the model's aspect
const float MARGIN = 0.02f; // breathing room so antialiased edges are not clipped
const float YAW = 195f;     // 180 = dead-on front; 195 gives a slight 3/4 turn
const int LAYER = 31;       // scratch layer, so the camera sees only the character

UnityEditor.SceneManagement.EditorSceneManager.NewScene(
    UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
    UnityEditor.SceneManagement.NewSceneMode.Single);

var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB);
if (prefab == null) return "prefab not found: " + PREFAB;

var go = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
go.transform.position = Vector3.zero;
go.transform.rotation = Quaternion.Euler(0f, YAW, 0f);
go.transform.localScale = Vector3.one;
foreach (var t in go.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = LAYER;

var rends = go.GetComponentsInChildren<Renderer>(true);
foreach (var r in rends) { var s = r as SkinnedMeshRenderer; if (s != null) s.updateWhenOffscreen = true; }

bool has = false; var b = new Bounds();
foreach (var r in rends) { if (!has) { b = r.bounds; has = true; } else b.Encapsulate(r.bounds); }

float halfH = b.extents.y * (1f + MARGIN);
float halfW = b.extents.x * (1f + MARGIN);
int W = Mathf.RoundToInt(OUT_H * (halfW / halfH));
if (W % 2 != 0) W++;

var camGo = new GameObject("__CutoutCam");
camGo.layer = LAYER;
var cam = camGo.AddComponent<Camera>();
cam.cullingMask = 1 << LAYER;
cam.clearFlags = CameraClearFlags.SolidColor;
cam.backgroundColor = new Color(0f, 0f, 0f, 0f); // transparent, so no green-screen key needed
cam.orthographic = true;
cam.orthographicSize = halfH;
cam.aspect = (float)W / OUT_H;
cam.nearClipPlane = 0.01f;
cam.farClipPlane = 100f;
cam.allowHDR = false;
cam.transform.position = new Vector3(b.center.x, b.center.y, b.center.z - 20f);
cam.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);

var urpType = System.Type.GetType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
if (urpType != null)
{
  var data = camGo.AddComponent(urpType);
  var pp = urpType.GetProperty("renderPostProcessing");
  if (pp != null) pp.SetValue(data, false);
}

// Harmless with the matcap materials, but keeps the render sane if they ever go lit.
var lightGo = new GameObject("__CutoutLight");
lightGo.layer = LAYER;
var lt = lightGo.AddComponent<Light>();
lt.type = LightType.Directional;
lt.intensity = 1.2f;
lt.cullingMask = 1 << LAYER;
lightGo.transform.rotation = Quaternion.Euler(35f, -35f, 0f);

var rt = new RenderTexture(W, OUT_H, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
rt.antiAliasing = 1;
cam.targetTexture = rt;
cam.Render();
var prev = RenderTexture.active;
RenderTexture.active = rt;
var tex = new Texture2D(W, OUT_H, TextureFormat.RGBA32, false);
tex.ReadPixels(new Rect(0, 0, W, OUT_H), 0, 0);
tex.Apply();
RenderTexture.active = prev;
cam.targetTexture = null;

System.IO.File.WriteAllBytes(OUT, tex.EncodeToPNG());

UnityEngine.Object.DestroyImmediate(tex);
rt.Release();
UnityEngine.Object.DestroyImmediate(rt);
UnityEngine.Object.DestroyImmediate(camGo);
UnityEngine.Object.DestroyImmediate(lightGo);
UnityEngine.Object.DestroyImmediate(go);

return "wrote " + OUT + " (" + W + "x" + OUT_H + ", aspect " + ((float)W / OUT_H).ToString("F4") + ")";
