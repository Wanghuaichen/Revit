﻿using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using MyRevit.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;


namespace MyRevit.MyTests.MepCurveAvoid
{
    /// <summary>
    /// 避让统筹管理
    /// </summary>
    public class AvoidElemntManager
    {
        List<AvoidElement> AvoidElements = new List<AvoidElement>();
        List<ValuedConflictNode> ConflictNodes = new List<ValuedConflictNode>();

        public void AddElements(List<Element> elements)
        {
            AvoidElements.AddRange(elements.Where(c => c is Pipe).Select(c => new AvoidElement(c as MEPCurve, AvoidElementType.Pipe)));
            AvoidElements.AddRange(elements.Where(c => c is Duct).Select(c => new AvoidElement(c as MEPCurve, AvoidElementType.Duct)));
            AvoidElements.AddRange(elements.Where(c => c is Conduit).Select(c => new AvoidElement(c as MEPCurve, AvoidElementType.Conduit)));
            AvoidElements.AddRange(elements.Where(c => c is CableTray).Select(c => new AvoidElement(c as MEPCurve, AvoidElementType.CableTray)));
        }

        /// <summary>
        /// 碰撞检测
        /// </summary>
        public void CheckConflict()
        {
            //基本碰撞&&构建碰撞网络
            foreach (var avoidElement in AvoidElements)
                avoidElement.SetConflictElements(AvoidElements, ConflictNodes);
            //价值分组
            for (int i = ConflictNodes.Count()-1; i >= 0; i--)
            {
                var conflictNode = ConflictNodes[i];
                    conflictNode.Settle(ConflictNodes, AvoidElements);
            }
        }

        /// <summary>
        /// 碰撞结果整合
        /// 
        /// </summary>
        internal void MergeConflict()
        {
            //foreach (var ConflictNode in ConflictNodes)
            //{
            //    ConflictNode.Settle(ConflictNodes);
            //}

            //foreach (var avoidElement in AvoidElements)
            //{
            //    var undealtConflictNodes = avoidElement.ConflictElements.Where(c => c.DealType == ConflictNodeDealType.Undealt).ToList();
            //    for (int i = 0; i < undealtConflictNodes.Count(); i++)
            //    {
            //        var current = undealtConflictNodes[i];
            //        bool isContinuous = false;
            //        while (i != undealtConflictNodes.Count() - 1)
            //        {
            //        }
            //        //current.IsContinuous()
            //        //是否连续
            //    }
            //}


            //TODO
            return;
        }

        /// <summary>
        /// 避让处理
        /// </summary>
        /// <param name="doc"></param>
        public void AutoAvoid(Document doc)
        {
            ////var groupedAvoidElements = AvoidElements.GroupBy(c => c.AvoidPriorityValue).OrderBy(c => c.Key);

            //var miniMepLength = UnitHelper.ConvertToFoot(96, VLUnitType.millimeter);//最短连接管长度 双向带连接件
            //var miniSpace = UnitHelper.ConvertToFoot(5, VLUnitType.millimeter);//避免碰撞及提供留白的安全距离
            //var angleToTurn = Math.PI / 4;//45°
            ////var linkHypotenuse = -1;//TODO 连接件的斜边长度
            //var orderedAvoidElements = AvoidElements.OrderBy(c => c.AvoidPriorityValue).ToList();
            //for (int i = orderedAvoidElements.Count() - 1; i >= 0; i--)
            //{
            //    var currentAvoidElement = orderedAvoidElements[i];
            //    var srcMep = currentAvoidElement.MEPElement;
            //    //TODO检查连续性
            //    if (i == 0)//TODO 暂且作为首个进行避让测试用
            //    {
            //        //单个避让
            //        for (int j = currentAvoidElement.ConflictNodes.Count() - 1; j >= 0; j--)
            //        {
            //            var currentElementToAvoid = currentAvoidElement.ConflictNodes[j];

            //            //拆分处理
            //            XYZ pointStart, pointEnd;
            //            var curve = (currentAvoidElement.MEPElement.Location as LocationCurve).Curve;
            //            pointStart = curve.GetEndPoint(0);
            //            pointEnd = curve.GetEndPoint(1);
            //            //TODO 确定拆分点
            //            var midPoint = currentElementToAvoid.ConflictPoint;
            //            var verticalDirection = new XYZ(0, 0, 1);//TODO 由避让元素决定上下翻转
            //            var parallelDirection = (pointStart - pointEnd).Normalize();//朝向Start

            //            #region 点位计算公式
            //            //Height=Math.Max(垂直的最短仅留白距离,构件的最短垂直间距)
            //            //WidthUp=最小管道的长度/2
            //            //WidthUp=Math.Max(WidthUp,水平的最短仅留白距离-构件的最短水平间距)
            //            //WidthUp=Math.Max(WidthUp,考虑切边的最短距离) 
            //            //WidthDown=WidthUp根据角度换算所得
            //            //WidthDown=Math.Max(WidthDown,水平的最短仅留白距离)
            //            #endregion

            //            //max(垂直最短留白距离,最小斜边长度,最短切割距离) 
            //            var height = currentAvoidElement.Height / 2 + currentElementToAvoid.AvoidElement.Height / 2 + miniSpace;
            //            //height = Math.Max(height,构件的最小高度);//TODO 考虑构件的最小高度需求
            //            var widthUp = miniMepLength / 2;
            //            //widthUp = Math.Max(widthUp, height - 构件的最小宽度); //TODO 考虑构件的最小宽度需求
            //            var diameterAvoid = Math.Max(currentAvoidElement.Width, currentAvoidElement.Height);
            //            var diameterToAvoid = Math.Max(currentElementToAvoid.AvoidElement.Width, currentElementToAvoid.AvoidElement.Height);
            //            widthUp = Math.Max(widthUp, (diameterAvoid / 2 + diameterToAvoid / 2 + miniSpace) / Math.Sin(angleToTurn) - height * Math.Tan(angleToTurn));
            //            var widthDown = widthUp + height / Math.Tan(angleToTurn);//水平最短距离对应的水平偏移
            //            widthDown = Math.Max(widthDown, currentAvoidElement.Width / 2 + currentElementToAvoid.AvoidElement.Width / 2 + miniSpace);

            //            //TODO 将点位计算独立出来 前置,将在统筹计划中发挥统筹效力
            //            var direction1 = (curve as Line).Direction;
            //            var direction2 = ((currentElementToAvoid.AvoidElement.MEPElement.Location as LocationCurve).Curve as Line).Direction;
            //            var faceAngle = direction1.AngleOnPlaneTo(direction2, new XYZ(0, 0, 1));
            //            widthUp = widthUp / Math.Abs(Math.Sin(faceAngle));
            //            widthDown = widthDown / Math.Abs(Math.Sin(faceAngle));
            //            var startSplit = midPoint + parallelDirection * widthDown;
            //            var endSplit = midPoint - parallelDirection * widthDown;
            //            midPoint += height * verticalDirection;
            //            var midStart = midPoint + parallelDirection * widthUp;
            //            var midEnd = midPoint - parallelDirection * widthUp;
            //            //创建管道
            //            var connector0 = srcMep.ConnectorManager.Connectors.GetConnectorById(0);
            //            var connector1 = srcMep.ConnectorManager.Connectors.GetConnectorById(1);
            //            Connector link0 = connector0.GetConnectedConnector();
            //            if (link0 != null)
            //                connector0.DisconnectFrom(link0);
            //            Connector link1 = connector1.GetConnectedConnector();
            //            if (link1 != null)
            //                connector0.DisconnectFrom(link1);
            //            bool isSameSide = (connector0.Origin - connector1.Origin).DotProduct(parallelDirection) > 0;
            //            //偏移处理
            //            var mepStart = doc.GetElement(ElementTransformUtils.CopyElement(doc, srcMep.Id, new XYZ(0, 0, 0)).First()) as MEPCurve;
            //            var mepEnd = doc.GetElement(ElementTransformUtils.CopyElement(doc, srcMep.Id, new XYZ(0, 0, 0)).First()) as MEPCurve;
            //            var leanMepStart = doc.GetElement(ElementTransformUtils.CopyElement(doc, srcMep.Id, new XYZ(0, 0, 0)).First()) as MEPCurve;
            //            var leanMepEnd = doc.GetElement(ElementTransformUtils.CopyElement(doc, srcMep.Id, new XYZ(0, 0, 0)).First()) as MEPCurve;
            //            var offsetMep = doc.GetElement(ElementTransformUtils.CopyElement(doc, srcMep.Id, new XYZ(0, 0, 0)).First()) as MEPCurve;
            //            if (link0 != null)
            //                mepStart.ConnectorManager.Connectors.GetConnectorById(0).ConnectTo(link0);
            //            if (link1 != null)
            //                mepEnd.ConnectorManager.Connectors.GetConnectorById(1).ConnectTo(link1);
            //            doc.Delete(srcMep.Id);
            //            //确定连接点,并重新连接
            //            (mepStart.Location as LocationCurve).Curve = Line.CreateBound(pointStart, startSplit);
            //            (leanMepStart.Location as LocationCurve).Curve = Line.CreateBound(startSplit, midStart);
            //            (offsetMep.Location as LocationCurve).Curve = Line.CreateBound(midStart, midEnd);
            //            (leanMepEnd.Location as LocationCurve).Curve = Line.CreateBound(midEnd, endSplit);
            //            (mepEnd.Location as LocationCurve).Curve = Line.CreateBound(endSplit, pointEnd);
            //            //TODO 连接件处理
            //            //TODO 需转移对mep2的碰撞
            //        }
            //    }
            //}
            //doc.Regenerate();
        }
    }
}