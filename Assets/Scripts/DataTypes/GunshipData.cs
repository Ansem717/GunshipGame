//---------------------------------------------------------
// file:	MasterController.cs
// author:	Andy Malik
// email:	andy.malik@digipen.edu
// term:	Spring 2026
//
// brief:	A persistant singleton instance to provide global user controls
//
// Copyright (c) 2026 DigiPen (USA) Corporation
//---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GunshipSize { Small, Medium, Large };

public class GunshipData {

    public GunshipSize Size;
    public float MaxHealth;
    public float Scale;
    private List<ScriptableObject> dataObjs;

    public GunshipData(GunshipSize Size, float MaxHealth, float Scale, List<ScriptableObject> dataObjs) {
        this.Size = Size;
        this.MaxHealth = MaxHealth;
        this.Scale = Scale;
        this.dataObjs = dataObjs;
    }

    public List<T> GetData<T>() {
        return dataObjs.OfType<T>().ToList();
    }

    public override string ToString() {
        string r = $"Size: {Size}, Scale: {Scale}, Scripts: [ ";
        foreach (ScriptableObject so in dataObjs) {
            r += $"{so}, ";
        }
        return r + "]";
    }


    static public GunshipData Get(GunshipSize size) {

        return size switch {
            GunshipSize.Small => new(GunshipSize.Small, 32, 0.8f, new() {
                Resources.Load<CustomPhysics_SO>("ScriptableObjects/Physics/GSP_Small"),
                Resources.Load<ChainGun_SO>("ScriptableObjects/Chaingun/GS_CG_Small"),
                Resources.Load<Missle_SO>("ScriptableObjects/Missle/GS_Missle_Small")
            }),
            GunshipSize.Medium => new(GunshipSize.Medium, 64, 1.3f, new() {
                Resources.Load<CustomPhysics_SO>("ScriptableObjects/Physics/GSP_Medium"),
                Resources.Load<ChainGun_SO>("ScriptableObjects/Chaingun/GS_CG_Medium"),
                Resources.Load<Missle_SO>("ScriptableObjects/Missle/GS_Missle_Medium")
            }),
            GunshipSize.Large => new(GunshipSize.Large, 128, 1.8f, new() {
                Resources.Load<CustomPhysics_SO>("ScriptableObjects/Physics/GSP_Large"),
                Resources.Load<ChainGun_SO>("ScriptableObjects/Chaingun/GS_CG_Large"),
                Resources.Load<ChainGun_SO>("ScriptableObjects/Chaingun/GS_CG_Large"),
                Resources.Load<Missle_SO>("ScriptableObjects/Missle/GS_Missle_Large")
            }),
            _ => throw new System.NotImplementedException(),
        };

    }

}
