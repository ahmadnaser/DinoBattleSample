using UnityEngine;
using System.Collections;
using System.Diagnostics;

public class Logger : MonoBehaviour {

    const string TAG_FORMAT = "[{0}]";
    const string TAG_FIREBASE = "Firebase";
    const string TAG_Flag = "Flag";
    const string TAG_FINDTARGET = "FindTarget";
    const string TAG_CONTROL_PANEL = "ControlPanel";
    const string TAG_FLAG_CONTROL = "FlagControl";
    const string TAG_GAME_CONTROL = "GameControl";

    const bool DEBUG_FINDTARGET = false;
    const bool DEBUG_FIREBASE = false;
    const bool DEBUG_CONTROL_PANEL = true;
    const bool DEBUG_FLAG_CONTROL = false;
    const bool DEBUG_GAME_CONTROL = true;

    public static void d(string tag, string format, params object[] args) {
        UnityEngine.Debug.Log(string.Format(TAG_FORMAT, tag) + string.Format(format, args));
    }

    public static void findTarget(string format, params object[] args) {
        if (DEBUG_FINDTARGET)
            d(TAG_FINDTARGET, format, args);
    }

    public static void firebase(string format, params object[] args)
    {
        if (DEBUG_FIREBASE)
            d(TAG_FIREBASE, format, args);
    }

    public static void controlPanel(string format, params object[] args)
    {
        if (DEBUG_CONTROL_PANEL)
            d(TAG_CONTROL_PANEL, format, args);
    }

    public static void flagControl(string format, params object[] args)
    {
        if (DEBUG_FLAG_CONTROL)
            d(TAG_FLAG_CONTROL, format, args);
    }

    public static void gameControl(string format, params object[] args)
    {
        if (DEBUG_GAME_CONTROL)
            d(TAG_GAME_CONTROL, format, args);
    }

    public static void dumpStack(string tag) {
        StackTrace st = new StackTrace(true);
        for (int i = 0; i < st.FrameCount; i++)
        {
            // Note that high up the call stack, there is only
            // one stack frame.
            StackFrame sf = st.GetFrame(i);
            d(tag, "{0} Method: {1}, Line: {2}", i, sf.GetMethod(), sf.GetFileLineNumber());
        }
    }
}
