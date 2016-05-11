using UnityEngine;
using System.Collections;
using System;
using Pathfinding;
using System.Threading;

public class BaseUnit : MonoBehaviour {
    private Seeker seeker;
    public bool damageable = true;
    GameObject target;

    private Humanoid mHumanoidHolder;
    public SpriteRenderer unitPic;

    public void setHumanoidHolder(Humanoid h) {
        mHumanoidHolder = h;
    }

    //void OnMouseDown()
    //{
    //    mHumanoidHolder.setChooseUnit(this);
    //}

    private bool select;

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
                //SpriteRenderer sr = gameObject.GetComponent<SpriteRenderer>();
                //mLastColor = sr.color;
                //sr.color = Color.yellow;
                mSelectArrow.enabled = true;
            }
            else {
                //SpriteRenderer sr = gameObject.GetComponent<SpriteRenderer>();
                //sr.color = mLastColor;
                mSelectArrow.enabled = false;
            }
        }
    }
    private bool moveOnCommandedPath;
    private bool beCommanded;

    public bool BeCommanded
    {
        get
        {
            return beCommanded;
        }
        set
        {
            beCommanded = value;
        }
    }

    public enum Faction
    {
        Neutral, Player, Enemy
    }

    private Faction myfaction;

    public Faction Myfaction
    {
        get
        {
            return myfaction;
        }
        set
        {
            myfaction = value;
        }
    }

    public int dominate;

    public int Dominate {
        get {
            return dominate;
        }

        set {
            dominate = value;
        }
    }

    public int life;

    public int Life
    {
        get
        {
            return life;
        }

        set
        {
            life = value;
        }
    }

    public int maxlife;

    public int attack;

    public int Attack
    {
        get
        {
            return attack;
        }

        set
        {
            attack = value;
        }
    }

    public int actionRange;
    public int detectRange;

    public Vector3 nextLocation;

    public bool IsPlayer {
        get {
            return Myfaction == Faction.Player;
        }
    }

    public float deathSpinMin = -100f;          // A value to give the minimum amount of Torque when dying
    public float deathSpinMax = 100f;			// A value to give the maximum amount of Torque when dying

    private string id;
    public string Id {
        get {
            return id;
        }

        set {
            id = value;
        }
    }

    public LifeBarControll lifeBarControll;
    public Collider2D mCollider;
    public SpriteRenderer mSelectArrow;
    public const string TagOfSelectArrow = "unitSelectArrow";
    public float SelectArrowShift;
    public float SelectArrowShiftFrameCounts;
    public float SelectArrowInit;
    private float SelectArrowReverse = 1f;
    private void animateSelectArrowIfNeed() {
        if (!Select)
            return;

        float y = (SelectArrowShift/ SelectArrowShiftFrameCounts) * SelectArrowReverse;
        //mSelectArrow.transform.localPosition.Set(mSelectArrow.transform.localPosition.x, y, mSelectArrow.transform.localPosition.z);
        mSelectArrow.transform.localPosition = new Vector3(mSelectArrow.transform.localPosition.x, mSelectArrow.transform.localPosition.y + y, mSelectArrow.transform.localPosition.z);
        if (Math.Abs(SelectArrowInit - mSelectArrow.transform.localPosition.y) >= SelectArrowShift)
            SelectArrowReverse *= -1f;

        Logger.d("test time", "init y {0}, current y{1}, shift y{2}, reverse {3}, abs {4}", SelectArrowInit, mSelectArrow.transform.localPosition.y, SelectArrowShift, SelectArrowReverse, Math.Abs(SelectArrowInit - mSelectArrow.transform.localPosition.y));
    }


    public string playerIdOfUnit;

    //The AI's speed per second
    public float speed = 100;
    //The max distance from the AI to a waypoint for it to continue to the next waypoint
    public float nextWaypointDistance = 3;

    public float attackRangeTolerence = 0.5f;

    public Vector3 getUnitPosition(Collider2D collider)
    {
        return new Vector3(collider.transform.localPosition.x + collider.offset.x, collider.transform.position.y + collider.offset.y, collider.transform.position.z);
    }

    public BaseUnit() { }

    void Awake()
    {
        //lifeBarControll = gameObject.GetComponentInChildren<LifeBarControll>();
        //mCollider = gameObject.GetComponent<Collider2D>();
        //SpriteRenderer[] sprites = gameObject.GetComponentsInChildren<SpriteRenderer>();
        //foreach (SpriteRenderer spr in sprites)
        //{
        //    if (TagOfSelectArrow.Equals(spr.tag)) {
        //        mSelectArrow = spr;
        //        SelectArrowInit = mSelectArrow.transform.localPosition.y;
        //        break;
        //    }
        //}
    }


    // Use this for initialization
    void Start()
    {

        if (!IsPlayer) { 
            unitPic.color = Color.red;
        }
        checkIfNeedToFindNextTarget();
    }

    private bool togglePause = false;

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("BaseUnit + Update " + isPlayer);
        //if (Input.GetMouseButtonDown(0))
        //{
        //    togglePause = !togglePause;
        //    return;
        //}

        //if (togglePause)
        //    return;
        animateSelectArrowIfNeed();

        if (BeCommanded) {
            //Vector3 direction = nextLocation - getUnitPosition(mCollider);// target.transform.localPosition - transform.localPosition;
            //transform.localPosition += direction.normalized * getSpeed() * Time.deltaTime;
            //if (Vector3.Distance(transform.localPosition, nextLocation) > 3)
            //    return;            
            mActionTarget = null;
            startFindPath(nextLocation, 0, 1, true);
            BeCommanded = false;
            moveOnCommandedPath = true;
            return;
        }

        if ((mNumberOfTargetToBeCalculated > 0 && mCurrentCalculatedNumberOfTargetPath < mNumberOfTargetToBeCalculated))
        {
            return;
        }

        findActionTarget();
        bool handle = takeActionOnTargetIfNeeded();

        if (!handle) //we only move when action not taken
            startMoveToTarget(speed * Time.deltaTime);
    }

    void FixedUpdate()
    {
        //startMoveToTarget(speed * Time.deltaTime);
        //findActionTarget();
        //takeActionOnTargetIfNeeded();
    }

    public float getSpeed() {
        return 20f;
    }

    public GameController gameController;
    public void setGameController(GameController controller)
    {
        gameController = controller;
    }

    public float repeatDamagePeriod = 2f;		// How frequently the unit can attack.
    private float lastHitTime;					// The time at which the player was last hit.

    void OnCollisionEnter2D(Collision2D col)
    {
        //BaseUnit unit = col.gameObject.GetComponent<BaseUnit>();
        //Debug.Log("OnCollisionEnter2D " + unit);
        //// If the colliding gameobject is an Enemy...
        ////if (col.gameObject.tag == "Enemy")
        //if (unit != null && unit.Myfaction != Myfaction)
        //{
        //    Debug.Log("OnCollisionEnter2D target " + unit.Myfaction);
        //    // ... and if the time exceeds the time of the last hit plus the time between hits...
        //    if (Time.time > lastHitTime + repeatDamagePeriod)
        //    {
        //        // ... take damage and reset the lastHitTime.
        //        attackEnemy(unit);
        //        lastHitTime = Time.time;
        //    }
        //}
    }

    void attackEnemy(BaseUnit enemy)
    {
        string enemyId = enemy.Id;
        int damage = Attack;
        Vector3 hurtVector = Vector3.one;
        int hurtForce = 1;
        bool finish = enemy.BeAttacked(damage, hurtVector, hurtForce);
        if (finish)
        {
            mActionTarget = null;
            //mPath = null;
        }

        //gameController.attackEnemy(this.playerIdOfUnit, this.Id, enemyId, damage, hurtVector, hurtForce);
    }

    public bool BeAttacked(int damage, Vector3 hurtVector, int hurtForce)
    {

        // Add a force to the player in the direction of the vector and multiply by the hurtForce.
        GetComponent<Rigidbody2D>().AddForce(hurtVector * hurtForce);

        // Reduce the player's health by 10.
        life -= damage;

        // Update what the health bar looks like.
        UpdateHealthBar();

        if (life <= 0)
        {
            Death();
            return true;
        }

        return false;
    }

    Boolean dead = false;

    void Death()
    {
        // Find all of the sprite renderers on this object and it's children.
        SpriteRenderer[] otherRenderers = GetComponentsInChildren<SpriteRenderer>();

        // Disable all of them sprite renderers.
        foreach (SpriteRenderer s in otherRenderers)
        {
            s.enabled = false;
        }

        // Set dead to true.
        dead = true;
        if (linkedControlPanel != null)
        {
            linkedControlPanel.setControlObject(null);
        }
        setLinkControlPanel(null);
        // Allow the enemy to rotate and spin it by adding a torque.
        //GetComponent<Rigidbody2D>().fixedAngle = true;
        //GetComponent<Rigidbody2D>().AddTorque(UnityEngine.Random.Range(deathSpinMin, deathSpinMax));

        // Find all of the colliders on the gameobject and set them all to be triggers.
        Collider2D[] cols = GetComponents<Collider2D>();
        foreach (Collider2D c in cols)
        {
            c.isTrigger = true;
        }

        mHumanoidHolder.onBaseUnitDead(this);

        enabled = false;

        Destroy(this.gameObject);
    }

    public void UpdateHealthBar()
    {
        float currentRatio = (float)life / (float)maxlife;
        lifeBarControll.UpdateLifeBar(currentRatio);
    }

    //Imply attack 
    protected void findActionTarget() {
        if (mActionTarget != null || moveOnCommandedPath)
            return;
        Vector3 unitPosition = getUnitPosition(mCollider);
        Collider2D[] objects = Physics2D.OverlapCircleAll(unitPosition, detectRange);
        GameObject target = null;
        if (objects == null || objects.Length <= 0) {
            //Logger.findTarget("findActionTarget null {0}", objects);
            return;
        } else {
            //Logger.findTarget("findActionTarget objects length {0}", objects.Length);
        }

        float minDistance = float.MaxValue;

        RaycastHit2D theClosestRay = new RaycastHit2D();
        Logger.findTarget("findActionTarget OverlapCircleAll {0}, {1} : {2}, {3}", unitPosition.x, unitPosition.y, objects.Length, this.Myfaction);
        foreach (Collider2D obj in objects)
        {
            if (obj.gameObject == this.gameObject)
                continue;

            BaseUnit unit = obj.gameObject.GetComponent<BaseUnit>();
            if (unit == null || unit.dead)
                continue;
            //Logger.findTarget("findActionTarget obj y: {0}", obj.gameObject.transform.localPosition.y);
            //RaycastHit2D hit = Physics2D.Linecast(transform.localPosition, obj.gameObject.transform.localPosition);
            //Logger.findTarget("findActionTarget {0}, {1}, distance: {2}/{3}", obj, hit.collider, minDistance, hit.distance);
            Vector3 objPosition = getUnitPosition(obj);
            RaycastHit2D[] hit = new RaycastHit2D[5];
            int intersect = Physics2D.LinecastNonAlloc(unitPosition, objPosition, hit);
            Logger.findTarget("findActionTarget LinecastNonAlloc {0}, {1} : {2}, {3}; {4}, {5}, {6}", transform.position.x, transform.position.y, obj.gameObject.transform.position.x, obj.gameObject.transform.position.y, unit.Myfaction, obj.offset, this.Myfaction);
            Logger.findTarget("findActionTarget LinecastNonAlloc {0}, {1} : {2}, {3}; {4}, {5}, {6}", unitPosition.x, unitPosition.y, objPosition.x, objPosition.y, unit.Myfaction, intersect, this.Myfaction);
            if (intersect != 2)//2 means this.collider and target.collider. No anyone else locates between the line casted.
            {
                Logger.findTarget("findActionTarget intersect {0}", intersect);
                continue;
            }

            float distance = Vector3.Distance(unitPosition, objPosition);
            Logger.findTarget("findActionTarget faction {0} {1}, distance {2}", unit.Myfaction, this.Myfaction, distance);
            if (minDistance > distance) {
                if (unit != null && unit.Myfaction != this.Myfaction)
                {
                    target = obj.gameObject;
                    minDistance = distance;
                    theClosestRay = hit[1];//[0] is the collider, [1] is the target collider
                }
            }
        }

        //Logger.findTarget("findActionTarget result {0}, {1}, minDistance {2}/{3}", target, mActionTarget, minDistance, theClosestRay.distance);
        //if (target == null && mPath == null) {
        //    findBaseTarget();
        //} else
        if(target != null && !target.Equals(mActionTarget)) {
            Logger.findTarget("startFind target path result");
            mActionTarget = target;
            //startFindPath(mActionTarget.transform.localPosition, 0, 1, true);
        }
    }

    protected bool takeActionOnTargetIfNeeded()
    {
        bool handle = false;
        if (mActionTarget == null)
            return handle;

        Vector3 unitPosition = getUnitPosition(mCollider);
        Vector3 targetPosition = getUnitPosition(mActionTarget.GetComponent<Collider2D>());
        RaycastHit2D[] hit = new RaycastHit2D[5];
        int intersect = Physics2D.LinecastNonAlloc(unitPosition, targetPosition, hit);
        if (intersect != 2)//2 means this.collider and target.collider. No anyone else locates between the line casted.
        {
            Logger.findTarget("takeActionOnTargetIfNeeded intersect {0}", intersect);
            mActionTarget = null;
            return handle;
        }

        Logger.findTarget("takeActionOnTargetIfNeeded target position this: ({0}, {1}), target: ({2}, {3})", unitPosition.x, unitPosition.y, targetPosition.x, targetPosition.y);
        Logger.findTarget("takeActionOnTargetIfNeeded target range {0}, distance {1} / {2}; {3}", actionRange, Vector3.Distance(targetPosition, unitPosition), hit[1].distance, this.Myfaction);
        if (actionRange >= hit[1].distance) {
            BaseUnit unit = mActionTarget.GetComponent<BaseUnit>();
            if (unit != null && unit.Myfaction != Myfaction)
            {
                Logger.findTarget("takeActionOnTargetIfNeeded target faction: {0}", unit.Myfaction);
                // ... and if the time exceeds the time of the last hit plus the time between hits...
                if (Time.time > lastHitTime + repeatDamagePeriod)
                {
                    // ... take damage and reset the lastHitTime.
                    attackEnemy(unit);
                    lastHitTime = Time.time;
                }
                handle = true;
            }
        }

        return handle;
    }

    int mNumberOfTargetToBeCalculated = 0;
    int mCurrentCalculatedNumberOfTargetPath = 0;
    GameObject[] mBaseTarget;
    GameObject mActionTarget;

    protected void findBaseTarget() {
        if (Myfaction == Faction.Player) { 
            mBaseTarget = GameObject.FindGameObjectsWithTag("BaseTarget");
        }
        else if (Myfaction == Faction.Enemy) {
            mBaseTarget = GameObject.FindGameObjectsWithTag("BaseTarget");
        }
        Logger.findTarget("initFindTarget {0}", mBaseTarget.Length);

        Vector3 unitPosition = getUnitPosition(mCollider);
        Collider2D[] objects = Physics2D.OverlapCircleAll(unitPosition, detectRange);

        foreach (GameObject obj in mBaseTarget) {
            RaycastHit2D[] hit = new RaycastHit2D[5];
            int intersect = Physics2D.LinecastNonAlloc(obj.transform.position, unitPosition, hit);
            if (intersect == 1 && hit[0].distance < detectRange)
            {
                Logger.findTarget("findBaseTarget intersect {0} near enough", intersect);
                return;
            }
            Logger.findTarget("findBaseTarget intersect not 1: {0}", intersect);
        }



        startFindPath(mBaseTarget[0].transform.localPosition, 0, mBaseTarget.Length, true);
    }

    protected void checkIfNeedToFindNextTarget() {
        ////Don't auto find base target
        //findBaseTarget();
    }

    protected void startMoveToTarget(float speedPerTime) {
        if (mActionTarget == null && mPath == null)
        {
            //Logger.findTarget("startMoveToTarget return {0}, {1}", mActionTarget, mPath);
            //We have no path to move after yet
            return;
        }

        if (mActionTarget == null && mPath != null && mPath.endOfPath())
        {
            //Logger.findTarget("End Of Path Reached");
            moveOnCommandedPath = false;
            checkIfNeedToFindNextTarget();
            return;
        }

        Vector3 unitPosition = getUnitPosition(mCollider);
        Vector3 targetPosition;

        float distanceToCollider = float.MaxValue;
        if (mActionTarget != null)
        {
            Logger.findTarget("startMoveToTarget mActionTarget");
            targetPosition = getUnitPosition(mActionTarget.GetComponent<Collider2D>());
            RaycastHit2D[] hit = new RaycastHit2D[5];
            int intersect = Physics2D.LinecastNonAlloc(unitPosition, targetPosition, hit);
            if (intersect != 2)//2 means this.collider and target.collider. Some one else locates between the line casted.
            {
                Logger.findTarget("startMoveToTarget intersect {0}", intersect);
                startFindPath(targetPosition, 0, 1, true);// find the path to the path
                mActionTarget = null;// lost target                
                return;
            }

            distanceToCollider = hit[1].distance - actionRange;
            Logger.findTarget("startMoveToTarget distanceToCollider {0}", distanceToCollider);
        } else {
            if (mPath == null)
                return;
            targetPosition = mPath.getCurrentNodeLocation();
            Logger.findTarget("startMoveToTarget mPath {0},{1}, {2}", targetPosition.x, targetPosition.y, mPath.currentWaypoint);
        }



        //Don't consider if there is a collider between this and mActionTarget. Because we'll set mActionTarget to be null if raycast dection fail in takeActionOnTargetIfNeeded();
        Vector3 dir = (targetPosition - unitPosition).normalized;
        dir = dir * Math.Min(speedPerTime, distanceToCollider);// speed * Time.deltaTime;
        //GetComponent<Rigidbody2D>().MovePosition(transform.localPosition + dir);
        Logger.findTarget("startMoveToTarget from ({0},{1}) toward ({2},{3}), add ({4},{5})={6}; {7}, {8}", unitPosition.x, unitPosition.y, targetPosition.x, targetPosition.y, dir.x, dir.y, dir.magnitude, this.Myfaction, Vector3.Distance(unitPosition, targetPosition));
        Logger.dumpStack("FindTarget");
        transform.position += dir;// same meaning as unitPosition += dir;
        Logger.findTarget("startMoveToTarget new position {0}, {1}", transform.position.x, transform.position.y);
        //Check if we are close enough to the next waypoint
        //If we are, proceed to follow the next waypoint
        if (mActionTarget == null)
            mPath.nextNodeIfNear(getUnitPosition(mCollider), nextWaypointDistance);
    }

    protected void startFindPath(Vector3 targetPosition)
    {
        startFindPath(targetPosition, mCurrentCalculatedNumberOfTargetPath, mNumberOfTargetToBeCalculated);
    }

    protected void startFindPath(Vector3 targetPosition, int currentCalculatedNumberOfTargetPath, int numberOfTargetToBeCalculated, bool reset = false)
    {
        this.mCurrentCalculatedNumberOfTargetPath = currentCalculatedNumberOfTargetPath;
        this.mNumberOfTargetToBeCalculated = numberOfTargetToBeCalculated;

        if (reset)
            mPath = null;

        if (mNumberOfTargetToBeCalculated <= 0)
            return;
        //Get a reference to the Seeker component we added earlier
        Seeker seeker = GetComponent<Seeker>();
        //Start a new path to the targetPosition, return the result to the OnPathComplete function
        seeker.StartPath(getUnitPosition(mCollider), targetPosition, OnPathComplete);
    }

    public void OnPathComplete(Path p)
    {
        mCurrentCalculatedNumberOfTargetPath++;
        Logger.findTarget("OnPathComplete {0}, {1}, {2}", mCurrentCalculatedNumberOfTargetPath, p.error, Thread.CurrentThread.ManagedThreadId);
        if (!p.error)
        {
            if (mPath == null)
            {
                mPath = new UnitPath(p, 0);
            }
            else {
                float totalLength = p.GetTotalLength();
                Logger.findTarget("calcu Path {0} vs {1}", totalLength, mPath.GetTotalLength());
                if (totalLength < mPath.GetTotalLength()) {
                    mPath = new UnitPath(p, 0, totalLength);                    
                }
            }
        }

        if (mCurrentCalculatedNumberOfTargetPath < mNumberOfTargetToBeCalculated) {
            startFindPath(mBaseTarget[mCurrentCalculatedNumberOfTargetPath].transform.localPosition);
        }
    }

    protected UnitPath mPath;

    protected class UnitPath
    {
        Path p;
        public int currentWaypoint;
        float totalLength = -1;
        int mCurrentCalculatedNumberOfTargetPath;
        int mNumberOfTargetToBeCalculated;

        public UnitPath(Path p, int currentWaypoint) {
            this.p = p;
            this.currentWaypoint = currentWaypoint;            
        }

        public UnitPath(Path p, int currentWaypoint, float totalLength)
        {
            this.p = p;
            this.currentWaypoint = currentWaypoint;
            this.totalLength = totalLength;
        }


        public float GetTotalLength() {
            if (totalLength >= 0)
                return totalLength;
            return p.GetTotalLength();
        }

        public bool endOfPath() {
            return currentWaypoint >= p.vectorPath.Count;
        }

        public void nextNodeIfNear(Vector3 currentPaosition, float tolerenceRange) {
            Logger.findTarget("nextNodeIfNear new position {0}, {1} ; {2}, {3} ; {4}; {5}", currentPaosition.x, currentPaosition.y, p.vectorPath[currentWaypoint].x, p.vectorPath[currentWaypoint].y, tolerenceRange, Vector3.Distance(currentPaosition, p.vectorPath[currentWaypoint]));
            if (Vector3.Distance(currentPaosition, p.vectorPath[currentWaypoint]) < tolerenceRange)
            {
                currentWaypoint++;
            }
        }

        public Vector3 getCurrentNodeLocation() {
            return p.vectorPath[currentWaypoint];
        }
    }

    private UnitControlPanel linkedControlPanel;

    public void setLinkControlPanel(UnitControlPanel panel)
    {
        linkedControlPanel = panel;
    }

    public UnitControlPanel getLinkControlPanel()
    {
        return linkedControlPanel;
    }
}
