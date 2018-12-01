using UnityEngine;
using SimpleJSON;

namespace U.movin
{
    public struct BodymovinContent
    {
        public string v;
        public float fr;
        public float ip;
        public float op;
        public int w;
        public int h;
        public string nm;

        public BodymovinLayer[] layers;

        public static BodymovinContent init(string jsonPath)
        {
            string json = Resources.Load<TextAsset>(jsonPath).text;

            JSONNode data = JSON.Parse(json);
            BodymovinContent content = new BodymovinContent
            {
                nm = data["nm"],
                v = data["v"],
                fr = data["fr"],
                ip = data["ip"],
                op = data["op"],
                w = data["w"],
                h = data["h"]
            };

            content.w = Mathf.FloorToInt(content.w);
            content.h = Mathf.FloorToInt(content.h);

            ParseLayers(ref content, data);
            return content;
        }

        public static void ParseLayers(ref BodymovinContent b, JSONNode n)
        {
            int j = 0;

            b.layers = new BodymovinLayer[n["layers"].Count];

            foreach (JSONNode d in n["layers"])
            {
                BodymovinLayer i = new BodymovinLayer
                {
                    nm = d["nm"],
                    inFrame = d["ip"],
                    outFrame = d["op"],
                    blendMode = d["bm"]
                };

                ParseShapes(ref i, d);
                b.layers[j] = i;
                j++;
            }
        }

        public static void ParseShapes(ref BodymovinLayer b, JSONNode n)
        {
            int j = 0;

            b.nm = n["nm"];
            b.parent = n["parent"];
            b.ind = n["ind"];
            b.shapes = new BodymovinShape[n["shapes"].Count];
            b.anchorPoint = new Vector3(n["ks"]["a"]["k"][0].AsFloat, -n["ks"]["a"]["k"][1], n["ks"]["a"]["k"][2]);
            b.position = new Vector3(n["ks"]["p"]["k"][0].AsFloat, -n["ks"]["p"]["k"][1], n["ks"]["p"]["k"][2]);
            b.rotationEuler = new Vector3(-n["ks"]["rx"]["k"].AsFloat, n["ks"]["ry"]["k"].AsFloat, -n["ks"]["rz"]["k"].AsFloat);
            b.rotationXSets = new BodymovinAnimatedProperties[n["ks"]["rx"]["k"].Count];
            b.rotationYSets = new BodymovinAnimatedProperties[n["ks"]["ry"]["k"].Count];
            b.rotationZSets = new BodymovinAnimatedProperties[n["ks"]["rz"]["k"].Count];
            b.scale = new Vector3(n["ks"]["s"]["k"][0].AsFloat * 0.01f, n["ks"]["s"]["k"][1] * 0.01f, n["ks"]["s"]["k"][2] * 0.01f);
            b.opacity = n["ks"]["o"]["k"].AsFloat;
            b.opacitySets = new BodymovinAnimatedProperties[n["ks"]["o"]["k"].Count];

            int positionAnimated = n["ks"]["p"]["a"].AsInt;
            b.positionSets = new BodymovinAnimatedProperties[positionAnimated == 1 ? n["ks"]["p"]["k"].Count : 0];

            int scaleAnimated = n["ks"]["s"]["a"].AsInt;
            b.scaleSets = new BodymovinAnimatedProperties[scaleAnimated == 1 ? n["ks"]["s"]["k"].Count : 0];

            if (b.opacitySets.Length > 0)
            {
                for (int i = 0; i < n["ks"]["o"]["k"].Count; i++)
                {
                    JSONNode k = n["ks"]["o"]["k"][i];
                    b.opacitySets[i] = new BodymovinAnimatedProperties
                    {
                        t = k["t"],
                        i = new Vector2(k["i"]["x"][0].AsFloat, k["i"]["y"][0].AsFloat),
                        o = new Vector2(k["o"]["x"][0].AsFloat, k["o"]["y"][0].AsFloat),

                        sf = k["s"][0].AsFloat,
                        ef = k["e"][0].AsFloat
                    };

                    //Debug.Log(i + " - " + b.rotationXSets[i].i + "  " + b.rotationXSets[i].o + "  " + b.rotationXSets[i].sf + "  " + b.rotationXSets[i].ef + "  " + b.rotationXSets[i].t);
                }
            }

            if (b.rotationXSets.Length > 0)
            {
                for (int i = 0; i < n["ks"]["rx"]["k"].Count; i++)
                {
                    JSONNode k = n["ks"]["rx"]["k"][i];
                    b.rotationXSets[i] = new BodymovinAnimatedProperties
                    {
                        t = k["t"],
                        i = new Vector2(k["i"]["x"][0].AsFloat, k["i"]["y"][0].AsFloat),
                        o = new Vector2(k["o"]["x"][0].AsFloat, k["o"]["y"][0].AsFloat),

                        sf = -k["s"][0].AsFloat,
                        ef = -k["e"][0].AsFloat
                    };

                    //Debug.Log(i + " - " + b.rotationXSets[i].i + "  " + b.rotationXSets[i].o + "  " + b.rotationXSets[i].sf + "  " + b.rotationXSets[i].ef + "  " + b.rotationXSets[i].t);
                }

                b.rotationEuler.x = b.rotationXSets[0].sf;
            }


            if (b.rotationYSets.Length > 0)
            {
                for (int i = 0; i < n["ks"]["ry"]["k"].Count; i++)
                {
                    JSONNode k = n["ks"]["ry"]["k"][i];
                    b.rotationYSets[i] = new BodymovinAnimatedProperties
                    {
                        t = k["t"],
                        i = new Vector2(k["i"]["x"][0].AsFloat, k["i"]["y"][0].AsFloat),
                        o = new Vector2(k["o"]["x"][0].AsFloat, k["o"]["y"][0].AsFloat),

                        sf = k["s"][0].AsFloat,
                        ef = k["e"][0].AsFloat
                    };

                    //Debug.Log(i + " - " + b.rotationYSets[i].i + "  " + b.rotationYSets[i].o + "  " + b.rotationYSets[i].sf + "  " + b.rotationYSets[i].ef + "  " + b.rotationYSets[i].t);
                }

                b.rotationEuler.y = b.rotationYSets[0].sf;
            }


            if (b.rotationZSets.Length > 0)
            {
                for (int i = 0; i < n["ks"]["rz"]["k"].Count; i++)
                {
                    JSONNode k = n["ks"]["rz"]["k"][i];
                    b.rotationZSets[i] = new BodymovinAnimatedProperties
                    {
                        t = k["t"],
                        i = new Vector2(k["i"]["x"][0].AsFloat, k["i"]["y"][0].AsFloat),
                        o = new Vector2(k["o"]["x"][0].AsFloat, k["o"]["y"][0].AsFloat),

                        sf = -k["s"][0].AsFloat,
                        ef = -k["e"][0].AsFloat
                    };

                    //Debug.Log(i + " - " + b.rotationZSets[i].i + "  " + b.rotationZSets[i].o + "  " + b.rotationZSets[i].sf + "  " + b.rotationZSets[i].ef + "  " + b.rotationZSets[i].t);
                }

                b.rotationEuler.z = b.rotationZSets[0].sf;
            }

            b.rotation = Quaternion.Euler(b.rotationEuler);


            if (b.scaleSets.Length > 0)
            {
                for (int i = 0; i < n["ks"]["s"]["k"].Count; i++)
                {
                    JSONNode k = n["ks"]["s"]["k"][i];
                    b.scaleSets[i] = new BodymovinAnimatedProperties
                    {
                        t = k["t"],
                        ix = k["i"]["x"],
                        iy = k["i"]["y"],
                        ox = k["o"]["x"],
                        oy = k["o"]["y"],

                        s = k["s"],
                        e = k["e"]
                    };

                    b.scaleSets[i].s *= 0.01f;
                    b.scaleSets[i].e *= 0.01f;

                    //Debug.Log(i + " scale - " + b.scaleSets[i].ix + "  " + b.scaleSets[i].ox + "  " + b.scaleSets[i].s + "  " + b.scaleSets[i].e + "  " + b.scaleSets[i].t);
                }

                b.scale = b.scaleSets[0].s;
            }



            if (b.positionSets.Length > 0)
            {
                for (int i = 0; i < n["ks"]["p"]["k"].Count; i++)
                {
                    JSONNode k = n["ks"]["p"]["k"][i];
                    b.positionSets[i] = new BodymovinAnimatedProperties
                    {
                        t = k["t"],
                        i = k["i"],
                        o = k["o"],
                        to = k["to"],
                        ti = k["ti"],

                        s = k["s"],
                        e = k["e"]
                    };

                    b.positionSets[i].s.y = -b.positionSets[i].s.y;
                    b.positionSets[i].e.y = -b.positionSets[i].e.y;

                    //Debug.Log(i + " - " + b.positionSets[i].i + "  " + b.positionSets[i].o + "  " + b.positionSets[i].s + "  " + b.positionSets[i].e + "  " + b.positionSets[i].t);
                }

                b.position = b.positionSets[0].s;
            }

            foreach (JSONNode d in n["shapes"])
            {
                BodymovinShape i = new BodymovinShape { ty = d["ty"] };

                ParseItems(ref i, d);
                b.shapes[j] = i;
                j++;

            }

        }

        public static void ParseItems(ref BodymovinShape b, JSONNode n)
        {
            int j = 0;
            b.it = new BodymovinShapeItem[n["it"].Count];



            /* ----- CAPTURE MULTIPLE PATHS ----- */

            int pathCount = 0;
            foreach (JSONNode d in n["it"])
            {
                if (d["ty"] == "sh") { pathCount += 1; }
            }

            b.paths = new BodymovinShapePath[pathCount];
            pathCount = 0;


            /* --------- */

            foreach (JSONNode d in n["it"])
            {
                BodymovinShapeItem i = new BodymovinShapeItem
                {
                    ty = d["ty"],
                    nm = d["nm"],
                    mn = d["mn"],
                    ix = d["ix"],
                    hd = d["hd"],
                    c = new float[] { d["c"]["k"][0], d["c"]["k"][1], d["c"]["k"][2], d["c"]["k"][3] },

                    w = d["w"]["k"],
                    ks = new BodymovinShapeVertices
                    {
                        a = d["ks"]["a"],
                        ix = d["ks"]["ix"],
                        ksets = new BodymovinAnimatedShapeProperties[d["ks"]["k"].Count],
                        k = new BodymovinShapeProperties
                        {
                            c = d["ks"]["k"]["c"],
                            i = new Vector2[d["ks"]["k"]["i"].Count],
                            o = new Vector2[d["ks"]["k"]["o"].Count],
                            v = new Vector2[d["ks"]["k"]["v"].Count],
                        }
                    },
                    path = new BodymovinShapePath { }
                };


                /* COLORS */

                int colorAnimated = d["c"]["a"].AsInt;
                if (colorAnimated == 1)
                {
                    i.cSets = new BodymovinAnimatedProperties[d["c"]["k"].Count];
                    for (int c = 0; c < d["c"]["k"].Count; c++)
                    {
                        JSONNode k = d["c"]["k"][c];

                        i.cSets[c] = new BodymovinAnimatedProperties
                        {
                            t = k["t"],
                            //i = new Vector2(k["i"]["x"][0].AsFloat, k["i"]["y"][0].AsFloat),
                            //o = new Vector2(k["o"]["x"][0].AsFloat, k["o"]["y"][0].AsFloat),

                            // Clamp tangents? - FIX
                            i = new Vector2(Mathf.Clamp(k["i"]["x"][0].AsFloat, -1, 1), Mathf.Clamp(k["i"]["y"][0].AsFloat, -1, 1)),
                            o = new Vector2(Mathf.Clamp(k["o"]["x"][0].AsFloat, -1, 1), Mathf.Clamp(k["o"]["x"][0].AsFloat, -1, 1)),
                            
                            s = new Vector3(k["s"][0].AsFloat, k["s"][1].AsFloat, k["s"][2].AsFloat),
                            e = new Vector3(k["e"][0].AsFloat, k["e"][1].AsFloat, k["e"][2].AsFloat)
                        };

                        //Debug.Log("s: " + i.cSets[c].s);
                    }
                    
                }

                /* VERTS */

                i.ks.pts = new BodyPoint[d["ks"]["k"]["v"].Count];

                for (int c = 0; c < d["ks"]["k"]["v"].Count; c++)
                {
                    JSONNode ni = d["ks"]["k"]["i"][c];
                    JSONNode no = d["ks"]["k"]["o"][c];
                    JSONNode nv = d["ks"]["k"]["v"][c];

                    i.ks.k.i[c] = new Vector2(ni[0].AsFloat, ni[1].AsFloat);
                    i.ks.k.o[c] = new Vector2(no[0].AsFloat, no[1].AsFloat);
                    i.ks.k.v[c] = new Vector2(nv[0].AsFloat, nv[1].AsFloat);

                    i.ks.pts[c] = new BodyPoint(i.ks.k.v[c], i.ks.k.i[c], i.ks.k.o[c]);
                }

                if (i.ks.pts.Length > 0)
                {
                    
                    i.path.points = i.ks.pts;
                    //Debug.Log("path verts:  " + i.path.points);
                }



                /* ANIMATED VERT SETS */

                if (i.path.points == null)
                {

                    i.path.animSets = new BodymovinAnimatedShapeProperties[d["ks"]["k"].Count];

                    for (int s = 0; s < d["ks"]["k"].Count; s++)
                    {


                        JSONNode k = d["ks"]["k"][s];
                        BodymovinAnimatedShapeProperties kset = new BodymovinAnimatedShapeProperties
                        {
                            t = k["t"],
                            i = k["i"],
                            o = k["o"],

                            s = new BodymovinShapeProperties
                            {
                                c = k["s"][0]["c"],
                                i = new Vector2[k["s"][0]["i"].Count],
                                o = new Vector2[k["s"][0]["o"].Count],
                                v = new Vector2[k["s"][0]["v"].Count],
                            },
                            e = new BodymovinShapeProperties
                            {
                                c = k["e"][0]["c"],
                                i = new Vector2[k["e"][0]["i"].Count],
                                o = new Vector2[k["e"][0]["o"].Count],
                                v = new Vector2[k["e"][0]["v"].Count],
                            },

                            pts = new BodyPoint[2][]
                        };


                        i.path.animSets[s] = kset;
                        i.path.animSets[s].pts[0] = new BodyPoint[k["s"][0]["v"].Count];
                        i.path.animSets[s].pts[1] = new BodyPoint[k["e"][0]["v"].Count];

                        //Debug.Log("set - " + kset.t + "  i - " + kset.i.ToString("F4") + "  o - " + kset.o.ToString("F4"));

                        if (kset.s.v.Length > 0)
                        {
                            for (int c = 0; c < k["s"][0]["v"].Count; c++)
                            {

                                /* START SET */

                                JSONNode ni = k["s"][0]["i"][c];
                                JSONNode no = k["s"][0]["o"][c];
                                JSONNode nv = k["s"][0]["v"][c];

                                kset.s.i[c] = new Vector2(ni[0].AsFloat, ni[1].AsFloat);
                                kset.s.o[c] = new Vector2(no[0].AsFloat, no[1].AsFloat);
                                kset.s.v[c] = new Vector2(nv[0].AsFloat, nv[1].AsFloat);


                                /* END SET */

                                ni = k["e"][0]["i"][c];
                                no = k["e"][0]["o"][c];
                                nv = k["e"][0]["v"][c];

                                kset.e.i[c] = new Vector2(ni[0].AsFloat, ni[1].AsFloat);
                                kset.e.o[c] = new Vector2(no[0].AsFloat, no[1].AsFloat);
                                kset.e.v[c] = new Vector2(nv[0].AsFloat, nv[1].AsFloat);


                                /* BOTH PTS */

                                kset.pts[0][c] = new BodyPoint(kset.s.v[c], kset.s.i[c], kset.s.o[c]);
                                kset.pts[1][c] = new BodyPoint(kset.e.v[c], kset.e.i[c], kset.e.o[c]);

                            }
                        }

                        i.ks.ksets[s] = kset;
                    }

                    if (i.path.animSets.Length > 0)
                    {
                        i.path.points = i.path.animSets[0].pts[0];
                    }

                }



                b.it[j] = i;

                if (i.ty == "st")
                {
                    b.strokeColor = i.c;
                    b.strokeColorSets = i.cSets != null && i.cSets.Length > 0 ? i.cSets : new BodymovinAnimatedProperties[0];
                    b.strokeHidden = i.hd;
                    b.strokeWidth = i.w;
                }

                if (i.ty == "fl")
                {
                    b.fillColor = i.c;
                    b.fillColorSets = i.cSets != null && i.cSets.Length > 0 ? i.cSets : new BodymovinAnimatedProperties[0];
                    b.fillHidden = i.hd;
                }

                if (i.ty == "sh")
                {
                    
                    b.item = i;

                    i.path.closed = i.path.animSets == null ? i.ks.k.c : i.ks.ksets[0].s.c;
                    b.paths[pathCount] = i.path;
                    pathCount += 1;

                    //Debug.Log("paths shape:  " + pathCount);
                    //Debug.Log("paths shape pts:  " + i.path.points.Length);
                    //Debug.Log("path: " + pathCount);
                }


                j++;
            }
        }
    }


    public struct BodymovinLayer
    {
        public string nm;
        public BodymovinShape[] shapes;
        public int parent;
        public int ind;

        public Vector3 anchorPoint;

        public Vector3 position;
        public BodymovinAnimatedProperties[] positionSets;

        public Vector3 scale;
        public BodymovinAnimatedProperties[] scaleSets;

        public float opacity;
        public BodymovinAnimatedProperties[] opacitySets;

        public Vector3 rotationEuler;
        public Quaternion rotation;
        public BodymovinAnimatedProperties[] rotationXSets;
        public BodymovinAnimatedProperties[] rotationYSets;
        public BodymovinAnimatedProperties[] rotationZSets;

        public float inFrame;
        public float outFrame;
        public int blendMode;
    }

    public struct BodymovinShape
    {
        public string ty;
        public BodymovinShapeItem[] it;
        public BodymovinShapeItem item;
        public float[] strokeColor;
        public BodymovinAnimatedProperties[] strokeColorSets;
        public float[] fillColor;
        public BodymovinAnimatedProperties[] fillColorSets;
        public bool strokeHidden;
        public float strokeWidth;
        public bool fillHidden;
        public BodymovinShapePath[] paths;
    }

    public struct BodymovinShapeItem
    {
        public string ty;
        public string nm;
        public string mn;
        public float[] c;
        public BodymovinAnimatedProperties[] cSets;
        public float w;
        public bool hd;
        public int ix;
        public BodymovinShapeVertices ks;
        public BodymovinShapePath path;
    }

    public struct BodymovinShapePath
    {
        public bool closed;
        public BodyPoint[] points;
        public BodymovinAnimatedShapeProperties[] animSets;
    }

    public struct BodymovinShapeVertices
    {
        public int a;
        public int ix;
        public BodymovinShapeProperties k;
        public BodymovinAnimatedShapeProperties[] ksets;

        // Simplify point aggregation
        public BodyPoint[] pts;
    }

    public struct BodymovinShapeProperties
    {
        public Vector2[] i;
        public Vector2[] o;
        public Vector2[] v;
        public bool c;
    }

    public struct BodymovinAnimatedShapeProperties
    {
        public float t;
        public Vector2 i;
        public Vector2 o;
        public BodymovinShapeProperties s;
        public BodymovinShapeProperties e;

        // Simplify point aggregation, start + end sets
        public BodyPoint[][] pts;
    }

    public struct BodymovinAnimatedProperties
    {
        public float t;

        public Vector3 ix;
        public Vector3 iy;
        public Vector3 ox;
        public Vector3 oy;

        public Vector2 i;
        public Vector2 o;
        public Vector3 ti;
        public Vector3 to;

        public Vector3 s;
        public Vector3 e;
        public float sf;
        public float ef;
    }


    // Custom structures

    public struct BodyPoint
    {
        public Vector2 i;
        public Vector2 o;
        public Vector2 p;

        public BodyPoint(Vector2 point, Vector2 inPoint, Vector2 outPoint)
        {
            i = inPoint;
            o = outPoint;
            p = point;
        }
    }

}