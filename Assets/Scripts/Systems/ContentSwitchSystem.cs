using System.Collections.Generic;
using AlphaECS.Unity;
using UnityEngine;
using System.Linq;
using AlphaECS;
using System;
using UniRx;

public class ContentSwitchSystem : SystemBehaviour
{
    private IGroup contentSwitchComponents;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory);

        contentSwitchComponents = CreateGroup(new HashSet<Type>() { typeof(ContentSwitchComponent), typeof(ViewComponent) });
    }

    public override void OnEnable()
    {
        base.OnEnable();

        contentSwitchComponents.OnAdd().Subscribe(entity =>
        {
            var contentSwitchComponent = entity.GetComponent<ContentSwitchComponent>();
            var viewComponent = entity.GetComponent<ViewComponent>();
            var rectTransform = viewComponent.Transforms[0] as RectTransform;
            var width = Display.displays[Mathf.Clamp(contentSwitchComponent.TargetDisplay, 0, Display.displays.Length - 1)].renderingWidth;
            var height = Display.displays[Mathf.Clamp(contentSwitchComponent.TargetDisplay, 0, Display.displays.Length - 1)].renderingHeight;
            var switchDisposer = new CompositeDisposable();
            var targetPosition = Vector3.zero;
            var xMultiplier = 1f;
            var yMultiplier = 1f;

            contentSwitchComponent.ScreenSize.DistinctUntilChanged().Subscribe(size =>
            {
                xMultiplier = width / size.x;
                yMultiplier = height / size.y;

                targetPosition = new Vector3(contentSwitchComponent.Offset.x * xMultiplier, contentSwitchComponent.Offset.y * yMultiplier, 0);
                rectTransform.sizeDelta = new Vector2(contentSwitchComponent.Size.x * xMultiplier, contentSwitchComponent.Size.y * yMultiplier);
                rectTransform.position = targetPosition;
            }).AddTo(this.Disposer).AddTo(contentSwitchComponent.Disposer);

            for (int i = 0; i < viewComponent.Transforms[0].childCount; i++)
            {
                var go = viewComponent.Transforms[0].GetChild(i).gameObject;
                contentSwitchComponent.Contents.Add(go);
                go.SetActive(false);
            }

            EventSystem.Publish(new ContentInitialEvent(contentSwitchComponent, -1, 0));

            Observable.EveryUpdate().Where(_ => !contentSwitchComponent.IsPlaying && contentSwitchComponent.Direction != Vector2.zero).Subscribe(__ =>
            {
                var nextIndex = contentSwitchComponent.Contents[0].activeSelf ? 1 : 0;
                var index = nextIndex == 0 ? 1 : 0;
                var originPosition = targetPosition - new Vector3(contentSwitchComponent.Direction.x * contentSwitchComponent.Size.x * xMultiplier, contentSwitchComponent.Direction.y * contentSwitchComponent.Size.y * yMultiplier, 0);
                var offset = targetPosition - originPosition;
                var time = 0f;

                contentSwitchComponent.Contents[nextIndex].transform.position = originPosition;
                EventSystem.Publish(new ContentSwitchStartEvent(contentSwitchComponent, index, nextIndex));
                contentSwitchComponent.Contents[nextIndex].SetActive(true);

                Observable.EveryUpdate().SkipWhile(_ => contentSwitchComponents.Entities.Where(e => e.GetComponent<ContentSwitchComponent>().ID == contentSwitchComponent.ID).Any(e => !e.GetComponent<ContentSwitchComponent>().IsReady)).Subscribe(_ =>
                {
                    contentSwitchComponent.Contents[nextIndex].transform.position = Vector3.Lerp(originPosition, targetPosition, contentSwitchComponent.EaseCurve.Evaluate(time));
                    contentSwitchComponent.Contents[index].transform.position = contentSwitchComponent.Contents[nextIndex].transform.position + offset;
                    EventSystem.Publish(new ContentSwitchStayEvent(contentSwitchComponent, index, nextIndex));
                    if (time == 1)
                    {
                        EventSystem.Publish(new ContentSwitchCompletedEvent(contentSwitchComponent, index, nextIndex));
                        contentSwitchComponent.Contents[index].SetActive(false);
                        contentSwitchComponent.Direction = Vector2.zero;
                        contentSwitchComponent.IsPlaying = false;
                        switchDisposer.Clear();
                    }
                    time = Mathf.Clamp01(time + contentSwitchComponent.Speed * Time.deltaTime);
                }).AddTo(this.Disposer).AddTo(contentSwitchComponent.Disposer).AddTo(switchDisposer);
                contentSwitchComponent.IsPlaying = true;
                contentSwitchComponent.Direction = Vector2.zero;
            }).AddTo(this.Disposer).AddTo(contentSwitchComponent.Disposer);
        }).AddTo(this.Disposer);

        EventSystem.OnEvent<LidarSwipeEvent>().Subscribe(evt =>
        {
            var list = contentSwitchComponents.Entities.Select(e => e.GetComponent<ContentSwitchComponent>()).Where(contentSwitchComponent => contentSwitchComponent.ID == evt.ID);
            if (list.All(contentSwitchComponent => !contentSwitchComponent.IsPlaying))
            {
                foreach (var contentSwitchComponent in list)
                {
                    contentSwitchComponent.Direction = evt.Distance;
                }
            }
        }).AddTo(this.Disposer);
    }
}
