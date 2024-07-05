using Leap;
using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticHandProvider : PostProcessProvider
{

    public Chirality chirality = Chirality.Right;

    public override void ProcessFrame(ref Frame inputFrame)
    {
        if (inputFrame == null) { return; }

        Hand h = inputFrame.GetHand(chirality);

        if(h!= null)
        {
            h.SetTransform(transform.position, transform.rotation);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
