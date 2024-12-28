using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    [SerializeField] PlayerInput playerInput;

    public InputAction Navigate { get; private set; }
    public InputAction Submit { get; private set; }
    public InputAction Cancel { get; private set; }
    public InputAction Point { get; private set; }
    public InputAction Click { get; private set; }
    public InputAction RightClick { get; private set; }
    public InputAction MiddleClick { get; private set; }
    public InputAction ScrollWheel { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeInput();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeInput()
    {
        Navigate = playerInput.actions["Navigate"];
        Submit = playerInput.actions["Submit"];
        Cancel = playerInput.actions["Cancel"];
        Point = playerInput.actions["Point"];
        Click = playerInput.actions["Click"];
        RightClick = playerInput.actions["RightClick"];
        MiddleClick = playerInput.actions["MiddleClick"];
        ScrollWheel = playerInput.actions["ScrollWheel"];
    }


}
