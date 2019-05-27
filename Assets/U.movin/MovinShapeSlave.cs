using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VectorGraphics;

namespace u.movin
{
    public class MovinShapeSlave : MovinShape
    {
        public MovinShape master;
        public BodymovinShapePath path;

        public MovinShapeSlave(MovinShape master, BodymovinShapePath path, float strokeWidth = 1f)
        {

            this.master = master;
            this.path = path;
            Transform parent = master.transform.parent;


            /* SHAPE PROPS */

            points = (BodyPoint[])path.points.Clone();
            motionSet = path.animSets;
            closed = path.closed;



            /* ANIM SETUP */

            MotionSetup(ref animated, ref motion, motionSet);
          


            /* GAMEOBJECT */

            gameObject = new GameObject(master.content.item.ty + " pts: " + points.Length + "  closed: " + closed);
            transform.SetParent(parent, false);
            transform.localPosition = master.transform.localPosition;

            mesh = new Mesh();
            filter = gameObject.AddComponent<MeshFilter>();
            filter.mesh = mesh;

            renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.material = master.renderer.material;

            sorting = gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>();
            sorting.sortingOrder = master.sorting.sortingOrder;


            /* SETUP VECTOR */

            fill = master.content.fillHidden || master.content.fillColor == null ? null : new SolidFill() { Color = master.fill.Color };
            stroke = master.content.strokeHidden || master.content.strokeColor == null ? null : new Stroke() { Color = master.stroke.Color, HalfThickness = master.content.strokeWidth * strokeWidth };
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

    }
}
 