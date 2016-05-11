using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : Humanoid {

    // Use this for initialization
    void Start() {
        Debug.Log("Player onStart+");
        Debug.Log("Player onStart-");
    }

    // Update is called once per frame
    protected override void onFixedUPdate()
    {
        onInteract();
    }


    public void onInteract()
    {
        Logger.controlPanel("onInteract {0}", Application.platform);
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (Input.touchCount > 0)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    checkTouch(Input.GetTouch(0).position);
                }
            }
        }
        else if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            Logger.controlPanel("onInteract Input.GetMouseButtonDown(0) {0}", Input.GetMouseButtonDown(0));
            if (Input.GetMouseButtonDown(0))
            {
                checkTouch(Input.mousePosition);
            }
        }
    }

    public void checkTouch(Vector3 pos)
    {
        Vector3 wp = Camera.main.ScreenToWorldPoint(pos);
        Vector3 touchPos = new Vector3(wp.x, wp.y, wp.z);
        Collider2D hit = Physics2D.OverlapPoint(touchPos);

        if (hit != null)
        {
            //Debug.Log(hit.transform.gameObject.name);
            //hit.transform.gameObject.SendMessage("Clicked", touchPos, SendMessageOptions.RequireReceiver);
            BaseUnit unit = hit.GetComponent<BaseUnit>();
            if (unit != null)
            {
                setChooseUnit(unit);

                if (mChooseUnit != null)
                {
                    UnitControlPanel linkedControlPanel = mChooseUnit.getLinkControlPanel();
                    if (linkedControlPanel != null)
                    {
                        setChooseControlPanel(linkedControlPanel);
                    }
                    return;
                }
            }

            UnitControlPanel controlPanel = hit.GetComponent<UnitControlPanel>();
            //TODO: fix this hack
            if (controlPanel != null && controlPanel.getPanelObject() == null)
            {
                controlPanel.setPanelObject(myUnits[0], 10);
            }

            if (controlPanel != null)
            {
                setChooseControlPanel(controlPanel);

                if (mChooseUnitControlPanel != null)
                {
                    Object controlObject = mChooseUnitControlPanel.getControlObject();
                    if (controlObject is BaseUnit && mChooseUnitControlPanel.Select)
                    {
                        setChooseUnit((BaseUnit)controlObject);

                    }
                    else {
                        setChooseUnit(null);
                    }
                }

                return;
            }

            Logger.controlPanel("hit != null {0}", hit);

        }
        else {
            if (mChooseUnit != null)
            {
                moveUnit(touchPos);
            }

            else if (mChooseUnitControlPanel != null)
            {
                Object panelObject = mChooseUnitControlPanel.getPanelObject();
                Logger.controlPanel("control panelObject {0}", panelObject);
                if (mChooseUnitControlPanel.CooldownReady && panelObject is GameObject && ((GameObject)panelObject).GetComponent<BaseUnit>() != null)
                {
                    createUnit((GameObject)panelObject, getSpawnlocation(), touchPos, mChooseUnitControlPanel);
                    mChooseUnitControlPanel.consumeCooldownReady();
                    //setChooseControlPanel(null);
                }
            }
            else {
                Logger.controlPanel("clear choose");
                setChooseUnit(null);
                setChooseControlPanel(null);
            }
            Logger.controlPanel("hit == null {0}, {1}", mChooseUnit, mChooseUnitControlPanel);
        }
    }

    protected override BaseUnit.Faction getHumanoidFaction()
    {
        return BaseUnit.Faction.Player;
    }

    protected void createUnit(GameObject gameObject, Vector3 spawnPos, Vector3 touchPos, UnitControlPanel panel)
    {
        Logger.firebase("createUnit {0}", gameObject);
        Logger.controlPanel("createUnit {0}", gameObject);

        gameController.eventCreateUnit(panel.getId(), null, spawnPos, touchPos);
    }

    protected void moveUnit(Vector3 p)
    {
        gameController.eventMoveUnit(mChooseUnit.Id, null, mChooseUnit.gameObject.transform.localPosition, p);
    }

    protected override BaseUnit onCreateUnit(RemoteEventBase theEvent)
    {
        BaseUnit unit = base.onCreateUnit(theEvent);
        setChooseUnit(unit);

        UnitControlPanel panel = gameController.getIdMatchedControlPanel(unit.Id);
        if (panel != null)
        {
            unit.setLinkControlPanel(panel);
            panel.setControlObject(unit);

        }        

        return unit;
    }

    //protected void createUnit2(GameObject gameObject, Vector3 touchPos, UnitControlPanel panel)
    //{
    //    Logger.firebase("createUnit {0}", gameObject);
    //    Logger.controlPanel("createUnit {0}", gameObject);

    //    Debug.Log("cameraPoint + newUnit x " + touchPos.x + ", y " + touchPos.y + ", z " + touchPos.z);
    //    GameObject obj = Instantiate(gameObject, touchPos, Quaternion.identity) as GameObject; ;// Instantiate(myUnits[0], p, Quaternion.identity) as GameObject;
    //    obj.transform.parent = gameController.transform;
    //    Logger.firebase("Player + newUnit {0}, x{0}, y{1}, z{2}", obj.GetInstanceID().ToString(), obj.transform.localPosition.x, obj.transform.localPosition.y, obj.transform.localPosition.z);
    //    BaseUnit unit = obj.GetComponent<BaseUnit>();
    //    unit.Id = obj.GetInstanceID().ToString();
    //    unit.Myfaction = BaseUnit.Faction.Player;
    //    unit.setGameController(gameController);
    //    unit.setHumanoidHolder(this);
    //    unitInstances.Add(unit.Id, unit);
    //    gameController.eventCreateUnit(unit.Id, null, touchPos, touchPos);

    //    unit.setLinkControlPanel(panel);
    //    panel.setControlObject(unit);
    //    setChooseUnit(unit);
    //}

    protected void createEnemyUnit()
    {
        Vector3 cameraPointP = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 p = new Vector3(cameraPointP.x, cameraPointP.y, transform.position.z);
        gameController.eventCreateEnemyUnit(Random.value.ToString(), null, p, p);
    }

    protected void moveAllUnit() {
        Vector3 cameraPointP = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 p = new Vector3(cameraPointP.x, cameraPointP.y, transform.position.z);
        Debug.Log("cameraPoint + newUnit x " + p.x + ", y " + p.y + ", z " + p.z);

        foreach (string unitKey in unitInstances.Keys)
        {
            BaseUnit unit;
            unitInstances.TryGetValue(unitKey, out unit);
            unit.nextLocation = p;
            Debug.Log("move + unit " + unitKey + "#" + unit + ", x " + p.x + ", y " + p.y + ", z " + p.z);
            gameController.eventMoveUnit(unit.Id, null, unit.gameObject.transform.localPosition, p);
        }
    }

    //protected void moveUnit2(Vector3 p)
    //{
    //    Debug.Log("cameraPoint + newUnit x " + p.x + ", y " + p.y + ", z " + p.z);
    //    mChooseUnit.nextLocation = p;
    //    Debug.Log("move + unit " + mChooseUnit.Id + "#" + mChooseUnit + ", x " + p.x + ", y " + p.y + ", z " + p.z);
    //    mChooseUnit.BeCommanded = true;
    //    gameController.eventMoveUnit(mChooseUnit.Id, null, mChooseUnit.gameObject.transform.localPosition, p);
    //}
}
