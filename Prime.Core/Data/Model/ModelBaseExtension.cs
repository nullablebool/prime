using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LiteDB;
using System.Linq;

namespace Prime.Core
{
    public static class ModelBaseExtension
    {
        public static OpResult SavePublic<T>(this T doc) where T : IModelBase
        {
            return OpResult.From(PublicContext.I.GetDb().Upsert(doc));
        }

        public static OpResult DeletePublic<T>(this T doc) where T : IModelBase
        {
            return OpResult.From(PublicContext.I.GetDb().Delete<T>(doc.Id));
        }

        public static OpResult Save<T>(this T doc, IDataContext context) where T : IModelBase
        {
            return OpResult.From(context.GetDb().Upsert(doc));
        }

        public static OpResult Delete<T>(this T doc, IDataContext context) where T : IModelBase
        {
            return OpResult.From(context.GetDb().Delete<T>(doc.Id));
        }

        public static T FirstOrDefault<T>(this LiteQueryable<T> query,  Expression<Func<T, bool>> predicate)
        {
            return query.Where(predicate).FirstOrDefault();
        }

        public static T CloneBson<T>(this T model)
        {
            var doc = BsonMapper.Global.ToDocument(model.GetType(), model);
            return BsonMapper.Global.ToObject<T>(doc);
        }

        public static OpResult SaveAll<T>(this IEnumerable<T> doc, IDataContext context) where T : IModelBase
        {
            return doc.Select(x => x.Save(context)).FirstOrDefault(x => !x.IsSuccess);
        }
    }
}