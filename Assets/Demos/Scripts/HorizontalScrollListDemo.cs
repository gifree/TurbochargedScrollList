﻿using Jing.TurbochargedScrollList;
using UnityEngine;
using UnityEngine.UI;

public class HorizontalScrollListDemo : BaseScrollListDemo
{
    HorizontalScrollList list;    

    protected override void InitItems()
    {
        var datas = new int[itemCount];
        for (int i = 0; i < datas.Length; i++)
        {
            datas[i] = i;
        }

        GameObject itemPrefab = transform.Find("Horizontal Scroll View/Item").gameObject;

        var ls = new HorizontalLayoutSettings();
        ls.gap = 10;
        ls.paddingRight = 500;
        list = new HorizontalScrollList(scrollView.GetComponent<ScrollRect>(), itemPrefab, ls);        
        list.onRenderItem += OnItemRender;
        list.onRebuildContent += OnRebuildContent;
        list.onRefresh += OnListRefresh;
        list.AddRange(datas);
    }

    protected void OnItemRender(ScrollListItem item, object data, bool isRefresh)
    {
        item.GetComponent<Item>().Refresh();
        Debug.LogFormat("渲染Item [idx:{0}, value:{1}]", item.index, data);
    }

    protected override void AddRange()
    {
        var datas = new int[InputNumber];
        list.AddRange(datas);
    }

    protected override void Clear()
    {
        list.Clear();
    }

    protected override void Insert()
    {
        list.Insert(2, 0);
    }

    protected override void Remove()
    {
        list.Remove(0);
    }

    protected override void RemoveAt()
    {
        list.RemoveAt(2);
    }

    protected override void ScrollToItem()
    {
        list.ScrollToItem(20);
    }

    protected override void ScrollToPosition()
    {
        list.ScrollToPosition(list.ContentWidth);
    }
}