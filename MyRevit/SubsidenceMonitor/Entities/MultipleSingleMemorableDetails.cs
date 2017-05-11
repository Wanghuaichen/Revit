﻿using Autodesk.Revit.DB;
using Microsoft.Office.Interop.Excel;
using MyRevit.SubsidenceMonitor.Interfaces;
using MyRevit.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyRevit.SubsidenceMonitor.Entities
{
    public class MultipleSingleMemorableDetails : MultipleSingleMemorableData<object, TList, TDetail>
    {
        public MultipleSingleMemorableDetails() : base()
        {
        }
        public MultipleSingleMemorableDetails(Document storage, TList list) : base(storage, list)
        {
        }

        protected override TDetail createNew()
        {
            return new TDetail()
            {
                IssueType = List.IssueType,
                List = List,
                IssueDateTime = List.IssueDate,//新增项增加默认日期用于时间校对
                IsLoad = true,//新增项认为无需加载处理
            };
        }
        protected override ParseResult importExcel(Workbook workbook)
        {
            return MemorableData.Data.IssueType.GetEntity().ParseInto(workbook, MemorableData.Data);
        }
        protected override void UpdateMemorableData()
        {
            MemorableData = new MemorableDetail(Storage, Datas[DataIndex]);
            MemorableData.Start();
        }
        public void AddElementIds(string nodeCode, List<ElementId> elementIds)
        {
            var targetNode = MemorableData.Data.Nodes.First(c => c.NodeCode == nodeCode);
            foreach (var elementId in elementIds)
            {
                var elementId_Int = elementId.IntegerValue;
                var elementNode = MemorableData.Data.Nodes.FirstOrDefault(c => c.ElementIds_Int.Contains(elementId_Int));
                if (elementNode == targetNode)
                    continue;
                if (elementNode != null && elementNode != targetNode)
                    elementNode.ElementIds_Int.Remove(elementId_Int);
                targetNode.ElementIds_Int.Add(elementId_Int);
            }
        }
        public void DeleteElementIds(string nodeCode, List<ElementId> elementIds)
        {
            var targetNode = MemorableData.Data.Nodes.First(c => c.NodeCode == nodeCode);
            foreach (var elementId in elementIds)
            {
                var elementId_Int = elementId.IntegerValue;
                targetNode.ElementIds_Int.Remove(elementId_Int);
            }
        }
        string GroupName = "监测系统";
        enum CustomParameters
        {
            监测类型,
            /// <summary>
            /// for All Except 钢支撑轴力监测
            /// </summary>
            测点编号,
            /// <summary>
            /// for 钢支撑轴力监测
            /// </summary>
            监测点,
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeCodes"></param>
        /// <param name="doc"></param>
        public void RenderNodeInfoToElements(List<string> nodeCodes, Document doc)
        {
            var nodes = MemorableData.Data.Nodes.Where(c => nodeCodes.Contains(c.NodeCode));
            foreach (var node in nodes)
            {
                for (int i = node.ElementIds_Int.Count - 1; i >= 0; i--)
                {
                    var element = doc.GetElement(new ElementId(node.ElementIds_Int[i]));
                    if (element == null)
                        node.ElementIds_Int.Remove(node.ElementIds_Int[i]);//获取ElementId的时候,需在文档中监测

                    //共享参数检测 添加共享参数
                    var parameter = element.GetParameters(CustomParameters.监测类型.ToString()).FirstOrDefault();
                    if (parameter == null)
                    {
                        if (node.IssueType == EIssueType.钢支撑轴力监测)
                        {
                            Revit_Document_Helper.AddSharedParameter(doc, GroupName, CustomParameters.监测类型.ToString(), element.Category, true);
                            Revit_Document_Helper.AddSharedParameter(doc, GroupName, CustomParameters.监测点.ToString(), element.Category, true);
                            parameter = element.GetParameters(CustomParameters.监测点.ToString()).First();
                            parameter.Set(node.NodeCode);
                        }
                        else
                        {
                            Revit_Document_Helper.AddSharedParameter(doc, GroupName, CustomParameters.监测类型.ToString(), element.Category, true);
                            Revit_Document_Helper.AddSharedParameter(doc, GroupName, CustomParameters.测点编号.ToString(), element.Category, true);
                            parameter = element.GetParameters(CustomParameters.测点编号.ToString()).First();
                            parameter.Set(node.NodeCode);
                        }
                        parameter = element.GetParameters(CustomParameters.监测类型.ToString()).FirstOrDefault();
                        parameter.Set(node.IssueType.ToString());
                    }
                    else
                    {
                        //赋值及冲突监测
                        var value = parameter.AsString();
                        if (value != null && value != node.IssueType.ToString())
                            throw new NotImplementedException($"构件({element.Name})存在重复,原先的类型:{value},现在的类型:{node.IssueType.ToString()}");
                        if (string.IsNullOrEmpty(value))
                            parameter.Set(node.IssueType.ToString());
                        if (node.IssueType == EIssueType.钢支撑轴力监测)
                            parameter = element.GetParameters(CustomParameters.监测点.ToString()).First();
                        else
                            parameter = element.GetParameters(CustomParameters.测点编号.ToString()).First();
                        parameter.Set(node.NodeCode);
                    }
                }
            }
        }
        public List<ElementId> GetElementIds(string nodeCode, Document doc)
        {
            List<ElementId> availableElementIds = new List<ElementId>();
            var targetNode = MemorableData.Data.Nodes.First(c => c.NodeCode == nodeCode);
            for (int i = targetNode.ElementIds_Int.Count - 1; i >= 0; i--)
            {
                var element = doc.GetElement(new ElementId(targetNode.ElementIds_Int[i]));
                if (element == null)
                    targetNode.ElementIds_Int.Remove(targetNode.ElementIds_Int[i]);//获取ElementId的时候,需在文档中监测
                else
                    availableElementIds.Add(element.Id);
            }
            return availableElementIds;
        }
        public List<ElementId> GetElementIds(List<string> nodeCodes, Document doc)
        {
            List<ElementId> availableElementIds = new List<ElementId>();
            foreach (var nodeCode in nodeCodes)
            {
                availableElementIds.AddRange(GetElementIds(nodeCode, doc));
            }
            return availableElementIds;
        }
        public List<ElementId> GetAllNodesElementIds(Document doc)
        {
            List<ElementId> availableElementIds = new List<ElementId>();
            foreach (var node in MemorableData.Data.Nodes)
            {
                availableElementIds.AddRange(GetElementIds(node.NodeCode, doc));
            }
            return availableElementIds;
        }
        public List<ElementId> GetCurrentMaxNodesElements(Document doc)
        {
            List<ElementId> results = new List<ElementId>();
            foreach (var node in MemorableData.Data.NodeDatas.GetCurrentMaxNodes())
            {
                results.AddRange(GetElementIds(node.NodeCode, doc));
            }
            return results;
        }
        public List<ElementId> GetTotalMaxNodesElements(Document doc)
        {
            List<ElementId> results = new List<ElementId>();
            foreach (var node in MemorableData.Data.NodeDatas.GetTotalMaxNodes())
            {
                results.AddRange(GetElementIds(node.NodeCode, doc));
            }
            return results;
        }
        public List<ElementId> GetCloseWarnNodesElements(Document doc, WarnSettings warnSettings)
        {
            List<ElementId> results = new List<ElementId>();
            foreach (var node in MemorableData.Data.NodeDatas.GetCloseWarn(warnSettings))
            {
                results.AddRange(GetElementIds(node.NodeCode, doc));
            }
            return results;
        }
        public List<ElementId> GetOverWarnNodesElements(Document doc, WarnSettings warnSettings)
        {
            List<ElementId> results = new List<ElementId>();
            foreach (var node in MemorableData.Data.NodeDatas.GetOverWarn(warnSettings))
            {
                results.AddRange(GetElementIds(node.NodeCode, doc));
            }
            return results;
        }
    }
}
