using RenderHeads.Media.AVProVideo;
using System.Collections.Generic;
using AlphaECS.Unity;
using UnityEngine;
using AlphaECS;
using System;
using UniRx;

public class VideoControlSystem : SystemBehaviour
{
    private IGroup videoControlComponents;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory);

        videoControlComponents = CreateGroup(new HashSet<Type>() { typeof(VideoControlComponent), typeof(ContentSwitchComponent) });
    }

    public override void OnEnable()
    {
        base.OnEnable();

        videoControlComponents.OnAdd().Subscribe(entity =>
        {
            var videoControlComponent = entity.GetComponent<VideoControlComponent>();
            var contentSwitchComponent = entity.GetComponent<ContentSwitchComponent>();
            var initialDisposer = new CompositeDisposable();
            var switchDisposer = new CompositeDisposable();

            EventSystem.OnEvent<ContentInitialEvent>().Where(evt => evt.ContentSwitchComponent == contentSwitchComponent).Subscribe(evt =>
            {
                foreach (var go in contentSwitchComponent.Contents)
                {
                    var displayUGUI = go.GetComponent<DisplayUGUI>();
                    var mediaPlayer = displayUGUI._mediaPlayer;
                    displayUGUI._scaleMode = videoControlComponent.ScaleMode;
                    videoControlComponent.DisplayUGUIs.Add(displayUGUI);
                    videoControlComponent.MediaPlayers.Add(mediaPlayer);
                    mediaPlayer.m_AutoOpen = false;
                    mediaPlayer.m_AutoStart = false;
                    mediaPlayer.Stop();
                }
                Observable.EveryUpdate().Subscribe(_ =>
                {
                    if (!LoadVideo(videoControlComponent.MediaPlayers[0], videoControlComponent.VideoFilePaths[videoControlComponent.CurrentIndex]))
                    {
                        videoControlComponent.CurrentIndex++;
                        if (videoControlComponent.CurrentIndex >= videoControlComponent.VideoFilePaths.Count)
                        {
                            videoControlComponent.CurrentIndex = 0;
                        }
                    }
                    else
                    {
                        initialDisposer.Clear();
                    }
                }).AddTo(this.Disposer).AddTo(videoControlComponent.Disposer).AddTo(initialDisposer).AddTo(switchDisposer);

                Observable.EveryUpdate().SkipWhile(_ => VideoIsNotReady(videoControlComponent.MediaPlayers[0]) || AudioIsNotReady(videoControlComponent.MediaPlayers[evt.Current])).Subscribe(_ =>
                {
                    contentSwitchComponent.Contents[evt.Current].SetActive(true);
                    videoControlComponent.MediaPlayers[evt.Current].Play();
                    switchDisposer.Clear();
                }).AddTo(this.Disposer).AddTo(videoControlComponent.Disposer).AddTo(switchDisposer);
            }).AddTo(this.Disposer).AddTo(videoControlComponent.Disposer);

            EventSystem.OnEvent<ContentSwitchStartEvent>().Where(evt => evt.ContentSwitchComponent == contentSwitchComponent).Subscribe(evt =>
            {
                switchDisposer.Clear();
                contentSwitchComponent.IsReady = false;
                SwitchVideo(videoControlComponent, contentSwitchComponent.Direction.normalized, evt.Current);

                Observable.EveryUpdate().SkipWhile(_ => VideoIsNotReady(videoControlComponent.MediaPlayers[evt.Current]) || AudioIsNotReady(videoControlComponent.MediaPlayers[evt.Current])).Subscribe(_ =>
                {
                    contentSwitchComponent.IsReady = true;
                    switchDisposer.Clear();
                }).AddTo(this.Disposer).AddTo(videoControlComponent.Disposer).AddTo(switchDisposer);
            }).AddTo(this.Disposer).AddTo(videoControlComponent.Disposer);

            EventSystem.OnEvent<ContentSwitchCompletedEvent>().Where(evt => evt.ContentSwitchComponent == contentSwitchComponent).Subscribe(evt =>
            {
                videoControlComponent.MediaPlayers[evt.Previous].Stop();
                videoControlComponent.MediaPlayers[evt.Current].Play();
            }).AddTo(this.Disposer).AddTo(videoControlComponent.Disposer);
        }).AddTo(this.Disposer);
    }

    private void SwitchVideo(VideoControlComponent videoControlComponent, Vector2 dir, int nextIndex)
    {
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
        {
            if (dir.x > 0)
            {
                videoControlComponent.CurrentIndex++;
            }
            else
            {
                videoControlComponent.CurrentIndex--;
            }
        }
        else
        {
            if (dir.y > 0)
            {
                videoControlComponent.CurrentIndex++;
            }
            else
            {
                videoControlComponent.CurrentIndex--;
            }
        }

        if (videoControlComponent.CurrentIndex >= videoControlComponent.VideoFilePaths.Count)
        {
            videoControlComponent.CurrentIndex = 0;
        }
        else if (videoControlComponent.CurrentIndex < 0)
        {
            videoControlComponent.CurrentIndex = videoControlComponent.VideoFilePaths.Count - 1;
        }

        while (!LoadVideo(videoControlComponent.MediaPlayers[nextIndex], videoControlComponent.VideoFilePaths[videoControlComponent.CurrentIndex]))
        {
            SwitchVideo(videoControlComponent, dir, nextIndex);
        }
    }

    private static bool LoadVideo(MediaPlayer mp, string filePath, bool url = false)
    {
        var nextVideoLocation = MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder;
        // Set the video file name and to load. 
        if (url)
        {
            nextVideoLocation = MediaPlayer.FileLocation.AbsolutePathOrURL;
        }

        // Load the video
        return mp.OpenVideoFromFile(nextVideoLocation, filePath, mp.m_AutoStart);
    }

    private static bool VideoIsNotReady(MediaPlayer mp)
    {
        return (mp != null && mp.TextureProducer != null && mp.TextureProducer.GetTextureFrameCount() <= 0);

    }
    private static bool AudioIsNotReady(MediaPlayer mp)
    {
        return (mp != null && mp.Control != null && mp.Control.CanPlay() && mp.Info.HasAudio() && !mp.Info.HasVideo());
    }
}
