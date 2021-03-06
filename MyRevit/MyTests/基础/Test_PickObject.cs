﻿using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MyRevit.MyTests.Utilities;
using System.Collections.Generic;

namespace MyRevit.MyTests.基础
{
    [Transaction(TransactionMode.Manual)]
    public class Test_PickObject : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var app = commandData.Application.Application;
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = commandData.Application.ActiveUIDocument.Document;
            var filterOfBranchPipe = new VLClassFilter(typeof(FamilyInstance), false, (element) =>
              {
                  var familySymbol = doc.GetElement(element.GetTypeId()) as FamilySymbol;
                  if (familySymbol == null)
                      return false;
                  return new List<string>() { "圆形套管", "椭圆形套管", "矩形套管" }.Contains(familySymbol.Family.Name);
              });
            var filterOfPunch = new VLClassFilter(typeof(FamilyInstance), false, (element) =>
            {
                var familySymbol = doc.GetElement(element.GetTypeId()) as FamilySymbol;
                if (familySymbol == null)
                    return false;
                return new List<string>() { "圆形洞口", "椭圆形洞口", "矩形洞口" }.Contains(familySymbol.Family.Name);
            });
            var elementReference = uiDoc.Selection.PickObject(ObjectType.Element, filterOfBranchPipe | filterOfPunch, "选择要添加的构件");
            if (elementReference == null)
                return Result.Cancelled;
            return Result.Succeeded;
        }
    }
}