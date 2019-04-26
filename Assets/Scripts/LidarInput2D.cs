using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UniRx;

public class LidarInput2D : MonoBehaviour
{
    public bool IsDraw;
    private List<GameObject> DrawPoints = new List<GameObject>();

    public LidarClient Client;
    public float PointSize = 0.02f;
    [HideInInspector] public List<Vector2> Points = new List<Vector2>();
    [HideInInspector] public List<Vector2> Blobs = new List<Vector2>();
    private List<Vector2> PreviousBlobs = new List<Vector2>();

    public Dictionary<Vector2, Vector2> BlobTable = new Dictionary<Vector2, Vector2>();
    public Dictionary<Vector2, GameObject> TrackedObjects = new Dictionary<Vector2, GameObject>();

    public float MinX = -6f;
    public float MaxX = 6f;
    public float MinY = -6f;
    public float MaxY = 6f;
    public float MinDistance = 0.05f;
    public float MaxDistance = 0.25f;

    public float XMultiplier = 1f;
    public float YMultiplier = 1f;

    public GameObject TrackedObjectPrefab;

    private char[] delimiters = new char[] { '\n' };

    public float BufferTime = 0.1f;
    private float bufferedTime;
    private List<Vector2> bufferPoints = new List<Vector2>();

    public delegate void PointerDownHandler(LidarInput2D input, GameObject go, Vector2 pos);
    public delegate void PointerUpHandler(LidarInput2D input, GameObject go, Vector2 pos);
    public delegate void DragHandler(LidarInput2D input, GameObject go, Vector2 pos, Vector2 dis, bool swipe);

    public event PointerDownHandler OnPointerDown;
    public event PointerUpHandler OnPointerUp;
    public event DragHandler OnDrag;

    private void Update()
    {
        var values = Client.ReceivedData.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

        //if (IsDraw)
        //{
        //    while (DrawPoints.Count > 0)
        //    {
        //        Destroy(DrawPoints[0]);
        //        DrawPoints.RemoveAt(0);
        //    }
        //    DrawPoints.Clear();
        //}
        Points.Clear();

        for (var i = 0; i < values.Length; i++)
        {
            var data = values[i].Split(',');
            var angle = float.Parse(data[0]);
            var distance = 0.001f * float.Parse(data[1]);
            var quality = float.Parse(data[2]);

            var x = Mathf.Sin(angle * Mathf.Deg2Rad) * distance * XMultiplier;
            var y = Mathf.Cos(angle * Mathf.Deg2Rad) * distance * YMultiplier;

            if (x > MinX && x < MaxX && y > MinY && y < MaxY && distance != 0)
            {
                var position = new Vector2(x, y);
                //if (IsDraw)
                //{
                //    var go = Instantiate(TrackedObjectPrefab);
                //    go.transform.position = position;
                //    go.transform.localScale = PointSize * Vector3.one;
                //    DrawPoints.Add(go);
                //}
                Points.Add(position);
            }
        }

        UpdateBlobs();
    }

    void UpdateBlobs()
    {
        // note that there is a bug with this logic because of how the points are handled in sequence...
        // ... ie, if your hand is above the LIDAR and it gets to ~360, the "rightmost" part of your hand will be quite far from the blob center...
        // ... which is left-shifted at this point
        if (Points.Count == 0)
        {
            if (bufferedTime < BufferTime && bufferPoints.Count > 0)
            {
                Points = bufferPoints.ToList();
                bufferedTime += Time.deltaTime;
            }
            else
            {
                while (TrackedObjects.Count > 0)
                {
                    var kvp = TrackedObjects.ElementAt(0);
                    Destroy(kvp.Value);
                    OnPointerUp(this, kvp.Value, kvp.Key);
                    TrackedObjects.Remove(kvp.Key);
                }

                TrackedObjects.Clear();
                Blobs.Clear();
                PreviousBlobs.Clear();
                BlobTable.Clear();
                return;
            }
        }
        else
        {
            bufferPoints = Points.ToList();
            bufferedTime = 0;
        }

        Blobs.Clear();
        BlobTable.Clear();

        // ensure starting point 0 is blob 0
        // then, make sure not to re-add it
        Blobs.Add(Points[0]);
        for (var i = 1; i < Points.Count; i++)
        {
            for (var j = 0; j < Blobs.Count; j++)
            {
                var distance = (Points[i] - Blobs[j]).magnitude;
                if (distance < MaxDistance)
                {
                    Blobs[j] = (Blobs[j] + Points[i]) / 2f;
                    break;
                }
                else if (j == Blobs.Count - 1)
                {
                    Blobs.Add(Points[i]);
                    break;
                }
            }
        }

        for (var i = 0; i < Blobs.Count; i++)
        {
            for (var j = 0; j < PreviousBlobs.Count; j++)
            {
                // match found ...
                if ((Blobs[i] - PreviousBlobs[j]).magnitude < MaxDistance)
                {
                    BlobTable.Add(Blobs[i], PreviousBlobs[j]);
                    break;
                }
            }
        }

        // pointer down
        for (var i = 0; i < Blobs.Count; i++)
        {
            if (!BlobTable.ContainsKey(Blobs[i]) || PreviousBlobs.Count == 0)
            {
                var go = Instantiate(TrackedObjectPrefab, Blobs[i], Quaternion.identity);
                var trackedObject = go.GetComponent<TrackedObject>();
                go.transform.localScale = PointSize * Vector3.one;
                var renderer = go.GetComponent<MeshRenderer>();
                if (renderer)
                {
                    renderer.enabled = false;
                    //renderer.enabled = IsDraw;
                    //if (IsDraw)
                    //{
                    //    renderer.material.SetColor("_Color", Color.red);
                    //}
                }
                trackedObject.ID = i;
                TrackedObjects.Add(Blobs[i], go);
                OnPointerDown(this, go, Blobs[i]);
            }
        }

        // pointer up
        for (var i = 0; i < PreviousBlobs.Count; i++)
        {
            if (!BlobTable.ContainsValue(PreviousBlobs[i]) && TrackedObjects.ContainsKey(PreviousBlobs[i]))
            {
                var go = TrackedObjects[PreviousBlobs[i]].gameObject;
                var trackedObject = go.GetComponent<TrackedObject>();
                trackedObject.ID = i;
                TrackedObjects.Remove(PreviousBlobs[i]);
                OnPointerUp(this, go, PreviousBlobs[i]);
                Destroy(go);
            }
        }

        // pointer drag
        for (var i = 0; i < Blobs.Count; i++)
        {
            var previousBlob = Vector2.zero;
            var isTracked = BlobTable.TryGetValue(Blobs[i], out previousBlob);
            if (isTracked && TrackedObjects.ContainsKey(previousBlob))
            {
                var go = TrackedObjects[previousBlob].gameObject;
                go.transform.position = Blobs[i];
                var trackedObject = go.GetComponent<TrackedObject>();
                trackedObject.ID = i;
                TrackedObjects.Remove(previousBlob);
                TrackedObjects.Add(Blobs[i], go);
                var deltas = Blobs[i] - previousBlob;
                OnDrag(this, go, Blobs[i], deltas, deltas.magnitude >= MinDistance);
            }
        }

        PreviousBlobs.Clear();
        for (var i = 0; i < TrackedObjects.Count; i++)
        {
            PreviousBlobs = TrackedObjects.Keys.ToList();
        }
    }
}