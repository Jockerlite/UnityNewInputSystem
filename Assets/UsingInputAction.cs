using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UIElements;

public class UsingInputAction : MonoBehaviour
{
    public GameObject objectInteraction;
    public bool worldTwoD;
    public bool doubleClick;
    [SerializeField, Range(0,5)] private int dragPosSwipeRotateMovePres; // As drop-down list
    public Camera camera;
    public GameObject trail;
    [SerializeField] private float minimumDistance = .2f;
    [SerializeField] private float maximumDistance = 1.0f;
    [SerializeField, Range(0f, 1f)] private float directionThreshold = 0.9f;
    private InputAction.CallbackContext contextSwipe;
    [SerializeField] private float smoothRotate = 1;

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
    private Quaternion rotatePosition;
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
    private Vector3 oldRotatePosition;
    private Vector3 newRotatePosition;
    private bool isRotate;
    InputAction.CallbackContext _context;
    NewControls newControls;
    public float sharpnessZoom; //speedzooom
    public float cameraPosition; // with point camera down
    public int cameraZoomMax;
    public int cameraZoomMin;
    public float cameraSpeed;
    public RaycastHit reyHit;
    public MouseState mouseState;
    private bool moveMouse;
    private bool touchEnable;
    private bool touchDisable;
    private bool rotateMouse;
    private float timeDoubleClick;
    private Vector3 mousePosition;
    private float speedMouse = 0.5f;
    private float speedMoveKey = 0.5f; 

    private void Awake()//started - !, performed - use press, canceled - end press
    {   

        //mouseState = gameObject.GetComponent<MouseState>();
        sphereRigidbody = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        touchPress = playerInput.actions["Press"];
         newControls = new NewControls();
        newControls.Enable();
        newControls.Player.Jump.performed += Space;
        newControls.Player.Move.performed += WASD;
        newControls.Player.Press.canceled += Press;
        //newControls.Player.Movement.performed += Move;
        if (dragPosSwipeRotateMovePres == 0)// what is drag ? whereb is drag and merg ?
        {
            if (worldTwoD)
                newControls.Player.Move.performed += DragDrop2D;  //Position is end
            else
                newControls.Player.Move.performed += DragDrop3D;  //Position is end
        }
        if (dragPosSwipeRotateMovePres == 1) // Move is equivalent stick, difference the Pose is this click object and click map? Need make radius zone
        {
            if(worldTwoD)
                newControls.Player.Move.performed += TouchPosition2D;  //DragDrop in process 
            else
                newControls.Player.Move.performed += TouchPosition3D;    //DragDrop in process 
        }
        if (dragPosSwipeRotateMovePres == 2) // no move, no colrLine
        {
            newControls.Player.Move.started += TouchStart;
            if (worldTwoD) 
                newControls.Player.Move.performed += Swipe2D;
            else
                newControls.Player.Move.performed += Swipe3D;//Swipe is drag
            newControls.Player.Move.canceled += TouchCanceled;    //Drag moving
        }
        if (dragPosSwipeRotateMovePres == 3)
            newControls.Player.Move.performed += RotateObject;
        if (dragPosSwipeRotateMovePres == 4)
            newControls.Player.Move.performed += MoveTouch;
        if (dragPosSwipeRotateMovePres == 5)
             newControls.Player.Press.performed += ZoomDoubleTap;//TouchPressed;

        newControls.Player.MouseScroll.performed += MouseScroll;
        newControls.Player.Aiming.started += EnableRotateMouse;
        newControls.Player.MouseMove.performed += MouseRotate;
        newControls.Player.Aiming.canceled += DisableRotateMouse;
        newControls.Player.Fire.started += EnableMoveMouse;
        newControls.Player.MouseMove.performed += MouseMove;
        newControls.Player.Fire.canceled += DisableMoveMouse;

        // newControls.Player.Tap.performed += ZoomDoubleTouch;
        newControls.Player.ZoomSecondary.performed += ZoomSecondaryTouch;
        //newControls.Touch.DoubleClick.performed += 
        //touchPress = playerInput.actions["Press"];
        //touchPosition = playerInput.actions["Move"];
        //touchDoublePosition = playerInput.actions["DoubleMove"];

    }
    //////////////////////////////// - Keyboard - ////////////////////////
    ///
    private void WASD(InputAction.CallbackContext context)
    {
        if (Keyboard.current[Key.W].wasPressedThisFrame)
        {
            sphereRigidbody.AddForce(Vector3.forward * speedMoveKey * Time.deltaTime, ForceMode.Impulse);
        }
        if (Keyboard.current[Key.S].wasPressedThisFrame)
        {
            sphereRigidbody.AddForce(Vector3.back * speedMoveKey * Time.deltaTime, ForceMode.Impulse);
        }
        if (Keyboard.current[Key.A].wasPressedThisFrame)
        {
            sphereRigidbody.AddForce(Vector3.left * speedMoveKey * Time.deltaTime, ForceMode.Impulse);
        }
        if (Keyboard.current[Key.D].wasPressedThisFrame)
        {
            sphereRigidbody.AddForce(Vector3.right * speedMoveKey * Time.deltaTime, ForceMode.Impulse);
        }
    }
    public void Space(InputAction.CallbackContext context)
    {
        Debug.Log("Jump_" + context.phase);
        sphereRigidbody.AddForce(Vector3.up * 5f, ForceMode.Impulse);

    }
    private void MusingKey(InputAction.CallbackContext context)
    {
        
    }

    /// //////////////////////////// - Mouse - //////////////////////////
    private void MouseScroll(InputAction.CallbackContext context) // zoom
    {
        Vector2 vec = context.ReadValue<Vector2>();
        Debug.Log("MouseScroll " + context.control);
        if (vec.x < 0 || vec.y < 0) camera.fieldOfView += Time.deltaTime * sharpnessZoom;
        if (vec.x > 0 || vec.y > 0) camera.fieldOfView -= Time.deltaTime*sharpnessZoom;

    }
    private void EnableMoveMouse(InputAction.CallbackContext context)
    {//+ double click
        float time = Time.fixedTime - timeDoubleClick;
        if (time <= 0.25)
        {
            //Debug.Log("DoubleClick " + time); 
            DoubleClick();
        }
        else 
        //Debug.Log("MouseFire " + moveMouse); 
        //Debug.LogAssertion(time + " MTime " + Time.fixedTime);
        timeDoubleClick = Time.fixedTime;
        moveMouse = true;

    }
    private void EnableRotateMouse(InputAction.CallbackContext context)
    {
        rotateMouse = true;

        isRotate = true;
        //Debug.Log("MouseAiming " + rotateMouse);
    }
    private void DisableMoveMouse(InputAction.CallbackContext context)
    {
        moveMouse = false;
        //Debug.Log("MouseFire " + moveMouse);
    }
    private void DisableRotateMouse(InputAction.CallbackContext context)
    {
        rotateMouse = false;
        //Debug.Log("MouseAiming " + rotateMouse);
    }
    private void MouseMove(InputAction.CallbackContext context)//rotation angle must be taken into account but now not hardcode, new two gameObject 1.Rotate 2. Move in relrtive 1
    {
        Vector2 vector = context.ReadValue<Vector2>();
        float width;
        float height;
        //Debug.Log("MouseMove" + moveMouse);
        if (!moveMouse) return;
        else
        {
            Vector2 screen = new Vector2(Screen.width, Screen.height);
            width = screen.x /100;
            height = screen.y /100;
            if (vector.x < 20 * width) camera.transform.position = new Vector3(camera.transform.position.x - 1 * Time.deltaTime, camera.transform.position.y, camera.transform.position.z);
            if (vector.y < 20 * height) camera.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y- 1 * Time.deltaTime, camera.transform.position.z);
            if (vector.x > 80 * width) camera.transform.position = new Vector3(camera.transform.position.x + 1 * Time.deltaTime, camera.transform.position.y, camera.transform.position.z);
            if (vector.y > 80 * height) camera.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y+ 1 * Time.deltaTime, camera.transform.position.z);
            //Vector2 vec = context.ReadValue<Vector2>();
            //Vector3 mouseWorldPoint = Camera.main.ScreenToWorldPoint(new Vector3(vec.x, vec.y, Camera.main.nearClipPlane));
            //camera.transform.position += mouseWorldPoint *Time.deltaTime ;
        }
    }
    private void DoubleClick()
    {
        return;
    }
    private void MouseRotate(InputAction.CallbackContext context)
    {
        if (!rotateMouse) return;
        else
        {
            Vector2 vec = context.ReadValue<Vector2>();
            Vector2 inputVector = context.ReadValue<Vector2>();
            Vector3 inputVector3 = new Vector3(inputVector.x, inputVector.y, 1);
            Vector3 pos = new Vector3(inputVector.x, inputVector.y, Camera.main.nearClipPlane);
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(pos);
            if (isRotate)
            {
                Debug.Log("Rotate");
                oldRotatePosition = inputVector3;
                isRotate = false;
            }
            else
            {
                newRotatePosition = inputVector3;
                Vector3 deltaRotate = oldRotatePosition - newRotatePosition;
                deltaRotate = new Vector3(deltaRotate.y * -1, deltaRotate.x, deltaRotate.z) * 10;
                rotatePosition = Quaternion.Euler(deltaRotate) * sphereRigidbody.transform.rotation;
                sphereRigidbody.transform.rotation = Quaternion.Lerp(sphereRigidbody.transform.rotation, rotatePosition, smoothRotate); //sharp rotation
                oldRotatePosition = newRotatePosition;

            }
            Debug.Log("Mouse Rotate" + vec);
        }
    }

    //private void CameraHightPositionMouse()
    //{
    //    Vector3 directionRay = camera.transform.TransformDirection(Vector3.forward);
    //    if (Physics.Raycast(camera.transform.position, directionRay, out reyHit, 100))
    //    {
    //        if (reyHit.collider.tag == "terrain")
    //        {
    //            if (reyHit.distance < cameraPosition)
    //                Debug.Log(reyHit.transform.name);
    //            camera.transform.position += new Vector3(0, cameraPosition - reyHit.distance, 0);
    //        }
    //        else { camera.transform.position -= new Vector3(0, reyHit.distance - cameraPosition, 0); }
    //    }
    //    if (Input.GetAxis("MouseScrollWhell") < 0 && cameraPosition < cameraZoomMin)
    //    {
    //        cameraPosition += sharpnessZoom * Time.deltaTime;
    //        cameraSpeed += 0.007f;
    //    }
    //    if (Input.GetAxis("MouseScrollWhell") > 0 && cameraPosition > cameraZoomMax)
    //    {
    //        cameraPosition -= sharpnessZoom * Time.deltaTime;
    //        cameraSpeed -= 0.007f;
    //    }
    //}
    //private void ZoomMouse()
    //{
    //    camera.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * sharpnessZoom;

    //}
    //private void CameraWITHPositionMouse()
    //{
    //    if (true)//mouseState.NewMouseState == mouseState.MouseState.Default)
    //    {
    //        if (20 > Input.mousePosition.x)
    //        {
    //            camera.transform.position -= new Vector3(cameraSpeed, 0, 0);
    //        }
    //        if ((Screen.width - 10) < Input.mousePosition.x)
    //        {
    //            camera.transform.position += new Vector3(cameraSpeed, 0, 0);
    //        }

    //        if (20 > Input.mousePosition.y)
    //        {
    //            camera.transform.position -= new Vector3(cameraSpeed, 0, 0);
    //        }
    //        if ((Screen.width - 10) < Input.mousePosition.y)
    //        {
    //            camera.transform.position += new Vector3(cameraSpeed, 0, 0);
    //        }
    //    }
    //}
    //private void Move(InputAction.CallbackContext context)
    //{
    //    Debug.Log(context);
    //    Vector2 inputVector = context.ReadValue<Vector2>();
    //    primaryPosition = new Vector3(inputVector.x, inputVector.y, Camera.main.nearClipPlane * ObjectDistanceCamera);
    //    if (zoomEnable) return;
    //    sphereRigidbody.AddForce(new Vector3(inputVector.x, 0, inputVector.y) * speedMove * 5 * Time.deltaTime, ForceMode.Impulse);
    //}

    private void Update()
    {
        touchEnable = Touchscreen.current.primaryTouch.press.isPressed;
        
            if(worldTwoD)
                Swipe2D(_context); 
            else Swipe3D(_context);
        //if (Touchscreen.current.primaryTouch.press.isPressed != true && startPosition.x != 0) SwipeDirection();
        //Debug.Log(Touchscreen.current.primaryTouch.press.isPressed);
    }

    //////////////////////////// - Touch  - //////////////////////////
    /// 
    
    private void DragDrop2D(InputAction.CallbackContext context)
    {
        if (zoomEnable) return;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.tag == "InteractionObject")
                {
                    Debug.Log("Hit" + hit.transform.name);
            }

            }
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
    public void TouchPosition(InputAction.CallbackContext context)
    {
        Debug.Log("PositionEnable2D");

        Vector2 inputVector = context.ReadValue<Vector2>();
        primaryPosition = new Vector3(inputVector.x, inputVector.y, Camera.main.nearClipPlane);
        if (zoomEnable) return;
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(primaryPosition);
        //Vector3 worldPoint = Camera.main.ScreenToWorldPoint(context.ReadValue<Vector2>());
        // sphere.transform.position = new Vector3(inputVector.x, sphere.transform.position.z, inputVector.y);
        sphereRigidbody.AddForce(new Vector3(worldPoint.x, worldPoint.y, 0) * speedMove * Time.deltaTime, ForceMode.Force);

        Debug.Log(context);
        Debug.Log(primaryPosition);
    }

    private void TouchStart(InputAction.CallbackContext context)
    {

        if (zoomEnable) return;
        //Debug.Log("SwipeStart");
        //Vector2 inputVector = context.ReadValue<Vector2>();
        //startPosition = new Vector3(inputVector.x, inputVector.y, Camera.main.nearClipPlane);
        _context = context;
    }
    private void Swipe2D(InputAction.CallbackContext context)
    {
        if (touchEnable && startPosition.x == 0)
        {
            _context = context;
            Vector2 inputVector = context.ReadValue<Vector2>();
            startPosition = new Vector3(inputVector.x, inputVector.y, Camera.main.nearClipPlane);

            //Debug.Log("go");
        }else if ( touchEnable == false && startPosition.x != 0)
        {
            Vector2 inputVector = context.ReadValue<Vector2>();
            endPosition = new Vector3(inputVector.x, inputVector.y, Camera.main.nearClipPlane);

            Vector2 vec = new Vector2(startPosition.x - endPosition.x, startPosition.y - endPosition.y);
            if (Mathf.Abs(vec.x) > Mathf.Abs(vec.y))
                if (vec.x > 50)
                {
                    sphereRigidbody.AddForce(Vector3.right * speedMoveKey * Time.deltaTime, ForceMode.Impulse);
                    Debug.Log("SwipeRight");
                }
                else if (vec.x < -50)
                {

                    Debug.Log("SwipeLeft");
                    sphereRigidbody.AddForce(Vector3.left * speedMoveKey * Time.deltaTime, ForceMode.Impulse);
                }
                else
                if (vec.y > 50)
                {
                    Debug.Log("SwipeUp");
                    sphereRigidbody.AddForce(Vector3.forward * speedMoveKey * Time.deltaTime, ForceMode.Impulse);
                }
                else if (vec.y < -50)
                {
                    Debug.Log("SwipeDown");
                    sphereRigidbody.AddForce(Vector3.back * speedMoveKey * Time.deltaTime, ForceMode.Impulse);
                }
                else Debug.Log(vec);
                    startPosition.x = 0;
        }
    }
    private void Swipe3D(InputAction.CallbackContext context)
    {

        if (Touchscreen.current.primaryTouch.press.isPressed)
        {
            Vector2 inputVector = context.ReadValue<Vector2>();
        }
    }
    private void TouchCanceled(InputAction.CallbackContext context)
    {
        //Debug.Log("SwipeEnd");
        if(worldTwoD) Swipe2D(context);
        else Swipe3D(context);
        
    }

    private void SwipeDirection(Vector2 vec)
    {
        if (Mathf.Abs(vec.x) > Mathf.Abs(vec.y))
            if (vec.x > 0) Debug.Log("SwipeRight");
            else Debug.Log("SwipeLeft");
        else
            if (vec.y > 0) Debug.Log("SwipeUp");
            else Debug.Log("SwipeDown");
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
    
    public void Press(InputAction.CallbackContext context)
    {
        isRotate = true;

    }
    private void RotateObject(InputAction.CallbackContext context)
    {

        Vector2 inputVector = context.ReadValue<Vector2>();
        Vector3 inputVector3 = new Vector3(inputVector.x, inputVector.y, 1);
        Vector3 pos = new Vector3(inputVector.x, inputVector.y, Camera.main.nearClipPlane);
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(pos);
        if (isRotate)
        {
            Debug.Log("Rotate");
            oldRotatePosition = inputVector3;
            isRotate = false;
        }
        else
        {
            newRotatePosition = inputVector3;
            Vector3 deltaRotate = oldRotatePosition - newRotatePosition;
            deltaRotate = new Vector3(deltaRotate.y * -1, deltaRotate.x , deltaRotate.z);
            rotatePosition = Quaternion.Euler(deltaRotate) * sphereRigidbody.transform.rotation;
            sphereRigidbody.transform.rotation = Quaternion.Lerp(sphereRigidbody.transform.rotation, rotatePosition, smoothRotate); //sharp rotation
            oldRotatePosition = newRotatePosition;

        }
        //Debug.Log(newRotatePosition);
        //Debug.Log(oldRotatePosition);
        
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
        if (timeDoubleTap == 0)
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
            timeDoubleTap = 0;
        }
        else {
            Debug.Log(time + "-DoubleTap-" + timeDoubleTap);
            camera.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y, camera.transform.position.z + distance * distanceTap);
            timeDoubleTap = 0;
    }
    }

    

}
