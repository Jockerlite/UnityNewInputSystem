using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class UsingInputAction : MonoBehaviour
{
    public GameObject ObjectInteraction;
    public bool WorldTwoD;
    public bool DoubleClick;
    [SerializeField, Range(0,5)] private int dragPosSwipeRotateMovePres; // As drop-down list
    public GameObject camera;
    public GameObject trail;
    [SerializeField] private float minimumDistance = .2f;
    [SerializeField] private float maximumDistance = 1.0f;
    [SerializeField, Range(0f, 1f)] private float directionThreshold = 0.9f;
    private InputAction.CallbackContext contextSwipe;
    [SerializeField] private float smooth = 1;

    private Rigidbody sphereRigidbody;
    private PlayerInput playerInput;
    private InputAction touchPress;
    private InputAction touchPosition;
    private InputAction touchDoublePosition;
    private float speedMove = 5f;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private Vector3 startDoublePosition;
    private Vector3 endDoublePosition;
    private float startTimePosition;
    private float endTimePosition;
    private float startDoubleTimePosition;
    private float endTimeDoublePosition;
    private float dethZoneSwipe;
    //private float dethTimeSwipe; 
    private float distance;
    private Vector3 primaryPosition;
    private Vector3 secondaryPosition;
    private bool zoomEnable;
    private Coroutine coroutineSwipe;
    private float ObjectDistanceCamera = 1.5f;
    private bool isometric;
    private float timeDoubleTap;
    private void Awake()//started - !, performed - use press, canceled - end press
    {   
        sphereRigidbody = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();

        NewControls newControls = new NewControls();
        newControls.Enable();
        newControls.Player.Jump.performed += Space;
        //newControls.Player.Movement.performed += Move;
        if (dragPosSwipeRotateMovePres == 0)// what is drag ? whereb is drag and merg ?
        {
            if (WorldTwoD)
                newControls.Player.Move.performed += DragDrop2D;  //Position is end
            else
                newControls.Player.Move.performed += DragDrop3D;  //Position is end
        }
        if (dragPosSwipeRotateMovePres == 1) // Move is equivalent stick, difference the Pose is this click object and click map? Need make radius zone
        {
            if(WorldTwoD)
                newControls.Player.Move.performed += TouchPosition2D;  //DragDrop in process 
            else
                newControls.Player.Move.performed += TouchPosition3D;    //DragDrop in process 
        }
        if (dragPosSwipeRotateMovePres == 2) // no move, no colrLine
        {
            newControls.Player.Move.performed += SwipeStart;   //Swipe is drag
            newControls.Player.Move.performed += SwipeEnd;    //Drag moving
        }
        if (dragPosSwipeRotateMovePres == 3)

            newControls.Player.Move.performed += RotateObject;

        if (dragPosSwipeRotateMovePres == 4)

            newControls.Player.Move.performed += MoveTouch;

        if (dragPosSwipeRotateMovePres == 5)

            newControls.Player.Press.performed += ZoomDoubleTap;//TouchPressed;

       // newControls.Player.Tap.performed += ZoomDoubleTouch;
        newControls.Player.ZoomSecondary.performed += ZoomSecondaryTouch;
        //newControls.Touch.DoubleClick.performed += 
        //touchPress = playerInput.actions["Press"];
        //touchPosition = playerInput.actions["Move"];
        //touchDoublePosition = playerInput.actions["DoubleMove"];

    }

    private void Move(InputAction.CallbackContext context)
    {
        Debug.Log(context);
        Vector2 inputVector = context.ReadValue<Vector2>();
        primaryPosition = new Vector3 (inputVector.x, inputVector.y, Camera.main.nearClipPlane* ObjectDistanceCamera);
        if (zoomEnable) return;
        sphereRigidbody.AddForce(new Vector3(inputVector.x, 0, inputVector.y) * speedMove * 5 * Time.deltaTime, ForceMode.Impulse);
    }
    public void Space(InputAction.CallbackContext context)
    {
        Debug.Log("Jump_" + context.phase);
        sphereRigidbody.AddForce(Vector3.up * 5f, ForceMode.Impulse);

    }
    private void DragDrop2D(InputAction.CallbackContext context)
    {
        if (zoomEnable) return;
        return;
    }
    private void DragDrop3D(InputAction.CallbackContext context)
    {
        if (zoomEnable) return;
        return;
    }
    private void TouchPressed(InputAction.CallbackContext context)
    {
        Debug.Log("Press");
        Vector2 inputVector = context.ReadValue<Vector2>();
        Vector3 pos = new Vector3(inputVector.x, inputVector.y, Camera.main.nearClipPlane * ObjectDistanceCamera);
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(pos);
        if (worldPoint.x == 0)
        {
            startPosition = worldPoint;
        }
        else
        {
            endPosition = worldPoint;
            //if (startPosition - endPosition)// need 
        }
    }
    public void TouchPosition3D(InputAction.CallbackContext context)
    {
        if (zoomEnable) return;
        if (distance > dethZoneSwipe) return;
        Debug.Log("PositionEnable3D");
        Vector2 inputVector = context.ReadValue<Vector2>();
        primaryPosition = new Vector3(inputVector.x, inputVector.y, Camera.main.nearClipPlane * ObjectDistanceCamera);
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(primaryPosition);
        sphereRigidbody.AddForce(new Vector3(worldPoint.x, worldPoint.y, 0) * speedMove * Time.deltaTime, ForceMode.Force);

        Debug.Log(context);
        Debug.Log(primaryPosition);
    }
    public void TouchPosition2D(InputAction.CallbackContext context)
    {
        Debug.Log("PositionEnable2D");
        Vector2 inputVector = context.ReadValue<UnityEngine.Vector2>();
        primaryPosition = new UnityEngine.Vector3(inputVector.x, inputVector.y, Camera.main.nearClipPlane * ObjectDistanceCamera);
        if (zoomEnable) return;
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(primaryPosition);
        
        sphereRigidbody.transform.position = worldPoint;

        Debug.Log(context);
        Debug.Log(primaryPosition);
    }
    //public void TouchPosition(InputAction.CallbackContext context)
    //{
    //    Debug.Log("PositionEnable2D");

    //    Vector2 inputVector = context.ReadValue<Vector2>();
    //    primaryPosition = new Vector3(inputVector.x, inputVector.y, Camera.main.nearClipPlane);
    //    if (zoomEnable) return;
    //    Vector3 worldPoint = Camera.main.ScreenToWorldPoint(primaryPosition);
    //    //Vector3 worldPoint = Camera.main.ScreenToWorldPoint(context.ReadValue<Vector2>());
    //    // sphere.transform.position = new Vector3(inputVector.x, sphere.transform.position.z, inputVector.y);
    //    sphereRigidbody.AddForce(new Vector3(worldPoint.x, worldPoint.y, 0) * speedMove * Time.deltaTime, ForceMode.Force);

    //    Debug.Log(context);
    //    Debug.Log(primaryPosition);
    //}
    //public void Submit()
    //{

    //    Debug.Log("Submit");
    //}
    //  Движение через свайпю Вращение вокруг объекта. Зоом. Зоом через двойной клик. Перемещение камеры через свайп 3Д. Премещение объекта в 3Д на поверхностях.
    //  Скролинг меню. Нажатие клавишь. Нажатие на объект с тегом.
    //private void OnEnable()
    //{
    //    touchPress.performed += TouchPressed;

    //    Debug.Log("Enable");
    //}
    //private void OnDisable()
    //{
    //    touchPress.performed -= TouchPressed;

    //    Debug.Log("Disable");
    //}

    private void SwipeStart(InputAction.CallbackContext context)
    {
        if (zoomEnable) return;

        Debug.Log("SwipeEnable");
        Vector2 inputVector = context.ReadValue<Vector2>();
        startPosition = new Vector3(inputVector.x, inputVector.y, Camera.main.nearClipPlane);
        Debug.Log(startPosition + "startPosition");
        trail.SetActive(true);
        trail.transform.position = startPosition;
        contextSwipe = context;
        coroutineSwipe = StartCoroutine("Trail");
    }
    private void SwipeEnd(InputAction.CallbackContext context)
    {
        StopCoroutine(coroutineSwipe);
        Vector2 inputVector = context.ReadValue<Vector2>();
        endPosition = new Vector3(inputVector.x, inputVector.y, Camera.main.nearClipPlane); 
        Debug.Log(endPosition + "endPosition");
        distance = Vector3.Distance(startPosition, endPosition);
        if (distance > dethZoneSwipe)
        {
            //TouchPosition3D(context);
        Debug.DrawLine(startPosition, endPosition, Color.red, 5f);
        Vector3 direction = startPosition - endPosition;
        Vector2 direction2D = new Vector2(direction.x, direction.y).normalized;
        SwipeDirection(direction2D);
        trail.SetActive(false);
            //    return;
        }
    }
    private IEnumerator Trail()
    {
        while (true)
        {
            trail.transform.position = contextSwipe.ReadValue<Vector2>();
            yield return null;
        }
    }
    private void SwipeDirection(in Vector2 direction)
    {
        if (Vector2.Dot(Vector2.up, direction) > directionThreshold)
        {
            Debug.Log("SwipeUp");

        }   else if (Vector2.Dot(Vector2.down, direction) > directionThreshold)
        {
            Debug.Log("SwipeDown");

        }   else if (Vector2.Dot(Vector2.left, direction) > directionThreshold)
        {
            Debug.Log("SwipeLeft");

        }   else if (Vector2.Dot(Vector2.right, direction) > directionThreshold)
        {
            Debug.Log("SwipeRight");

        }

    }
    private void RotateObject(InputAction.CallbackContext context)
    {
        Debug.Log("Rotate");

        Vector2 inputVector = context.ReadValue<Vector2>();
        Vector3 pos = new Vector3(inputVector.x, inputVector.y, Camera.main.nearClipPlane);
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(pos);
        //Vector3 worldPoint = Camera.main.ScreenToWorldPoint(context.ReadValue<Vector2>());
        // sphere.transform.position = new Vector3(inputVector.x, sphere.transform.position.z, inputVector.y);
        if (startPosition.x == 0)   //Standart rotate
            startPosition = worldPoint;
        else {
            endPosition = worldPoint;
            //need calculate whith samiami
            //sphereRigidbody.transform.rotation = new Vector2()
        }
        //sphereRigidbody.transform.rotation = Quaternion.Lerp(sphereRigidbody.transform.rotation, Quaternion.Euler(worldPoint), smooth); //sharp rotation
        //sphereRigidbody.transform.rotation = Quaternion.Slerp(sphereRigidbody.transform.rotation, Quaternion.Euler(worldPoint), smooth); //slow anim
        //sphereRigidbody.transform.rotation = Quaternion.RotateTowards(sphereRigidbody.transform.rotation, Quaternion.Euler(worldPoint), smooth);//normal anim
        //aaasphereRigidbody.transform.rotation = Quaternion.LookRotation(worldPoint); // Monitors the position of the mouse or looks at the position of the mouse

        //sphereRigidbody.AddForce(new Vector3(worldPoint.x, worldPoint.y, 0) * speed * Time.deltaTime, ForceMode.Force);

        Debug.Log(context);
        Debug.Log(pos);
        startPosition = new Vector3 (0,0,0);
    }

    private void MoveTouch(InputAction.CallbackContext context)
    {
        return;
    }

   
    private void ZoomSecondaryTouch(InputAction.CallbackContext context)
    {
        zoomEnable = true;
        float speedZoom = 1f;
        Vector2 inputVector = context.ReadValue<Vector2>();
        secondaryPosition = new Vector3(inputVector.x, inputVector.y, Camera.main.nearClipPlane);
        float distance = Vector3.Distance(secondaryPosition, primaryPosition) ;
        
        Debug.Log("zoomSecondary-" + secondaryPosition + "-DISTANCE-"+ distance);
        camera.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y, camera.transform.position.z + distance * speedZoom);
        zoomEnable = false;
    }
   
    private void ZoomDoubleTap(InputAction.CallbackContext context) //Double click
    {
        float distanceTap = 3f;
        float time;
        if (timeDoubleTap == null)
        {
            timeDoubleTap = Time.time;
            return;
        }
        else
        {
            time = Time.time;
        }

        Vector2 inputVector = context.ReadValue<Vector2>();
        secondaryPosition = new Vector3(inputVector.x, inputVector.y, Camera.main.nearClipPlane);
        float distance = Vector3.Distance(secondaryPosition, primaryPosition);

        if (distance > 100 || time > 3f) {

            Debug.Log(time +"-NotTapDouble-"+ timeDoubleTap +"_distance-"+distance);
        }
        else {
            Debug.Log(time + "-DoubleTap-" + timeDoubleTap);
            camera.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y, camera.transform.position.z + distance * distanceTap);
        }
    }

    private void Update()
    {
       /* if (Touchscreen.current.wasUpdatedThisFrame) //Mouse.current.leftButton.wasPressedThisFrame
        {
            //check click
        }*/
    }


}
