using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;

public class GameController : MonoBehaviour {
    IFirebase FireBase_battlelist;
    IFirebase FireBase_battle;
    IFirebase FireBase_battle_event;
    //public Humanoid[] humanoid;
    public Humanoid player;
    public Humanoid enemy;
    protected Dictionary<string, Humanoid> mHumanoids = new Dictionary<string, Humanoid>();

    public float timeLeft;
    public GUIText timerText;
    public GUIText scoreText;
    public GameObject winText;
    public GameObject loseText;
    public GameObject restartButton;
    public GameObject startButton;
    public GameObject controlUIPanelHost;
    UnitControlPanel[] unitControlPanels;
    public GameObject startingCamp_player;

    private string TIME;
    private string SCORE;
    private string SCORE_FORMAT = "{0} / {1}";

    public int GameOverScore;

    private float playerScore = 0;
    public float PlayerScore {
        get {
            return playerScore;
        }
        set
        {
            if (!_gameProcessing)
                return;

            playerScore = value;
            if (playerScore >= GameOverScore)
                gameOver();
        }
    }

    private float enemyScore = 0;
    public float EnemyScore
    {
        get
        {
            return enemyScore;
        }
        set
        {
            if (!_gameProcessing)
                return;
            enemyScore = value;
            if (enemyScore >= GameOverScore)
                gameOver();
        }
    }


    public static void ClearLog()
    {
        //var assembly = Assembly.GetAssembly(typeof(UnityEditor.ActiveEditorTracker));
        //var type = assembly.GetType("UnityEditorInternal.LogEntries");
        //var method = type.GetMethod("Clear");
        //method.Invoke(new object(), null);
    }

    private float maxWidth;
    private bool _gameProcessing = false;

    public void decideScreenSize() {
        Vector3 upperCorner = new Vector3(Screen.width, Screen.height, 1f);
        Vector3 targetWidth = Camera.main.ScreenToWorldPoint(upperCorner);
        maxWidth = targetWidth.x;
	}

    void Start() {
        TIME = timerText.text + "\n";
        SCORE = scoreText.text + "\n";
    }

    void FixedUpdate()
    {
        if (gotoStartGame) {
            gameStart();
            gotoStartGame = false;
            return;
        }

        if (_gameProcessing)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft < 0)
            {
                timeLeft = 0;                
                gameOver();
            }
            timerText.text = TIME + Mathf.RoundToInt(timeLeft);
        }

        scoreText.text = SCORE + string.Format(SCORE_FORMAT, new string[]{ ((int)playerScore).ToString(), ((int)enemyScore).ToString()});
    }

    public bool isGameProcessing ()
    {
        return !_gameProcessing;
    }

    public void gameOver() {
        _gameProcessing = false;
        if (playerScore > enemyScore) {
            winText.SetActive(true);
            loseText.SetActive(false);
        } else if (playerScore < enemyScore) {
            winText.SetActive(false);
            loseText.SetActive(true);
        } else {
            //draw game
        }
    }

    private const string FIRE_BASE_HOST = "https://boiling-torch-7529.firebaseio.com/";
    private static string BATTLE_LIST = "battlelist";
    private EventHandler<ChangedEventArgs> battlelistvalueUpdateEventHandler;
    private EventHandler<ChangedEventArgs> battlePlayerChildAddedEventHandler;
    private static string BATTLE_ID = "battle/test0410/";

    private static string BATTLE_STATUS = "battle_status";
    private static string BATTLE_STATUS_WAITING = "waiting";
    private static string BATTLE_STATUS_FIGHTING = "fighting";
    public string playerId = "11111";// UnityEngine.Random.Range(0, int.MaxValue);
    public string enemyId = "22222";
    private bool gotoStartGame = false;
    public void gamePrepare() {
        ClearLog();
        getPlayer_Battle_EnemyId();
    }

    private void getBattleId() { }
    private void getEnemyId() {
        //have BATTLE ID first
        FireBase_battle = Firebase.CreateNew(FIRE_BASE_HOST + "battle/" + BATTLE_ID);
        FireBase_battle.Push().SetValue(playerId);
        battlePlayerChildAddedEventHandler = (object sender, ChangedEventArgs e) =>
        {
            IDataSnapshot dataSnap = e.DataSnapshot;
            Logger.gameControl("FireBase_battle ChildAdded {0}, {1}", dataSnap.Key, dataSnap.StringValue);
            if (!playerId.Equals(dataSnap.StringValue))
            {
                enemyId = dataSnap.StringValue;
                gotoStartGame = true;
                //Loom.QueueOnMainThread(() =>
                //{
                //    gameStart();
                //});
                FireBase_battle.ChildAdded -= battlePlayerChildAddedEventHandler;
            }

        };
        FireBase_battle.ChildAdded += battlePlayerChildAddedEventHandler;
    }

    private void getPlayer_Battle_EnemyId() {
        FireBase_battlelist = Firebase.CreateNew(FIRE_BASE_HOST + BATTLE_LIST);
        playerId = System.Guid.NewGuid().ToString();
        //IFirebase FireBase_battle = FireBase_battlelist.Push();
        //battleId = FireBase_battle.Key;
        
        Logger.gameControl("getPlayer_Battle_EnemyId playerId {0}, battleId {1}", playerId, BATTLE_ID);
        FireBase_battlelist.ChildAdded += (object sender, ChangedEventArgs e) =>
        {
            IDataSnapshot dataSnap = e.DataSnapshot;
            Logger.gameControl("FireBase_battlelist ChildAdded {0}, {1}", dataSnap.Key, dataSnap.StringValue);
        };

        battlelistvalueUpdateEventHandler = (object sender, ChangedEventArgs e) =>
            {
            IDataSnapshot dataSnap = e.DataSnapshot;
            Logger.gameControl("FireBase_battlelist ValueUpdated {0}, {1}", dataSnap.Key, dataSnap.StringValue);
                try
                {
                    Dictionary<string, object> dic = dataSnap.DictionaryValue;
                    object temp = null;
                    bool findBattleId = false;
                    foreach (string battleId in dic.Keys)
                    {
                        if (battleId.Contains("skip"))
                            continue;
                        //dic.TryGetValue(battleId, out temp);
                        string status = dataSnap.Child(battleId).Child(BATTLE_STATUS).StringValue;
                        Logger.gameControl("FireBase_battlelist ValueUpdated {0}, {1}", battleId, status);
                        if (BATTLE_STATUS_WAITING.Equals(status))
                        {
                            dataSnap.Child(battleId).Ref.Child(playerId).SetValue(playerId);
                            dataSnap.Child(battleId).Child(BATTLE_STATUS).Ref.SetValue(BATTLE_STATUS_FIGHTING);
                            BATTLE_ID = battleId;
                            findBattleId = true;
                            break;
                        }
                        //Dictionary<string, object> battlevalue = (Dictionary<string, object>)temp;
                        //object temp2 = null;
                        //foreach (string battleinfo in battlevalue.Keys)
                        //{
                        //    if ("status".Equals(battleinfo))
                        //    {
                        //        battlevalue.TryGetValue(battleId, out temp2);
                        //    }
                        //}
                    }

                    if (!findBattleId)
                    {
                        IFirebase ref_battle = FireBase_battlelist.Push();
                        BATTLE_ID = ref_battle.Key;
                        ref_battle.Child(playerId).SetValue(playerId);
                        ref_battle.Child(BATTLE_STATUS).SetValue(BATTLE_STATUS_WAITING);
                    }

                    FireBase_battlelist.ValueUpdated -= battlelistvalueUpdateEventHandler;
                    getEnemyId();
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.Message);
                }
            };

        FireBase_battlelist.ValueUpdated += battlelistvalueUpdateEventHandler;

        FireBase_battlelist.ChildChanged += (object sender, ChangedEventArgs e) =>
        {
            IDataSnapshot dataSnap = e.DataSnapshot;
            Logger.gameControl("FireBase_battlelist ChildChanged {0}, {1}", dataSnap.Key, dataSnap.StringValue);
        };
    }

    public UnitControlPanel getIdMatchedControlPanel(string id) {
        if (id == null)
            return null;

        foreach (UnitControlPanel panel in unitControlPanels)
        {
            if (id.Equals(panel.getId())) {
                return panel;
            }
        }

        return null;
    }

    // Use this for initialization
    private void gameStart () {
        Debug.Log("GameController start+");
        _gameProcessing = true;
        controlUIPanelHost.SetActive(true);
        unitControlPanels = controlUIPanelHost.GetComponentsInChildren<UnitControlPanel>();

        createHumanoid();


        FireBase_battle_event = FireBase_battle.Child(RemoteEventBase.KEY_EVENT_ROOT);
        ////firebase.AuthWithPassword("kk60258@gmail.com", "iloveawei", (AuthData auth) =>
        ////{
        ////    Debug.Log("auth success!!" + auth.Uid);
        ////}, (FirebaseError e) =>
        ////{
        ////    Debug.Log("auth failure!!");
        ////});

        FireBase_battle_event.ChildAdded += (object sender, ChangedEventArgs e) =>
        {
           IDataSnapshot dataSnap = e.DataSnapshot;
            
            
            Logger.firebase("Child added! " + sender + ", " + e + ", " + dataSnap.Key);

            try
            {
                List<RemoteEventBase> result = FireBaseUtil.getBattleEventValue(dataSnap);
                onRemoteEvents(result);
            } catch (Exception ex) {
                Debug.Log(ex.Message);
            }

        //    ////String x = dataSnap.Child("x").StringValue;
        //    ////String y = dataSnap.Child("y").StringValue;
        //    ////String z = dataSnap.Child("z").StringValue;

            //    //Dictionary<string, object> value = dataSnap.DictionaryValue;
            //    //string x = value["x"].ToString();
            //    //string y = value["y"].ToString();
            //    //string z = value["z"].ToString();

            //    //if (debugFirebase)
            //    //    Debug.Log("Child added! " + dataSnap.Key + ", s " + dataSnap.DictionaryValue + ", x " + x + ", y " + y + ", z " + z);

            //    //Vector3 p = new Vector3(float.Parse(x), float.Parse(y), float.Parse(z));
            //    //foreach (Humanoid h in humanoid)
            //    //{
            //    //    h.onRemoteUpdate(Int32.Parse(dataSnap.Key), p);
            //    //};
        };

        //firebase.ChildRemoved += (object sender, ChangedEventArgs e) =>
        //{
        //    IDataSnapshot dataSnap = e.DataSnapshot;
        //    if (debugFirebase)
        //        Debug.Log("Child removed! " + sender + ", " + e + ", " + dataSnap);
        //};

        FireBase_battle_event.ValueUpdated += (object sender, ChangedEventArgs ex) => {
           IDataSnapshot dataSnap = ex.DataSnapshot;
           Logger.firebase("Child ValueUpdated! " + sender + ", " + ex + ", " + dataSnap.Key);

        //    //String x = dataSnap.Child("x").StringValue;
        //    //String y = dataSnap.Child("y").StringValue;
        //    //String z = dataSnap.Child("z").StringValue;

        //    //Debug.Log("Child ValueUpdated! " + dataSnap.Key + ", " + dataSnap.StringValue + ", " + x + ", " + y + ", " + z);

        //    //Vector3 p = new Vector3(x, y, z);
        //    //foreach (Humanoid h in humanoid)
        //    //{
        //    //    h.onRemoteUpdate(Int32.Parse(dataSnap.Key), p);
        //    //};

        //    //try
        //    //{
        //    //    Dictionary<string, object> value = dataSnap.DictionaryValue;
        //    //    string x = value["x"].ToString();
        //    //    string y = value["y"].ToString();
        //    //    string z = value["z"].ToString();

        //    //    Debug.Log("Child ValueUpdated! " + dataSnap.Key + ", s " + dataSnap.DictionaryValue + ", x " + x + ", y " + y + ", z " + z);

        //    //    Vector3 p = new Vector3(float.Parse(x), float.Parse(y), float.Parse(z));
        //    //    foreach (Humanoid h in humanoid)
        //    //    {
        //    //        h.onRemoteUpdate(Int32.Parse(dataSnap.Key), p);
        //    //    };
        //    //}
        //    //catch (Exception exx) {
        //    //    Debug.Log(exx.ToString());
        //    //}
        };

        FireBase_battle_event.ChildChanged += (object sender, ChangedEventArgs e) => {
            //firebase.Child(enemyId.ToString()).ChildChanged += (object sender, ChangedEventArgs e) => {
            IDataSnapshot dataSnap = e.DataSnapshot;
            
            Logger.firebase("Child ChildChanged! " + sender + ", " + e + ", " + dataSnap.Key);
            
            //String x = dataSnap.Child("x").StringValue;
            //String y = dataSnap.Child("y").StringValue;
            //String z = dataSnap.Child("z").StringValue;

            //Debug.Log("Child ChildChanged! " + dataSnap.Key + ", " + x + ", " + y + ", " + z);

            //Vector3 p = new Vector3(float.Parse(x), float.Parse(y), float.Parse(z));
            //foreach (Humanoid h in humanoid)
            //{
            //    h.onRemoteUpdate(Int32.Parse(dataSnap.Key), p);
            //};
/*
            try
            {
                List<JsonBaseUnit> result = JsonBaseUnit.getFirebaseValue(dataSnap);
                foreach (Humanoid h in humanoid)
                {
                    foreach (JsonBaseUnit json in result)
                    {
                        h.onRemoteUpdate(json);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
            }*/
        };

        //firebase.ChildChanged += (object sender, ChangedEventArgs e) => {
        //firebase.Child(playerId.ToString()).Child("attack").ChildChanged += (object sender, ChangedEventArgs e) => {
            //IDataSnapshot dataSnap = e.DataSnapshot;
            //if (debugFirebase)
            //    Debug.Log("attack ChildChanged! " + sender + ", " + e + ", " + dataSnap.Key);

 /*           try
            {
                List<JsonBaseAttack> result = JsonBaseAttack.getFirebaseValue(dataSnap);
                foreach (Humanoid h in humanoid)
                {
                    foreach (JsonBaseAttack json in result)
                    {
                        //h.onRemoteUpdate(json.getId(), json.getVector3());
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
            }*/
        //};

        FireBase_battle.SetValue("GameStart!");
        
        //Dictionary<string, object> value = new Dictionary<string, object>();
        //Dictionary<string, object> location = new Dictionary<string, object>();

        //location.Add("x", "9");
        //location.Add("y", "9");
        //location.Add("z", "999");
        //value.Add("action", "move");
        //value.Add("action_unit_id", "123");
        //value.Add("current_location", location);

        //firebase.Child("FightLog").Push().SetValue(value);

        //value.Add("target_location", location);
        //firebase.Child("FightLog").Push().SetValue(value);

        Debug.Log("GameController start-");
    }

    public void onRemoteEvents(List<RemoteEventBase> result)
    {
        foreach (RemoteEventBase theEvent in result)
        {
            if (RemoteEventBase.VALUE_ACTION_NAME_MOVE.Equals(theEvent.getActionName())) {
                Humanoid h;
                mHumanoids.TryGetValue(theEvent.getPlayerId(), out h);
                h.onRemoteUpdateMove(theEvent);
            }
            else if (RemoteEventBase.VALUE_ACTION_NAME_CREATE.Equals(theEvent.getActionName()))
            {
                Humanoid h;
                mHumanoids.TryGetValue(theEvent.getPlayerId(), out h);
                h.onRemoteCreate(theEvent);
            }
        }
    }

    // Update is called once per frame
    void Update () {

    }

    void createHumanoid() {
        //Humanoid[] temp = new Humanoid[humanoid.Length];
        //for (int i = 0; i < humanoid.Length; ++i) {
        //    Humanoid obj = Instantiate(humanoid[i], Vector3.zero, Quaternion.identity) as Humanoid;
        //    obj.setGameController(this);
        //    obj.transform.parent = transform;
        //    temp[i] = obj;
        //    Debug.Log("createHumanoid " + " " + i + " " + obj);
        //}
        //humanoid = temp;
        Humanoid obj = Instantiate(player, Vector3.zero, Quaternion.identity) as Humanoid;
        obj.setGameController(this);
        obj.transform.parent = transform;
        obj.Id = playerId;
        obj.setStartingCamp(startingCamp_player);
        mHumanoids.Add(playerId, obj);

        obj = Instantiate(enemy, Vector3.zero, Quaternion.identity) as Humanoid;
        obj.setGameController(this);
        obj.transform.parent = transform;
        obj.Id = enemyId;
        mHumanoids.Add(enemyId, obj);
        
    }

    public void eventMoveUnit(string actorId, string act_target, Vector3 currentLocation, Vector3 targetLocation) {
        RemoteEventBase eventBase = new RemoteEventBase(null, RemoteEventBase.VALUE_ACTION_NAME_MOVE, actorId, act_target, playerId, Time.realtimeSinceStartup.ToString(), currentLocation, targetLocation);
        FireBaseUtil.setBattleEventValue(FireBase_battle, eventBase);
    }

    public void eventCreateUnit(string actorId, string act_target, Vector3 currentLocation, Vector3 targetLocation)
    {
        RemoteEventBase eventBase = new RemoteEventBase(null, RemoteEventBase.VALUE_ACTION_NAME_CREATE, actorId, act_target, playerId, Time.realtimeSinceStartup.ToString(), currentLocation, targetLocation);
        FireBaseUtil.setBattleEventValue(FireBase_battle, eventBase);
    }

    public void eventCreateEnemyUnit(string actorId, string act_target, Vector3 currentLocation, Vector3 targetLocation)
    {
        RemoteEventBase eventBase = new RemoteEventBase(null, RemoteEventBase.VALUE_ACTION_NAME_CREATE, actorId, act_target, enemyId, Time.realtimeSinceStartup.ToString(), currentLocation, targetLocation);
        FireBaseUtil.setBattleEventValue(FireBase_battle, eventBase);
    }

    //public void updateUnit(GameObject obj) {
    //    int id = obj.GetInstanceID();
    //    IFirebase refFirebase = firebase.Child(playerId.ToString()).Child(id.ToString());
    //    Vector3 newP = obj.transform.localPosition;//
    //    JsonBaseUnit json = new JsonBaseUnit(playerId, id, newP.x, newP.y, newP.z);
    //    JsonBaseUnit.setFirebaseValue(refFirebase, json);

    //    if (debugFirebase)
    //        Debug.Log("GameController + updateUnit " + obj.GetInstanceID().ToString() + " x " + newP.x + ", y "
    //         + newP.y + ", z " + newP.z);

    //    if (debugAutoSetEnemyFirebase) {
    //        int enemyUnitId = id * 1000;
    //        IFirebase enemyFirebase = firebase.Child(enemyId.ToString()).Child(enemyUnitId.ToString());
    //        Vector3 enemyNewP = mirrorLocation(newP);
    //        JsonBaseUnit enemyjson = new JsonBaseUnit(enemyId, enemyUnitId, enemyNewP.x, enemyNewP.y, enemyNewP.z);
    //        JsonBaseUnit.setFirebaseValue(enemyFirebase, enemyjson);

    //        if (debugFirebase)
    //            Debug.Log("GameController + debugAutoSetEnemyFirebase updateUnit " + enemyUnitId + " x " + enemyNewP.x + ", y "
    //             + enemyNewP.y + ", z " + enemyNewP.z);
    //    }
    //}

    //    public void attackEnemy(int beAttackedPlayerId, int attackerId, int beAttackedId, int damage, Vector3 hurtVector, int hurtForce)
    //{
    //    JsonBaseAttack json = new JsonBaseAttack(beAttackedPlayerId , attackerId, beAttackedId, damage, hurtVector, hurtForce);
    //}

    //public void newUnit(GameObject obj) {
    //    IFirebase refFirebase = firebase.Child(obj.GetInstanceID().ToString() + "00");
    //    Dictionary<string, object> value = new Dictionary<string, object>();
    //    Vector3 newP = mirrorLocation(obj.transform.localPosition);
    //    value.Add("x", newP.x);
    //    value.Add("y", newP.y);
    //    value.Add("z", newP.z);
    //    refFirebase.SetValue(value);

    //    Debug.Log("GameController + newUnit " + obj.GetInstanceID().ToString() + "00 x " + newP.x + ", y "
    //         + newP.y + ", z " + newP.z);
    //}

    public static Vector3 mirrorLocation(Vector3 p) {
        Vector3 newP = new Vector3(p.x, - p.y, p.z);
        Logger.firebase("GameController + mirrorLocation " + p.x + ":" + newP.x + "," + p.y + ":" + newP.y + "," + p.z + ":" + newP.z);
        return newP;
    }


    //public class JsonBaseUnit
    //{
    //    private int playerIdOfUnit;
    //    private int id;
    //    private float x, y, z;
    //    private Vector3 vector3;
    //    public JsonBaseUnit() { }
    //    public JsonBaseUnit(int playerIdOfUnit, int id, Vector3 p)
    //    {
    //        this.playerIdOfUnit = playerIdOfUnit;
    //        this.id = id;
    //        this.x = p.x;
    //        this.y = p.y;
    //        this.z = p.z;
    //        this.vector3 = p;
    //    }
    //    public JsonBaseUnit(int playerIdOfUnit, int id, float x, float y, float z)
    //    {
    //        this.playerIdOfUnit = playerIdOfUnit;
    //        this.id = id;
    //        this.x = x;
    //        this.y = y;
    //        this.z = z;
    //        this.vector3 = new Vector3(x, y, z);
    //    }

    //    public int getPlayerIdOfUnit()
    //    {
    //        return playerIdOfUnit;
    //    }

    //    public int getId()
    //    {
    //        return id;
    //    }
    //    public float getX()
    //    {
    //        return x;
    //    }
    //    public float getY()
    //    {
    //        return y;
    //    }
    //    public float getZ()
    //    {
    //        return z;
    //    }
    //    public Vector3 getVector3() {
    //        return vector3;
    //    }

    //    public static void setFirebaseValue(IFirebase refFirebase, JsonBaseUnit json) {
    //        Dictionary<string, object> value = new Dictionary<string, object>();
    //        value.Add("x", json.x);
    //        value.Add("y", json.y);
    //        value.Add("z", json.z);
    //        refFirebase.SetValue(value);
    //    }

    //    public static List<JsonBaseUnit> getFirebaseValue(IDataSnapshot dataSnap)
    //    {
    //        List<JsonBaseUnit> result = new List<JsonBaseUnit>();
    //        if (enemyId.ToString().Equals(dataSnap.Key))
    //        {


    //            //var obj = MiniJSON.Json.Deserialize(dataSnap.StringValue);
    //            if (debugFirebase) {
    //                Debug.Log("getFirebaseValue enemyId id " + dataSnap.Key);
    //                Debug.Log("getFirebaseValue String " + dataSnap.StringValue);
    //            }

    //            Dictionary<string, object> dic = dataSnap.DictionaryValue;
    //            foreach (string s in dic.Keys)
    //            {
    //                object obj2 = null;
    //                Debug.Log("getFirebaseValue dic.Keys " + s);
    //                dic.TryGetValue(s, out obj2);

    //                Dictionary<string, object> value = (Dictionary<string, object>)obj2;
    //                Debug.Log("getFirebaseValue dic.obj " + obj2);
    //                JsonBaseUnit jsonBaseUnit = JsonBaseUnit.parseOne(enemyId, int.Parse(s), value);
    //                result.Add(jsonBaseUnit);
    //            }
    //        }
    //        else {
    //            if (debugFirebase)
    //                Debug.Log("getFirebaseValue dataSnap.Key " + dataSnap.Key);

    //            int id = int.Parse(dataSnap.Key);
    //            Dictionary<string, object> value = dataSnap.DictionaryValue;
    //            if (debugFirebase)
    //                Debug.Log("getFirebaseValue 2 " + dataSnap.Key + ", " + dataSnap.DictionaryValue);

    //            JsonBaseUnit jsonBaseUnit = JsonBaseUnit.parseOne(enemyId, id, value);
    //            result.Add(jsonBaseUnit);
    //        }

    //        if (debugFirebase)
    //            Debug.Log("Child ChildChanged! " + dataSnap.Key + ", s " + dataSnap.DictionaryValue + ", count " + result.Count);

    //        return result;
    //    }

    //    private static JsonBaseUnit parseOne(int enemyId, int id, Dictionary<string, object> value) {
    //        string x = value["x"].ToString();
    //        string y = value["y"].ToString();
    //        string z = value["z"].ToString();
    //        if (debugFirebase)
    //            Debug.Log("JsonBaseUnit parseOne " + id + ", s " + ", x " + x + ", y " + y + ", z " + z);

    //        Vector3 p = new Vector3(float.Parse(x), float.Parse(y), float.Parse(z));
    //        return new JsonBaseUnit(enemyId, id, p);
    //    }
    //}

    //public class JsonBaseAttack
    //{
    //    public int beAttackedPlayerId;
    //    public int attackerId;
    //    public int beAttackedId;
    //    public int damage;
    //    public Vector3 hurtVector;
    //    public int hurtForce;

    //    public JsonBaseAttack(int beAttackedPlayerId, int attackerId, int beAttackedId, int damage, Vector3 hurtVector, int hurtForce)
    //    {
    //        this.beAttackedPlayerId = beAttackedPlayerId;
    //        this.attackerId = attackerId;
    //        this.beAttackedId = beAttackedId;
    //        this.damage = damage;
    //        this.hurtVector = hurtVector;
    //        this.hurtForce = hurtForce;
    //    }

    //    public static void setFirebaseValue(IFirebase refFirebase, JsonBaseAttack json)
    //    {
    //        IFirebase attackFirebase = refFirebase.Child(json.beAttackedPlayerId.ToString()).Child("attack").Push();
    //        Dictionary<string, object> value = new Dictionary<string, object>();

    //        value.Add("beAttackedPlayerId", json.beAttackedPlayerId);
    //        value.Add("attackerId", json.attackerId);
    //        value.Add("beAttackedId", json.beAttackedId);
    //        value.Add("damage", json.damage);
    //        value.Add("x", json.hurtVector.x);
    //        value.Add("y", json.hurtVector.y);
    //        value.Add("z", json.hurtVector.z);
    //        value.Add("hurtForce", json.hurtForce);

    //        attackFirebase.SetValue(value);
    //        if (debugFirebase)
    //            Debug.Log("JsonBaseAttack setFirebaseValue " + attackFirebase.Key + ",  attackerId " + json.attackerId + ", beAttackedId " + json.beAttackedId);
    //    }

    //    public static List<JsonBaseAttack> getFirebaseValue(IDataSnapshot dataSnap)
    //    {
    //        List<JsonBaseAttack> result = new List<JsonBaseAttack>();
    //        if (debugFirebase)
    //        {
    //            Debug.Log("JsonBaseAttack getFirebaseValue dataSnap.Key " + dataSnap.Key);
    //            Debug.Log("JsonBaseAttack getFirebaseValue dataSnap.StringValue " + dataSnap.StringValue);
    //        }


    //        Dictionary<string, object> dic = dataSnap.DictionaryValue;
    //        foreach (string s in dic.Keys)
    //        {
    //            object obj2 = null;
    //            Debug.Log("JsonBaseAttack getFirebaseValue dic.Keys " + s);
    //            dic.TryGetValue(s, out obj2);

    //            Dictionary<string, object> value = (Dictionary<string, object>)obj2;
    //            Debug.Log("JsonBaseAttack getFirebaseValue dic.obj " + obj2);
    //            JsonBaseAttack jsonBaseAttack = JsonBaseAttack.parseOne(int.Parse(s), value);
    //            result.Add(jsonBaseAttack);
    //        }

    //        return result;
    //    }

    //    private static JsonBaseAttack parseOne(int id, Dictionary<string, object> value)
    //    {
    //        int beAttackedPlayerId = int.Parse(value["beAttackedPlayerId"].ToString());
    //        int attackerId = int.Parse(value["attackerId"].ToString());
    //        int beAttackedId = int.Parse(value["beAttackedId"].ToString());
    //        int damage = int.Parse(value["damage"].ToString());
    //        float x = float.Parse(value["x"].ToString());
    //        float y = float.Parse(value["y"].ToString());
    //        float z = float.Parse(value["z"].ToString());
    //        int hurtForce = int.Parse(value["hurtForce"].ToString());

    //        if (debugFirebase)
    //            Debug.Log("JsonBaseAttack parseOne " + beAttackedPlayerId + ", " + attackerId + ", " + beAttackedId + ", " + damage);

    //        Vector3 p = new Vector3(x, y, z);

    //        return new JsonBaseAttack(beAttackedPlayerId, attackerId, beAttackedId, damage, p, hurtForce);
    //    }
    //}
}
