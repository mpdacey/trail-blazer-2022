using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalTorch : MonoBehaviour
{
    public FinalAnimation finalAnimation;
    private int currentlyLit = 0;
    public int FinalLitTorches
    {
        get { return currentlyLit; }
    }

    public void LightLock()
    {
        currentlyLit++;
    }

    public void ResetFinalTorch()
    {
        finalAnimation.ResetTrigger();
        currentlyLit = 0;
        GetComponent<Torch>().ResetTorch();
        GetComponent<SphereCollider>().enabled = false;

        for(int i = 1; i < transform.childCount; i++)
        {
            GameObject torch = transform.GetChild(i).gameObject;
            torch.GetComponent<Torch>().ResetTorch();
            torch.GetComponent<SphereCollider>().enabled = false;
        }
    }
}
