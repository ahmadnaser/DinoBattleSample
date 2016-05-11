using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class FireBaseUtil {

    private static List<string> mHandledEventList = new List<string>();

    public FireBaseUtil() { }
    public static void setBattleEventValue(IFirebase refFirebase, RemoteEventBase theEvent)
    {        
        Dictionary<string, object> currentlocatoin = new Dictionary<string, object>();
        currentlocatoin.Add(RemoteEventBase.KEY_VECTOR_X, theEvent.getCurrentLocation().x);
        currentlocatoin.Add(RemoteEventBase.KEY_VECTOR_Y, theEvent.getCurrentLocation().y);
        currentlocatoin.Add(RemoteEventBase.KEY_VECTOR_Z, theEvent.getCurrentLocation().z);

        Dictionary<string, object> targetlocation = new Dictionary<string, object>();
        targetlocation.Add(RemoteEventBase.KEY_VECTOR_X, theEvent.getTargetLocation().x);
        targetlocation.Add(RemoteEventBase.KEY_VECTOR_Y, theEvent.getTargetLocation().y);
        targetlocation.Add(RemoteEventBase.KEY_VECTOR_Z, theEvent.getTargetLocation().z);

        Dictionary<string, object> value = new Dictionary<string, object>();
        value.Add(RemoteEventBase.KEY_ACTION_NAME, theEvent.getActionName());
        value.Add(RemoteEventBase.KEY_ACTOR, theEvent.getActorId());
        value.Add(RemoteEventBase.KEY_ACT_TARGET, theEvent.getActTarget());
        value.Add(RemoteEventBase.KEY_PLAYER_ID, theEvent.getPlayerId());

        value.Add(RemoteEventBase.KEY_LOC_CURRENT, currentlocatoin);
        value.Add(RemoteEventBase.KEY_LOC_TARGET, targetlocation);
        value.Add(RemoteEventBase.KEY_TIME_STAMP, theEvent.getTimeStamp());

        refFirebase.Child(RemoteEventBase.KEY_EVENT_ROOT).Push().SetValue(value);
    }

    /***
    The Firebase structure should be eventRoot--eventId
                                              --eventId
                                              --eventId
                                              --eventId
    And dataSnap.key is the eventId.
    */
    public static List<RemoteEventBase> getBattleEventValue(IDataSnapshot dataSnap)
    {
        List<RemoteEventBase> result = new List<RemoteEventBase>();

            Logger.firebase("getFirebaseValue event root " + dataSnap.Key);
            Logger.firebase("getFirebaseValue String " + dataSnap.StringValue);

        Dictionary<string, object> dic = dataSnap.DictionaryValue;
        string eventId = dataSnap.Key;
        //foreach (string eventId in dic.Keys)
        //{
            if (mHandledEventList.Contains(eventId))
            {
                Logger.firebase("getFirebaseValue ignore " + eventId);
                return null;
            }
            //object eventValue = null;
            //dic.TryGetValue(eventId, out eventValue);
            object eventValue = dataSnap.DictionaryValue;

            Logger.firebase("getFirebaseValue handle event id " + eventId);
            Logger.firebase("getFirebaseValue handle event content " + eventValue);

            Dictionary<string, object> theEvents = (Dictionary<string, object>) eventValue;

            string actionName = null, actor = null, act_target = null, playerId = null, timeStamp = null;
            Vector3 currentLocation = Vector3.one, targetLocation = Vector3.one;
            float x = 0, y = 0, z = 0;
            Dictionary<string, object> temploc = null;

            object temp = null;
            object tempVector = null;

            foreach (string key in theEvents.Keys)
            {
                temp = null;
                theEvents.TryGetValue(key, out temp);

                Logger.firebase("getFirebaseValue handle event content key " + key);
                Logger.firebase("getFirebaseValue handle event content value " + temp);

                switch (key)
                {
                    case RemoteEventBase.KEY_ACTION_NAME:
                        actionName = (string)temp;
                        break;
                    case RemoteEventBase.KEY_ACTOR:
                        actor = (string)temp;
                        break;
                    case RemoteEventBase.KEY_ACT_TARGET:
                        act_target = (string)temp;
                        break;
                    case RemoteEventBase.KEY_PLAYER_ID:
                        playerId = (string)temp;
                        break;
                    case RemoteEventBase.KEY_TIME_STAMP:
                        timeStamp = (string)temp;
                        break;
                    case RemoteEventBase.KEY_LOC_CURRENT:
                        temploc = (Dictionary<string, object>)temp;
                        foreach (string vectorkey in temploc.Keys)
                        {
                            temploc.TryGetValue(vectorkey, out tempVector);
                            {
                                Debug.Log("getFirebaseValue handle locCurrent event key " + vectorkey);
                                Debug.Log("getFirebaseValue handle locCurrent event value " + tempVector.ToString());
                                Debug.Log("getFirebaseValue handle locCurrent event value " + float.Parse(tempVector.ToString()));
                                Debug.Log("getFirebaseValue handle locCurrent event value " + tempVector);
                            }
                            switch (vectorkey)
                            {
                                case RemoteEventBase.KEY_VECTOR_X:
                                    x = float.Parse(tempVector.ToString());
                                    break;
                                case RemoteEventBase.KEY_VECTOR_Y:
                                    y = float.Parse(tempVector.ToString());
                                    break;
                                case RemoteEventBase.KEY_VECTOR_Z:
                                    z = float.Parse(tempVector.ToString());
                                    break;
                            }
                        }
                        currentLocation = new Vector3(x, y, z);
                        break;
                    case RemoteEventBase.KEY_LOC_TARGET:
                        temploc = (Dictionary<string, object>)temp;
                        foreach (string vectorkey in temploc.Keys)
                        {
                            temploc.TryGetValue(vectorkey, out tempVector);
                            switch (vectorkey)
                            {
                                case RemoteEventBase.KEY_VECTOR_X:
                                    x = float.Parse(tempVector.ToString());
                                    break;
                                case RemoteEventBase.KEY_VECTOR_Y:
                                    y = float.Parse(tempVector.ToString());
                                    break;
                                case RemoteEventBase.KEY_VECTOR_Z:
                                    z = float.Parse(tempVector.ToString());
                                    break;
                            }
                        }
                        targetLocation = new Vector3(x, y, z);
                        break;
                    default:
                        break;
                }                
            }
            RemoteEventBase singleEvent = new RemoteEventBase(eventId, actionName, actor, act_target, playerId, timeStamp, currentLocation, targetLocation);
            mHandledEventList.Add(eventId);
            result.Add(singleEvent);
            Logger.firebase("getFirebaseValue add event " + eventId);
        //}
        
            Logger.firebase("getFirebaseValue count " + result.Count);

        return result;
    }

    //public void AddOrUpdateParty(Dictionary<string, object> d)
    //{
    //    string dSer = JsonConvert.SerializeObject(d);
    //    Debug.Log("Serialized object: " + dSer);
    //}
}
