using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Text;
using System;

public class Benchmark {
    Dictionary<string, List<DateTime>> startTimes;
    Dictionary<string, List<DateTime>> endTimes;
    Dictionary<string, TimeSpan> averageTime;
    Dictionary<string, TimeSpan> totalTime;
    Dictionary<string, int> numberOfBenchmarks;
    string BenchmarkFamilyName;
    int PadN = 30;
    public Benchmark(string _BenchmarkFamilyName)
    {
        BenchmarkFamilyName = _BenchmarkFamilyName;
        startTimes = new Dictionary<string, List<DateTime>>();
        endTimes = new Dictionary<string, List<DateTime>>();
        averageTime = new Dictionary<string, TimeSpan>();
        totalTime = new Dictionary<string, TimeSpan>();
        numberOfBenchmarks = new Dictionary<string, int>();
    }
    public static void Start(Benchmark b, string s)
    {
        if (b != null)
        {
            b.StartBenchmark(s);
        }
    }
    public static void End(Benchmark b, string s)
    {
        if (b != null)
        {
            b.EndBenchmark(s);
        }
    }
    public static void Write(Benchmark b, string s)
    {
        if (b != null)
        {
            b.WriteBenchmarkToDebug();
        }
    }
    public void StartBenchmark(string MarkName)
    {
        if (!startTimes.ContainsKey(MarkName))
        {
            startTimes[MarkName] = new List<DateTime>();
        }
        startTimes[MarkName].Add(DateTime.Now);
    }
    public void EndBenchmark(string MarkName)
    {
        if (!endTimes.ContainsKey(MarkName))
        {
            endTimes[MarkName] = new List<DateTime>();
        }
        endTimes[MarkName].Add(DateTime.Now);
    }
    private void TabulateBenchmarks()
    {
        foreach (KeyValuePair<string, List<DateTime>> kvp in startTimes)
        {
            int NumberOfBenchmarks = 0;
            string keyString = kvp.Key;
            //Debug.Log("Tabulating " + keyString);
            totalTime[keyString] = TimeSpan.Zero;
            for (int i = 0; i < kvp.Value.Count(); i++)
            {
                TimeSpan elapsedTime = endTimes[keyString][i] - startTimes[keyString][i];
                totalTime[keyString] += elapsedTime;
                NumberOfBenchmarks++;
            }
            averageTime[keyString] = new TimeSpan(totalTime[keyString].Ticks / NumberOfBenchmarks);
            numberOfBenchmarks[keyString] = NumberOfBenchmarks;
        }
    }
    private List<string> BenchmarkText()
    {
        TabulateBenchmarks();
        List<string> BenchLines = new List<string>();
        string LineString = new string('-', PadN - 5);
        string[] titles = new string[] { "Benchmark Family", "Benchmark Name", "Total Time", "Average Time", "Number" };
        string titleString = "";
        string headerString = "";
        foreach (string title in titles)
        {
            titleString += title.PadRight(PadN);
            headerString += LineString.PadRight(PadN);
        }
        BenchLines.Add("Current Time:" + DateTime.Now.ToString());
        BenchLines.Add(titleString);
        BenchLines.Add(headerString);
        foreach (KeyValuePair<string, TimeSpan> kvp in totalTime)
        {
            string k = kvp.Key;
            string totalTimeString = FormatTimespan(totalTime[k]);
            string averageTimeString = FormatTimespan(averageTime[k]);
            string MiddleString = BenchmarkFamilyName.PadRight(PadN);
            MiddleString += k.PadRight(PadN);
            MiddleString += totalTimeString.PadRight(PadN);
            MiddleString += averageTimeString.PadRight(PadN);
            MiddleString += numberOfBenchmarks[k].ToString().PadRight(PadN);
            BenchLines.Add(MiddleString);
        }
        BenchLines.Add(headerString);
        BenchLines.Add("");
        return BenchLines;
    }
    public void WriteBenchmarkToDebug()
    {
        List<string> tlines = BenchmarkText();
        foreach (string t in tlines)
        {
            Debug.Log(t);
        }
    }
    public string FormatTimespan(TimeSpan ts)
    {
        return ts.Hours.ToString() + ":" + ts.Minutes.ToString() + ":" + ts.Seconds.ToString() + ":" + ts.Milliseconds.ToString();
    }
}
