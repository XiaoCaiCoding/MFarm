using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFarm.Inventory
{
    public class Box : MonoBehaviour
    {
        public InventoryBag_SO boxBagTemplate;
        public InventoryBag_SO boxBagData;

        public GameObject mouseIcon;
        private bool canOpen = false;
        private bool isOpen;
        public int index;
        private void OnEnable()
        {
            if(boxBagData == null)
            {
                boxBagData = Instantiate(boxBagTemplate);
            }
            //var key = this.name + index;
            //if (InventoryManager.Instance.GetBoxDataList(key) != null)
            //{
            //    boxBagData.itemList = InventoryManager.Instance.GetBoxDataList(key);
            //}
            ////新建箱子
            //else
            //{
            //    if (index == 0)
            //        index = InventoryManager.Instance.BoxDataAmount;
            //    InventoryManager.Instance.AddBoxDataDict(this);
            //}
            
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                canOpen = true;
                mouseIcon.SetActive(true);
            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                canOpen = false;
                mouseIcon.SetActive(false);
            }
        }
        private void Update()
        {
            if(canOpen&& !isOpen && Input.GetMouseButtonDown(1))
            {
                //打开箱子
                EventHandler.CallBaseBagOpenEvent(SlotType.Box, boxBagData);
                isOpen = true;
            }
            if(isOpen&& !canOpen)
            {
                EventHandler.CallBaseBagCloseEvent(SlotType.Box, boxBagData);
                isOpen = false;
            }
            if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                EventHandler.CallBaseBagCloseEvent(SlotType.Box, boxBagData);
                isOpen = false;
            }
        }

        public void InitBox(int boxIndex)
        {
            index = boxIndex;
            var key = this.name + index;
            if (InventoryManager.Instance.GetBoxDataList(key) != null)  //刷新地图刷新数据
            {
                boxBagData.itemList = InventoryManager.Instance.GetBoxDataList(key);
            }
            //新建箱子
            else
            {
                InventoryManager.Instance.AddBoxDataDict(this);
            }
        }
    }
}

