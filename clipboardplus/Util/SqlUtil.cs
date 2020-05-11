using clipboardplus.Model;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace clipboardplus.Util
{
    public class DbSet<T> : SimpleClient<T> where T : class, new()
    {
        public DbSet(SqlSugarClient context) : base(context)
        {

        }
        //SimpleClient中的方法满足不了你，你可以扩展自已的方法
        public List<T> GetByIds(dynamic[] ids)
        {
            return Context.Queryable<T>().In(ids).ToList(); ;
        }
    }

    public class SqlUtil<T> where T : class, new()
    {
        public SqlUtil(StringBuilder dbpath, DbType dbType)
        {
            if (dbpath != null)
            {
                dbPath = dbpath.Append("/clipboardplus.db").ToString();
            }
            Console.WriteLine(dbPath);
            Console.WriteLine(dbType.ToString());
            Db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = dbPath,
                DbType = dbType,
                InitKeyType = InitKeyType.Attribute,//从特性读取主键和自增列信息
                IsAutoCloseConnection = true
            });
            //调式代码 用来打印SQL 
            Db.Aop.OnLogExecuting = (sql, pars) =>
            {
                Console.WriteLine(sql + "\r\n" +
                    Db.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
                Console.WriteLine();
            };
            Db.CodeFirst.InitTables<Record>();//Create Tables
            Db.CodeFirst.InitTables<Zone>();//Create Tables
        }
        private string dbPath = Path.Combine("DataSource = " + Environment.CurrentDirectory, "clipboardplus.db");//没有数据库会创建数据库
        public SqlSugarClient Db;//用来处理事务多表查询和复杂的操作
        public DbSet<Zone> ZoneDb { get { return new DbSet<Zone>(Db); } }//用来处理Student表的常用操作
        public DbSet<T> CurrentDb { get { return new DbSet<T>(Db); } }//用来处理T表的常用操作
    }
}