using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crop : MonoBehaviour
{

    public CropDetails cropDetails;
    public TileDetails tileDetails;
    private int harvestActionCount;

    public bool CanHavest => tileDetails.growthDays >= cropDetails.TotalGrowthDays;

    private Animator anim;
    private Transform PlayerTransform => FindObjectOfType<Player>().transform;


    public void ProcessToolAction(ItemDetails tool,TileDetails tile)
    {
        tileDetails = tile;

        //����ʹ�ô���
        int requireActionCount = cropDetails.GetTotalRequireCount(tool.itemID);
        if (requireActionCount == -1) return;

        anim = GetComponentInChildren<Animator>();

        //���������
        if (harvestActionCount < requireActionCount)
        {
            harvestActionCount++;

            //�ж��Ƿ��ж��� ��ľ
            if (anim != null)
            {
                if (PlayerTransform.transform.position.x < transform.position.x)
                    anim.SetTrigger("RotateRight");
                else
                    anim.SetTrigger("RotateLeft");
            }
            //��������
            if(cropDetails.hasParticalEffect)
                EventHandler.CallParticleEffectEvent(cropDetails.effectType,transform.position + cropDetails.effectPos);

            if (cropDetails.soundEffect != SoundName.none)
            {
                EventHandler.CallPlaySoundEvent(cropDetails.soundEffect);
            }
        }

        if (harvestActionCount >= requireActionCount)
        {
            if (cropDetails.generateAtPlayerPosition || !cropDetails.hasAnimation)
            {
                //����ũ����
                SpawnHarvestItems();
            }
            else if (cropDetails.hasAnimation)
            {
                if (PlayerTransform.transform.position.x < transform.position.x)
                    anim.SetTrigger("FallingRight");
                else
                    anim.SetTrigger("FallingLeft");
                EventHandler.CallPlaySoundEvent(SoundName.TreeFalling);
                StartCoroutine(HarvestAfterAnimation());
            }
        }
    }
    private IEnumerator HarvestAfterAnimation()
    {
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName("END"))
        {
            yield return null;
        }

        SpawnHarvestItems();
        //ת��������
        if (cropDetails.transferItemID > 0)
        {
            CreateTransferCrop();
        }
    }
    private void CreateTransferCrop()
    {
        tileDetails.seedItemID = cropDetails.transferItemID;
        tileDetails.daysSinceLastHarvest = -1;
        tileDetails.growthDays = 0;
        
        EventHandler.CallRefreshCurrentMap();
    }
    public void SpawnHarvestItems()
    {
        for (int i = 0; i < cropDetails.producedItemID.Length; i++)
        {
            int amountToProduce;

            if (cropDetails.producedMinAmount[i] == cropDetails.producedMaxAmount[i])
            {
                //����ֻ����ָ��������
                amountToProduce = cropDetails.producedMinAmount[i];
            }
            else    //��Ʒ�������
            {
                amountToProduce = Random.Range(cropDetails.producedMinAmount[i], cropDetails.producedMaxAmount[i] + 1);
            }

            //ִ������ָ����������Ʒ
            for (int j = 0; j < amountToProduce; j++)
            {
                if (cropDetails.generateAtPlayerPosition)
                {
                    EventHandler.CallHarvestAtPlayerPosition(cropDetails.producedItemID[i]);
                }
                else    //�����ͼ��������Ʒ
                {
                    var dirX = transform.position.x > PlayerTransform.position.x ? 1 : -1;

                    var spawnPos = new Vector3(transform.position.x + Random.Range(dirX, cropDetails.spawnRadius.x * dirX),
                        transform.position.y+Random.Range(-cropDetails.spawnRadius.y,cropDetails.spawnRadius.y),0);

                    EventHandler.CallInstantiateItemInScene(cropDetails.producedItemID[i], spawnPos);
                }
            }
        }

        if (tileDetails != null)
        {
            tileDetails.daysSinceLastHarvest++;

            //�Ƿ�����ظ�����
            if (cropDetails.daysToRegrow > 0 && tileDetails.daysSinceLastHarvest < cropDetails.regrowTimes - 1)
            {
                tileDetails.growthDays = cropDetails.TotalGrowthDays - cropDetails.daysToRegrow;
                //ˢ������
                EventHandler.CallRefreshCurrentMap();
            }
            else    //�����ظ�����
            {
                tileDetails.daysSinceLastHarvest = -1;
                tileDetails.seedItemID = -1;
                tileDetails.canDig = true;
                //FIXME:�Լ����
                // tileDetails.daysSinceDug = -1; //�ջ����ʵ�󣬿���ʧ
                tileDetails.daysSinceDug = 0;   //ˢ�¿ӵ�ʱ��
            }

            Destroy(gameObject);
        }
    }
}