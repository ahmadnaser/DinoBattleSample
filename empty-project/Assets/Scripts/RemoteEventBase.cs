using UnityEngine;
using System.Collections;

public class RemoteEventBase {

    public const string KEY_EVENT_ROOT = "event";

    public const string KEY_ACTION_NAME = "actionName";
    public const string KEY_ACTOR = "actor";
    public const string KEY_ACT_TARGET = "act_target";
    public const string KEY_PLAYER_ID = "playerId";
    public const string KEY_LOC_CURRENT = "locCurrent";
    public const string KEY_LOC_TARGET = "locTarget";
    public const string KEY_TIME_STAMP = "timeStamp";
    public const string KEY_VECTOR_X = "vector_x";
    public const string KEY_VECTOR_Y = "vector_y";
    public const string KEY_VECTOR_Z = "vector_z";

    private string actionName;
    private string actorId;
    private string act_target;
    private string timeStamp;
    private string playerId;
    private string eventId;
    private Vector3 currentLocation, targetLocation;

    public enum Action
    {
        CREATE, MOVE
    }

    public const string VALUE_ACTION_NAME_MOVE = "move";
    public const string VALUE_ACTION_NAME_CREATE = "create";
    public const string VALUE_ACTION_NAME_SYNC_STATUS = "sync_status";

    public static string getActionValue(Action action) {
        switch (action) {
            case Action.MOVE:
                return VALUE_ACTION_NAME_MOVE;
            case Action.CREATE:
                return VALUE_ACTION_NAME_CREATE;
        }
        return null;
    }

    public RemoteEventBase() { }
    public RemoteEventBase(string eventId, string actionName, string actor, string act_target, string playerId, string timeStamp, Vector3 currentLocation, Vector3 targetLocation) {
        this.eventId = eventId;
        this.actionName = actionName;
        this.actorId = actor;
        this.act_target = act_target;
        this.playerId = playerId;
        this.timeStamp = timeStamp;
        this.currentLocation = currentLocation;
        this.targetLocation = targetLocation;
    }

    public string getActionName()
    {
        return actionName;
    }
    public string getActorId()
    {
        return actorId;
    }
    public string getActTarget()
    {
        return act_target;
    }
    public string getTimeStamp()
    {
        return timeStamp;
    }
    public string getPlayerId()
    {
        return playerId;
    }
    public string getEventId()
    {
        return eventId;
    }
    public Vector3 getCurrentLocation()
    {
        return currentLocation;
    }
    public Vector3 getTargetLocation()
    {
        return targetLocation;
    }
}

