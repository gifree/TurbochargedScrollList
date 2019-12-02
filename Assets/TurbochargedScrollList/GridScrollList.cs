﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Jing.TurbochargedScrollList
{
    struct GridPos
    {
        public int x { get; private set; }

        public int y { get; private set; }

        public float pixelX { get; private set; }

        public float pixelY { get; private set; }

        public int index { get; private set; }

        int _gridRowCount;
        int _gridColCount;
        float _gridW;
        float _gridH;

        public GridPos(int x, int y, int gridColCount, int gridRowCount, float gridW, float gridH)
        {
            this._gridRowCount = gridRowCount;
            this._gridColCount = gridColCount;
            this._gridW = gridW;
            this._gridH = gridH;

            if (gridColCount == 0 || gridRowCount == 0)
            {
                this.x = this.y = 0;
                this.pixelX = this.pixelY = 0;
                this.index = -1;
            }
            else
            {
                this.x = x >= gridColCount ? gridColCount - 1 : x;
                this.y = y >= gridRowCount ? gridRowCount - 1 : y;
                this.pixelX = x * _gridW;
                this.pixelY = y * _gridH;
                this.index = y * _gridColCount + x;
            }
        }

        public void ChangePos(int x, int y)
        {
            this.x = x;
            this.y = y;
            this.pixelX = x * _gridW;
            this.pixelY = y * _gridH;
            this.index = y * _gridColCount + x;
        }

        /// <summary>
        /// 将index在x、y轴上的位置
        /// </summary>
        public void AxisFlip()
        {
            this.index = x * _gridRowCount + y;
        }

    }


    public enum EGridConstraint
    {
        /// <summary>
        /// 根据视口自动填充
        /// </summary>
        FLEXIBLE,

        /// <summary>
        /// 固定列数
        /// </summary>
        FIXED_COLUMN_COUNT,

        /// <summary>
        /// 固定行数
        /// </summary>
        FIXED_ROW_COUNT,
    }

    public class GridScrollList : GridScrollList<object>
    {
        //public GridScrollList(GameObject scrollView, OnRenderItem itemRender) : base(scrollView, itemRender)
        //{
        //}

        public GridScrollList(GameObject scrollView, OnRenderItem itemRender, Vector2 gap, EGridConstraint constraint, int constraintCount = 0) : base(scrollView, itemRender, gap, constraint, constraintCount)
        {

        }

        public GridScrollList(GameObject scrollView, GameObject itemPrefab, OnRenderItem itemRender, Vector2 gap, EGridConstraint constraint, int constraintCount = 0) : base(scrollView, itemPrefab, itemRender, gap, constraint, constraintCount)
        {
        }

        public virtual void AddRange<TInput>(IEnumerable<TInput> collection)
        {
            foreach (var data in collection)
            {
                Add(data);
            }
        }
    }

    public class GridScrollList<TData> : BaseScrollList<TData>
    {
        /// <summary>
        /// 列表项之间的间距
        /// </summary>
        public Vector2 gap { get; private set; }

        /// <summary>
        /// 列表项排列方式限定
        /// </summary>
        public EGridConstraint constraint { get; private set; }

        /// <summary>
        /// 当constraint不为「FLEXIBLE」，根据该值确定列数或者行数
        /// </summary>
        public int constraintCount { get; private set; }


        /// <summary>
        /// 行数
        /// </summary>
        public int rowCount { get; private set; }

        /// <summary>
        /// 列数
        /// </summary>
        public int colCount { get; private set; }

        /// <summary>
        /// item宽度加上间距的值
        /// </summary>
        float _bigW;

        /// <summary>
        /// item高度加上间距的值
        /// </summary>
        float _bigH;

        //public GridScrollList(GameObject scrollView, OnRenderItem itemRender)
        //{
        //    InitScrollView(scrollView);

        //    var layout = content.GetComponent<GridLayoutGroup>();
        //    this.gap = layout.spacing;
        //    switch (layout.constraint)
        //    {
        //        case GridLayoutGroup.Constraint.Flexible:
        //            this.constraint = EGridConstraint.FLEXIBLE;
        //            break;
        //        case GridLayoutGroup.Constraint.FixedColumnCount:
        //            this.constraint = EGridConstraint.FIXED_COLUMN_COUNT;
        //            break;
        //        case GridLayoutGroup.Constraint.FixedRowCount:
        //            this.constraint = EGridConstraint.FIXED_ROW_COUNT;
        //            break;
        //    }
        //    this.constraintCount = layout.constraintCount;
        //    layout.enabled = false;
        //    //GameObject.Destroy(layout);

        //    AutoInitItem(itemRender);
        //}

        public GridScrollList(GameObject scrollView, OnRenderItem itemRender, Vector2 gap, EGridConstraint constraint, int constraintCount = 0)
        {
            InitScrollView(scrollView);

            this.gap = gap;
            this.constraint = constraint;
            this.constraintCount = constraintCount;
            if (EGridConstraint.FLEXIBLE == constraint)
            {
                scrollRect.horizontal = false;
            }

            AutoInitItem(itemRender);
        }

        public GridScrollList(GameObject scrollView, GameObject itemPrefab, OnRenderItem itemRender, Vector2 gap, EGridConstraint constraint, int constraintCount = 0)
        {
            InitScrollView(scrollView);

            this.gap = gap;
            this.constraint = constraint;
            this.constraintCount = constraintCount;
            if (EGridConstraint.FLEXIBLE == constraint)
            {
                scrollRect.horizontal = false;
            }

            InitItem(itemPrefab, itemRender);
        }

        protected override bool AdjustmentItemSize(ScrollListItem item)
        {
            if (item.width != itemDefaultfSize.x || item.height != itemDefaultfSize.y)
            {
                item.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, itemDefaultfSize.x);
                item.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, itemDefaultfSize.y);
            }
            return false;
        }

        protected override void Refresh(UpdateData updateConfig, out int lastStartIndex)
        {
            var scrollX = -content.localPosition.x;
            if (scrollX < 0)
            {
                scrollX = 0;
            }
            var scrollY = content.localPosition.y;
            if (scrollY < 0)
            {
                scrollY = 0;
            }

            bool isColByCol = constraint == EGridConstraint.FIXED_ROW_COUNT ? true : false;

            //计算出关联的所有Item的索引
            List<GridPos> list = new List<GridPos>();
            lastStartIndex = 0;
            if (colCount > 0 && rowCount > 0)
            {
                //根据滚动区域左上角算出起始item的二维位置;
                GridPos startPos = new GridPos((int)(scrollX / _bigW), (int)(scrollY / _bigH), colCount, rowCount, _bigW, _bigH);
                lastStartIndex = startPos.index;
                //根据滚动区域左下角算出结束item的二维位置
                GridPos endPos = new GridPos((int)((scrollX + viewportSize.x) / _bigW), (int)((scrollY + viewportSize.y) / _bigH), colCount, rowCount, _bigW, _bigH);

                if(isColByCol)
                {
                    RefreshGridPosColByCol(list, ref startPos, ref endPos);
                }
                else
                {
                    RefreshGridPosRowByRow(list, ref startPos, ref endPos);
                }
            }

            /// <summary>
            /// 最后一次显示的Item的缓存
            /// </summary>
            Dictionary<ScrollListItemModel<TData>, ScrollListItem> lastShowingItems = new Dictionary<ScrollListItemModel<TData>, ScrollListItem>(_showingItems);

            _showingItems.Clear();

            for (int i = 0; i < list.Count; i++)
            {
                var gridPos = list[i];
                var model = _itemModels[gridPos.index];

                ScrollListItem item = CreateItem(model, gridPos.index, lastShowingItems);
                _showingItems[model] = item;
                item.rectTransform.localPosition = new Vector3(gridPos.pixelX, -gridPos.pixelY, 0);
            }

            //回收没有使用的item
            foreach (var item in lastShowingItems.Values)
            {
                item.gameObject.SetActive(false);
                _recycledItems.Add(item);
            }
        }
        
        
        /// <summary>
        /// 从左往右，从上往下依次刷新
        /// ↓ ↓ ↓ Example
        /// 0 1 2
        /// 3 4 5
        /// 6 7 
        /// </summary>
        void RefreshGridPosRowByRow(List<GridPos> output, ref GridPos startPos, ref GridPos endPos)
        {
            output.Clear();
            for (int y = startPos.y; y <= endPos.y; y++)
            {
                for (int x = startPos.x; x <= endPos.x; x++)
                {
                    var gp = new GridPos(x, y, colCount, rowCount, _bigW, _bigH);
                    if (gp.index >= 0 && gp.index < _itemModels.Count)
                    {
                        output.Add(gp);
                    }
                    else
                    {
                        break;
                    }
                }
            }            
        }

        /// <summary>
        /// 从上往下，从左往右依次刷新
        /// ↓ ↓ ↓ Example
        /// 0 3 6 
        /// 1 4 7
        /// 2 5 
        /// </summary>
        void RefreshGridPosColByCol(List<GridPos> output, ref GridPos startPos, ref GridPos endPos)
        {
            output.Clear();
            for (int x = startPos.x; x <= endPos.x; x++)
            {
                for (int y = startPos.y; y <= endPos.y; y++)
                {
                    var gp = new GridPos(x, y, colCount, rowCount, _bigW, _bigH);
                    gp.AxisFlip();
                    if (gp.index >= 0 && gp.index < _itemModels.Count)
                    {
                        output.Add(gp);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }        

        protected override void ResizeContent(UpdateData updateConfig)
        {
            float contentW = 0;
            float contentH = 0;
            int itemAmount = _itemModels.Count;

            _bigW = itemDefaultfSize.x + gap.x;
            _bigH = itemDefaultfSize.y + gap.y;

            colCount = 0;
            rowCount = 0;

            //计算Content大小
            switch (constraint)
            {
                case EGridConstraint.FLEXIBLE: //根据视口确定Content大小，并且计算出constraint数量
                    contentW = viewportSize.x;
                    //计算出constraintCount
                    constraintCount = (int)((contentW + gap.x) / _bigW);
                    //确定高度,通过item总数和constraintCount算出                    
                    if (itemAmount > 0)
                    {
                        rowCount = (itemAmount - 1) / constraintCount + 1;
                        contentH = rowCount * _bigH - gap.y;
                    }

                    colCount = constraintCount;
                    break;
                case EGridConstraint.FIXED_COLUMN_COUNT: //根据列数确定Content大小

                    colCount = constraintCount;
                    if (_itemModels.Count < colCount)
                    {
                        colCount = _itemModels.Count;
                    }

                    contentW = colCount * _bigW - gap.x;
                    //确定高度,通过item总数和constraintCount算出
                    if (itemAmount > 0)
                    {
                        rowCount = (itemAmount - 1) / colCount + 1;
                        contentH = rowCount * _bigH - gap.y;
                    }

                    break;
                case EGridConstraint.FIXED_ROW_COUNT: //根据行数确定Content大小

                    rowCount = constraintCount;
                    if (_itemModels.Count < rowCount)
                    {
                        rowCount = _itemModels.Count;
                    }

                    contentH = rowCount * _bigH - gap.y;
                    if (contentH < 0)
                    {
                        contentH = 0;
                    }
                    //确定宽度,通过item总数和constraintCount算出                    
                    if (itemAmount > 0)
                    {
                        colCount = (itemAmount - 1) / rowCount + 1;
                        contentW = colCount * _bigW - gap.x;
                    }
                    break;
            }

            if (contentW < 0)
            {
                contentW = 0;
            }

            if (contentH < 0)
            {
                contentH = 0;
            }

            SetContentSize(contentW, contentH);
        }

        public override void ScrollToItem(int index)
        {
            
        }
    }
}
