using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Enemy : Humanoid
{
    // Use this for initialization
    public void Start()
    {
        Debug.Log("Enemy onStart+");
        Debug.Log("Enemy onStart-");
    }

    protected override BaseUnit.Faction getHumanoidFaction()
    {
        return BaseUnit.Faction.Enemy;
    }

    protected override Vector3 convertLocationByHumanoid(Vector3 vp)
    {
        return new Vector3(vp.x, vp.y * -1f, vp.z * -1f);
    }

}
