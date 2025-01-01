using System;

public static class EventManager
{
    public static Action OnCategoryLoaded; // 目录加载完毕

    public static Action<TrackData> OnTrackSelected; // 曲目选中
    
    public static Action OnTrackDataLoaded; // 数据加载完毕
}
