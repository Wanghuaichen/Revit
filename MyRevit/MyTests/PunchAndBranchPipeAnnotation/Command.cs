﻿using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace MyRevit.MyTests.PBPA
{
    [Transaction(TransactionMode.Manual)]
    class PBPACommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var app = commandData.Application.Application;
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = commandData.Application.ActiveUIDocument.Document;
            return new PBPASet(uiApp).DoCmd() ? Result.Succeeded : Result.Failed;
        }
    }
}
