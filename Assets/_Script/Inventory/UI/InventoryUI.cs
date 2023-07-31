using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MFarm.Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        public ItemToolTip itemToolTip;
        [Header("拖拽图片")]
        public Image dragItem;
        [Header("玩家背包UI")]
        [SerializeField] private GameObject bagUI;
        [SerializeField] private SlotUI[] playerSlots;

        [Header("通用背包")]
        [SerializeField] private GameObject baseBag;
        public GameObject shopSlotPrefab;
        public GameObject boxSlotPrefab;

        [Header("交易UI")]
        public TradeUI tradeUI;
        public TextMeshProUGUI playermoneyText;

        [SerializeField] private SlotUI[] playrSlots;
        [SerializeField] private List<SlotUI> baseBagSlots;

        private bool bagOpened;
        private void OnEnable()
        {
            EventHandler.UpdateInventoryUI += OnUpdateInventoryUI;
            EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
            EventHandler.BaseBagOpenEvent += OnBaseBagOpenEvent;
            EventHandler.BaseBagCloseEvent += OnBaseBagCloseEvent;
            EventHandler.ShowTradeUI += OnShowTradeUI;
        }

        private void OnDisable()
        {
            EventHandler.UpdateInventoryUI -= OnUpdateInventoryUI;
            EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
            EventHandler.BaseBagOpenEvent -= OnBaseBagOpenEvent;
            EventHandler.BaseBagCloseEvent -= OnBaseBagCloseEvent;
            EventHandler.ShowTradeUI -= OnShowTradeUI;
        }

        /// <summary>
        /// 打开通用包裹事件
        /// </summary>
        /// <param name="slotType"></param>
        /// <param name="bagData"></param>
        private void OnBaseBagOpenEvent(SlotType slotType, InventoryBag_SO bagData)
        {
            //TODO: 通用箱子prefab
            GameObject prefab = slotType switch
            {
                SlotType.Shop => shopSlotPrefab,
                SlotType.Box => boxSlotPrefab,
                _ => null,
            };

            baseBag.SetActive(true);

            baseBagSlots = new List<SlotUI>();

            for(int i = 0; i < bagData.itemList.Count; i++)
            {
                var slot = Instantiate(prefab, baseBag.transform.GetChild(1)).GetComponent<SlotUI>();
                slot.slotIndex = i;
                baseBagSlots.Add(slot);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(baseBag.GetComponent<RectTransform>());

            if(slotType == SlotType.Shop)
            {
                bagUI.GetComponent<RectTransform>().localPosition = new Vector3(200, 0);
                bagUI.SetActive(true);
                bagOpened = true;
            }

            //更新UI显示
            OnUpdateInventoryUI(InventoryLocation.Box, bagData.itemList);
        }
        /// <summary>
        /// 关闭通用包裹事件
        /// </summary>
        /// <param name="slotType"></param>
        /// <param name="bagData"></param>
        private void OnBaseBagCloseEvent(SlotType slotType, InventoryBag_SO bagData)
        {
            baseBag.SetActive(false);
            itemToolTip.gameObject.SetActive(false);
            UpdateSlotHighLight(-1);

            foreach (var slot in baseBagSlots)
            {
                Destroy(slot.gameObject);
            }
            baseBagSlots.Clear();

            if (slotType == SlotType.Shop)
            {
                bagUI.GetComponent<RectTransform>().localPosition = new Vector3(0, 0);
                bagUI.SetActive(false);
                bagOpened = false;
            }
        }
        private void OnShowTradeUI(ItemDetails item, bool isSell)
        {
            tradeUI.gameObject.SetActive(true);
            tradeUI.SetupTradeUI(item, isSell);
        }
        private void Start()
        {
            //给每个格子附上索引值
            for (int i = 0; i < playerSlots.Length; i++)
            {
                playerSlots[i].slotIndex = i;
            }

            bagOpened = bagUI.activeInHierarchy;
            playermoneyText.text = InventoryManager.Instance.playerMoney.ToString();
        }
        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.B))
            {
                OpenBagUI();
            }
        }
        private void OnBeforeSceneUnloadEvent()
        {
            UpdateSlotHighLight(-1);
        }
        private void OnUpdateInventoryUI(InventoryLocation location, List<InventoryItem> list)
        {
            switch (location)
            {
                case InventoryLocation.Player:
                    for (int i = 0; i < playerSlots.Length; i++)
                    {
                        if (list[i].itemAmount > 0)
                        {
                            var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
                            playerSlots[i].UpdateSlot(item, list[i].itemAmount);
                        }
                        else
                        {
                            playerSlots[i].UpdateEmptySlot();
                        }
                    }
                    break;
                case InventoryLocation.Box:
                    for (int i = 0; i < baseBagSlots.Count; i++)
                    {
                        if (list[i].itemAmount > 0)
                        {
                            var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
                            baseBagSlots[i].UpdateSlot(item, list[i].itemAmount);
                        }
                        else
                        {
                            baseBagSlots[i].UpdateEmptySlot();
                        }
                    }
                    break;
            }

            playermoneyText.text = InventoryManager.Instance.playerMoney.ToString();
        }
        public void OpenBagUI()
        {
            bagOpened = !bagOpened;

            bagUI.SetActive(bagOpened);
        }
        /// <summary>
        /// 更新高亮显示
        /// </summary>
        /// <param name="index">索引</param>
        public void UpdateSlotHighLight(int index)
        {
            foreach(var slot in playerSlots)
            {
                if(slot.isSelected && slot.slotIndex == index)
                {
                    slot.slotHightlight.gameObject.SetActive(true);
                }
                else
                {
                    slot.isSelected = false;
                    slot.slotHightlight.gameObject.SetActive(false);
                }
            }
        }
    }
}

