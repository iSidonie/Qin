using System;
using System.Collections.Generic;

[Serializable]
public class TrackData
{
    public string id;
    public string uid;
    public string name;
    public string artist;
    public string version;
    public string audioFile;
    public string positionFile;
    public string musicFile;
    public string sheetFile;
}

[Serializable]
public class LevelData
{
    public string id;
    public string name;
    public List<TrackData> tracks;
}

[Serializable]
public class CategoryData
{
    public string id;
    public string name;
    public List<LevelData> levels;
}

[Serializable]
public class CategoryRoot
{
    public List<CategoryData> categories;
}
