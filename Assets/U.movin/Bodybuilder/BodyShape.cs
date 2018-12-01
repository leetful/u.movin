using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VectorGraphics;

namespace U.movin
{
    public class BodyShape
    {

        public GameObject gameObject;
        public Transform transform
        {
            get { return gameObject.transform; }
        }

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

        public Movin body;
        public BodyLayer layer;
        public BezierPathSegment[] segments;
        public bool closed;
        public bool animated = false;
        public MotionProps motion;

        public BodyShape(BodyLayer layer, BodymovinShape content, float strokeMultiplier = 1f)
        {

            this.content = content;
            if (content.points == null) { Debug.Log("NO PTS ARRAY"); return; }
            if (content.points.Length <= 1) { Debug.Log("DON'T DRAW SHAPE -> NO PTS"); return; }

            this.layer = layer;
            this.body = layer.body;
            Transform parent = layer.transform;

            points = (BodyPoint[])content.points.Clone();
            closed = content.closed;

            animated = content.animSets != null;
            if (animated)
            {
                motion = new MotionProps()
                {
                    keys = content.animSets.Length
                };

                if (motion.keys >= 1) { SetKeyframe(0); }
            }


            gameObject = new GameObject(content.item.ty + " pts: " + points.Length + "  closed: " + content.closed);
            transform.SetParent(parent, false);
            transform.localPosition = -layer.content.anchorPoint;

            mesh = new Mesh();
            filter = gameObject.AddComponent<MeshFilter>();
            filter.mesh = mesh;



            renderer = gameObject.AddComponent<MeshRenderer>();
            //Debug.Log("sort:  " + renderer.sortingOrder);

            //renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.material = new Material(Shader.Find("Unlit/Vector"));
            
            //renderer.material.color = new Color(item.c[0], item.c[1], item.c[2]);

            Color stClr;
            stClr = (content.strokeColor == null) ? new Color(1, 1, 1) : new Color(content.strokeColor[0], content.strokeColor[1], content.strokeColor[2]);
            
            Color flClr;
            flClr = (content.fillColor == null) ? new Color(1, 1, 1) : new Color(content.fillColor[0], content.fillColor[1], content.fillColor[2]);

            SolidFill fill = content.fillHidden || content.fillColor == null ? null : new SolidFill() { Color = flClr };
            Stroke stroke = content.strokeHidden || content.strokeColor == null ? null : new Stroke() { Color = stClr, HalfThickness = content.strokeWidth * strokeMultiplier };

            shape = new Shape()
            {
                Fill = fill,
                PathProps = new PathProperties() { Stroke = stroke, },
                FillTransform = Matrix2D.identity
            };

            options = new VectorUtils.TessellationOptions()
            {
                StepDistance = 1000.0f,
                MaxCordDeviation = 0.05f,
                MaxTanAngleDeviation = 0.05f,
                SamplingStepSize = 0.01f
            };

            scene = new Scene()
            {
                Root = new SceneNode() { Shapes = new List<Shape> { shape } }
            };


            //if (keys > 0) { mesh.MarkDynamic(); }
            UpdateMesh();

        }


        public void UpdateSegments(BodyPoint[] pts, ref BezierPathSegment[] segs)
        {
            float y = -1f;
           
            for (int i = 0; i < pts.Length; i++)
            {
                BodyPoint point = pts[i];

                // Next point...

                bool last = i >= pts.Length - 1;
                BodyPoint nextPoint = last ? pts[0] : pts[i + 1];


                // UPDATE segment

                segs[i].P0.x = point.p.x;
                segs[i].P0.y = point.p.y * y;

                segs[i].P1.x = point.p.x + point.o.x;
                segs[i].P1.y = (point.p.y + point.o.y) * y;

                segs[i].P2.x = nextPoint.p.x + nextPoint.i.x;
                segs[i].P2.y = (nextPoint.p.y + nextPoint.i.y) * y;
              
            }

            int l = segs.Length - 1;
            
            segs[l].P0.x = pts[0].p.x;
            segs[l].P0.y = pts[0].p.y * y;

            segs[l].P1.x = segs[l].P1.y = segs[l].P2.x = segs[l].P2.y = 0;

        }

        public BezierPathSegment[] ConvertPointsToSegments(BodyPoint[] pts)
        {
            float y = -1f;

            int cnt = pts.Length + (closed ? 1 : 0);
            BezierPathSegment[] segs = new BezierPathSegment[cnt];

            int i = 0;
            foreach (BodyPoint point in pts)
            {

                // Next point...

                bool last = i >= pts.Length - 1;
                BodyPoint nextPoint = last ? pts[0] : pts[i + 1];


                // Make segment

                BezierPathSegment s = new BezierPathSegment()
                {
                    P0 = new Vector2(point.p.x, point.p.y * y),
                    P1 = new Vector2((point.p.x + point.o.x), (point.p.y + point.o.y) * y),
                    P2 = new Vector2((nextPoint.p.x + nextPoint.i.x), (nextPoint.p.y + nextPoint.i.y) * y)
                };

                segs[i] = s;
                i += 1;
            }

            if (pts.Length > 0 && i == cnt - 1)
            {
                BezierPathSegment final = new BezierPathSegment()
                {
                    P0 = new Vector2(pts[0].p.x, pts[0].p.y * y)
                };

                segs[i] = final;
            }


            /* READOUT */

            //foreach (BezierPathSegment s in segs)
            //{
            //    Debug.Log("P0: " + s.P0 + "  P1: " + s.P1 + "  P2: " + s.P2);
            //}

            return segs;
        }


        public void Update(float frame)
        {
            if (motion.completed) { return; }
            if (motion.keys <= 0)
            {
                //Debug.Log(">>>>> NO KEYFRAMES TO ANIMATE!");
                motion.completed = true;
                return;
            }

            if (frame >= motion.endFrame)
            {
                if (motion.key + 1 == content.animSets.Length - 1)
                {
                    motion.completed = true;
                    //Debug.Log("****** Shape Animation done! ******");
                    return;
                }

                SetKeyframe(motion.key + 1);
            }

            motion.percent = (frame - motion.startFrame) / (motion.endFrame - motion.startFrame);

            /* ----- CUBIC BEZIER ----- */
            float ease = Ease.CubicBezier(Vector2.zero, motion.currentOutTangent, motion.nextInTangent, Vector2.one, motion.percent);


            for (int i = 0; i < points.Length; i++)
            {
                points[i].p = startPoints[i].p + ((endPoints[i].p - startPoints[i].p) * ease);
                points[i].i = startPoints[i].i + ((endPoints[i].i - startPoints[i].i) * ease);
                points[i].o = startPoints[i].o + ((endPoints[i].o - startPoints[i].o) * ease);

            }

            UpdateMesh();
        }


        public void SetKeyframe(int k = 0)
        {
            motion.completed = false;
            if (motion.keys <= 0) { return; }

            motion.key = k;
            motion.startFrame = content.animSets[motion.key].t;
            motion.endFrame = content.animSets.Length > motion.key ? content.animSets[motion.key + 1].t : motion.startFrame;
            startPoints = content.animSets[motion.key].pts[0];
            endPoints = content.animSets[motion.key].pts[1];

            motion.currentOutTangent = content.animSets[motion.key].o;
            motion.nextInTangent = content.animSets[motion.key].i;
        }


        public void UpdateMesh()
        {
            if (segments == null)
            {
                segments = ConvertPointsToSegments(points);
                shape.Contours = new BezierContour[] { new BezierContour() { Segments = segments, Closed = closed } };
            }
            else
            {
                UpdateSegments(points, ref segments);
                //segments = ConvertPointsToSegments(points);
                //shape.Contours = new BezierContour[] { new BezierContour() { Segments = segments, Closed = closed } };
            }

            geoms = VectorUtils.TessellateScene(scene, options);
            VectorUtils.FillMesh(mesh, geoms, 1.0f);
        }

    }
}