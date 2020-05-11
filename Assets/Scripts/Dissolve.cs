using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Dissolve : MonoBehaviour
{
    float Delay;
    Color colorBegin;
    Color colorEnd;
    Transform[] allChildren;
    CanvasGroup thisCanvas;
    float start_Volume;
    public float fadeTime= 7;

    public AudioSource audio_Source;
    

    // Start is called before the first frame update
    void Start()
    {

        audio_Source = this.GetComponent<AudioSource>();
        start_Volume = audio_Source.volume;
        colorEnd = new Color(colorBegin.r, colorBegin.g, colorBegin.b, 0f);
        allChildren = gameObject.GetComponentsInChildren<Transform>();
    }

    // Update is called once per frame
    public void Update()
    {
        StartCoroutine(FadeAudioSource.StartFade(audio_Source,fadeTime , 0f));


        
        
       
        foreach (Transform child in allChildren)
        {
            if (child.gameObject.GetComponent<Renderer>() != null)
            {
                if (child.gameObject.GetComponent<Renderer>().material.color.a != 0f)
                {
                    changeAlpha(child.gameObject.GetComponent<Renderer>(), child.gameObject.GetComponent<Renderer>().material.color, colorEnd);
                    // Debug.Log(child.gameObject.GetComponent<Renderer>().material.color);
                }
                else
                {
                    Debug.Log("Destroying " + gameObject.name+ " in dissolve");
                    Destroy(gameObject);   

                }

            }
        }


    }

    void changeAlpha(Renderer rd, Color colorB, Color colorE)
    {
        Delay += Time.deltaTime;
        rd.material.color = Color.Lerp(colorB, colorE, Delay * 0.04f);
    }

    //reset the image back to opaque
    public void resetAlpha()
    {
        GetComponent<Renderer>().material.color = new Color(colorBegin.r, colorBegin.g, colorBegin.b, 1f);
    }

 //Added the comment here nivi 
 //Got the comment
}
