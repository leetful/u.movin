
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
    public GameObject container;

    public Transform transform
    {
        get { return gameObject.transform; }
    }

    public BodymovinContent content;
    public Updater updater;
    private MovinLayer[] layers;
    private MovinLayer[] layersByIndex;

    public float scale;
    public bool playing = false;
    public bool paused = false;
    public float frameRate = 0;
    public float totalFrames = 0;

    public float time = 0;                  // Local time (since animation began)
    public float frame = 0;                 // Animation frame
    public bool loop;
    public bool complete = false;
    public float quality;

    public float strokeScale;
    public int sort;
    public VectorUtils.TessellationOptions options;


    /* ---- EVENTS ---- */

    public System.Action OnComplete;


    public Movin(Transform parent, string path, int sort = 0, float scale = 0.1f, float strokeScale = 0.5f, bool loop = true, float quality = 0.4f)
    {
        this.loop = loop;
        this.sort = sort;
        this.scale = scale;
        this.strokeScale = strokeScale;

        content = BodymovinContent.init(path);

        gameObject = new GameObject("body - " + path);
        transform.SetParent(parent, false);
       
        container = new GameObject("container - " + path);
        container.transform.SetParent(transform, false);
        container.transform.localScale *= scale;
        container.transform.localPosition -= new Vector3(content.w / 2, -(content.h / 2), 0) * scale;

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
            // SamplingStepSize = 0.01f
            SamplingStepSize = quality
        };


        /* ----- CREATE LAYERS ----- */

        layersByIndex = new MovinLayer[content.highestLayerIndex + 1];
        // Debug.Log("highestIndex:  "  + content.highestLayerIndex);

        for (int i = 0; i < content.layers.Length; i++)
        {
            MovinLayer layer = new MovinLayer(this, content.layers[i], content.layers.Length - i);
            
            layers[i] = layer;
            layersByIndex[layer.content.ind] = layers[i];
        }

        

        /* ----- SET PARENTS ----- */

        foreach (MovinLayer layer in layers){
            int p = layer.content.parent;
            if (p <= 0){ continue; }

            layer.transform.SetParent(layersByIndex[p].content.shapes.Length > 0 ? 
                layersByIndex[p].transform.GetChild(0) : layersByIndex[p].transform, false);
        }


        /* ----- GET FRAME UPDATES ----- */

        updater = gameObject.AddComponent<Updater>();
        updater.fired += Update;
    }



    private void Update()
    {
        if (!playing) { return; }

        time += Time.deltaTime;
        frame = time * frameRate;

        //Debug.Log("t:  " + time);

        if (frame >= totalFrames)
        {
            Stop();
            
            //Debug.Log("****** COMP Animation done! ******");
            complete = !loop;
            OnComplete?.Invoke();

            if (loop)
            {
                ResetKeyframes();
                Play();
            }

            return;
        }

        UpdateLayers();
    }

    public void UpdateLayers()
    {
        foreach (MovinLayer layer in layers)
        {
            float f = frame - layer.content.startTime;
            layer.Update(f);
        }
    }


    private void ResetKeyframes()
    {
        time = 0;

        foreach (MovinLayer layer in layers)
        {
            layer.ResetKeyframes();
        }
    }





    /* ------ PUBLIC METHODS ------ */


    public void SetColor(Color c, bool fill = true, bool stroke = false)
    {
        foreach (MovinLayer layer in layers)
        {
            foreach (MovinShape s in layer.shapes)
            {
                if (fill)
                    s.UpdateFillColor(c, true);

                if (stroke)
                    s.UpdateStrokeColor(c, true);
            }
        }
    }

    public void SetOpacity(float o)
    {
        foreach (MovinLayer layer in layers)
        {
            foreach (MovinShape s in layer.shapes)
            {
                s.UpdateOpacity(o * 100f);
            }
        }
    }

    public void RandomFrame(bool play = false)
    {
        int n = Random.Range(0, (int)totalFrames);
        SetFrame(n, play);
    }
    
    public void SetFrame(int n = 0, bool play = false)
    {
        frame = Mathf.Clamp(n, 0, totalFrames);
        time = frame / frameRate;

        UpdateLayers();
        
        if (play)  {
            playing = true;
        }
    }

    public Transform FindLayer(string n)
    {
        foreach (MovinLayer layer in layers)
        {
            if (n == layer.content.nm) { return layer.transform;  }
        }
        return null;
    }


    public void Play()
    {
        if (complete){
            complete = false;
            ResetKeyframes();
        }

        playing = true;
        paused = false;
    }

    public void Pause()
    {
        playing = false;
        paused = true;
    }

    public void Stop()
    {
        playing = false;
        paused = false;
    }


}
