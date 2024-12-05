using System;
using System.Collections.Generic;

[Serializable]
public class PositionDataStructure
{
    public string trackName;
    public string performer;
    public List<PageData> pages;
}

[Serializable]
public class PageData
{
    public int pageNumber;
    public List<NotationPositionData> notations;
}

[Serializable]
public class NotationPositionData
{
    public int x;
    public int y;
    public int id;
    public string type; // "Main" or "Continuation"
    public int parentId;
}
