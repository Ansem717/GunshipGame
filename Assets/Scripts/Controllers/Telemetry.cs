//---------------------------------------------------------
// file:	Telemetry.cs
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// course:	DES 315
// term:	Spring 2026
//
// brief:	Records and saves key data into a csv
//
// Copyright (c) 2026 DigiPen (USA) Corproation
//---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Telemetry : MonoBehaviour {

    public static Telemetry Instance;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    ////////////////////


    private class TelemetryEntry {
        public int frameStamp;
        public string name;
    }

    /*
     * KEEP THE PREVIOUS ENTRIES FOR REFERENCE
     * 
    private class TelemetryCardEntry : TelemetryEntry {
        public int playerID;
        public bool didWin;

        public TelemetryCardEntry(string _name, int playerID, bool didWin) {
            frameStamp = Time.frameCount;
            
            this.playerID = playerID;
            this.didWin = didWin;
            
            if (_name.Contains('"')) _name = _name.Replace("\"", "\"\""); //'escape' any quotation marks inside of the name
            name = _name.Contains(',') || _name.Contains('\n') || _name.Contains('"') ? $"\"{_name}\"" : _name; //wrap the name in quotation marks due to special characters

        }

        public override string ToString() => $"{frameStamp},{name},{playerID},{didWin}";
    }

    private List<TelemetryCardEntry> cardEntries;

    private class TelemetryTrickEntry : TelemetryEntry {
        public int Size;
        public int playerID;

        public TelemetryTrickEntry(int Size, int playerID) {
            frameStamp = Time.frameCount;
            this.Size = Size;
            this.playerID = playerID;
            name = "Trick";
        }

        public override string ToString() => $"{frameStamp},{name},{playerID},{Size}";
    }

    private List<TelemetryTrickEntry> trickEntries;

    public enum TelemetryMenuOption { Open, Resume, Settings, ShowZones, HideZones, ShowDebug, HideDebug, SpeedSlow, SpeedNormal, SpeedFast, SpeedUltra, SettingsBack }

    private class TelemetryMenuEntry : TelemetryEntry {

        public TelemetryMenuEntry(TelemetryMenuOption opt) {
            frameStamp = Time.frameCount;
            name = opt.ToString();
        }

        public override string ToString() => $"{frameStamp},{name}"; 

    }

    private List<TelemetryMenuEntry> menuEntries;
    */

    private class TelemetryPerformanceEntry {
        public float timeStamp;
        public int blockSize;
        public int minFrame;
        public float meanFrame;
        public int midFrame;

        public TelemetryPerformanceEntry(float timeStamp, int blockSize, int minFrame, float meanFrame, int midFrame) {
            this.timeStamp = timeStamp;
            this.blockSize = blockSize;
            this.minFrame = minFrame;
            this.meanFrame = meanFrame;
            this.midFrame = midFrame;
        }

        public override string ToString() => $"{timeStamp},{blockSize},{minFrame},{meanFrame},{midFrame}";

    }

    private List<TelemetryPerformanceEntry> performanceEntries;


    ///////////////////
    //// UNIT TEST ////
    ///////////////////

    public bool UnitTest;

    public void Start() {


        //cardEntries = new();
        //trickEntries = new();
        //menuEntries = new();
        performanceEntries = new();

        if (!UnitTest) return;

        ///Note: was not updated for performance entries.
        
        //RecordCardEntry("RawCard", 0, false, 0);
        //RecordCardEntry("StructCard", 1, false, 1);
        //RecordCardEntry("Another raw card", 2, true, 2);

        //RecordTrickEntry(3, 2, 2);
        //RecordTrickEntry(10, 1, 5);

        //RecordMenuEntry(TelemetryMenuOption.Open);
        //RecordMenuEntry(TelemetryMenuOption.Settings);
        //RecordMenuEntry(TelemetryMenuOption.ShowZones);
        //RecordMenuEntry(TelemetryMenuOption.SettingsBack);
        //RecordMenuEntry(TelemetryMenuOption.Resume);

        //RecordCardEntry("CARD_FOUR", 0, false, 10);
        //RecordCardEntry("CARD_FIVE", 1, false, 11);
        //RecordCardEntry("CARD_SIX", 2, true, 12);

        Debug.LogError("Telemetry Unit Test Complete. Quitting Game.");

        MasterController.Singleton.QuitGame();
    }


    /////////////////
    //// METHODS ////
    /////////////////

    //public void RecordCardEntry(string name, int playerID, bool didWin, int turn) => cardEntries.Add(new TelemetryCardEntry(name, playerID, didWin));
    //public void RecordTrickEntry(int Size, int playerID, int turn) => trickEntries.Add(new TelemetryTrickEntry(Size, playerID, turn));
    //public void RecordMenuEntry(TelemetryMenuOption opt) => menuEntries.Add(new TelemetryMenuEntry(opt));
    public void RecordPerformanceEntry(int blockSize, int minFrame, float meanFrame, int midFrame) => performanceEntries.Add(new TelemetryPerformanceEntry(Time.realtimeSinceStartup, blockSize, minFrame, meanFrame, midFrame));

    private void OnDestroy() => Save();

    //private void WriteCardHeadLine(StreamWriter stream) => stream.WriteLine("FRAMESTAMP,TURN,CARD NAME,PLAYER ID,DID TAKE TRICK?");
    //private void WriteTrickHeadLine(StreamWriter stream) => stream.WriteLine("FRAMESTAMP,TURN,NAME,PLAYER ID,TRICK SIZE");
    //private void WriteMenuHeadLine(StreamWriter stream) => stream.WriteLine("FRAMESTAMP,TURN,MENU OPTION");
    //private void WriteAllHeadline(StreamWriter stream) => stream.WriteLine("FRAMESTAMP,TURN,NAME,PLAYER ID,TRICK SIZE |or| DID TAKE TRICK?");
    private void WritePerformanceHeadLine(StreamWriter stream) => stream.WriteLine("TIMESTAMP,BLOCK SIZE,WORST,MEAN,MEDIAN");

    public void Save() {

        // File Managing //
        string dateString = "MMM_dd";
        string timeString = "hh_mm_ss";

        DateTime now = DateTime.Now;

        // Build the directory path and ensure it exists
        string directoryPath = Application.dataPath + "/Telemetry/" + now.ToString(dateString);
        Directory.CreateDirectory(directoryPath); // Creates directory if it doesn't exist
        string filePath = directoryPath + "/" + now.ToString(timeString) + "_Telemetry.csv";

        using StreamWriter stream = new(filePath);

        // Ensure data is in correct order
        //List<TelemetryCardEntry> cardEntriesSorted = cardEntries.OrderBy(e => e.turn).ToList();
        //List<TelemetryTrickEntry> trickEntriesSorted = trickEntries.OrderBy(e => e.turn).ToList();
        //List<TelemetryMenuEntry> menuEntriesSorted = menuEntries.OrderBy(e => e.frameStamp).ToList();
        List<TelemetryPerformanceEntry> performanceEntriesSorted = performanceEntries.OrderBy(e => e.timeStamp).ToList();

        //Write file metadata first line
        stream.WriteLine($"{now.ToString(dateString)},{now.ToString(timeString).Replace("_", ":")}");

        stream.WriteLine();
        //Write card entries
        /*
        WriteCardHeadLine(stream);

        foreach (TelemetryCardEntry cEntry in cardEntriesSorted) stream.WriteLine(cEntry);

        stream.WriteLine();
        //Write trick entries
        WriteTrickHeadLine(stream);

        foreach (TelemetryTrickEntry tEntry in trickEntriesSorted) stream.WriteLine(tEntry);

        stream.WriteLine();
        //Write Menu Entries
        WriteMenuHeadLine(stream);

        foreach (TelemetryMenuEntry menuEntry in menuEntriesSorted) stream.WriteLine(menuEntry);

        stream.WriteLine();
        //Write all together sorted by frame
        List<TelemetryEntry> allEntries = new();
        allEntries.AddRange(cardEntries);
        allEntries.AddRange(trickEntries);
        allEntries.AddRange(menuEntries);

        WriteAllHeadline(stream);

        foreach (TelemetryEntry entry in allEntries.OrderBy(e => e.frameStamp)) {
            if (entry is TelemetryCardEntry c) stream.WriteLine(c);
            else if (entry is TelemetryTrickEntry t) stream.WriteLine(t);
            else if (entry is TelemetryMenuEntry m) stream.WriteLine(m);
        }

        stream.WriteLine();
        */
        //Write perforance frame data
        WritePerformanceHeadLine(stream);
        foreach (TelemetryPerformanceEntry pEntry in performanceEntriesSorted) stream.WriteLine(pEntry);

        stream.Close();
    }

}
