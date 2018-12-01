using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VectorGraphics;

namespace U.movin
{
    public class MovinShapeSlave
    {
        public GameObject gameObject;
        public Transform transform
        {
            get { return gameObject.transform; }
        }

        public MovinShape master;
        public BodymovinShape content;
        public Shape shape;
        public Scene scene;
        public Mesh mesh;
        public MeshFilter filter;
        public MeshRenderer renderer;
        public List<VectorUtils.Geometry> geoms;
        private VectorUtils.TessellationOptions options;

        public BodyPoint[] points;
        public BodyPoint[] startPoints;
        public BodyPoint[] endPoints;

        public BezierPathSegment[] segments;
        public bool closed;

        PathProperties props;
        SolidFill fill;
        Stroke stroke;

        public bool animated = false;
        public MotionProps motion;

        public BodymovinShapePath path;

        public MovinShapeSlave(MovinShape master, BodymovinShapePath path, float strokeScale = 1f)
        {

            this.master = master;
            this.path = path;
            Transform parent = master.transform.parent;

            points = (BodyPoint[])path.points.Clone();
            closed = path.closed;


            /* ANIM SETUP */

            MotionSetup(ref animated, ref motion, path.animSets);
          

            /* GAMEOBJECT */

            gameObject = new GameObject(master.content.item.ty + " pts: " + points.Length + "  closed: " + closed);
            transform.SetParent(parent, false);
            transform.localPosition = master.transform.localPosition;

            mesh = new Mesh();
            filter = gameObject.AddComponent<MeshFilter>();
            filter.mesh = mesh;

            renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.material = master.renderer.material;


            /* SETUP VECTOR */

            fill = master.content.fillHidden || master.content.fillColor == null ? null : new SolidFill() { Color = master.fill.Color };
            stroke = master.content.strokeHidden || master.content.strokeColor == null ? null : new Stroke() { Color = master.stroke.Color, HalfThickness = master.content.strokeWidth * strokeScale };
            props = new PathProperties() { Stroke = stroke };

            shape = new Shape() {
                Fill = fill,
                PathProps = props,
                FillTransform = Matrix2D.identity
            };

            options = master.options;
            
            scene = new Scene() {
                Root = new SceneNode() { Shapes = new List<Shape> { shape } }
            };

            UpdateMesh();

        }

        
        public void Update(float frame)
        {
            
            /* ----- ANIM PROPS ----- */

            if (animated && !motion.completed) {
                UpdateProperty(frame, ref motion, path.animSets);
            }
           
            if (animated && !motion.completed)
                FillMesh();
        }


        public void UpdateOpacity(float opacity)
        {
            Color c = renderer.material.color;
            c.a = opacity * 0.01f;

            renderer.material.color = c;
        }

        public void UpdateStrokeColor(Color c)
        {
            stroke.Color = c;
            props.Stroke = stroke;
            FillMesh();
        }

        public void UpdateFillColor(Color c)
        {
            fill.Color = c;
            shape.Fill = fill;
            FillMesh();
        }


        public void UpdateProperty(float frame, ref MotionProps m, BodymovinAnimatedShapeProperties[] set)
        {

            /* ----- CHECK FOR COMPLETE ----- */

            if (m.keys <= 0)
            {
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


            /* ----- UPDATE POINTS ----- */

            for (int i = 0; i < points.Length; i++)
            {
                if (m.percent < 0)
                {
                    // BACK TO START OF KEYFRAME
                    points[i].p = startPoints[i].p;
                    points[i].i = startPoints[i].i;
                    points[i].o = startPoints[i].o;
                }
                else
                {
                    points[i].p = startPoints[i].p + ((endPoints[i].p - startPoints[i].p) * ease);
                    points[i].i = startPoints[i].i + ((endPoints[i].i - startPoints[i].i) * ease);
                    points[i].o = startPoints[i].o + ((endPoints[i].o - startPoints[i].o) * ease);
                }
            }


            /* ----- UPDATE MESH ----- */

            UpdateMesh(false);

        }




        public void ResetKeyframes()
        {
            if (animated) { SetKeyframe(ref motion, path.animSets, 0); }
        }




        /* ----- MOTION SETUP ------ */

        public void MotionSetup(ref bool b, ref MotionProps prop, BodymovinAnimatedShapeProperties[] set)
        {
            b = set != null && set.Length > 0;
            if (b)
            {
                prop = new MotionProps { keys = set.Length };
                SetKeyframe(ref prop, set, 0);
            }
        }



        /* ----- KEYFRAME SETTERS ----- */

        public void SetKeyframe(ref MotionProps prop, BodymovinAnimatedShapeProperties[] set, int k = 0)
        {
            prop.completed = false;
            if (prop.keys <= 0) { return; }

            prop.key = k;
            prop.startFrame = set[k].t;
            prop.endFrame = set.Length > k ? set[k + 1].t : prop.startFrame;
            prop.currentOutTangent = set[k].o;
            prop.nextInTangent = set[k].i;

            if (set == path.animSets)
            {
                startPoints = set[k].pts[0];
                endPoints = set[k].pts[1];
            }
           
        }



        /* ----- UPDATE MESH ----- */

        public void UpdateMesh(bool redraw = true)
        {
            if (segments == null) {
                segments = master.ConvertPointsToSegments(points);
                shape.Contours = new BezierContour[] { new BezierContour() { Segments = segments, Closed = closed } };
            } else {
                master.UpdateSegments(points, ref segments);
            }

            if (redraw)
                FillMesh();
        }

        public void FillMesh()
        {
            geoms = VectorUtils.TessellateScene(scene, options);
            VectorUtils.FillMesh(mesh, geoms, 1.0f);
        }

    }
}
 