
using UnityEngine;
using U.movin;
using Unity.VectorGraphics;

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
    private MovinLayer[] layers;
    private MovinLayer[] layersByIndex;

    public static float scaleFactor = 0.1f;
    public bool playing = false;
    public float frameRate = 0;
    public float totalFrames = 0;

    public float time = 0;                  // Local time (since animation began)
    public float frame = 0;                 // Animation frame
    public bool loop = true;

    public float strokeScale;
    public VectorUtils.TessellationOptions options;

    public Movin(Transform parent, string path, float strokeScale = 0.6f)
    {

        this.strokeScale = strokeScale;

        gameObject = new GameObject("body - " + path);
        transform.SetParent(parent, false);
        transform.localScale *= scaleFactor;

        content = BodymovinContent.init(path);
        frameRate = content.fr;
        totalFrames = content.op;
        layers = new MovinLayer[content.layers.Length];
        
        if (content.layers.Length <= 0) { Debug.Log(">>>> NO LAYERS, ABORT..."); return; }


        /* ----- SHAPE OPTIONS ----- */

        options = new VectorUtils.TessellationOptions()
        {
            StepDistance = 1000.0f,
            MaxCordDeviation = 0.05f,
            MaxTanAngleDeviation = 0.05f,
            SamplingStepSize = 0.01f
        };


        /* ----- CREATE LAYERS ----- */

        int highestIndex = 0;
        for (int i = 0; i < content.layers.Length; i++)
        {
            MovinLayer layer = new MovinLayer(this, content.layers[i]);
            layers[i] = layer;

            highestIndex = layer.content.ind > highestIndex ? layer.content.ind : highestIndex;
        }


        /* ----- ARRAY BY INDEX ----- */

        layersByIndex = new MovinLayer[highestIndex + 1];
        for (int i = 0; i < content.layers.Length; i++)
        {
            layersByIndex[layers[i].content.ind] = layers[i];
        }


        /* ----- SET PARENTS ----- */

        for (int i = 0; i < content.layers.Length; i++)
        {
            int p = layers[i].content.parent;
            if (p <= 0)
                continue;

            layers[i].transform.SetParent(layersByIndex[p].transform, false);

        }


        /* ----- GET FRAME UPDATES ----- */

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


        foreach (MovinLayer layer in layers)
        {
            layer.Update(frame);
        }
    }


    public void ResetKeyframes()
    {
        time = 0;

        foreach (MovinLayer layer in layers)
        {
            layer.ResetKeyframes();
        }
    }
}
