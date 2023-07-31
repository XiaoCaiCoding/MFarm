using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFarm.Save;

public class Player : MonoBehaviour,ISaveable
{

    private Rigidbody2D rb;

    private float moveInputX;
    private float moveInputY;
    private Vector2 moveInput;
    private float mouseX;
    private float mouseY;

    private Animator[] animtors;

    public float speed;
    private bool isMoving;
    private bool inputDisable;
    private bool useTool=false;

    public string GUID => GetComponent<DataGUID>().guid;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animtors = GetComponentsInChildren<Animator>();
        inputDisable = true;
    }
    private void OnEnable()
    {
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.MoveToPosition += OnMoveToPosition;
        EventHandler.MouseClickedEvent += OnMouseClickedEvent;
        EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.MoveToPosition -= OnMoveToPosition;
        EventHandler.MouseClickedEvent -= OnMouseClickedEvent;
        EventHandler.UpdateGameStateEvent -= OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
    }

    private void OnEndGameEvent()
    {
        inputDisable = true;
    }

    private void OnStartNewGameEvent(int obj)
    {
        inputDisable = false;
        transform.position = Settings.playerStartPos;
    }

    private void Start()
    {
        ISaveable saveable = this;
        saveable.RegisterSaveable();
    }
    private void OnUpdateGameStateEvent(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.GamePlay:
                inputDisable = false;
                break;
            case GameState.Pause:
                inputDisable = true;

                break;
        }
    }

    private void OnMouseClickedEvent(Vector3 mouseWorldPos, ItemDetails itemDetails)
    {
        if(itemDetails.itemType != ItemType.Commodity&& itemDetails.itemType != ItemType.Furniture && itemDetails.itemType != ItemType.Seed)
        {
            mouseX = mouseWorldPos.x - transform.position.x;
            mouseY = mouseWorldPos.y - (transform.position.y + 0.85f);

            if (Mathf.Abs(mouseX) > Mathf.Abs(mouseY))
                mouseY = 0;
            else
                mouseX = 0;

            StartCoroutine(UseToolRoutine(mouseWorldPos, itemDetails));
        }
        else
        {
            EventHandler.CallExecuteActionAfterAnimation(mouseWorldPos, itemDetails);
        }
    }

    private IEnumerator UseToolRoutine(Vector3 mouseWorldPos, ItemDetails itemDetails)
    {
        useTool = true;
        inputDisable = true;
        yield return null;
        foreach (var anim in animtors)
        {
            anim.SetTrigger("useTool");
            anim.SetFloat("InputX", mouseX);
            anim.SetFloat("InputY", mouseY);
        }
        yield return new WaitForSeconds(0.45f);
        EventHandler.CallExecuteActionAfterAnimation(mouseWorldPos, itemDetails);
        yield return new WaitForSeconds(0.25f);
        //µÈ´ý¶¯»­½áÊø
        useTool = false;
        inputDisable = false;
    }
    private void Update()
    {
        if (!inputDisable)
            PlayerInput();
        else
            isMoving = false;
        SwitchAnimation();
    }
    private void FixedUpdate()
    {
        if(!inputDisable)
            Movement();
    }
    
    private void PlayerInput()
    {
        //if(moveInputY==0)
        moveInputX = Input.GetAxisRaw("Horizontal");
        //if(moveInputY==0)
        moveInputY = Input.GetAxisRaw("Vertical");

        if (moveInputX != 0 && moveInputY != 0)
        {
            moveInputX = moveInputX * 0.6f;
            moveInputY = moveInputY * 0.6f;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveInputX = moveInputX * 0.5f;
            moveInputY = moveInputY * 0.5f;
        }
        moveInput = new Vector2(moveInputX, moveInputY);

        isMoving = moveInput != Vector2.zero;
    }
    private void Movement()
    {
        rb.MovePosition(rb.position + moveInput * speed * Time.deltaTime);
    }
    private void SwitchAnimation()
    {
        foreach (var anim in animtors)
        {
            anim.SetBool("isMoving", isMoving);
            anim.SetFloat("mouseX", mouseX);
            anim.SetFloat("mouseY", mouseY);
            if (isMoving)
            {
                anim.SetFloat("InputX", moveInputX);
                anim.SetFloat("InputY", moveInputY);
            }
        }
    }
    private void OnBeforeSceneUnloadEvent()
    {
        inputDisable = true;
    }

    private void OnAfterSceneLoadedEvent()
    {
        inputDisable = false;
    }

    private void OnMoveToPosition(Vector3 targetPosition)
    {
        transform.position = targetPosition;
    }

    public GameSaveData GenerateGameSaveData()
    {
        GameSaveData saveData = new GameSaveData();
        saveData.characterPosDict = new Dictionary<string, SerializableVector3>();
        saveData.characterPosDict.Add(this.name, new SerializableVector3(transform.position));
        //Debug.Log(saveData.characterPosDict[this.name].x.ToString()+ saveData.characterPosDict[this.name].y.ToString()+ saveData.characterPosDict[this.name].z);
        return saveData;
    }

    public void RestoreGameData(GameSaveData gameSaveData)
    {
        var targetPosition = gameSaveData.characterPosDict[this.name].ToVector3();

        transform.position = targetPosition;
    }
}
