using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(CanvasGroup),typeof(RectTransform))]
    internal class UICellPool : MonoBehaviour
    {
        //模板
        private Dictionary<string,UITableCell> cellTemplates = new Dictionary<string,UITableCell>();

        //类型池子
        private Dictionary<string, Queue<UITableCell>> pools = new Dictionary<string, Queue<UITableCell>>();

        private void Awake()
        {
            var canvasGroup = this.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.ignoreParentGroups = true;
            
            var rect = this.GetComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.anchorMin = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        //添加模板到缓存池子中
        public void Add(UITableCell cellTemplate , int initCreateCount)
        {
#if UNITY_EDITOR
            if (string.IsNullOrWhiteSpace(cellTemplate.Id))
            {
                Debug.LogError("填入的 UIScrollCell 模板 的 id 错误:" + cellTemplate.Id);
            }

            if (cellTemplates.ContainsValue(cellTemplate))
            {
                Debug.LogError("填入的 UIScrollCell 模板 的 id 错误,多个相同 : " + cellTemplate.Id);
            }
#endif
            cellTemplates.Add(cellTemplate.Id,cellTemplate);
            for (int i = 0; i < initCreateCount; i++)
            {
                var cell = InstantiateCell(cellTemplate,this.transform);
                if (!pools.TryGetValue(cellTemplate.Id, out var pool))
                {
                    pool = new Queue<UITableCell>(initCreateCount);
                }
                if (!pool.Contains(cell))
                {
                    pool.Enqueue(cell);
                }
                pools[cellTemplate.Id] = pool;
            }
        }

        //回收
        public void Recycle(UITableCell cell)
        {
            if (!pools.TryGetValue(cell.Id, out var pool))
            {
                pool = new Queue<UITableCell>(3);
            }

            if (pool.Contains(cell)) return;
            cell.transform.SetParent(transform);
            pool.Enqueue(cell);
        }
        
        //将池子里面的数据全部清除
        public void DestroyAll()
        {
            foreach (var item in pools.Values)
            {
                foreach (var cell in item)
                {
                    Object.DestroyImmediate(cell.gameObject);
                }
                item.Clear();
            }
            pools.Clear();
        }

        //获取一个 cell
        public UITableCell Query(string id)
        {
            UITableCell cell = null;
            if (pools.TryGetValue(id, out var pool) && pool.Count > 0)
            {
                cell = pool.Dequeue();
            }
            else
            {
                if (cellTemplates.TryGetValue(id,out cell))
                {
                    cell = InstantiateCell(cell);
                }
            }
            return cell; //池子中没有了,就新建一个对象
        }
        
        //创建 cell
        private static UITableCell InstantiateCell(UITableCell cell,Transform parent = null)
        {
            var go = UnityEngine.Object.Instantiate<GameObject>(cell.gameObject, Vector3.zero, Quaternion.identity,parent);
            var tViewCell = go.GetComponent<UITableCell>();
            tViewCell.Id = cell.Id;
            tViewCell.element = go.GetComponent<LayoutElement>();
            tViewCell.rectTransform = go.GetComponent<RectTransform>();
            return tViewCell;
        }
        
    }
}