using UnityEngine;
using System;

public class FlagControl : MonoBehaviour {

    public float range = 100.0f;

    public float full = 100000;
    public float initRatio = 0.5f;
    public float recoverRatio = 0.0001f;
    private float current = 50000;
    private float currentRatio = 0.5f;

    public LifeBarControll lifeBarControll;

    public SpriteRenderer flagrange;

    public GameController gameController;
    void Awake()
    {
        //lifeBarControll = gameObject.GetComponentInChildren<LifeBarControll>();
        currentRatio = initRatio;
        current = currentRatio * full;

        //SpriteRenderer[] objs = gameObject.GetComponentsInChildren<SpriteRenderer>();
        //foreach (SpriteRenderer spr in objs)
        //{
        //    Debug.Log("Awake FlagControl tag" + spr.tag);
        //    if ("flagrange".Equals(spr.tag))
        //    {
        //        flagrange = spr;
        //        flagrange.transform.localScale = Vector3.one * range * 2;
        //        break;
        //    }
        //}
        flagrange.transform.localScale = Vector3.one * range * 2;
    }
	// Use this for initialization
	void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void FixedUpdate() {
        CheckNearObjects();
    }

    void CheckNearObjects()
    {
        Collider2D[] objects = Physics2D.OverlapCircleAll(transform.position, range);

        //if (GameController.debugLifeBar)
        //    Debug.Log("CheckNearObjects objects length " + objects.Length);

        int enemyDominate = 0;
        int playDominate = 0;
        foreach (Collider2D obj in objects)
        {
            //if (GameController.debugLifeBar)
            //    Debug.Log("CheckNearObjects obj " + obj);
            BaseUnit unit = obj.gameObject.GetComponent<BaseUnit>();
            if (unit != null)
            {
                //if (GameController.debugLifeBar)
                //    Debug.Log("CheckNearObjects unit " + unit);

                if (unit.Myfaction == BaseUnit.Faction.Enemy) {
                    enemyDominate += unit.Dominate;
                } else if (unit.Myfaction == BaseUnit.Faction.Player) { 
                    playDominate += unit.Dominate;
                }
            }
        }

        if (enemyDominate == 0 && playDominate == 0 && (currentRatio != 0.5f && currentRatio != 0 && currentRatio != 1))
        {
            //recover if not occupied by any player and no units in the flag range
            if(currentRatio > 0.5f)
                currentRatio = Math.Max(Math.Min(currentRatio - recoverRatio, 1f), 0f);
            else if (currentRatio < 0.5f)
                currentRatio = Math.Max(Math.Min(currentRatio + recoverRatio, 1f), 0f);
        }
        else {
            current = Math.Max(Math.Min(current + (playDominate - enemyDominate), full), 0f);
            currentRatio = current / full;
        }
        
        //if (GameController.debugLifeBar)
        Debug.Log("CheckNearObjects " + current + ", " + playDominate + ", " + enemyDominate + ", " + currentRatio + ", " + currentState);
        lifeBarControll.UpdateLifeBar(currentRatio);
        if (currentRatio <= 0) {
            currentState = Faction.Enemy;
        } else if (currentRatio >= 1)
        {
            currentState = Faction.Player;
        } else {
            if (currentRatio <= 0.5f && currentState == Faction.Player)
            {
                currentState = Faction.Neutral;
            } else if (currentRatio >= 0.5f && currentState == Faction.Enemy)
            {
                currentState = Faction.Neutral;
            }
        }

        if (currentState == Faction.Player) {
            gameController.PlayerScore += increaseAount;
        }
        else if (currentState == Faction.Enemy)
        {
            gameController.EnemyScore += increaseAount;
        }
    }

    public float increaseAount;

    private Faction currentState = Faction.Neutral;
    public enum Faction
    {
        Neutral, Player, Enemy
    }
}
