using UnityEngine;
using System.Collections;

public class Startbutton : MonoBehaviour
{
    
    public GameController gameController;
    private Vector3 initScale;
    // Use this for initialization
    void Start()
    {
        Debug.Log("Startbutton start+");
        initScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Startbutton update+");
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
                    Vector3 pos = Input.GetTouch(0).position;
                    if (this.GetComponent<GUITexture>().HitTest(pos))
                    {
                        touchDown();
                    }
                } else if (Input.GetTouch(0).phase == TouchPhase.Ended) {
                    if (isTouchDown)
                    {
                        touchUp();
                    }
                }
            }
        }
        else if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            Logger.controlPanel("onInteract Input.GetMouseButtonDown(0) {0}", Input.GetMouseButtonDown(0));
            //if (Input.GetMouseButtonDown(0))
            //{
                
            //    checkTouch(Input.mousePosition);
            //}
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 pos = Input.mousePosition;
                if (this.GetComponent<GUITexture>().HitTest(pos))
                {
                    touchDown();
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (isTouchDown)
                {
                    touchUp();
                }
            }
        }
    }

    private bool isTouchDown = false;

    public void touchDown()
    {
        this.transform.localScale = new Vector3(initScale.x - 0.01f, initScale.y - 0.01f, initScale.z);
        isTouchDown = true;
    }

    public void touchUp()
    {
        this.transform.localScale = initScale;
        gameController.gamePrepare();
        gameObject.SetActive(false);
        isTouchDown = false;
    }

    public void checkTouch(Vector3 pos)
    {
        //Vector3 wp = Camera.main.ScreenToWorldPoint(pos);
        //Vector3 touchPos = new Vector3(wp.x, wp.y, wp.z);
        //Collider2D hit = Physics2D.OverlapPoint(touchPos);

        if (this.GetComponent<GUITexture>().HitTest(pos))
        {
            Logger.controlPanel("HitTest");
            gameController.gamePrepare();
            gameObject.SetActive(false);
        }

        //foreach (Touch touch in Input.touches)
        //{
        //    if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
        //    {
        //        if (guiTexture.HitTest(new Vector3(touch.position.x, touch.position.y, 0))
        //            {
        //            //Do something here on button click
        //        }
        //    }

        //}
    }

    //public void startGame() {
    //    Logger.controlPanel("startGame");
    //    gameController.gameStart();
    //    gameObject.SetActive(false);
    //}
}