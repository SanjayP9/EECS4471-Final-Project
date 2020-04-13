using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereTranslate : MonoBehaviour
{
    public bool collideWithControl;

    private void OnTriggerStay(Collider other)
    {
        if (!collideWithControl)
            collideWithControl = other.gameObject.name == "SphereControl";
    }

    private void OnTriggerExit(Collider other)
    {
        collideWithControl = false;
    }
}
