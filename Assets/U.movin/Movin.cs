
using UnityEngine;
using U.movin;

namespace U.movin
{
    public struct MotionProps
    {
        public int key;                     // Current keyframe
        public int keys;                    // Total keyframes
        public float startFrame;            // Frame current animation started
        public float endFrame;              // Frame current animation ends
        public float percent;               // Percentage to reach next key
        public bool completed;              // Animation complete

        public Vector2 currentOutTangent;   // Current keyframe out tangent
        public Vector2 nextInTangent;       // Next keyframe in tangent
    }
}


public class Movin
{
    public GameObject gameObject;
    public Transform transform
    {
        get { return gameObject.transform; }
    }

    public BodymovinContent content;
    public Updater updater;
    private BodyLayer[] layers;

    public static float scaleFactor = 0.1f;
    public bool playing = false;
    public float frameRate = 0;
    public float totalFrames = 0;

    public float time = 0;                  // Local time (since animation began)
    public float frame = 0;                 // Animation frame
    public bool loop = true;


    public Movin(Transform parent, string path)
    {
        gameObject = new GameObject("body - " + path);
        transform.SetParent(parent, false);
        transform.localScale *= scaleFactor;

        content = BodymovinContent.init(path);
        frameRate = content.fr;
        totalFrames = content.op;
        layers = new BodyLayer[content.layers.Length];


        for (int i = 0; i < content.layers.Length; i++)
        {

            BodyLayer layer = new BodyLayer(this, content.layers[i]);
            //layers[i] = layer;
            layers[layer.content.ind -1] = layer;
        }

        for (int i = 0; i < content.layers.Length; i++)
        {
            int p = layers[i].content.parent;
            if (p == 0)
                continue;

            layers[i].transform.SetParent(layers[p - 1].transform, false);
            
        }

        updater = gameObject.AddComponent<Updater>();
        updater.fired += Update;
    }

    public void Play()
    {
        playing = true;
    }

    public void Stop()
    {
        playing = false;
    }




    public void Update()
    {
        if (!playing) { return; }

        time += Time.deltaTime;
        frame = time * frameRate;

        //Debug.Log("t:  " + time);

        if (frame >= totalFrames)
        {
            Stop();
            //Debug.Log("****** COMP Animation done! ******");

            if (loop)
            {
                ResetKeyframes();
                Play();
            }

            return;
        }


        foreach (BodyLayer layer in layers)
        {
            layer.Update(frame);
        }
    }


    public void ResetKeyframes()
    {
        time = 0;

        foreach (BodyLayer layer in layers)
        {
            layer.ResetKeyframes();
        }
    }
}
