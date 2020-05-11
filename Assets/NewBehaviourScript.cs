using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    float Delay;
    Color colorBegin;
    Color colorEnd;
    Transform[] allChildren;
    CanvasGroup thisCanvas;

    Renderer rd_old;

    //Start is called before the first frame update
    void Start()
    {
        colorEnd = new Color(colorBegin.r, colorBegin.g, colorBegin.b, 0f);
        rd_old = gameObject.GetComponent<Renderer>();

    }

    // Update is called once per frame
    void Update()
    {
        allChildren = gameObject.GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child.gameObject.GetComponent<Renderer>() != null)
            {                
                if (child.gameObject.GetComponent<Renderer>().material.color.a != 0f)
                    changeAlpha(child.gameObject.GetComponent<Renderer>(), child.gameObject.GetComponent<Renderer>().material.color, colorEnd);

            }
        }

        //changeAlpha();
        //resetAlpha();
    }

    //function to change the alpha value - to fade and destroy the object  
     void changeAlpha(Renderer rd, Color colorB, Color colorE){   

            Delay += Time.deltaTime;
            rd.material.color = Color.Lerp(colorB, colorE, Delay * 0.04f);
    }
    
    //function to reset the alpha value - to make the object visible
    void resetAlpha()
    {
        GetComponent<Renderer>().material.color = new Color(colorBegin.r, colorBegin.g, colorBegin.b, 1f);
    }

}
