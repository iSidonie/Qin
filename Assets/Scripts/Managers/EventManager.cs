using System;

public static class EventManager
{
    public static Action OnCategoryLoaded; // Ŀ¼�������

    public static Action<TrackData> OnTrackSelected; // ��Ŀѡ��
    
    public static Action OnTrackDataLoaded; // ���ݼ������
}
