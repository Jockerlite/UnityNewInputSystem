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

    private Rigidbody sphereRigidbody;
    private PlayerInput playerInput;
    private InputAction touchPress;
    private InputAction touchPosition;
    private InputAction touchDoublePosition;
    private float speed = 1f;
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
    private float primaryDelta;
    private float zoomDelta;
    private float zoomEndDelta;
    private Coroutine coroutineSwipe;
    private float ObjectDistanceCamera = 1.5f;
    private bool isometric;
    private void Awake()//started - !, performed - use press, canceled - end press
    {   
        sphereRigidbody = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();

        NewControls newControls = new NewControls();
        newControls.Enable();
        newControls.Player.Jump.performed += Space;
        //newControls.Player.Movement.performed += Move;
        if (dragPosSwipeRotateMovePres == 0)
        {
            if (WorldTwoD)
                newControls.Player.Move.performed += DragDrop2D;  //Position is end
            else
                newControls.Player.Move.performed += DragDrop3D;  //Position is end
        }
        if (dragPosSwipeRotateMovePres == 1)
        {
            if(WorldTwoD)
                newControls.Player.Press.canceled += TouchPosition2D;  //DragDrop in process 
            else
                newControls.Player.Move.canceled += TouchPosition3D;    //DragDrop in process 
        }
        if (dragPosSwipeRotateMovePres == 2)
        {
            newControls.Player.Move.started += SwipeStart;   //Swipe is drag
            newControls.Player.Move.canceled += SwipeEnd;    //Drag moving
        }
        if (dragPosSwipeRotateMovePres == 3)

            newControls.Player.Move.performed += RotateObject;

        if (dragPosSwipeRotateMovePres == 4)

            newControls.Player.Move.performed += MoveTouch;

        if (dragPosSwipeRotateMovePres == 5)

            newControls.Player.Press.performed += TouchPressed;

        newControls.Player.Tap.performed += ZoomDoubleTouch;
        newControls.Player.Zoom.performed += ZoomPrimaryTouch;
        //newControls.Touch.DoubleClick.performed += 
        //touchPress = playerInput.actions["Press"];
        //touchPosition = playerInput.actions["Move"];
        //touchDoublePosition = playerInput.actions["DoubleMove"];

    }

    private void Move(InputAction.CallbackContext context)
    {
        Debug.Log(context);
        Vector2 inputVector = context.ReadValue<Vector2>();
        speed = 5f;
        sphereRigidbody.AddForce(new Vector3(inputVector.x, 0, inputVector.y) * speed * 5 * Time.deltaTime, ForceMode.Impulse);
    }
    public void Space(InputAction.CallbackContext context)
    {
        Debug.Log("Jump_" + context.phase);
        sphereRigidbody.AddForce(Vector3.up * 5f, ForceMode.Impulse);

    }
    private void DragDrop2D(InputAction.CallbackContext context)
    {
        return;
    }
    private void DragDrop3D(InputAction.CallbackContext context)
    {
        return;
    }
    private void TouchPressed(InputAction.CallbackContext context)
    {
        float pos = context.ReadValue<float>();
        Debug.Log(pos);
    }
    public void TouchPosition3D(InputAction.CallbackContext context)
    {
        if (distance > dethZoneSwipe) return;
        Debug.Log("PositionEnable3D");
        Vector2 inputVector = context.ReadValue<Vector2>();
        Vector3 pos = new Vector3(inputVector.x, inputVector.y, Camera.main.nearClipPlane * ObjectDistanceCamera);
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(pos);
        sphereRigidbody.AddForce(new Vector3(worldPoint.x, worldPoint.y, 0) * speed * Time.deltaTime, ForceMode.Force);

        Debug.Log(context);
        Debug.Log(pos);
    }
    public void TouchPosition2D(InputAction.CallbackContext context)
    {
        Debug.Log("PositionEnable2D");
        UnityEngine.Vector2 inputVector = context.ReadValue<UnityEngine.Vector2>();
        UnityEngine.Vector3 pos = new UnityEngine.Vector3(inputVector.x, inputVector.y, Camera.main.nearClipPlane * ObjectDistanceCamera);
        UnityEngine.Vector3 worldPoint = Camera.main.ScreenToWorldPoint(pos);
        
        sphereRigidbody.transform.position = worldPoint;

        Debug.Log(context);
        Debug.Log(pos);
    }
    public void TouchPosition(InputAction.CallbackContext context)
    {
        Debug.Log("PositionEnable2D");

        Vector2 inputVector = context.ReadValue<Vector2>();
        Vector3 pos = new Vector3(inputVector.x, inputVector.y, Camera.main.nearClipPlane);
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(pos);
        //Vector3 worldPoint = Camera.main.ScreenToWorldPoint(context.ReadValue<Vector2>());
        // sphere.transform.position = new Vector3(inputVector.x, sphere.transform.position.z, inputVector.y);
        sphereRigidbody.AddForce(new Vector3(worldPoint.x, worldPoint.y, 0) * speed * Time.deltaTime, ForceMode.Force);

        Debug.Log(context);
        Debug.Log(pos);
    }
    public void Submit()
    {

        Debug.Log("Submit");
    }
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

        Debug.Log("SwipeEnable");
        Vector2 inputVector = context.ReadValue<Vector2>();
        startPosition = new Vector3(inputVector.x, inputVector.y, Camera.main.nearClipPlane);
        Debug.Log(startPosition + "startPosition");
        trail.SetActive(true);
        trail.transform.position = startPosition;
        contextSwipe = context;
        coroutineSwipe = StartCoroutine("Trail");
    }
    private  IEnumerator Trail()
    {
        while (true)
        {
            trail.transform.position = contextSwipe.ReadValue<Vector2>();
            yield return null;
        }
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
        return;
    }

    private void MoveTouch(InputAction.CallbackContext context)
    {
        return;
    }

    private void ZoomPrimaryTouch(InputAction.CallbackContext context)
    {
        primaryDelta = context.ReadValue<float>();
    }
    private void ZoomDoubleTouch(InputAction.CallbackContext context)
    {
        float delta = context.ReadValue<float>() - primaryDelta;
        if (zoomEndDelta != 0)
        {
            zoomEndDelta = delta;
            ZoomTouch();
        }
        else
        {
            zoomDelta = delta;
        }
    }
    private void ZoomTouch()
    {
        if ( zoomDelta > zoomEndDelta) 
        {
            camera.transform.position = new Vector3(0, 0, 0); //need to calculate the direction vector, and then the direction itself
        }
        else
        {

        }
            //Clear data with work logic
            zoomDelta = 0;
        zoomEndDelta = 0;
    }
    private void Tap() //Double click
    {

    }

    private void Update()
    {
       /* if (Touchscreen.current.wasUpdatedThisFrame) //Mouse.current.leftButton.wasPressedThisFrame
        {
            //check click
        }*/
    }


}
