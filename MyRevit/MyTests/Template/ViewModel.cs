﻿using System;
using Autodesk.Revit.UI;
using MyRevit.MyTests.VLBase;
using MyRevit.Utilities;
using Autodesk.Revit.UI.Selection;
using System.Linq;
using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace MyRevit.MyTests.Template
{
    public enum TemplateViewType
    {
        Closing = -1,//右上角或Alt+F4关闭
        Close = 0,//按钮关闭或ESC关闭
        Idle = 1,//闲置
        PickSinglePipe_Pipe,//选择单管 管道
        PickSinglePipe_Location,//选择单管 定位
        GenerateSinglePipe,//单管标注生成
        PickMultiplePipes,//选择多管
    }

    public class TemplateViewModel : VLViewModel<TemplateModel, TemplateWindow>
    {
        public TemplateViewModel(UIApplication app) : base(app)
        {
            Model = new TemplateModel("");
            View = new TemplateWindow(this);
            //用以打开时更新页面
            //LoadSetting();
            AnnotationType = TemplateAnnotationType.SPL;
            LocationType = TemplateLocationType.Center;
        }

        public override void Execute()
        {
            var viewType = (TemplateViewType)Enum.Parse(typeof(TemplateViewType), ViewType.ToString());
            switch (viewType)
            {
                case TemplateViewType.Idle:
                    View = new TemplateWindow(this);
                    View.ShowDialog();
                    break;
                case TemplateViewType.Close:
                    View.Close();
                    //SaveSetting();
                    break;
                case TemplateViewType.Closing:
                    //SaveSetting();
                    break;
                case TemplateViewType.PickSinglePipe_Pipe:
                    View.Close();
                    if (!VLHookHelper.DelegateKeyBoardHook(() =>
                    {
                        //业务逻辑处理
                        //选择符合类型的过滤
                        var targetType = Model.GetFilter();
                        Model.TargetIds = new List<ElementId>() { UIDocument.Selection.PickObject(ObjectType.Element, targetType, "请选择管道标注点").ElementId };
                        if (Model.TargetIds.Count > 0)
                            ViewType = (int)TemplateViewType.PickSinglePipe_Location;
                    }))
                        ViewType = (int)TemplateViewType.Idle;
                    Execute();
                    break;
                case TemplateViewType.PickSinglePipe_Location:
                    if (!VLHookHelper.DelegateKeyBoardHook(() =>
                    {
                        ////业务逻辑处理
                        //var target = Document.GetElement(Model.TargetIds.First());
                        //var targetLocation = target.Location as LocationCurve;
                        //var p0 = targetLocation.Curve.GetEndPoint(0);
                        //var p1 = targetLocation.Curve.GetEndPoint(1);
                        //var pStart = new XYZ((p0.X + p1.X) / 2, (p0.Y + p1.Y) / 2, (p0.Z + p1.Z) / 2);
                        //var pEnd = new VLPointPicker().PickPointWithLinePreview(UIApplication, pStart);
                        //if (pEnd == null)
                        //    ViewType = TemplateViewType.Idle;
                        //else
                        //    ViewType = TemplateViewType.GenerateSinglePipe;
                    }))
                        ViewType = (int)TemplateViewType.Idle;
                    Execute();
                    break;
                case TemplateViewType.GenerateSinglePipe:
                    if (VLTransactionHelper.DelegateTransaction(Document, "GenerateSinglePipe", (Func<bool>)(() =>
                        {
                            var element = Document.GetElement(Model.TargetIds.First());
                            var Collection = TemplateContext.GetCollection(Document);
                            //避免重复生成 由于一个对象可能在不同的视图中进行标注设置 所以还是需要重复生成的
                            var existedModels = Collection.Data.Where(c => c.TargetIds.Intersect(Model.TargetIds, new VLElementIdComparer()) != null);
                            if (existedModels != null)
                            {
                                foreach (var existedModel in existedModels)
                                {
                                    Collection.Data.Remove(existedModel);
                                    TemplateContext.Creator.Clear(Document, existedModel);
                                }
                            }
                            TemplateContext.Creator.Generate(Document, Model, element);
                            Collection.Data.Add(Model);
                            Collection.Save(Document);
                            return true;
                        })))
                            ViewType = (int)TemplateViewType.PickSinglePipe_Pipe;
                    else
                        ViewType = (int)TemplateViewType.Idle;
                    Execute();
                    break;
                default:
                    throw new NotImplementedException("功能未实现");
            }
        }

        #region RatioButtons

        #region TemplateTargetType
        TemplateTargetType TargetType
        {
            get
            {
                return Model.TargetType;
            }
            set
            {
                Model.TargetType = value;
                RaisePropertyChanged("TargetType_Pipe");
                RaisePropertyChanged("TargetType_Duct");
                RaisePropertyChanged("TargetType_CableTray");
                RaisePropertyChanged("TargetType_Conduit");
            }
        }
        public bool TargetType_Pipe
        {
            get { return TargetType == TemplateTargetType.Pipe; }
            set { if (value) TargetType = TemplateTargetType.Pipe; }
        }
        public bool TargetType_Duct
        {
            get { return TargetType == TemplateTargetType.Duct; }
            set { if (value) TargetType = TemplateTargetType.Duct; }
        }
        public bool TargetType_CableTray
        {
            get { return TargetType == TemplateTargetType.CableTray; }
            set { if (value) TargetType = TemplateTargetType.CableTray; }
        }
        public bool TargetType_Conduit
        {
            get { return TargetType == TemplateTargetType.Conduit; }
            set { if (value) TargetType = TemplateTargetType.Conduit; }
        }
        #endregion

        #region TemplateAnnotationType
        TemplateAnnotationType AnnotationType
        {
            get
            {
                return Model.AnnotationType;
            }
            set
            {
                Model.AnnotationType = value;
                RaisePropertyChanged("AnnotationType_SPL");
                RaisePropertyChanged("AnnotationType_SL");
                RaisePropertyChanged("AnnotationType_PL");
                UpdateModelAnnotationPrefix();
            }
        }
        public bool AnnotationType_SPL
        {
            get { return AnnotationType == TemplateAnnotationType.SPL; }
            set { if (value) AnnotationType = TemplateAnnotationType.SPL; }
        }
        public bool AnnotationType_SL
        {
            get { return AnnotationType == TemplateAnnotationType.SL; }
            set { if (value) AnnotationType = TemplateAnnotationType.SL; }
        }
        public bool AnnotationType_PL
        {
            get { return AnnotationType == TemplateAnnotationType.PL; }
            set { if (value) AnnotationType = TemplateAnnotationType.PL; }
        }
        #endregion

        #region TemplateLocationType
        TemplateLocationType LocationType
        {
            get
            {
                return Model.LocationType;
            }
            set
            {
                Model.LocationType = value;
                RaisePropertyChanged("LocationType_Center");
                RaisePropertyChanged("LocationType_Top");
                RaisePropertyChanged("LocationType_Bottom");
                UpdateModelAnnotationPrefix();
            }
        }
        public bool LocationType_Center
        {
            get { return LocationType == TemplateLocationType.Center; }
            set { if (value) LocationType = TemplateLocationType.Center; }
        }
        public bool LocationType_Top
        {
            get { return LocationType == TemplateLocationType.Top; }
            set { if (value) LocationType = TemplateLocationType.Top; }
        }
        public bool LocationType_Bottom
        {
            get { return LocationType == TemplateLocationType.Bottom; }
            set { if (value) LocationType = TemplateLocationType.Bottom; }
        }
        #endregion

        #region TemplateTextType
        TemplateTextType TextType
        {
            get
            {
                return Model.TextType;
            }
            set
            {
                Model.TextType = value;
                RaisePropertyChanged("TextType_OnLine");
                RaisePropertyChanged("TextType_OnEdge");
            }
        }
        public bool TextType_OnLineOrOnLeft
        {
            get { return TextType == TemplateTextType.TextType_OnLineOrOnLeft; }
            set { if (value) TextType = TemplateTextType.TextType_OnLineOrOnLeft; }
        }
        public bool TextType_OnEdgeOrOnMiddle
        {
            get { return TextType == TemplateTextType.TextType_OnEdgeOrOnMiddle; }
            set { if (value) TextType = TemplateTextType.TextType_OnEdgeOrOnMiddle; }
        }
        #endregion

        #region Texts
        private string centerPrefix = "CL+";
        public string CenterPrefix
        {
            get
            {
                return centerPrefix;
            }

            set
            {
                centerPrefix = value;
                RaisePropertyChanged("SPLPreview");
                RaisePropertyChanged("SLPreview");
                RaisePropertyChanged("PLPreview");
            }
        }

        private void UpdateModelAnnotationPrefix()
        {
            if (Model.LocationType == TemplateLocationType.Center)
                Model.AnnotationPrefix = CenterPrefix;
            else if (Model.LocationType == TemplateLocationType.Top)
                Model.AnnotationPrefix = TopPrefix;
            else if (Model.LocationType == TemplateLocationType.Bottom)
                Model.AnnotationPrefix = BottomPrefix;
        }

        private string topPrefix = "TL+";
        public string TopPrefix
        {
            get
            {
                return topPrefix;
            }

            set
            {
                topPrefix = value;
                RaisePropertyChanged("SPLPreview");
                RaisePropertyChanged("SLPreview");
            }
        }

        private string bottomPrefix = "BL+";
        public string BottomPrefix
        {
            get
            {
                return bottomPrefix;
            }

            set
            {
                bottomPrefix = value;
                RaisePropertyChanged("SPLPreview");
                RaisePropertyChanged("SLPreview");
            }
        }

        public string SPLPreview { get { return Model.GetPreview(); } }
        public string SLPreview { get { return Model.GetPreview(); } }
        public string PLPreview { get { return Model.GetPreview(); } }
        #endregion

        #endregion

    }
}
