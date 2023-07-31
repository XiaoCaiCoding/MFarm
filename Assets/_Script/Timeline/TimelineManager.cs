using UnityEngine;
using UnityEngine.Playables;
public class TimelineManager : Singleton<TimelineManager>
{
    public PlayableDirector startDirector;
    public PlayableDirector currentDirector;
    private bool isDone;
    public bool IsDone { set => isDone = value; }
    private bool isPause;

    protected override void Awake()
    {
        base.Awake();
        currentDirector = startDirector;
    }
    private void OnEnable()
    {
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
    }



    private void OnDisable()
    {
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
    }
    private void OnStartNewGameEvent(int obj)
    {
        currentDirector = FindObjectOfType<PlayableDirector>();
        if (currentDirector != null)
            currentDirector.Play();
    }


    private void Update()
    {
        if(isPause && (Input.GetKeyDown(KeyCode.Space)|| Input.GetMouseButtonDown(0)) && isDone)
        {
            isPause = false;
            currentDirector.playableGraph.GetRootPlayable(0).SetSpeed(1d);
        }
    }
    public void PauseTimeline(PlayableDirector director)
    {
        currentDirector = director;
        currentDirector.playableGraph.GetRootPlayable(0).SetSpeed(0d);

        isPause = true;
    }
}
