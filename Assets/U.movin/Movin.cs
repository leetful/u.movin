
using UnityEngine;
using u.movin;
using Unity.VectorGraphics;

namespace u.movin
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

        public Vector3 startValue;
        public Vector3 endValue;
    }
}


public class Movin
{
    public GameObject gameObject;
    public GameObject container;
    public Transform transform {
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

    public float strokeWidth;
    public int sort;
    public VectorUtils.TessellationOptions options;


    /* ---- BLENDING ---- */

    public bool blending = false;
    public BodymovinContent blendContent;
    public string blendPath;

    /* ---- EVENTS ---- */

    public System.Action OnComplete;




    public Movin(Transform parent, string path, int sort = 0, float scale = 1f, float strokeWidth = 0.5f, bool loop = true, float quality = 0.4f)
    {
        gameObject = new GameObject();
        transform.SetParent(parent, false);

        container = new GameObject();
        container.transform.SetParent(transform, false);

        MovinInit(path, sort, scale, strokeWidth, loop, quality);


        /* ----- GET FRAME UPDATES ----- */

        updater = gameObject.AddComponent<Updater>();
        updater.fired += Update;
    }


    private void MovinInit(string path, int sort = 0, float scale = 1f, float strokeWidth = 0.5f, bool loop = true, float quality = 0.4f){
        
        scale *= 0.1f;  // Reduce default scale

        gameObject.name = "body - " + path;
        container.name = "container - " + path;

        this.loop = loop;
        this.sort = sort;
        this.scale = scale;
        this.strokeWidth = strokeWidth;

        content = BodymovinContent.init(path);
        
        if (content.layers == null || content.layers.Length <= 0) {  
            Debug.Log(">>>>  NO CONTENT LAYERS, ABORT!  <<<<"); 
            return;  
        }
        
        container.transform.localScale = Vector3.one * this.scale;
        container.transform.localPosition -= new Vector3(content.w / 2, -(content.h / 2), 0) * scale;

        frameRate = content.fr;
        totalFrames = content.op;
        layers = new MovinLayer[content.layers.Length];


        /* ----- SHAPE OPTIONS ----- */

        options = new VectorUtils.TessellationOptions() {
            StepDistance = 1000.0f,
            MaxCordDeviation = 0.05f,
            MaxTanAngleDeviation = 0.05f,
            // SamplingStepSize = 0.01f
            SamplingStepSize = quality
        };


        /* ----- CREATE LAYERS ----- */

        layersByIndex = new MovinLayer[content.highestLayerIndex + 1];

        for (int i = 0; i < content.layers.Length; i++) {
            MovinLayer layer = new MovinLayer(this, content.layers[i], content.layers.Length - i);
            
            layers[i] = layer;
            layersByIndex[layer.content.ind] = layers[i];
        }

        

        /* ----- SET PARENTS ----- */

        for (int i = 0; i < layers.Length; i++) {
            MovinLayer layer = layers[i];
            int p = layer.content.parent;
            if (p <= 0){ continue; }

            layer.transform.SetParent(layersByIndex[p].content.shapes.Length > 0 ? 
                layersByIndex[p].transform.GetChild(0) : layersByIndex[p].transform, false);
        }

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

            if (blending){
                blending = false;
                UpdateLayersWithContent(blendContent, blendPath);
            }

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
        for (int i = 0; i < layers.Length; i++) {
            float f = frame - layers[i].content.startTime;
            layers[i].Update(f);
        }
    }


    private void ResetKeyframes()
    {
        time = 0;

        for (int i = 0; i < layers.Length; i++) {
            layers[i].ResetKeyframes();
        }
    }





    /* ------ PUBLIC METHODS ------ */


    public void SetColor(Color c, bool fill = true, bool stroke = false)
    {
        for (int i = 0; i < layers.Length; i++) {
            for (int j = 0; j < layers[i].shapes.Length; j++) {
                MovinShape s = layers[i].shapes[j];

                if (fill)
                    s.UpdateFillColor(c, true);

                if (stroke)
                    s.UpdateStrokeColor(c, true);
            }
        }
    }

    public void SetOpacity(float o)
    {
         for (int i = 0; i < layers.Length; i++) {
            for (int j = 0; j < layers[i].shapes.Length; j++) {
                MovinShape s = layers[i].shapes[j];
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
        for (int i = 0; i < layers.Length; i++) {
            if (n == layers[i].content.nm) { return layers[i].transform;  }
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




    public void Blend(string path, float duration = 30f, Vector2[] ease = null){
        BodymovinContent blend = BodymovinContent.init(path);

        loop = false;
        totalFrames = duration;

        time = 0;
        frame = 0;

        blending = true;
        blendPath = path;
        blendContent = blend;

        if (ease == null){ 
            ease = Ease.StrongOut; 
        }

        for (int i = 0; i < layers.Length; i++) {
            layers[i].CreateBlendKeyframe(blend.layers[i], duration, ease);
        }

        Play();
    }



    /*  DESTROY AND REPLACE CONTENTS  */


    public void ClearContent(){
        if (container == null){ return; }

        for (int i = 0; i < container.transform.childCount; i++){
            if (Application.isPlaying){
                Object.Destroy(container.transform.GetChild(i).gameObject);
            } else {
                Object.DestroyImmediate(container.transform.GetChild(i).gameObject);
            }
        }
    }

    public void ChangeContent(string path, int sort = 0, float scale = 1f, float strokeWidth = 0.5f, bool loop = true, float quality = 0.4f){
        ClearContent();
        MovinInit(path, sort, scale, strokeWidth, loop, quality);
    } 



    /*  REPLACE EXISTING LAYER CONTENT WITH NEW DATA  */
    

    public void UpdateLayersWithContent(string path){
        UpdateLayersWithContent(BodymovinContent.init(path), path);
    }

    public void UpdateLayersWithContent(BodymovinContent c, string path){
        content = c;

        gameObject.name = "body - " + path;
        container.name = "container - " + path;
        container.transform.localPosition = Vector3.zero;
        container.transform.localPosition -= new Vector3(content.w / 2, -(content.h / 2), 0) * scale;

        frameRate = content.fr;
        totalFrames = content.op;

        time = 0;
        frame = 0;

        for (int i = 0; i < layers.Length; i++) {
            layers[i].UpdateLayersWithContent(content.layers[i]);
        }

        loop = true;
        Play();
    }

}
