//---------------------------------------------------------
// file:	FrameBlock.cs
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// term:	Spring 2026
//
// brief:	A data type to group the number of frames each second into blocks
//
// Copyright (c) 2026 DigiPen (USA) Corporation
//---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FrameBlock {

    private readonly int min = 4;
    private readonly int max = 60;

    public List<int> frames;

    private readonly int _size;
    public int Size { get => _size; }

    private float elapsed = 0;
    private int accumulatedFrames = 0;

    private float tElapsed = 0;

    public FrameBlock(int size) {

        if (size < min) _size = min;
        else if (size > max) _size = max;
        else _size = size;

        frames = new();
    }

    public void Update() {
        elapsed += Time.deltaTime;
        tElapsed += Time.deltaTime;
        accumulatedFrames++;
        if (elapsed >= 1f) {
            elapsed = 0;
            frames.Add(accumulatedFrames);
            accumulatedFrames = 0;
            MasterController.Singleton.actionListsDirty = true;

            if (frames.Count > Size) {
                frames.RemoveAt(0); //if we hit our capacity, we must discard the first item to keep the block "moving"
            }
        }


        if (tElapsed >= Size) {
            //wait "Size" seconds for a full block and record telemetry
            Telemetry.Instance.RecordPerformanceEntry(Size, Worst(), Mean(), Median());
            tElapsed = 0;
        }

    }

    public float Mean() {
        if (frames.Count == 0) return 0;
        return (float)frames.Average();
    }

    public int Median() {
        if (frames.Count == 0) return 0;
        return frames.OrderBy(f => f).ToList()[frames.Count / 2]; //sort the frames, put em in a list, grab the middle spot
    }

    public int Worst() {
        if (frames.Count == 0) return 0;
        return frames.Min();
    }

    public int Best() {
        if (frames.Count == 0) return 0;
        return frames.Max();
    }

}

