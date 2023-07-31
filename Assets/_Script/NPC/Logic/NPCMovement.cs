using System;
using System.Collections;
using System.Collections.Generic;
using MFarm.AStar;
using UnityEngine;
using UnityEngine.SceneManagement;
using MFarm.Save;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class NPCMovement : MonoBehaviour,ISaveable
{
    public ScheduleDataList_SO scheduleData;
    private SortedSet<ScheduleDetails> scheduleSet;
    private ScheduleDetails currentSchedule;
    //临时存储信息
    [SerializeField] private string currentScene;
    private string targetScene;
    private Vector3Int currentGridPosition;
    private Vector3Int targetGridPosition;

    public string StartScene { set => currentScene = value; }

    [Header("移动属性")]
    public float normalSpeed = 2f;
    private float minSpeed = 1;
    private float maxSpeed = 3;
    private Vector2 dir;
    public bool isMoving;
    private Vector3Int nextGridPosition;
    private Vector3 nextWorldPosition;
    

    //Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D coll;
    private Animator anim;

    private Grid grid;

    private Stack<MoveStep> MoveSteps;
    private Coroutine npcMovementCoroutine;

    private bool isInitialised;
    private bool npcMove;
    private bool sceneLoaded;
    public bool interactable;
    private bool isFirstLoad;
    private Season currentSeason;

    //动画计时器
    private float animationBreakTime;
    private bool canPlayStopAnimation;
    private AnimationClip stopAnimationClip;

    public AnimationClip blankAnimationClip;
    private AnimatorOverrideController animOverride;

    private TimeSpan GameTime => TimeManager.Instance.GameTime;

    public string GUID => GetComponent<DataGUID>().guid;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        MoveSteps = new Stack<MoveStep>();

        animOverride = new AnimatorOverrideController(anim.runtimeAnimatorController);
        anim.runtimeAnimatorController = animOverride;

        scheduleSet = new SortedSet<ScheduleDetails>();
        foreach (var schedule in scheduleData.scheduleList)
        {
            scheduleSet.Add(schedule);
        }
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.GameMinuteEvent += OnGameMinuteEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.GameMinuteEvent -= OnGameMinuteEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;

    }


    private void Start()
    {
        ISaveable saveable = this;
        saveable.RegisterSaveable();
    }
    private void Update()
    {
        if (sceneLoaded)
            SwitchAnimation();

        animationBreakTime -= Time.deltaTime;
        canPlayStopAnimation = animationBreakTime < 0;
    }
    private void FixedUpdate()
    {
        if (sceneLoaded)
            Movement();
    }
    private void OnStartNewGameEvent(int obj)
    {
        isInitialised = false;
        isFirstLoad = true;
    }
    private void OnEndGameEvent()
    {
        sceneLoaded = false;
        npcMove = false;
        if (npcMovementCoroutine != null)
            StopCoroutine(npcMovementCoroutine);
    }
    private void OnGameMinuteEvent(int minute, int hour,int day, Season season)
    {
        int time = (hour * 100) + minute;
        currentSeason = season;
        ScheduleDetails macthSchedule = null;
        foreach (var schedule in scheduleSet)
        {
            if(schedule.Time == time)
            {
                if (schedule.season != season)
                    continue;
                if (schedule.day != day && schedule.day != 0)
                    continue;
                macthSchedule = schedule;
            }
            else if(schedule.Time > time)
            {
                break;
            }
        }
        if (macthSchedule != null)
            BuildPath(macthSchedule);
    }

    private void OnBeforeSceneUnloadEvent()
    {
        sceneLoaded = false;
    }

    private void OnAfterSceneLoadedEvent()
    {
        grid = FindObjectOfType<Grid>();
        CheckVisiable();

        if (!isInitialised)
        {
            InitNPC();
            isInitialised = true;
        }
        sceneLoaded = true;

        if(!isFirstLoad)
        {
            currentGridPosition = grid.WorldToCell(transform.position);
            var schedule = new ScheduleDetails(0, 0, 0, 0, currentSeason, targetScene, (Vector2Int)targetGridPosition, stopAnimationClip, interactable);

            BuildPath(schedule);
            isFirstLoad = true;
        }
    }


    private void InitNPC()
    {
        targetScene = currentScene;

        //保存当前坐标的网格中心点
        currentGridPosition = grid.WorldToCell(transform.position);
        transform.position = new Vector3(currentGridPosition.x + Settings.gridCellSize / 2f, currentGridPosition.y + Settings.gridCellSize / 2f, 0);

        targetGridPosition = currentGridPosition;
    }
    /// <summary>
    /// 主要移动方法
    /// </summary>
    private void Movement()
    {
        if (!npcMove)
        {
            if (MoveSteps.Count > 0)
            {
                MoveStep step = MoveSteps.Pop();

                currentScene = step.sceneName;

                CheckVisiable();

                nextGridPosition = (Vector3Int)step.gridCoordinate;
                TimeSpan stepTime = new TimeSpan(step.hour, step.minute, step.second);

                MoveToGridPosition(nextGridPosition, stepTime);
            }
            else if(!isMoving && canPlayStopAnimation)
            {
                StartCoroutine(SetStopAnimation());
            }
        }
    }

    private void MoveToGridPosition(Vector3Int gridPos, TimeSpan stepTime)
    {
        npcMovementCoroutine = StartCoroutine(MoveRoutine(gridPos, stepTime));
    }

    private IEnumerator MoveRoutine(Vector3Int gridPos, TimeSpan stepTime)
    {
        npcMove = true;
        nextWorldPosition = GetWorldPostion(gridPos);

        //还有时间用来移动
        if (stepTime > GameTime)
        {
            //用来移动的时间差，以秒为单位
            float timeToMove = (float)(stepTime.TotalSeconds - GameTime.TotalSeconds);
            //实际移动距离
            float distance = Vector3.Distance(transform.position, nextWorldPosition);
            //实际移动速度
            float speed = Mathf.Max(minSpeed, (distance / timeToMove / Settings.secondThreshold));

            if (speed <= maxSpeed)
            {
                while (Vector3.Distance(transform.position, nextWorldPosition) > Settings.pixelSize)
                {
                    dir = (nextWorldPosition - transform.position).normalized;

                    Vector2 posOffset = new Vector2(dir.x * speed * Time.fixedDeltaTime, dir.y * speed * Time.fixedDeltaTime);
                    rb.MovePosition(rb.position + posOffset);
                    yield return new WaitForFixedUpdate();
                }
            }
        }
        //如果时间已经到了就瞬移
        rb.position = nextWorldPosition;
        currentGridPosition = gridPos;
        nextGridPosition = currentGridPosition;

        npcMove = false;
    }

    private Vector3 GetWorldPostion(Vector3Int gridPos)
    {
        Vector3 worldPos = grid.CellToWorld(gridPos);
        return new Vector3(worldPos.x + Settings.gridCellSize / 2f, worldPos.y + Settings.gridCellSize / 2f);
    }

    private void CheckVisiable()
    {
        if (currentScene == SceneManager.GetActiveScene().name)
            SetActiveInScene();
        else
            SetInactiveInScene();
    }
    /// <summary>
    /// 根据Schedule构建路径
    /// </summary>
    /// <param name="schedule"></param>
    public void BuildPath(ScheduleDetails schedule)
    {
        MoveSteps.Clear();
        currentSchedule = schedule;
        this.targetGridPosition = (Vector3Int)schedule.targetGridPosition;
        this.stopAnimationClip = schedule.clipAtStop;
        this.targetScene = schedule.targetScene;
        this.interactable = currentSchedule.interactable;

        if (schedule.targetScene == currentScene)
        {
            AStar.Instance.BuildPath(schedule.targetScene, (Vector2Int)currentGridPosition, schedule.targetGridPosition, MoveSteps);
        }
        else if(schedule.targetScene != currentScene)
        {
            SceneRoute sceneRoute = NPCManager.Instance.GetSceneRoute(currentScene, schedule.targetScene);

            if (sceneRoute != null)
            {
                for (int i = 0; i < sceneRoute.scenePathList.Count; i++)
                {
                    Vector2Int fromPos;
                    Vector2Int gotoPos;
                    ScenePath path = sceneRoute.scenePathList[i];

                    if (path.fromGridCell.x >= Settings.maxGridSize)
                    {
                        fromPos = (Vector2Int)currentGridPosition;
                    }
                    else
                    {
                        fromPos = path.fromGridCell;
                    }

                    if (path.gotoGridCell.x >= Settings.maxGridSize)
                    {
                        gotoPos = schedule.targetGridPosition;
                    }
                    else
                    {
                        gotoPos = path.gotoGridCell;
                    }

                    AStar.Instance.BuildPath(path.sceneName, fromPos, gotoPos, MoveSteps);
                }
            }
        }

        if (MoveSteps.Count > 1)
        {
            //更新每一步对应的时间戳
            UpdateTimeOnPath();
        }
    }


    private void UpdateTimeOnPath()
    {
        MoveStep previousSetp = null;

        TimeSpan currentGameTime = GameTime;

        foreach (MoveStep step in MoveSteps)
        {
            if (previousSetp == null)
                previousSetp = step;

            step.hour = currentGameTime.Hours;
            step.minute = currentGameTime.Minutes;
            step.second = currentGameTime.Seconds;

            TimeSpan gridMoveStepTime;

            if (MoveInDiagonal(step, previousSetp))
                gridMoveStepTime = new TimeSpan(0, 0, (int)(Settings.gridCellDiagonalSize / normalSpeed / Settings.secondThreshold));
            else
                gridMoveStepTime = new TimeSpan(0, 0, (int)(Settings.gridCellSize / normalSpeed / Settings.secondThreshold));

            //累加获得下一步的时间戳
            currentGameTime = currentGameTime.Add(gridMoveStepTime);
            //循环下一步
            previousSetp = step;
        }
    }

    /// <summary>
    /// 判断是否走斜方向
    /// </summary>
    /// <param name="currentStep"></param>
    /// <param name="previousStep"></param>
    /// <returns></returns>
    private bool MoveInDiagonal(MoveStep currentStep, MoveStep previousStep)
    {
        return (currentStep.gridCoordinate.x != previousStep.gridCoordinate.x) && (currentStep.gridCoordinate.y != previousStep.gridCoordinate.y);
    }

    private void SwitchAnimation()
    {
        isMoving = transform.position != GetWorldPostion(targetGridPosition);

        anim.SetBool("isMoving", isMoving);
        if (isMoving)
        {
            anim.SetBool("Exit", true);
            anim.SetFloat("dirX", dir.x);
            anim.SetFloat("dirY", dir.y);
        }
        else
        {
            anim.SetBool("Exit", false);
        }
    }
    private IEnumerator SetStopAnimation()
    {
        //强制面向镜头
        anim.SetFloat("dirX", 0f);
        anim.SetFloat("dirY", -1f);

        animationBreakTime = Settings.animationBreakTime;

        if (stopAnimationClip != null)
        {
            animOverride[blankAnimationClip] = stopAnimationClip;
            anim.SetBool("EventAnimation", true);
            yield return null;
            anim.SetBool("EventAnimation", false);
        }
        else
        {
            animOverride[stopAnimationClip] = blankAnimationClip;
            anim.SetBool("EventAnimation", false);
        }
    }
    #region 设置NPC显示情况
    private void SetActiveInScene()
    {
        spriteRenderer.enabled = true;
        coll.enabled = true;
        //TODO:影子关闭
        // transform.GetChild(0).gameObject.SetActive(true);
    }

    private void SetInactiveInScene()
    {
        spriteRenderer.enabled = false;
        coll.enabled = false;
        //TODO:影子关闭
        // transform.GetChild(0).gameObject.SetActive(false);
    }
    #endregion
    public GameSaveData GenerateGameSaveData()
    {
        GameSaveData saveData = new GameSaveData();
        saveData.characterPosDict = new Dictionary<string, SerializableVector3>();
        saveData.characterPosDict.Add("targetGridPosition", new SerializableVector3(targetGridPosition));
        saveData.characterPosDict.Add("currentPosition", new SerializableVector3(transform.position));
        //Debug.Log("currentPosition:" + new SerializableVector3(transform.position));
        saveData.dataSceneName = currentScene;
        saveData.targetScene = this.targetScene;
        saveData.interactable = this.interactable;
        saveData.timeDict = new Dictionary<string, int>();
        saveData.timeDict.Add("currentSeason", (int)currentSeason);

        return saveData;
    }

    public void RestoreGameData(GameSaveData saveData)
    {
        isInitialised = true;
        isFirstLoad = false;

        currentScene = saveData.dataSceneName;
        targetScene = saveData.targetScene;

        Vector3 pos = saveData.characterPosDict["currentPosition"].ToVector3();
        Vector3Int gridPos = (Vector3Int)saveData.characterPosDict["targetGridPosition"].ToVector2Int();

        transform.position = pos;
        targetGridPosition = gridPos;

        if (saveData.animationInstanceID != 0)
        {
            this.stopAnimationClip = Resources.InstanceIDToObject(saveData.animationInstanceID) as AnimationClip;
        }

        this.interactable = saveData.interactable;
        this.currentSeason = (Season)saveData.timeDict["currentSeason"];
    }

}