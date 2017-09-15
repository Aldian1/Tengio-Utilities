namespace Tengio
{


    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class AutoSetlayer : MonoBehaviour
    {
        private InteractableBehaviour interactable;
        void Start()
        {
            interactable = GetComponentInParent<InteractableBehaviour>();
        }
        private void Update()
        {
            if (interactable.IsDragging())
            {
                this.GetComponent<SpriteRenderer>().sortingLayerName = GetComponentInParent<SpriteRenderer>().sortingLayerName;
            }
        }
    }
}