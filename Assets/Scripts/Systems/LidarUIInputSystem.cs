using System.Collections.Generic;
using AlphaECS.Unity;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using AlphaECS;
using System;
using UniRx;

public class LidarUIInputSystem : SystemBehaviour
{
    private IGroup lidarUIInputComponents;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory);

        lidarUIInputComponents = CreateGroup(new HashSet<Type>() { typeof(LidarUIInputComponent), typeof(ViewComponent) });
    }

    public override void OnEnable()
    {
        base.OnEnable();

        lidarUIInputComponents.OnAdd().Subscribe(entity =>
        {
            var lidarUIInputComponent = entity.GetComponent<LidarUIInputComponent>();
            var viewComponent = entity.GetComponent<ViewComponent>();
            var points = new List<GameObject>();
            var scopes = new List<GameObject>();

            var xMultiplier = Display.displays[lidarUIInputComponent.TargetDisplay < Display.displays.Length ? lidarUIInputComponent.TargetDisplay : 0].renderingWidth / lidarUIInputComponent.Size.x;
            var yMultiplier = Display.displays[lidarUIInputComponent.TargetDisplay < Display.displays.Length ? lidarUIInputComponent.TargetDisplay : 0].renderingHeight / lidarUIInputComponent.Size.y;

            Observable.EveryLateUpdate().Where(_ => lidarUIInputComponent.LidarInput2D.IsDraw).Subscribe(_ =>
            {
                while (scopes.Count > 0)
                {
                    Destroy(scopes[0]);
                    scopes.RemoveAt(0);
                }

                var min = new Vector2(lidarUIInputComponent.LidarInput2D.MinX, lidarUIInputComponent.LidarInput2D.MinY);
                var max = new Vector2(lidarUIInputComponent.LidarInput2D.MaxX, lidarUIInputComponent.LidarInput2D.MaxY);

                var scope = Instantiate(lidarUIInputComponent.LiderInputScopePrefab, viewComponent.Transforms[0]);
                var rectTransform = scope.transform as RectTransform;
                var center = 0.5f * (min + max);
                scope.transform.position = new Vector3((center.x + lidarUIInputComponent.Offset.x) * xMultiplier, (center.y + lidarUIInputComponent.Offset.y) * yMultiplier, 0);
                rectTransform.sizeDelta = new Vector2((max.x - min.x) * xMultiplier, (max.y - min.y) * yMultiplier);
                scopes.Add(scope);

                while (points.Count > 0)
                {
                    Destroy(points[0]);
                    points.RemoveAt(0);
                }

                foreach (var position in lidarUIInputComponent.LidarInput2D.Points)
                {
                    var go = Instantiate(lidarUIInputComponent.LidarUIPointPrefab, viewComponent.Transforms[0]);

                    go.transform.position = new Vector3((position.x + lidarUIInputComponent.Offset.x) * xMultiplier, (position.y + lidarUIInputComponent.Offset.y) * yMultiplier, 0);
                    points.Add(go);
                }
                foreach (var kvp in lidarUIInputComponent.LidarInput2D.TrackedObjects)
                {
                    var go = Instantiate(lidarUIInputComponent.LidarUIPointPrefab, viewComponent.Transforms[0]);

                    go.transform.position = new Vector3((kvp.Key.x + lidarUIInputComponent.Offset.x) * xMultiplier, (kvp.Key.y + lidarUIInputComponent.Offset.y) * yMultiplier, 0);
                    go.GetComponent<Image>().color = Color.red;
                    points.Add(go);
                }
            }).AddTo(this.Disposer).AddTo(lidarUIInputComponent.Disposer);

            lidarUIInputComponent.LidarInput2D.OnPointerDown += OnPointerDown;
            lidarUIInputComponent.LidarInput2D.OnPointerUp += OnPointerUp;
            lidarUIInputComponent.LidarInput2D.OnDrag += OnDrag;
        }).AddTo(this.Disposer);

        lidarUIInputComponents.OnRemove().Subscribe(entity =>
        {
            var lidarUIInputComponent = entity.GetComponent<LidarUIInputComponent>();

            lidarUIInputComponent.LidarInput2D.OnPointerDown -= OnPointerDown;
            lidarUIInputComponent.LidarInput2D.OnPointerUp -= OnPointerUp;
            lidarUIInputComponent.LidarInput2D.OnDrag -= OnDrag;
        }).AddTo(this.Disposer);
    }

    private void OnPointerDown(LidarInput2D lidarInput2D, GameObject go, Vector2 pos)
    {
        foreach (var input in lidarUIInputComponents.Entities.Select(e => e.GetComponent<LidarUIInputComponent>()).Where(input => input.LidarInput2D == lidarInput2D))
        {
            EventSystem.Publish(new LidarPointerDownEvent(input.ID, go, pos));
        }
    }

    private void OnPointerUp(LidarInput2D lidarInput2D, GameObject go, Vector2 pos)
    {
        foreach (var input in lidarUIInputComponents.Entities.Select(e => e.GetComponent<LidarUIInputComponent>()).Where(input => input.LidarInput2D == lidarInput2D))
        {
            EventSystem.Publish(new LidarPointerUpEvent(input.ID, go, pos));
        }
    }

    private void OnDrag(LidarInput2D lidarInput2D, GameObject go, Vector2 pos, Vector2 dis)
    {
        foreach (var input in lidarUIInputComponents.Entities.Select(e => e.GetComponent<LidarUIInputComponent>()).Where(input => input.LidarInput2D == lidarInput2D))
        {
            if (dis == Vector2.zero)
            {
                continue;
            }
            if (Mathf.Abs(dis.x) > Mathf.Abs(dis.y))
            {
                EventSystem.Publish(new LidarSwipeEvent(input.ID, go, pos, new Vector2(dis.x / Mathf.Abs(dis.x), 0)));
            }
            else
            {
                EventSystem.Publish(new LidarSwipeEvent(input.ID, go, pos, new Vector2(0, dis.y / Mathf.Abs(dis.y))));
            }
        }
    }
}
