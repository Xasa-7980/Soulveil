using UnityEngine;
using UnityEngine.Events;
namespace AtmosferaAnime
{
 // Optional hook on a VFX prefab. Keeps the package independent from VFX Graph.
 // Connect these UnityEvents to your own prefab controller when needed.
 public sealed class StatusVFXPrefabReceiver : MonoBehaviour
 {
  public UnityEvent<float> intensityChanged=new UnityEvent<float>();
  public UnityEvent activated=new UnityEvent();
  public UnityEvent deactivated=new UnityEvent();
  public float Intensity {get;private set;}
  internal void SetIntensity(float value)
  {
   if(Mathf.Abs(Intensity-value)<.001f)return;
   Intensity=value;intensityChanged.Invoke(value);
  }
  internal void Activate(){activated.Invoke();}
  internal void Deactivate(){deactivated.Invoke();}
 }
}
