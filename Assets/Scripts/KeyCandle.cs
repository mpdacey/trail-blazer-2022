using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyCandle : MonoBehaviour
{
    public GameObject lockedTorch;

    public void TriggerTorchLock(GameObject player)
    {
        if(lockedTorch != null)
        {
            if(lockedTorch.TryGetComponent<LockedTorch>(out LockedTorch component))
            {
                component.LightLock(player);
            }
            else
            {
                Debug.LogWarning("Torch doesn't have LockedTorch component");
            }
        }
        else
        {
            Debug.LogError("No Allecated Locked Torch");
        }
    }
}
