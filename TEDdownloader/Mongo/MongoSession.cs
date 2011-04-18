using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Norm.Collections;
using Norm.Linq;
using Norm.Responses;
using Norm;


namespace TEDdownloader
{
    internal class MongoSession : IDisposable
    {

        private string _connectionString;

        public MongoSession()
        {
            _connectionString = "mongodb://localhost/test";
        }

        public MongoSession(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Delete<T>(System.Linq.Expressions.Expression<Func<T, bool>> expression) where T : class, new()
        {
            //not efficient, NoRM should do this in a way that sends a single command to MongoDB.
            var items = All<T>().Where(expression);
            foreach (T item in items)
            {
                Delete(item);
            }
        }

        public void Delete<T>(T item) where T : class, new()
        {
            using (var db = Mongo.Create(_connectionString))
            {
                db.Database.GetCollection<T>().Delete(item);
            }
        }

        public void DeleteAll<T>() where T : class, new()
        {
            using (var db = Mongo.Create(_connectionString))
            {
                db.Database.DropCollection(typeof(T).Name);
            }
        }

        public T Single<T>(System.Linq.Expressions.Expression<Func<T, bool>> expression) where T : class, new()
        {
            T retval = default(T);
            using (var db = Mongo.Create(_connectionString))
            {
                retval = db.GetCollection<T>().AsQueryable()
                           .Where(expression).SingleOrDefault();
            }
            return retval;
        }

        public IQueryable<T> All<T>() where T : class, new()
        {
            //don't keep this longer than you need it.
            var db = Mongo.Create(_connectionString);
            return db.GetCollection<T>().AsQueryable();
        }

        public void Add<T>(T item) where T : class, new()
        {
            using (var db = Mongo.Create(_connectionString))
            {
                db.GetCollection<T>().Insert(item);
            }
        }
        
        public void Save<T>(T item) where T : class, new()
        {
            using (var db = Mongo.Create(_connectionString))
            {
                db.GetCollection<T>().Save(item);
            }
        }

        public void Add<T>(IEnumerable<T> items) where T : class, new()
        {
            //this is WAY faster than doing single inserts.
            using (var db = Mongo.Create(_connectionString))
            {
                db.GetCollection<T>().Insert(items);
            }
        }

        public void Update<T>(T item) where T : class, new()
        {
            using (var db = Mongo.Create(_connectionString))
            {
                db.GetCollection<T>().UpdateOne(item, item);
                //db.GetCollection<T>().U
                
            }
        }

        public void Dispose()
        {
            this.Dispose();
        }
    
    }
}
