﻿using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MyRevit.MyTests.Utilities;
using MyRevit.Utilities;
using PmSoft.Optimization.DrawingProduction;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Interop;

namespace MyRevit.MyTests.CompoundStructureAnnotation
{
    /// <summary>
    /// CompoundStructureAnnotation ViewType
    /// </summary>
    public enum CSAViewType
    {
        Idle,
        Select,
        Generate,
        Close,
    }

    /// <summary>
    /// CompoundStructureAnnotation 文字的定位方案
    /// </summary>
    public enum CSALocationType
    {
        /// <summary>
        /// 在线上
        /// </summary>
        OnLine,
        /// <summary>
        /// 在线端
        /// </summary>
        OnEdge,
    }

    /// <summary>
    /// CompoundStructureAnnotation数据的载体
    /// </summary>
    public class CSAModel
    {
        //public CSAModel()
        //{
        //    Document = null;
        //    Texts = new List<string>();
        //    TextLocations = new List<XYZ>();
        //}
        public CSAModel(Document doc)
        {
            Document = doc;
            Texts = new List<string>();
            TextLocations = new List<XYZ>();
        }

        /// <summary>
        /// 文档
        /// </summary>
        public Document Document { set; get; }

        /// <summary>
        /// 文字样式
        /// </summary>
        public ElementId TextNoteTypeElementId { set; get; }

        /// <summary>
        /// 文字的定位方案
        /// </summary>
        public CSALocationType CSALocationType { set; get; }

        #region 需要留存的数据
        /// <summary>
        /// 标注元素 对象,可以是墙,屋顶,伸展屋顶等等
        /// </summary>
        public ElementId TargetId { set; get; }
        /// <summary>
        /// 线 对象
        /// </summary>
        public ElementId LineId { get; set; }
        /// <summary>
        /// 文字 对象
        /// </summary>
        public List<ElementId> TextNoteIds { set; get; }
        #endregion

        /// <summary>
        /// 需要显示的结构标注信息
        /// </summary>
        public List<string> Texts { set; get; }
        /// <summary>
        /// 线 位置
        /// </summary>
        public XYZ LineLocation { set; get; }
        /// <summary>
        /// 文字 位置
        /// </summary>
        public List<XYZ> TextLocations { set; get; }

        /// <summary>
        /// 坐标定位,平行于标注对象
        /// </summary>
        public XYZ ParallelVector = null;
        /// <summary>
        /// 坐标定位,垂直于标注对象
        /// </summary>
        public XYZ VerticalVector = null;

    }

    /// <summary>
    /// 为CSAModel数据服务的处理类
    /// </summary>
    public static class CSAModelEx
    {
        /// <summary>
        /// 获取文字的样式方案
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static List<TextNoteType> GetTextNoteTypes(this CSAModel model)
        {
            return new FilteredElementCollector(model.Document).OfClass(typeof(TextNoteType)).ToElements().Select(p => p as TextNoteType).ToList();
        }

        /// <summary>
        /// 线 参数设置
        /// </summary>
        public static void UpdateLineParameters(this CSAModel model, FamilyInstance line, double lineHeight, double lineWidth, double space, int textCount)
        {
            line.GetParameters(TagProperty.线高度1.ToString()).First().Set(UnitHelper.ConvertToFoot(lineHeight, AnnotationConstaints.UnitType));
            line.GetParameters(TagProperty.线宽度.ToString()).First().Set(UnitHelper.ConvertToFoot(lineWidth, AnnotationConstaints.UnitType));
            line.GetParameters(TagProperty.线下探长度.ToString()).First().Set(0);
            line.GetParameters(TagProperty.间距.ToString()).First().Set(UnitHelper.ConvertToFoot(space, AnnotationConstaints.UnitType));
            line.GetParameters(TagProperty.文字行数.ToString()).First().Set(textCount);
        }

        /// <summary>
        /// 获取 CompoundStructure 对象
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static CompoundStructure GetCompoundStructure(this CSAModel model, Element element)
        {
            CompoundStructure compoundStructure = null;
            if (element is Wall)
            {
                compoundStructure = (element as Wall).WallType.GetCompoundStructure();
            }
            else if (element is Floor)
            {
                compoundStructure = (element as Floor).FloorType.GetCompoundStructure();
            }
            else if (element is ExtrusionRoof)//TODO 屋顶有多种类型
            {
                compoundStructure = (element as ExtrusionRoof).RoofType.GetCompoundStructure();
            }
            else
            {
                throw new NotImplementedException("暂不支持该类型的对象,未能成功获取CompoundStructure");
            }
            return compoundStructure;
        }

        /// <summary>
        /// 从 CompoundStructure 中获取标注信息
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="compoundStructure"></param>
        /// <returns></returns>
        public static List<string> FetchTextsFromCompoundStructure(this CSAModel model, Document doc, CompoundStructure compoundStructure)
        {
            var layers = compoundStructure.GetLayers();
            var texts = new List<string>();
            foreach (var layer in layers)
            {
                if (layer.MaterialId.IntegerValue < 0)
                    continue;
                var material = doc.GetElement(layer.MaterialId);
                if (material == null)
                    continue;
                texts.Add(layer.Width + doc.GetElement(layer.MaterialId).Name);
            }
            model.Texts = texts;
            return texts;
        }

        /// <summary>
        /// 从需要标注的对象中获取标注的源位置
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static XYZ GetElementLocation(this CSAModel model, Element element)
        {
            var locationCurve = element.Location as LocationCurve;
            var location = (locationCurve.Curve.GetEndPoint(0) + locationCurve.Curve.GetEndPoint(1)) / 2;
            return location;
        }

        /// <summary>
        /// 从需要标注的对象中获取标注的源位置
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static void FetchLocations(this CSAModel model, Element element, FamilyInstance line)
        {
            var locationCurve = element.Location as LocationCurve;
            model.LineLocation = (locationCurve.Curve.GetEndPoint(0) + locationCurve.Curve.GetEndPoint(1)) / 2;
            //平行,垂直 向量
            XYZ parallelVector = null;
            XYZ verticalVector = null;
            parallelVector = (locationCurve.Curve as Line).Direction;
            verticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
            parallelVector = LocationHelper.GetVectorByQuadrant(parallelVector, QuadrantType.OneAndFour);
            verticalVector = LocationHelper.GetVectorByQuadrant(verticalVector, QuadrantType.OneAndTwo);
            double xyzTolarance = 0.01;
            if (Math.Abs(verticalVector.X) > 1 - xyzTolarance)
            {
                verticalVector = new XYZ(-verticalVector.X, -verticalVector.Y, verticalVector.Z);
            }
            model.ParallelVector = parallelVector;
            model.VerticalVector = verticalVector;
            var height = Convert.ToDouble(line.GetParameters(TagProperty.线高度1.ToString()).First().AsValueString()) + (model.Texts.Count() - 1) * AnnotationConstaints.TextHeight;
            var textSize = PipeAnnotationContext.TextSize;
            var widthScale = PipeAnnotationContext.WidthScale;
            for (int i = 0; i < model.Texts.Count(); i++)
            {
                var textLength = System.Windows.Forms.TextRenderer.MeasureText(model.Texts[i], AnnotationConstaints.Font).Width;
                var actualLength = textLength / (textSize * widthScale);
                switch (model.CSALocationType)
                {
                    case CSALocationType.OnLine:
                        model.TextLocations.Add(GetTagHeadPositionWithParam(model, height, 0.2, 0.5, i, actualLength));
                        break;
                    case CSALocationType.OnEdge:
                        model.TextLocations.Add(GetTagHeadPositionWithParam(model, height, 0.2, 0.5, i, actualLength));
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 按XY的点的Z轴旋转
        /// </summary>
        /// <param name="point"></param>
        /// <param name="xyz"></param>
        /// <param name="verticalVector"></param>
        public static void RotateByXY(this LocationPoint point, XYZ xyz, XYZ verticalVector)
        {
            Line axis = Line.CreateBound(xyz, xyz.Add(new XYZ(0, 0, 10)));
            if (verticalVector.X > AnnotationConstaints.MiniValueForXYZ)
                point.Rotate(axis, 2 * Math.PI - verticalVector.AngleTo(new XYZ(0, 1, verticalVector.Z)));
            else
                point.Rotate(axis, verticalVector.AngleTo(new XYZ(0, 1, verticalVector.Z)));
        }

        /// <summary>
        /// 按XY的点的Z轴旋转
        /// </summary>
        /// <param name="point"></param>
        /// <param name="xyz"></param>
        /// <param name="verticalVector"></param>
        public static void RotateByXY(this Location point, XYZ xyz, XYZ verticalVector)
        {
            Line axis = Line.CreateBound(xyz, xyz.Add(new XYZ(0, 0, 10)));
            if (verticalVector.X > AnnotationConstaints.MiniValueForXYZ)
                point.Rotate(axis, 2 * Math.PI - verticalVector.AngleTo(new XYZ(0, 1, verticalVector.Z)));
            else
                point.Rotate(axis, verticalVector.AngleTo(new XYZ(0, 1, verticalVector.Z)));
        }


        /// <summary>
        /// 获取OnLineEdge方案的标注点位
        /// </summary>
        /// <param name="model">模型数据</param>
        /// <param name="height">线高度</param>
        /// <param name="horizontalFix">对应策略的水平修正</param>
        /// <param name="verizontalFix">对应策略的垂直修正</param>
        /// <param name="i">第几个标注文字</param>
        /// <param name="actualLength">文本长度</param>
        /// <returns></returns>
        private static XYZ GetTagHeadPositionWithParam(CSAModel model,  double height, double horizontalFix, double verizontalFix, int i, double actualLength)
        {
            XYZ parallelVector = model.ParallelVector;
            XYZ verticalVector = model.VerticalVector;
            XYZ startPoint = model.LineLocation;
            var result = startPoint + (UnitHelper.ConvertToFoot(height - i * AnnotationConstaints.TextHeight, AnnotationConstaints.UnitType)) * verticalVector
            + horizontalFix * parallelVector + verizontalFix * verticalVector
            + actualLength / 25.4 * parallelVector;
            return result;
        }
    }


    public class ViewModelBase : DependencyObject, INotifyPropertyChanged
    {

        #region INotifyPropertyChanged
        /// <summary>
        /// 实现INPC接口 监控属性改变
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    /// <summary>
    /// CompoundStructureAnnotation ViewModel
    /// </summary>
    public class CSAViewModel : ViewModelBase
    {
        public CSAViewModel(Document doc)
        {
            ViewType = CSAViewType.Idle;
            Model = new CSAModel(doc);
            LocationType = CSALocationType.OnLine;
        }

        public CSAViewType ViewType { set; get; }
        public CSAModel Model { set; get; }

        #region 绑定用属性 需在ViewModel中初始化
        CSALocationType LocationType
        {
            get
            {
                return Model.CSALocationType;
            }
            set
            {
                Model.CSALocationType = value;
                RaisePropertyChanged("IsDocument");
                RaisePropertyChanged("IsLinkDocument");
            }
        }
        public bool IsOnLine
        {
            get { return LocationType == CSALocationType.OnLine; }
            set { if (value) LocationType = CSALocationType.OnLine; }
        }
        public bool IsOnEdge
        {
            get { return LocationType == CSALocationType.OnEdge; }
            set { if (value) LocationType = CSALocationType.OnEdge; }
        } 

        public List<TextNoteType> TextNoteTypes { get { return Model.GetTextNoteTypes(); } }
        public ElementId TextNoteTypeElementId
        {
            get { return Model.TextNoteTypeElementId; }
            set
            {
                Model.TextNoteTypeElementId = value;
                this.RaisePropertyChanged("Type");
            }
        }
        #endregion


        public void Execute(CompoundStructureAnnotationWindow window, CompoundStructureAnnotationSet set, UIDocument uiDoc)
        {
            switch (ViewType)
            {
                case CSAViewType.Idle:
                    window = new CompoundStructureAnnotationWindow(set);
                    IntPtr rvtPtr = Autodesk.Windows.ComponentManager.ApplicationWindow;
                    WindowInteropHelper helper = new WindowInteropHelper(window);
                    helper.Owner = rvtPtr;
                    window.ShowDialog();
                    break;
                case CSAViewType.Select:
                    if (window.IsActive)
                        window.Close();
                    using (PmSoft.Common.RevitClass.PickObjectsMouseHook MouseHook = new PmSoft.Common.RevitClass.PickObjectsMouseHook())
                    {
                        MouseHook.InstallHook(PmSoft.Common.RevitClass.PickObjectsMouseHook.OKModeENUM.Objects);
                        try
                        {
                            Model.TargetId = uiDoc.Selection.PickObject(ObjectType.Element, new ClassFilter(typeof(Wall))).ElementId;
                            MouseHook.UninstallHook();
                            ViewType = CSAViewType.Generate;
                        }
                        catch (Exception ex)
                        {
                            MouseHook.UninstallHook();
                            ViewType = CSAViewType.Idle;
                        }
                    }
                    break;
                case CSAViewType.Generate:
                    var doc = uiDoc.Document;
                    if (TransactionHelper.DelegateTransaction(doc, "生成结构标注", (Func<bool>)(() =>
                    {
                        var element = doc.GetElement(Model.TargetId);
                        CompoundStructure compoundStructure = Model.GetCompoundStructure(element);//获取文本载体
                        if (compoundStructure == null)
                            return false;
                        var texts = Model.FetchTextsFromCompoundStructure(doc, compoundStructure);//获取文本数据
                        if (texts.Count == 0)
                            return false;
                        var lineFamilySymbol = CSAConstraints.GetMultipleTagSymbol(doc);//获取线标注类型
                        var line = doc.Create.NewFamilyInstance(new XYZ(0,0,0), lineFamilySymbol, doc.ActiveView);//生成 线
                        Model.FetchLocations(element,line);//计算内容定位
                        var targetLocation = Model.LineLocation;
                        var lineLocation = Model.LineLocation;
                        var textLocations = Model.TextLocations;
                        ElementTransformUtils.MoveElement(doc, line.Id, lineLocation);//线定位
                        LocationPoint locationPoint = line.Location as LocationPoint;//线 旋转处理
                        locationPoint.RotateByXY(lineLocation, Model.VerticalVector);
                        Model.LineId = line.Id;
                        Model.UpdateLineParameters(line, 500, 200, AnnotationConstaints.TextHeight, Model.Texts.Count());//线参数设置
                        List<TextNote> textNotes = new List<TextNote>();
                        foreach (var text in Model.Texts)//生成 文本
                        {
                            var textLocation = textLocations[Model.Texts.IndexOf(text)];
                            var textNote = TextNote.Create(doc, doc.ActiveView.Id, textLocation, text, Model.TextNoteTypeElementId);
                            textNotes.Add(textNote);
                            textNote.Location.RotateByXY(textLocation, Model.VerticalVector);
                        }
                        Model.TextNoteIds = textNotes.Select(c => c.Id).ToList();
                        return true;
                    })))
                        ViewType = CSAViewType.Select;
                    else
                        ViewType = CSAViewType.Idle;
                    break;
                case CSAViewType.Close:
                    window.Close();
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// CompoundStructureAnnotation_Constraints
    /// </summary>s
    public class CSAConstraints
    {
        private static FamilySymbol MultipleTagSymbol { set; get; }
        public static FamilySymbol GetMultipleTagSymbol(Document doc)
        {
            if (MultipleTagSymbol == null || !MultipleTagSymbol.IsValidObject)
                LoadFamilySymbols(doc);
            return MultipleTagSymbol;
        }

        /// <summary>
        /// 获取标注族
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static bool LoadFamilySymbols(Document doc)
        {
            MultipleTagSymbol = FamilySymbolHelper.LoadFamilySymbol(doc, "结构做法标注", "引线标注_文字在右端");
            return true;
        }
    }
}