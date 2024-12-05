using System;
using System.Collections.Generic;

[Serializable]
public class AudioDataStructure
{
    public string performer;
    public string trackName;
    public List<SectionData> sections;
}

[Serializable]
public class SectionData
{
    public int sectionIndex;
    public string title;
    public List<NotationAudioData> notations;
}

[Serializable]
public class NotationAudioData
{
    public string note;
    public int notationIndex;
    public int id;
    public float time;
}
