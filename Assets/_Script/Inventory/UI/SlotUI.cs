using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MFarm.Inventory
{
    public class SlotUI : MonoBehaviour,IPointerClickHandler,IBeginDragHandler,IDragHandler,IEndDragHandler
    {
        [Header("�����ȡ")]
        [SerializeField] private Image slotImage;
        [SerializeField] private TextMeshProUGUI amountText;
        public Image slotHightlight;
        [SerializeField] private Button button;
        [Header("��������")]
        public SlotType slotType;
        public bool isSelected;
        public int slotIndex;

        //��Ʒ��Ϣ
        public ItemDetails itemDetails;
        public int itemAmount;

        public InventoryUI inventoryUI => GetComponentInParent<InventoryUI>();
        public InventoryLocation location
        {
            get
            {
                return slotType switch
                {
                    SlotType.Bag => InventoryLocation.Player,
                    SlotType.Box => InventoryLocation.Box,
                    _ => InventoryLocation.Player
                };
            }
        }
        private void Start()
        {
            isSelected = false;
            if (itemDetails == null)
            {
                UpdateEmptySlot();
            }
        }

        /// <summary>
        /// ���¸���UI����Ϣ
        /// </summary>
        /// <param name="item">ItemDetails</param>
        /// <param name="amount">��������</param>
        public void UpdateSlot(ItemDetails item, int amount)
        {
            itemDetails = item;
            slotImage.sprite = item.itemIcon;
            slotImage.enabled = true;
            itemAmount = amount;
            amountText.text = amount.ToString();
            button.interactable = true;
        }

        /// <summary>
        /// ��Slot����Ϊ��
        /// </summary>
        public void UpdateEmptySlot()
        {
            if (isSelected)
            {
                isSelected = false;

                inventoryUI.UpdateSlotHighLight(-1);
                EventHandler.CallItemSelectedEvent(itemDetails, isSelected);
            }
            itemDetails = null;
            slotImage.enabled = false;
            amountText.text = string.Empty;
            button.interactable = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (itemDetails == null) return;
            isSelected = !isSelected;
            inventoryUI.UpdateSlotHighLight(slotIndex);

            if(slotType == SlotType.Bag)
            {
                EventHandler.CallItemSelectedEvent(itemDetails, isSelected);
            }
        }
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (itemAmount != 0)
            {
                inventoryUI.dragItem.enabled = true;
                inventoryUI.dragItem.sprite = slotImage.sprite;
                inventoryUI.dragItem.SetNativeSize();

                isSelected = true;
                inventoryUI.UpdateSlotHighLight(slotIndex);
            }
            
        }

        public void OnDrag(PointerEventData eventData)
        {
            inventoryUI.dragItem.transform.position = Input.mousePosition;
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            inventoryUI.dragItem.enabled = false;
            //Debug.Log(eventData.pointerCurrentRaycast.gameObject);

            if (eventData.pointerCurrentRaycast.gameObject != null)
            {
                if (eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>() == null)
                {
                    return;
                }
                var targetSlot = eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>();
                int targetIndex = targetSlot.slotIndex;

                //��player����Χ�ڽ�����Ʒ
                if(slotType == SlotType.Bag && targetSlot.slotType == SlotType.Bag)
                {
                    InventoryManager.Instance.SwapItem(slotIndex, targetIndex);
                }
                else if(slotType == SlotType.Shop && targetSlot.slotType == SlotType.Bag) //��
                {
                    EventHandler.CallShowTradeUI(itemDetails, false);
                }
                else if(slotType == SlotType.Bag && targetSlot.slotType == SlotType.Shop)   //��
                {
                    EventHandler.CallShowTradeUI(itemDetails, true);
                }
                else if(slotType != SlotType.Shop && targetSlot.slotType != SlotType.Shop && slotType != targetSlot.slotType)
                {
                    //�米�����ݽ�����Ʒ
                    InventoryManager.Instance.SwapItem(location, slotIndex, targetSlot.location, targetSlot.slotIndex);
                }


                //������и�����ʾ
                inventoryUI.UpdateSlotHighLight(-1);
            }
            //else    //�������ڵ���
            //{
            //    if (itemDetails.canDropped)
            //    {
            //        //����Ӧ�����ͼ����
            //        var pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));

            //        EventHandler.CallInstantiateItemInScene(itemDetails.itemID, pos);
            //    }
            //}
        }

    }

}