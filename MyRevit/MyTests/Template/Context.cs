﻿using Autodesk.Revit.DB;
using PmSoft.Optimization.DrawingProduction;
using PmSoft.Optimization.DrawingProduction.Utils;

namespace MyRevit.MyTests.Template
{
    class TemplateContext
    {
        #region Creator
        private static TemplateCreator _Creator = null;
        public static TemplateCreator Creator
        {
            get
            {
                return _Creator ?? (_Creator = new TemplateCreator());
            }
        }
        #endregion


        #region Storage
        public static bool IsEditing;
        private static TemplateModelCollection Collection;
        private static CollectionStorageEntity CStorageEntity = new CollectionStorageEntity();
        /// <summary>
        /// 取数据Collection
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static TemplateModelCollection GetCollection(Document doc)
        {
            //if (Collection != null)
            //    return Collection;
            Collection = DelegateHelper.DelegateTryCatch(
                () =>
                {
                    string data = ExtensibleStorageHelper.GetData(doc, CStorageEntity, CStorageEntity.FieldOfData);
                    return new TemplateModelCollection(data);
                },
                () =>
                {
                    return new TemplateModelCollection("");
                }
            );
            return Collection;
        }

        /// <summary>
        /// 保存Collection
        /// </summary>
        /// <param name="doc"></param>
        public static bool Save(Document doc)
        {
            if (Collection == null)
                return false;
            var data = Collection.ToData();
            return DelegateHelper.DelegateTryCatch(
                () =>
                {
                    ExtensibleStorageHelper.SetData(doc, CStorageEntity, CStorageEntity.FieldOfData, data);
                    return true;
                },
                () =>
                {
                    ExtensibleStorageHelper.RemoveStorage(doc, CStorageEntity);
                    ExtensibleStorageHelper.SetData(doc, CStorageEntity, CStorageEntity.FieldOfData, data);
                    return false;
                }
            );
        }
        #endregion

    }
}