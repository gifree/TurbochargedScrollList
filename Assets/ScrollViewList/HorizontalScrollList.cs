﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jing.TurbochargedScrollList
{
    /// <summary>
    /// 水平滚动列表
    /// </summary>
    public class HorizontalScrollList<TData> : BaseScrollList<TData>
    {
        public HorizontalScrollList(GameObject scrollView, GameObject itemPrefab, OnRenderItem itemRender, float gap = 0) : base(scrollView, itemPrefab, itemRender, gap)
        {

        }

        protected override void RebuildContent()
        {
            float w = 0;
            for(int i = 0; i < _itemModels.Count; i++)
            {
                w += (_itemModels[i].width + gap);
            }
            w -= gap;

            SetContentSize(w, viewportSize.y);

            Refresh();
        }

        protected override void Refresh()
        {
            UpdateViewportSize();

            //内容容器宽度
            var contentWidth = content.rect.width;            

            //content的滚动是负数
            contentRenderStartPos = -content.localPosition.x;
            if(contentRenderStartPos < 0)
            {
                contentRenderStartPos = 0;
            }
            else if(contentRenderStartPos > contentWidth - viewportSize.x)
            {
                contentRenderStartPos = contentWidth - viewportSize.x;
            }

            int dataIdx;
            float startPos = 0;

            for(dataIdx = 0; dataIdx < _itemModels.Count; dataIdx++)
            {
                var dataRight = startPos + _itemModels[dataIdx].width;
                if (dataRight >= contentRenderStartPos)
                {
                    //就是我了
                    break;
                }

                startPos = dataRight + gap;
            }                                 
            
            //显示的内容刚好大于这个值即可           
            float contentWidthLimit = viewportSize.x;
            float itemX = startPos;

            /// <summary>
            /// 最后一次显示的Item的缓存
            /// </summary>
            Dictionary<int, ScrollListItem> lastShowingItems = new Dictionary<int, ScrollListItem>(_showingItems);

            _showingItems.Clear();

            while(dataIdx < _itemModels.Count)
            {
                var model = _itemModels[dataIdx];

                ScrollListItem item = CreateItem(model.data, dataIdx, lastShowingItems);
                //item.gameObject.name += $"_{_itemModels[dataIdx].height}";
                _showingItems[dataIdx] = item;

                var pos = Vector3.zero;
                pos.x = itemX;
                item.rectTransform.localPosition = pos;
                //下一个item的X坐标
                itemX += (item.width + gap);
                //下一个item的索引
                dataIdx++;

                if (itemX - contentRenderStartPos >= contentWidthLimit)
                {
                    break;
                }
            }

            //回收没有使用的item
            foreach(var item in lastShowingItems.Values)
            {
                //如果不要内存池，则直接Destroy即可
                //GameObject.Destroy(item.gameObject);

                item.gameObject.SetActive(false);
                _recycledItems.Add(item);
            }            
        }

        protected override void CheckItemsSize()
        {
            foreach(var item in _showingItems.Values)
            {
                if (item.width != _itemModels[item.index].width)
                {
                    //Debug.Log($"item[{item.index}]的尺寸改变 {_itemModels[item.index].height} => {item.height}");
                    _itemModels[item.index].width = item.width;
                    MarkDirty(EUpdateType.REBUILD);
                }
            }
        }
    }
}