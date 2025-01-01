using System;
using System.Collections.Generic;

[Serializable]
public class TrackData
{
    public string id;
    public string name;
    public string artist;
    public string version;
    public string audioFile;
    public string positionFile;
    public string musicFile;
    public SheetPages sheetPages;
}

[Serializable]
public class SheetPages
{
    public string baseName;
    public int pageCount;
}

[Serializable]
public class LevelData
{
    public string levelId;
    public string name;
    public List<TrackData> tracks;
}

[Serializable]
public class CategoryData
{
    public string categoryId;
    public string name;
    public List<LevelData> levels;
}

[Serializable]
public class CategoryRoot
{
    public List<CategoryData> categories;
}
