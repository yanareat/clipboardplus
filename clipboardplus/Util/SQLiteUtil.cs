using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class SQLiteUtil<E>
{
    public SQLiteUtil(StringBuilder dbpath)
    {
        if (dbpath != null)
        {
            dbPath = dbpath.Append("/clipboardplus.db").ToString();
        }
        Console.WriteLine(dbPath);
        db = new SQLiteConnection(dbPath);
        db.CreateTable<E>();//表已存在不会重复创建
    }

    #region 属性
    /// <summary>
    /// 数据库路径
    /// </summary>
    private string dbPath = Path.Combine(Environment.CurrentDirectory, "clipboardplus.db");//没有数据库会创建数据库
    
    /// <summary>
    /// 数据库连接
    /// </summary>
    private SQLiteConnection db;
    #endregion

    #region 方法
    /// <summary>
    /// 添加
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="model"></param>
    /// <returns></returns>
    public int Add<T>(T model)
    {
        return db.Insert(model);
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="model"></param>
    /// <returns></returns>
    public int Update<T>(T model)
    {
        return db.Update(model);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="model"></param>
    /// <returns></returns>
    public int Delete<T>(T model)
    {
        return db.Update(model);
    }

    /// <summary>
    /// 查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <returns></returns>
    public List<T> Query<T>(string sql) where T : new()
    {
        return db.Query<T>(sql);
    }

    /// <summary>
    /// SQL执行
    /// </summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    public int Execute(string sql)
    {
        return db.Execute(sql);
    }
    #endregion
}