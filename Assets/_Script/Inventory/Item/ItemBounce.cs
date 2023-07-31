using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFarm.Inventory
{
    public class ItemBounce : MonoBehaviour
    {
        private Transform spriteTrans;
        private BoxCollider2D coll;
        public float gravity = 3.5f;

        private bool isGround;
        private float distance;
        private Vector2 direction;
        private Vector3 targetPos;

        private void Awake()
        {
            spriteTrans = transform.GetChild(0);
            coll = GetComponent<BoxCollider2D>();
            coll.enabled = false;
        }
        private void Update()
        {
            Bounce();
        }
        /// <summary>
        /// 初始化扔出物品的目标和方向
        /// </summary>
        /// <param name="target">目标坐标</param>
        /// <param name="dir">指向目标的方向</param>
        public void InitBounceItem(Vector3 target,Vector2 dir)
        {
            coll.enabled = false;
            targetPos = target;
            direction = dir;
            distance = Vector3.Distance(target, transform.position);

            spriteTrans.position += Vector3.up * 1.5f;
        }
        private void Bounce()
        {
            isGround = spriteTrans.position.y <= transform.position.y;

            //父物体往目标位置移动（在Inspector中设置重力为负数)
            if (Vector3.Distance(transform.position, targetPos) > 0.1f)
            {
                transform.position += (Vector3)direction * distance * -gravity * Time.deltaTime;    //方向*距离*重力加速度*时间间隔
            }

            if (!isGround)
            {
                spriteTrans.position += Vector3.up * gravity * Time.deltaTime;  //方向*距离*重力加速度*时间间隔
            }
            else
            {
                spriteTrans.position = transform.position;
                coll.enabled = true;
            }
        }
    }
}

