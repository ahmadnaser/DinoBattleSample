using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Humanoid : MonoBehaviour {
    protected GameObject StartingCamp;

    private string id;
    public string Id
    {
        get {
            return id;
        }

        set
        {
            id = value;
        }
    }
    protected GameController gameController;
    public GameObject[] myUnits;
    protected Dictionary<string, BaseUnit> unitInstances = new Dictionary<string, BaseUnit>();
    // Use this for initialization
    void Start () {
        Debug.Log("Humanoid start+");
        onStart();
        Debug.Log("Humanoid start-");
    }

    protected virtual void onStart() {
    }

    protected virtual void onUpdate()
    {
    }

    protected virtual void onFixedUPdate()
    {
    }

    void Update() {
        onUpdate();
    }

    ArrayList toCreateList = new ArrayList();
    ArrayList copyToCreateList = new ArrayList();
    ArrayList toUpdateList = new ArrayList();
    ArrayList copyToUpdateList = new ArrayList();
    System.Object mylock = new System.Object();
    System.Object unitlock = new System.Object();

    // Update is called once per frame
    void FixedUpdate()
    {
        lock (mylock)
        {
            foreach (System.Object o in toCreateList)
            {
                copyToCreateList.Add(o);
            }
            toCreateList.Clear();
        }


        foreach (System.Object o in copyToCreateList)
        {
            RemoteEventBase todoEvent = (RemoteEventBase)o;

            Debug.Log("Humanoid onRemoteCreate +" + " " + todoEvent.getEventId() + " " + todoEvent.getCurrentLocation().y);
            onCreateUnit(todoEvent);
        }
        copyToCreateList.Clear();


        lock (unitlock)
        {
            foreach (System.Object o in toUpdateList)
            {
                copyToUpdateList.Add(o);
            }
            toUpdateList.Clear();
        }

        foreach (System.Object o in copyToUpdateList)
        {
            RemoteEventBase todoEvent = (RemoteEventBase)o;

            BaseUnit result = null;
            if (unitInstances.TryGetValue(todoEvent.getActorId(), out result))
            {
                Logger.firebase("Humanoid onRemoteUpdate find " + todoEvent.getActorId() + ", " + todoEvent.getCurrentLocation().y + ", " + gameObject.transform.localPosition.y + ", " + todoEvent.getTargetLocation().y);

                onMoveUnit(todoEvent, result);

            }
            else {

                Logger.firebase("Humanoid onRemoteUpdate not find " + todoEvent.getActorId());

                onCreateUnit(todoEvent);
            }
        }
        copyToUpdateList.Clear();

        onFixedUPdate();
    }

    protected virtual BaseUnit onCreateUnit(RemoteEventBase theEvent)
    {

        Logger.firebase("Humanoid onRemoteUpdate not find " + theEvent.getActorId());
        Logger.firebase("Humanoid onRemoteCreate +" + " " + theEvent.getActorId());
        BaseUnit temp = null;
        if (unitInstances.TryGetValue(theEvent.getActorId(), out temp)) {
            Logger.firebase("Humanoid onRemoteCreate fail due to exist +" + " " + theEvent.getActorId());
            return null;
        }
        GameObject obj = Instantiate(myUnits[0], convertLocationByHumanoid(theEvent.getCurrentLocation()), Quaternion.identity) as GameObject;
        obj.transform.parent = gameController.transform;
        BaseUnit unit = obj.GetComponent<BaseUnit>();
        unit.setGameController(gameController);
        unit.Myfaction = getHumanoidFaction();
        unit.Id = theEvent.getActorId();
        unit.playerIdOfUnit = theEvent.getPlayerId();
        unit.setHumanoidHolder(this);
        unitInstances.Add(unit.Id, unit);

        onMoveUnit(theEvent, unit);

        return unit;
    }

    protected virtual void onMoveUnit(RemoteEventBase theEvent, BaseUnit unit)
    {
        unit.gameObject.transform.localPosition = convertLocationByHumanoid(theEvent.getCurrentLocation());
        unit.nextLocation = convertLocationByHumanoid(theEvent.getTargetLocation());
        unit.BeCommanded = true;
    }

    protected virtual BaseUnit.Faction getHumanoidFaction()
    {
        return BaseUnit.Faction.Neutral;
    }

    protected virtual Vector3 convertLocationByHumanoid(Vector3 vp)
    {
        return vp;
    }

    public void onRemoteCreate(RemoteEventBase theEvent)
    {

        Logger.firebase("Humanoid onRemoteCreate +" + " " + theEvent.getEventId());

        lock (mylock)
        {
            toCreateList.Add(theEvent);
        }


    }

    public void onRemoteUpdateMove(RemoteEventBase theEvent)
    {

        Logger.firebase("Humanoid onRemoteUpdate +" + " " + theEvent.getEventId());

        lock (unitlock)
        {
            toUpdateList.Add(theEvent);
        }
    }




    public void setGameController(GameController controller) {
        gameController = controller;
    }

    protected BaseUnit mChooseUnit;

    public void setChooseUnit(BaseUnit unit)
    {
        if (mChooseUnit != null && mChooseUnit != unit)
        {
            mChooseUnit.Select = false;
        }

        if (unit != null)
        {
            unit.Select = true;
        }
        mChooseUnit = unit;
    }

    public void onBaseUnitDead(BaseUnit unit) {
        if (mChooseUnit != null && mChooseUnit == unit) {
            unit.Select = false;
            mChooseUnit = null;
        }

        if (unit != null) {
            unitInstances.Remove(unit.Id);
        }
    }

    protected UnitControlPanel mChooseUnitControlPanel;

    public void setChooseControlPanel(UnitControlPanel controlPanel) {
        if (mChooseUnitControlPanel != null && mChooseUnitControlPanel != controlPanel) {
            mChooseUnitControlPanel.Select = false;
        }

        if (controlPanel != null) {
            controlPanel.toggleSelect();
        }

        mChooseUnitControlPanel = controlPanel;
    }

    public void setStartingCamp(GameObject obj) {
        StartingCamp = obj;
    }

    protected Vector3 getSpawnlocation() {
        return StartingCamp.transform.localPosition;
    }

}
