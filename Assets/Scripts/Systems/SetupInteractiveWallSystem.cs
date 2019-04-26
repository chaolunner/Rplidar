using System.Collections.Generic;
using AlphaECS.Unity;
using UnityEngine;
using AlphaECS;
using System;
using UniRx;

public class SetupInteractiveWallSystem : SystemBehaviour
{
    public InteractiveWallConfiguration Configuration;
    private IGroup lidarUIInputComponents;
    private IGroup videoControlComponents;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory);

        lidarUIInputComponents = CreateGroup(new HashSet<Type>() { typeof(LidarUIInputComponent) });
        videoControlComponents = CreateGroup(new HashSet<Type>() { typeof(VideoControlComponent), typeof(ContentSwitchComponent), typeof(TagComponent) });
    }

    public override void OnEnable()
    {
        base.OnEnable();

        for (int i = 0; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
        }

#if UNITY_EDITOR
        AppDataSystem.Save(Configuration, "InteractiveWallConfiguration");
#else
        Configuration = AppDataSystem.Load<InteractiveWallConfiguration>("InteractiveWallConfiguration");
#endif

        Cursor.visible = Configuration.CursorVisible;
        Screen.SetResolution((int)Configuration.Resolution.x, (int)Configuration.Resolution.y, Configuration.FullScreenMode, Configuration.PreferredRefreshRate);

        lidarUIInputComponents.OnAdd().Subscribe(entity =>
        {
            var lidarUIInputComponent = entity.GetComponent<LidarUIInputComponent>();

            if (lidarUIInputComponent.ID < Configuration.Areas.Count)
            {
                var config = Configuration.Areas[lidarUIInputComponent.ID];
                lidarUIInputComponent.Size = config.WallSize;
                lidarUIInputComponent.Offset = config.LidarPosition;
                lidarUIInputComponent.Rotation = Quaternion.Euler(config.LidarRotation);
                lidarUIInputComponent.LidarInput2D.IsDraw = config.EnableLidarMap;
                lidarUIInputComponent.LidarInput2D.Client.Host = config.LidarHost;
                lidarUIInputComponent.LidarInput2D.Client.Port = config.LidarPort;
                lidarUIInputComponent.LidarInput2D.Client.MotorPWM = config.LidarMotorPWM;
                lidarUIInputComponent.LidarInput2D.XMultiplier = config.LidarScale.x;
                lidarUIInputComponent.LidarInput2D.YMultiplier = config.LidarScale.y;
                lidarUIInputComponent.LidarInput2D.MinX = config.LidarInputMinX;
                lidarUIInputComponent.LidarInput2D.MinY = config.LidarInputMinY;
                lidarUIInputComponent.LidarInput2D.MaxX = config.LidarInputMaxX;
                lidarUIInputComponent.LidarInput2D.MaxY = config.LidarInputMaxY;
                lidarUIInputComponent.LidarInput2D.MinDistance = config.LidarInputMinDistance;
                lidarUIInputComponent.LidarInput2D.MaxDistance = config.LidarInputMaxDistance;
            }
        }).AddTo(this.Disposer);

        videoControlComponents.OnAdd().Subscribe(entity =>
        {
            var videoControlComponent = entity.GetComponent<VideoControlComponent>();
            var contentSwitchComponent = entity.GetComponent<ContentSwitchComponent>();
            var tagComponent = entity.GetComponent<TagComponent>();

            if (contentSwitchComponent.ID < Configuration.Areas.Count)
            {
                var config = Configuration.Areas[contentSwitchComponent.ID];
                if (tagComponent.Tag == Tag.Text)
                {
                    contentSwitchComponent.Size = config.TextSize;
                    contentSwitchComponent.Offset = config.TextOffset;
                    videoControlComponent.VideoFilePaths = Configuration.TextFilePaths;
                }
                else if (tagComponent.Tag == Tag.Video)
                {
                    contentSwitchComponent.Size = config.VideoSize;
                    contentSwitchComponent.Offset = config.VideoOffset;
                    videoControlComponent.VideoFilePaths = Configuration.VideoFilePaths;
                }
                contentSwitchComponent.Speed = config.VideoSwitchSpeed;
                contentSwitchComponent.ScreenSize.Value = config.WallSize;
                videoControlComponent.ScaleMode = config.VideoScaleMode;
            }
        }).AddTo(this.Disposer);
    }
}
