using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoDB.Bson;

namespace TEDdownloader
{
    class RepositoryClass
    {
        private MongoDatabase _db;

        public MongoDatabase Db()
        {
            if (_db != null) return _db;
            MongoUrl url = new MongoUrl("mongodb://localhost/?safe=true");

            string connectionString = "mongodb://localhost";
            MongoServer server = MongoServer.Create(connectionString);

            //var server = new MongoServer();
            server.Connect();
            _db = server.GetDatabase("test");
            return _db;
        }

        //public void Insert(Document document, string collectionName)
        //{
        //    var collection = Db().GetCollection(collectionName);
        //    collection.Insert(document);
        //}

        //public IEnumerable<TDocument> getListOf<TDocument>(string whereClause, string fromCollection) 
        //{
        //    var docs = Db().GetCollection(fromCollection)
        //    .Find(whereClause).Documents;     
        //    return docsToCollection<TDocument>(docs);
        //}

        //private IEnumerable<TDocument> docsToCollection<TDocument>(IEnumerable<Document> documents) 
        //{
        //    var list = new List<TDocument>();
        //    var settings = new JsonSerializerSettings();

        //    foreach (var document in documents)
        //    {
        //        var docType = Activator.CreateInstance<TDocument>();
        //        docType.InternalDocument = document;
        //        list.Add(docType);
        //    }
        //    return list;
        //}	

    }
}
