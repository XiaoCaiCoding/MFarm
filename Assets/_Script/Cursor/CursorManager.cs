using MFarm.CropPlant;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MFarm.Map;
using MFarm.Inventory;

public class CursorManager : MonoBehaviour
{
    public Sprite normal, tool, seed, item;

    private Sprite currentSprite; //储存当前图片
    private Image cursorImage;
    private RectTransform cursorCanvas;

    private Image buildImage;

    private Camera mainCamera;
    private Grid currentGrid;

    private Vector3 mouseWorldPos;
    private Vector3Int mouseGridPos;

    private bool cursorEnable;
    //鼠标位置是否可用（工具或种子）
    private bool cursorPositionValid;

    private ItemDetails currentItem;

    private Transform PlayerTransform => FindObjectOfType<Player>().transform;
    private void OnEnable()
    {
        EventHandler.ItemSelectedEvent += OnItemSelectedEvent;
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
    }
    private void OnDisable()
    {
        EventHandler.ItemSelectedEvent -= OnItemSelectedEvent;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
    }


    private void Start()
    {
        cursorCanvas = GameObject.FindGameObjectWithTag("CursorCanvas").GetComponent<RectTransform>();
        cursorImage = cursorCanvas.GetChild(0).GetComponent<Image>();
        //拿到建造图标
        buildImage = cursorCanvas.GetChild(1).GetComponent<Image>();
        buildImage.gameObject.SetActive(false);


        currentSprite = normal;
        SetCursorImage(currentSprite);

        mainCamera = Camera.main;
    }
    private void Update()
    {
        if (cursorCanvas == null) return;

        cursorImage.transform.position = Input.mousePosition;

        if (!InteractWithUI() && cursorEnable)
        {
            SetCursorImage(currentSprite);
            CheckCursorValid();
            CheckPlayerInput();
        }
        else
        {
            SetCursorImage(normal);
            buildImage.gameObject.SetActive(false);
        }
    }
    #region 设置鼠标样式
    /// <summary>
    /// 设置鼠标图片
    /// </summary>
    /// <param name="sprite">选择要切换的图片</param>
    private void SetCursorImage(Sprite sprite)
    {
        cursorImage.sprite = sprite;
        cursorImage.color = new Color(1, 1, 1, 1);
    }
    /// <summary>
    /// 鼠标可用
    /// </summary>
    private void SetCursorValid()
    {
        cursorPositionValid = true;
        cursorImage.color = new Color(1, 1, 1, 1);
        buildImage.color = new Color(1, 1, 1, 0.5f);
    }
    /// <summary>
    /// 鼠标不可用
    /// </summary>
    private void SetCursorInValid()
    {
        cursorPositionValid = false;
        cursorImage.color = new Color(1, 0, 0, 0.4f);
        buildImage.color = new Color(1, 0, 0, 0.5f);
    }
    #endregion
    private void CheckCursorValid()
    {
        mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z)); ;
        mouseGridPos = currentGrid.WorldToCell(mouseWorldPos);
        var playerGridPos = currentGrid.WorldToCell(PlayerTransform.position);
        //Debug.Log("mouseWorldPos:" + mouseWorldPos);
        //Debug.Log("mouseGridPos:" + mouseWorldPos + "playerGridPos:" + playerGridPos);
        //建造图片跟随移动
        buildImage.rectTransform.position = Input.mousePosition;

        //TODO:判断在使用范围内
        if(Mathf.Abs(mouseGridPos.x-playerGridPos.x)>currentItem.itemUseRadius|| Mathf.Abs(mouseGridPos.y - playerGridPos.y) > currentItem.itemUseRadius)
        {
            SetCursorInValid();
            return;
        }

        //Debug.Log("WorldPos:" + mouseWorldPos + "GridPos:" + mouseGridPos);
        TileDetails currentTile = GridMapManager.Instance.GetTileDetailsOnMousePosition(mouseGridPos);

        //Debug.Log(currentTile);
        if (currentTile != null)
        {
            CropDetails currentCrop = CropManager.Instance.GetCropDetails(currentTile.seedItemID);
            Crop crop = GridMapManager.Instance.GetCropObject(mouseWorldPos);
            //TODO: workflow 补充所有物品类型的判断
            //Debug.Log("CanDropItem:"+currentTile.canDropItem);
            switch (currentItem.itemType)
            {
                case ItemType.Seed:
                    if (currentTile.daysSinceDug > -1 && currentTile.seedItemID == -1) SetCursorValid(); else SetCursorInValid();
                        break;
                case ItemType.Commodity:
                    if (currentTile.canDropItem && currentItem.canDropped) SetCursorValid(); else SetCursorInValid();
                    break;
                case ItemType.HoeTool:
                    if (currentTile.canDig) SetCursorValid(); else SetCursorInValid();
                    break;
                case ItemType.WaterTool:
                    if (currentTile.daysSinceDug > -1 && currentTile.daysSinceWatered == -1) SetCursorValid(); else SetCursorInValid();
                    break;
                case ItemType.BreakTool:
                case ItemType.ChopTool:
                    if (crop != null)
                    {
                        if (crop.CanHavest && crop.cropDetails.CheckToolAvailable(currentItem.itemID)) SetCursorValid(); else SetCursorInValid();
                    }
                    else
                        SetCursorInValid();
                    break;

                case ItemType.CollectTool:
                    if (currentCrop != null)
                    {
                        if(currentCrop.CheckToolAvailable(currentItem.itemID))
                            if (currentTile.growthDays >= currentCrop.TotalGrowthDays) SetCursorValid(); else SetCursorInValid();
                    }
                    else
                        SetCursorInValid();
                    break;
                case ItemType.ReapTool:
                    
                    if(GridMapManager.Instance.HaveReapableItemsInRadius(mouseWorldPos, currentItem)) SetCursorValid(); else SetCursorInValid();
                    break;

                case ItemType.Furniture:
                    buildImage.gameObject.SetActive(true);    //需要添加此命令
                    var bluePrintDetails = InventoryManager.Instance.bluePrintData.GetBluePrintDetails(currentItem.itemID);

                    if (currentTile.canPlaceFurniture && InventoryManager.Instance.CheckStock(currentItem.itemID) && !HaveFurnitureInRadius(bluePrintDetails))
                        SetCursorValid();
                    else
                        SetCursorInValid();
                    break;
            }
        }
        else
        {
            SetCursorInValid();
        }
    }
    private void CheckPlayerInput()
    {
        if (Input.GetMouseButtonDown(0) && cursorPositionValid)
        {
            //Debug.Log("已按下左键");
            //Debug.Log("当前物品ID："+currentItem.itemID);
            //执行方法
            EventHandler.CallMouseClickedEvent(mouseWorldPos, currentItem);
        }
    }
    private bool InteractWithUI()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }
        return false;
    }
    private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
    {
        if (!isSelected)
        {
            currentItem = null;
            cursorEnable = false;
            currentSprite = normal;
        }
        else
        {
            currentItem = itemDetails;
            //TODO: WOKEFLOW: 添加所有类型对应图片
            currentSprite = itemDetails.itemType switch
            {
                ItemType.Seed => seed,
                ItemType.Commodity => item,
                ItemType.ChopTool => tool,
                ItemType.BreakTool => tool,
                ItemType.HoeTool => tool,
                ItemType.ReapTool => tool,
                ItemType.Furniture => tool,
                ItemType.CollectTool =>tool,

                _ => normal
            };
            cursorEnable = true;

            if(itemDetails.itemType == ItemType.Furniture)
            {
                buildImage.gameObject.SetActive(true);
                buildImage.sprite = itemDetails.itemOnWorldSprite;
                buildImage.SetNativeSize();
            }
        }
    }
    private bool HaveFurnitureInRadius(BluePrintDetails bluePrintDetails)
    {
        //var collider = Physics2D.OverlapPoint(mouseWorldPos);
        //if(collider.gameObject.GetComponent<Furniture>()!=null)
        //{
        //    //Debug.Log(collider.gameObject.name);
        //    return true;
        //}
        //return false;
        var buildItem = bluePrintDetails.buildPrefab;
        Vector2 point = mouseWorldPos;
        var size = buildItem.GetComponent<BoxCollider2D>().size;

        var otherColl = Physics2D.OverlapBox(point, size, 0);
        if (otherColl != null)
            return otherColl.GetComponent<Furniture>();
        return false;
    }
    private void OnBeforeSceneUnloadEvent()
    {
        cursorEnable = false;
    }

    private void OnAfterSceneLoadedEvent()
    {
        currentGrid = FindObjectOfType<Grid>();
    }
}
