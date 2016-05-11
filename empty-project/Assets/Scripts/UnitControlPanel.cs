using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UnitControlPanel : MonoBehaviour {

    private bool select;

    public void toggleSelect()
    {
        Select = !select;
    }

    public bool Select
    {
        get
        {
            return select;
        }
        set
        {
            select = value;
            if (select)
            {
                mUnselectPanel.enabled = false;
                mSelectPanel.enabled = true;
                if (controlObject is BaseUnit) {
                    ((BaseUnit)controlObject).Select = true;
                }
            }
            else {
                mUnselectPanel.enabled = true;
                mSelectPanel.enabled = false;
            }
        }
    }

    private bool cooldownReady = true;

    public bool CooldownReady
    {
        get
        {
            return cooldownReady;
        }
        set
        {
            cooldownReady = value;
            if (cooldownReady)
            {
            }
            else {
            }
        }
    }

    Object panelObject;
    Object controlObject;
    float cooldownTime;

    public const string TagOfPanelSelect = "unitControlPanelSelect";
    public const string TagOfPanelUnselect = "unitControlPanelUnselect";

    public SpriteRenderer mUnselectPanel;
    public SpriteRenderer mSelectPanel;

    public void setPanelObject(Object obj, float cooldown) {
        panelObject = obj;
        id = System.Guid.NewGuid().ToString();
        this.cooldownTime = cooldown;
    }

    public void setControlObject(Object obj)
    {
        controlObject = obj;
        if (controlObject == null)
        {
            resetCircularSlider();
        } else
        {
            if (controlObject is BaseUnit)
            {
                ((BaseUnit)controlObject).Select = true;
            }
        }
    }

    public Object getControlObject()
    {
        return controlObject;
    }

    public Object getPanelObject()
    {
        return panelObject;
    }

    public Image circularSilder;            //Drag the circular image i.e Slider in our case

    public void consumeCooldownReady()
    {
        CooldownReady = false;
    }

    public void resetCircularSlider() {
        circularSilder.fillAmount = 1f;
        startCooldown = true;
    }

    private bool startCooldown = false;

    private void updateCircularSlider()
    {
        circularSilder.fillAmount -= Time.deltaTime / cooldownTime;
        Logger.controlPanel("updateCircularSlider {0}", circularSilder.fillAmount);
        if (circularSilder.fillAmount <= 0)
        { //cooldownReady
            circularSilder.fillAmount = 0;
            CooldownReady = true;
            startCooldown = false;
        }        
    }

    void Start () {

        //SpriteRenderer[] sprites = gameObject.GetComponentsInChildren<SpriteRenderer>();
        //foreach (SpriteRenderer spr in sprites)
        //{
        //    if (TagOfPanelUnselect.Equals(spr.tag))
        //    {
        //        mUnselectPanel = spr;
        //        continue;
        //    } else if (TagOfPanelSelect.Equals(spr.tag))
        //    {
        //        mSelectPanel = spr;
        //        continue;
        //    }
        //}
    }
	
	void Update () {
        if (startCooldown)
            updateCircularSlider();
	}

    private string id = string.Empty;

    public string getId() {
        return id;
    }
}
