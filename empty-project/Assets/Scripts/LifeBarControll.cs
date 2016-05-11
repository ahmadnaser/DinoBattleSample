using UnityEngine;
using System.Collections;

public class LifeBarControll : MonoBehaviour {
    public SpriteRenderer lifeBar;			// Reference to the sprite renderer of the health bar.
    public SpriteRenderer lifeBarBg;			// Reference to the sprite renderer of the health bar.
    private Vector3 lifeBarScale;				// The local scale of the health bar initially (with full health).
    public Color bgColor;
    public Color fgColor;
    // Use this for initialization
    void Awake() {
        lifeBar.material.color = fgColor;
        lifeBarScale = lifeBar.transform.localScale;
        lifeBarBg.material.color = bgColor;
        //SpriteRenderer[] objs = gameObject.GetComponentsInChildren<SpriteRenderer>();
        //foreach (SpriteRenderer spr in objs)
        //{
        //    Debug.Log("Awake spr tag" + spr.tag);
        //    if ("lifebar".Equals(spr.tag))
        //    {
        //        lifeBar = spr;
        //        lifeBar.material.color = fgColor;
        //        lifeBarScale = lifeBar.transform.localScale;
        //        //lifeBar.color = Color.green;
        //        //if (GameController.debugLifeBar)
        //            Debug.Log("Awake lifeBar " + lifeBar.GetInstanceID() + fgColor + ", " + Color.green + ", " + lifeBar.material.color + ", " + lifeBarScale.x);
        //    }

        //    if ("lifebarbg".Equals(spr.tag))
        //    {
        //        lifeBarBg = spr;
        //        lifeBarBg.material.color = bgColor;

        //        //if (GameController.debugLifeBar)
        //            Debug.Log("Awake lifeBarBg " + lifeBarBg.GetInstanceID() + bgColor + ", " + Color.red + ", " + lifeBarBg.material.color);
        //    }
        //}   
    }

	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        //Debug.Log("update lifeBar " + lifeBar.GetInstanceID() + fgColor + ", " + Color.green + ", " + lifeBar.transform.localScale.x + ", " + lifeBar.material.color);
        //Debug.Log("update lifeBarBg " + lifeBarBg.GetInstanceID() + bgColor + ", " + Color.red + ", " + lifeBarBg.transform.localScale.x + ", " + lifeBarBg.material.color);

    }

    public void UpdateLifeBar(float ratio)
    {
        // Set the health bar's colour to proportion of the way between green and red based on the player's health.
        //lifeBar.material.color = Color.Lerp(Color.green, Color.red, 1 - ratio);

        // Set the scale of the health bar to be proportional to the player's health.
        lifeBar.transform.localScale = new Vector3(lifeBarScale.x * ratio, 1, 1);

        Logger.flagControl("UpdateLifeBar " + ratio + ", " + lifeBarScale.x);
    }
}
