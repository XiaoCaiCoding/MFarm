using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System;
using System.Linq;

public class ItemEditor : EditorWindow
{
    private ItemDataList_SO database;
    private List<ItemDetails> itemList = new List<ItemDetails>();
    private List<ItemDetails> afterSeachItemList = new List<ItemDetails>();
    private VisualTreeAsset itemRowTemplate;

    private ScrollView itemDetailsSection;
    private ItemDetails activeItem;
    private ItemType itemTypeForSearch = ItemType.Seed;

    //默认预览图片
    private Sprite defaultIcon;

    private VisualElement iconPreview;

    private ListView itemListView;

    [MenuItem("M Studio/ItemEditor")]
    public static void ShowExample()
    {
        ItemEditor wnd = GetWindow<ItemEditor>();
        wnd.titleContent = new GUIContent("ItemEditor");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        //VisualElement label = new Label("Hello World! From C#");
        //root.Add(label);

        //// Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UIBuilder/ItemEditor.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();
        root.Add(labelFromUXML);

        //变量赋值
        itemListView = root.Q<VisualElement>("ItemList").Q<ListView>("ListView");
        itemDetailsSection = root.Q<ScrollView>("ItemDetails");
        iconPreview = itemDetailsSection.Q<VisualElement>("Icon");

        //根据类型搜索物品
        root.Q<VisualElement>("ItemList").Q<EnumField>("SearchByType").Init(itemTypeForSearch);
        root.Q<VisualElement>("ItemList").Q<EnumField>("SearchByType").RegisterValueChangedCallback(evt =>
        {
            itemTypeForSearch = (ItemType)evt.newValue;
            itemListView.itemsSource = SearchByItemType(itemTypeForSearch);
            Action<VisualElement, int> bindItem = (e, i) =>
            {
                if (i < afterSeachItemList.Count)
                {
                    if (afterSeachItemList[i].itemIcon != null)
                        e.Q<VisualElement>("Icon").style.backgroundImage = afterSeachItemList[i].itemIcon.texture;
                    e.Q<Label>("Name").text = afterSeachItemList[i].itemName == null ? "NO ITEM" : afterSeachItemList[i].itemName;
                }
            };
            itemListView.bindItem = bindItem;
            itemListView.Rebuild();
        });

        //获得按键
        root.Q<Button>("AddButton").clicked += OnAddItemClicked;
        root.Q<Button>("DeleteButton").clicked += OnDeleteClicked;

        //拿到模板数据
        itemRowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UIBuilder/ItemRowTemplate.uxml");

        //拿默认Icon图片
        defaultIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/M Studio/Art/Items/Icons/icon_M.png");

        //加载数据
        LoadDataBase();

        GenerateListView();

    }
    #region 按键事件
    private void OnDeleteClicked()
    {
        itemList.Remove(activeItem);
        afterSeachItemList.Remove(activeItem);
        itemListView.Rebuild();
        itemDetailsSection.visible = false;

    }

    private void OnAddItemClicked()
    {
        ItemDetails newItem = new ItemDetails();
        newItem.itemName = "NEW ITEM";
        newItem.itemID = 1001 + itemList.Count;
        newItem.itemType = itemTypeForSearch;
        itemList.Add(newItem);
        afterSeachItemList.Add(newItem);
        itemListView.Rebuild();
    }
    #endregion

    /// <summary>
    /// 读取物品数据库
    /// </summary>
    private void LoadDataBase()
    {
        var dataArray = AssetDatabase.FindAssets("t:ItemDataList_SO");

        if (dataArray.Length > 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(dataArray[0]);
            database = AssetDatabase.LoadAssetAtPath(path, typeof(ItemDataList_SO)) as ItemDataList_SO;
        }

        itemList = database.itemDetailsList;

        //如果不标记则无法保存数据
        EditorUtility.SetDirty(database);

        //Debug.Log(itemList[0].itemID);
    }
    private void GenerateListView()
    {
        Func<VisualElement> makeItem = () => itemRowTemplate.CloneTree();
        Action<VisualElement, int> bindItem = (e, i) =>
         {
             if (i < itemList.Count)
             {
                 if (itemList[i].itemIcon != null)
                     e.Q<VisualElement>("Icon").style.backgroundImage = itemList[i].itemIcon.texture;
                 e.Q<Label>("Name").text = itemList[i].itemName == null ? "NO ITEM" : itemList[i].itemName;
             }
         };
        itemListView.fixedItemHeight = 60;
        itemListView.itemsSource = itemList;
        itemListView.makeItem = makeItem;
        itemListView.bindItem = bindItem;

        itemListView.onSelectionChange += OnListSelectionChange;
    }
    private void OnListSelectionChange(IEnumerable<object> selectedItem)
    {
        activeItem = (ItemDetails)selectedItem.First();
        GetItemDetails();
        itemDetailsSection.visible = true;
    }
    private void GetItemDetails()
    {
        itemDetailsSection.MarkDirtyRepaint();

        itemDetailsSection.Q<IntegerField>("ItemID").value = activeItem.itemID;
        itemDetailsSection.Q<IntegerField>("ItemID").RegisterValueChangedCallback(evt => {
            activeItem.itemID = evt.newValue;
        });

        itemDetailsSection.Q<TextField>("ItemName").value = activeItem.itemName;
        itemDetailsSection.Q<TextField>("ItemName").RegisterValueChangedCallback(evt =>
        {
            activeItem.itemName = evt.newValue;
            itemListView.Rebuild();
        });

        iconPreview.style.backgroundImage = activeItem.itemIcon == null ? defaultIcon.texture : activeItem.itemIcon.texture;
        itemDetailsSection.Q<ObjectField>("ItemIcon").value = activeItem.itemIcon;
        itemDetailsSection.Q<ObjectField>("ItemIcon").RegisterValueChangedCallback(evt =>
        {
            Sprite newIcon = evt.newValue as Sprite;
            activeItem.itemIcon = newIcon;
            iconPreview.style.backgroundImage = newIcon == null ? defaultIcon.texture : newIcon.texture;
            itemListView.Rebuild();
        });

        //其他所有变量的绑定
        itemDetailsSection.Q<ObjectField>("ItemSprite").value = activeItem.itemOnWorldSprite;
        itemDetailsSection.Q<ObjectField>("ItemSprite").RegisterValueChangedCallback(evt =>
        {
            activeItem.itemOnWorldSprite = (Sprite)evt.newValue;
        });

        itemDetailsSection.Q<EnumField>("ItemType").Init(activeItem.itemType);
        itemDetailsSection.Q<EnumField>("ItemType").value = activeItem.itemType;
        itemDetailsSection.Q<EnumField>("ItemType").RegisterValueChangedCallback(evt =>
        {
            activeItem.itemType = (ItemType)evt.newValue;
        });

        itemDetailsSection.Q<TextField>("Description").value = activeItem.itemDescription;
        itemDetailsSection.Q<TextField>("Description").RegisterValueChangedCallback(evt =>
        {
            activeItem.itemDescription = evt.newValue;
        });

        itemDetailsSection.Q<IntegerField>("ItemUseRadius").value = activeItem.itemUseRadius;
        itemDetailsSection.Q<IntegerField>("ItemUseRadius").RegisterValueChangedCallback(evt =>
        {
            activeItem.itemUseRadius = evt.newValue;
        });

        itemDetailsSection.Q<Toggle>("CanPickedup").value = activeItem.canPickedup;
        itemDetailsSection.Q<Toggle>("CanPickedup").RegisterValueChangedCallback(evt =>
        {
            activeItem.canPickedup = evt.newValue;
        });

        itemDetailsSection.Q<Toggle>("CanDropped").value = activeItem.canDropped;
        itemDetailsSection.Q<Toggle>("CanDropped").RegisterValueChangedCallback(evt =>
        {
            activeItem.canDropped = evt.newValue;
        });

        itemDetailsSection.Q<Toggle>("CanCarried").value = activeItem.canCarried;
        itemDetailsSection.Q<Toggle>("CanCarried").RegisterValueChangedCallback(evt =>
        {
            activeItem.canCarried = evt.newValue;
        });

        itemDetailsSection.Q<IntegerField>("Price").value = activeItem.itemPrice;
        itemDetailsSection.Q<IntegerField>("Price").RegisterValueChangedCallback(evt =>
        {
            activeItem.itemPrice = evt.newValue;
        });

        itemDetailsSection.Q<Slider>("SellPercentage").value = activeItem.sellPercentage;
        itemDetailsSection.Q<Slider>("SellPercentage").RegisterValueChangedCallback(evt =>
        {
            activeItem.sellPercentage = evt.newValue;
        });
    }
    private List<ItemDetails> SearchByItemType(ItemType itemType)
    {
        afterSeachItemList.Clear();
        foreach(var item in itemList)
        {
            if(item.itemType == itemType)
            {
                afterSeachItemList.Add(item);
            }
        }
        return afterSeachItemList;
    }
}