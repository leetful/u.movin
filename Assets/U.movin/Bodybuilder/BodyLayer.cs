
using UnityEngine;

namespace U.movin
{
    public class BodyLayer
    {
        public GameObject gameObject;
        public Transform transform
        {
            get { return gameObject.transform; }
        }

        public Movin body;
        public BodymovinLayer content;
        public BodyShape[] shapes;

        public MotionProps mpos;
        public MotionProps mscale;
        public MotionProps mrotx;
        public MotionProps mroty;
        public MotionProps mrotz;
        public MotionProps mopacity;

        public Vector3 positionOffset;
        public Vector3 finalRotation = Vector3.zero;

        public bool positionAnimated = false;
        public bool scaleAnimated = false;
        public bool rotationXAnimated = false;
        public bool rotationYAnimated = false;
        public bool rotationZAnimated = false;
        public bool opacityAnimated = false;


        public BodyLayer(Movin body, BodymovinLayer layer)
        {
            this.body = body;
            this.content = layer;

            gameObject = new GameObject(content.ind + "  " + content.nm);
            transform.SetParent(body.transform, false);

            positionOffset = new Vector3(body.content.w / 2, -(body.content.h / 2), 0);

            transform.localPosition = content.position - positionOffset;
            transform.localRotation = content.rotation;
            transform.localScale = content.scale;

            finalRotation = content.rotationEuler;


            /* POSITION ANIM SETUP */

            MotionSetup(ref positionAnimated, ref mpos, content.positionSets);

            /* SCALE ANIM SETUP */

            MotionSetup(ref scaleAnimated, ref mscale, content.scaleSets);

            /* ROTATION ANIM SETUP */

            MotionSetup(ref rotationXAnimated, ref mrotx, content.rotationXSets);
            MotionSetup(ref rotationYAnimated, ref mroty, content.rotationYSets);
            MotionSetup(ref rotationZAnimated, ref mrotz, content.rotationZSets);

            /* OPACITY ANIM SETUP */

            MotionSetup(ref opacityAnimated, ref mopacity, content.opacitySets);
           


            /* SHAPES */

            //Debug.Log("layer index:  " + content.ind + "   parent:  " + content.parent);

            shapes = new BodyShape[content.shapes.Length];

            int j = 0;
            for (int i = content.shapes.Length - 1; i >= 0; i--)
            {
                BodyShape shape = new BodyShape(this, content.shapes[i], 0.85f);
                shapes[i] = shape;

                //shape.transform.localPosition += new Vector3(0, 0, -32 * j);
                j += 1;
            }
        }


        public void MotionSetup(ref bool b, ref MotionProps prop, BodymovinAnimatedProperties[] set)
        {
            b = set.Length > 0;
            if (b)
            {
                prop = new MotionProps {
                    keys = set.Length
                };

                SetKeyframe(ref prop, set, 0);
            }
        }


        public void SetKeyframe(ref MotionProps prop, BodymovinAnimatedProperties[] set, int k = 0)
        {
            prop.completed = false;
            if (prop.keys <= 0) { return; }

            prop.key = k;
            prop.startFrame = set[k].t;
            prop.endFrame = set.Length > k ? set[k + 1].t : prop.startFrame;
            prop.currentOutTangent = set[k].o;
            prop.nextInTangent = set[k].i;

            //Debug.Log("key: " + k + "   out: " + set[k].o + "     nxt in: " + set[k].i);

        }


        public void Update(float frame)
        {

            /* ----- IN + OUT POINTS FOR LAYER ----- */

            if (!gameObject.activeInHierarchy && frame >= content.inFrame) { gameObject.SetActive(true); }
            if (!gameObject.activeInHierarchy) { return; }

            if (gameObject.activeInHierarchy && (frame >= content.outFrame || frame < content.inFrame))
            {
                gameObject.SetActive(false);
                return;
            }


            /* ----- SEND DOWN UPDATES ----- */

            foreach (BodyShape shape in shapes)
            {
                shape.Update(frame);
            }


            /* ----- ANIM PROPS ----- */

            if (opacityAnimated && !mopacity.completed) {
                UpdateProperty(frame, ref mopacity, content.opacitySets);
            }
            if (positionAnimated && !mpos.completed) {
                UpdateProperty(frame, ref mpos, content.positionSets);
            }
            if (scaleAnimated && !mscale.completed) {
                UpdateProperty(frame, ref mscale, content.scaleSets);
            }
            if (rotationXAnimated && !mrotx.completed) {
                UpdateProperty(frame, ref mrotx, content.rotationXSets);
            }
            if (rotationYAnimated && !mroty.completed) {
                UpdateProperty(frame, ref mroty, content.rotationYSets);
            }
            if (rotationZAnimated && !mrotz.completed) {
                UpdateProperty(frame, ref mrotz, content.rotationZSets);
            }

            if (rotationXAnimated || rotationYAnimated || rotationZAnimated) {
                transform.localRotation = Quaternion.Euler(finalRotation);
            }
        }

        public void UpdateProperty(float frame, ref MotionProps m, BodymovinAnimatedProperties[] set)
        {

            /* ----- CHECK FOR COMPLETE ----- */

            if (m.keys <= 0) {
                //Debug.Log(">>> NO PROP KEYS TO ANIMATE!");
                m.completed = true;
                return;
            }

            if (frame >= m.endFrame)
            {
                if (m.key + 1 == set.Length - 1)
                {
                    m.completed = true;
                    //Debug.Log("****** Prop Animation done! ******");
                    return;
                }

                SetKeyframe(ref m, set, m.key + 1);
            }


            /* ----- PERCENT KEYFRAME COMPLETE ----- */

            m.percent = (frame - m.startFrame) / (m.endFrame - m.startFrame);


            /* ----- CUBIC BEZIER EASE ----- */

            float ease = Ease.CubicBezier(Vector2.zero, m.currentOutTangent, m.nextInTangent, Vector2.one, m.percent);


            /* ----- UPDATE PROPERTY ----- */

            if (set == content.positionSets) {
                transform.localPosition = Value3(m, set, ease) - positionOffset;
            } else if (set == content.scaleSets)  {
                transform.localScale = Value3(m, set, ease);
            } else if (set == content.rotationXSets) {
                finalRotation.x = Value1(m, set, ease);
            } else if (set == content.rotationYSets)  {
                finalRotation.y = Value1(m, set, ease);
            } else if (set == content.rotationZSets) {
                finalRotation.z = Value1(m, set, ease);
            } else if (set == content.opacitySets) {
                foreach (BodyShape s in shapes) {
                    s.UpdateOpacity(Value1(m, set, ease));
                }
            }

        }



        public Vector3 Value3(MotionProps m, BodymovinAnimatedProperties[] set, float ease)
        {
            return m.percent < 0 ?
                    set[m.key].s : set[m.key].s + ((set[m.key].e - set[m.key].s) * ease);
        }

        public float Value1(MotionProps m, BodymovinAnimatedProperties[] set, float ease)
        {
            return m.percent < 0 ?
                    set[m.key].sf : set[m.key].sf + ((set[m.key].ef - set[m.key].sf) * ease);
        }



        public void ResetKeyframes()
        {
            if (positionAnimated) { SetKeyframe(ref mpos, content.positionSets, 0); }
            if (scaleAnimated) { SetKeyframe(ref mscale, content.scaleSets, 0); }
            if (rotationXAnimated) { SetKeyframe(ref mrotx, content.rotationXSets, 0); }
            if (rotationYAnimated) { SetKeyframe(ref mroty, content.rotationYSets, 0); }
            if (rotationZAnimated) { SetKeyframe(ref mrotz, content.rotationZSets, 0); }
            if (opacityAnimated) { SetKeyframe(ref mopacity, content.opacitySets, 0); }

            foreach (BodyShape shape in shapes)
            {
                shape.ResetKeyframes();
            }

        }
    }
}