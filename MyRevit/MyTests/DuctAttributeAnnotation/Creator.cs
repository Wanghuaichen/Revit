﻿using Autodesk.Revit.DB;
using MyRevit.MyTests.DAA;

namespace MyRevit.MyTests.DAA
{
    public class DAACreator
    {
        public void Generate(Document doc, DAAModel model)
        {
            Generate(doc, model);
        }
        public void Generate(Document doc, DAAModel model, Element element, XYZ offset)
        {
            //CompoundStructure compoundStructure = model.GetCompoundStructure(element);//获取文本载体
            //if (compoundStructure == null)
            //    return;
            //var texts = model.FetchTextsFromCompoundStructure(doc, compoundStructure);//获取文本数据
            //if (texts.Count == 0)
            //    return;
            //if (texts.Count == 1)
            //{
            //    TaskDialog.Show("警告", "暂不支持单层的结构做法标注");
            //}
            //else
            //{
            //    model.TargetId = element.Id;//主体
            //    var lineFamilySymbol = VLConstraintsForCSA.GetMultipleTagSymbol(doc);//获取线标注类型 
            //    bool isRegenerate = offset != null;
            //    FamilyInstance line;
            //    if (isRegenerate)
            //    {
            //        line = doc.GetElement(model.LineId) as FamilyInstance;
            //        model.CalculateLocations(element, line, offset);//计算内容定位
            //        Clear(doc, model);
            //        line = doc.Create.NewFamilyInstance(new XYZ(0, 0, 0), lineFamilySymbol, doc.ActiveView);//生成 线
            //    }
            //    else
            //    {
            //        line = doc.Create.NewFamilyInstance(new XYZ(0, 0, 0), lineFamilySymbol, doc.ActiveView);//生成 线
            //        model.CalculateLocations(element, line, offset);//计算内容定位
            //    }
            //    var lineLocation = model.LineLocation;
            //    var textLocations = model.TextLocations;
            //    ElementTransformUtils.MoveElement(doc, line.Id, lineLocation);//线定位
            //    LocationPoint locationPoint = line.Location as LocationPoint;//线 旋转处理
            //    locationPoint.RotateByXY(lineLocation, model.VerticalVector);
            //    model.LineId = line.Id;
            //    model.UpdateLineParameters(line, model.LineHeight, model.LineWidth, model.LineSpace, model.Texts.Count());//线参数设置
            //    List<TextNote> textNotes = new List<TextNote>();
            //    foreach (var text in model.Texts)//生成 文本
            //    {
            //        var textLocation = model.TextLocations[model.Texts.IndexOf(text)];
            //        var textNote = TextNote.Create(doc, doc.ActiveView.Id, textLocation, text, model.TextNoteTypeElementId);
            //        textNotes.Add(textNote);
            //        textNote.Location.RotateByXY(textLocation, model.VerticalVector);
            //    }
            //    model.TextNoteIds = textNotes.Select(c => c.Id).ToList();
            //    //测试用
            //    //GraphicsDisplayerManager.Display(@"E:\WorkingSpace\Outputs\Images\1023结构做法标注.png", lines, Model.TextLocations);
            //}
        }
    }
}
