using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Magio
{
    [ExecuteAlways]
    public class GrassBinder : MonoBehaviour
    {
        public MagioObjectEffect eff;

        public Transform interactable;

        public float interactableSize = 1;

        Vector3 interactablePrevPos = Vector3.zero;
        bool firstPrevPos = false;

        private void Start()
        {
            MagioObjectEffect.VFXProps prop = eff.GetVFXPropertyValue("InteractableSize");
            prop.floatValue = interactableSize;
            eff.SetVFXPropertyValue(prop);
        }

        // Update is called once per frame
        void Update()
        {
            if (!eff) return;
            MagioObjectEffect.VFXProps prop = eff.GetVFXPropertyValue("InteractablePosition");
            prop.vector3Value = interactable.position;
            eff.SetVFXPropertyValue(prop);

            if (!firstPrevPos)
            {
                interactablePrevPos = interactable.position;
                firstPrevPos = true;
            }
            else
            {
                MagioObjectEffect.VFXProps velProp = eff.GetVFXPropertyValue("InteractableVelocity");
                velProp.vector3Value = (interactable.position - interactablePrevPos) / Time.deltaTime;
                eff.SetVFXPropertyValue(velProp);

                interactablePrevPos = interactable.position;
            }
        }
    }
}

