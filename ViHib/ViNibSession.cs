using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;

using ViHib.Attributes;
using ViHib.Exceptions;
using ViHib.Utils;


namespace ViHib
{
    public class ViHibSession : IDisposable
    {
        private ViHibConfiguration conf;
        private DbConnection m_connection;
        private DbTransaction m_tranaction;

        private static object lc = new object();
        private Dictionary<Type, HidTypeInfo> loadedTypes = new Dictionary<Type, HidTypeInfo>();

        public ViHibSession(ViHibConfiguration c)
        {
            conf = c;

            DbProviderFactory m_factory = DbProviderFactories.GetFactory(conf.providerName);
            m_connection = m_factory.CreateConnection();
            m_connection.ConnectionString = conf.connectionString;
            m_connection.Open();
            m_tranaction = m_connection.BeginTransaction();

            #region Заглавная таблица

            using (var com = m_connection.CreateCommand())
            {
                com.Transaction = m_tranaction;
                try
                {
                    com.CommandText = "select count(1) as c from " + HibTableName;

                    using (var r = com.ExecuteReader())
                    {
                        r.Read();
                        r.Close();
                    }
                }
                catch (Exception)
                {
                    com.CommandText = string.Format(@"CREATE TABLE [{1}](
		[parcode] {0} NOT NULL,
		[parval] {0}  ) 

", conf.GetSqlCon(typeof(string).ToString()), HibTableName);

                    com.ExecuteNonQuery();
                }
            }

            #endregion
        }

        string HibTableName
        {
            get
            {
                return conf.TabelPrfix + "ViHid";
            }
        }

        string IdByType(Type type)
        {

            return loadedTypes[type].idInfo.colName;

        }

        public void AddClass(Type type)
        {
            if (!Attribute.IsDefined(type, typeof(Table))) // проверка на существование атрибута
                throw new NotCompatibleClass();

            HidTypeInfo hti = new HidTypeInfo();

            List<MemberInfo> pks = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(n => Attribute.IsDefined(n, typeof(Id))).ToList();

            if (pks.Count != 1)
                throw new PrimaryKeyException();

            List<MemberInfo> columns = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(n => Attribute.IsDefined(n, typeof(Id)) || Attribute.IsDefined(n, typeof(Column))).ToList();

            var tt = (Attribute.GetCustomAttribute(type, typeof(Table)) as Table);

            string tableName = conf.TabelPrfix + "t" + (tt.Name ?? type.Name);
            if (tt.force)
                tableName = tt.Name;
            if(string.IsNullOrEmpty(tableName))
                throw new NotCompatibleClass();

            hti.TableName = tableName;

            if (tableName == HibTableName)
                throw new TypeNameError();

            WriteUtil.WriteLine(tableName);

            string idColName = "";
            bool cr = false; // create table
            bool dt = false; // delete table

            string ct = "CREATE TABLE [" + tableName + "](";

            foreach (var c in columns)
            {
                Column col = null;
                var otrs = c.GetCustomAttributes(typeof(Column), true);
                if (otrs.Length != 0)
                    col = (Column)otrs[0];

                Id id = null;
                otrs = c.GetCustomAttributes(typeof(Id), true);
                if (otrs.Length != 0)
                    id = (Id)otrs[0];

                string ctype = GetSqlTypeByMem(c);

                string name = (col?.Name) ?? c.Name;

                ct += ("[" + name + "] " + ctype + " ") + (id != null ? " NOT NULL " : "") + ", \n";

                HidTypeInfo.HidFieldInfo h = new HidTypeInfo.HidFieldInfo()
                {
                    Mem = c,
                    type = HidTypeInfo.HidFieldInfo.MemType(c),
                    colName = name,
                    SQlType = GetTypeClassByMem(c)
                };

                if (id != null)
                { idColName = name; hti.idInfo = h; }

                hti.fields.Add(h);
            }


            ct += " CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED (	[" + idColName + "] ) ) ";

            using (var com = m_connection.CreateCommand())
            {
                com.Transaction = m_tranaction;
                com.CommandText = "select parval from " + HibTableName + " where parcode = " + conf.parPre + "p";
                var p = com.CreateParameter();
                p.DbType = System.Data.DbType.String;
                p.ParameterName = "p";
                p.Value = "Table_HashCode_" + tableName;
                com.Parameters.Add(p);

                using (var rd = com.ExecuteReader())
                {
                    if (!rd.Read() || rd[0].ToString() != ct.GetHashCode().ToString())
                    {
                        cr = true; dt = true;
                    }


                    rd.Close();
                }
            }

            if (cr)
            {
                if (dt)
                    using (var com = m_connection.CreateCommand())
                    {
                        com.Transaction = m_tranaction;
                        try
                        {
                            com.CommandText = "DROP TABLE " + tableName;
                            com.ExecuteNonQuery();
                        }
                        catch (Exception) { }
                    }

                using (var com = m_connection.CreateCommand())
                {
                    com.Transaction = m_tranaction;
                    com.CommandText = ct;
                    com.ExecuteNonQuery();
                }

                using (var com = m_connection.CreateCommand())
                {
                    com.Transaction = m_tranaction;
                    com.CommandText = "INSERT INTO " + HibTableName + " (parcode , parval ) VALUES (" + conf.parPre + "p1 , " + conf.parPre + "p2)";

                    var p = com.CreateParameter();
                    p.DbType = System.Data.DbType.String;
                    p.ParameterName = "p1";
                    p.Value = "Table_HashCode_" + tableName;
                    com.Parameters.Add(p);

                    p = com.CreateParameter();
                    p.DbType = System.Data.DbType.String;
                    p.ParameterName = "p2";
                    p.Value = ct.GetHashCode().ToString();
                    com.Parameters.Add(p);

                    com.ExecuteNonQuery();

                    this.Commit();
                }

            }

            loadedTypes.Add(type, hti);

            //    m_tranaction.Commit();
        }

        private void NextSeq(object o)
        {
            var type = o.GetType();
            var mem = loadedTypes[type];

            string tableName = mem.TableName;

            foreach (var c in mem.fields)
            {

                SequenceGenerator seq = null;
                var otrs = c.Mem.GetCustomAttributes(typeof(SequenceGenerator), true);
                if (otrs.Length != 0)
                    seq = (SequenceGenerator)otrs[0];

                if (seq == null)
                    continue;

                lock (lc)
                {
                    DbProviderFactory m_factory = DbProviderFactories.GetFactory(conf.providerName);
                    using (var _connection = m_factory.CreateConnection())
                    {
                        _connection.ConnectionString = conf.connectionString;
                        _connection.Open();

                        //  var tr = _connection.BeginTransaction();

                        using (var com = _connection.CreateCommand())
                        {
                            //  com.Transaction = tr;
                            com.CommandText = "select parval from " + HibTableName + " where parcode = " + conf.parPre + "p";
                            var p = com.CreateParameter();
                            p.DbType = System.Data.DbType.String;
                            p.ParameterName = "p";
                            p.Value = "seq_state_" + tableName + "_" + c.colName;
                            com.Parameters.Add(p);

                            using (var rd = com.ExecuteReader())
                            {
                                if (!rd.Read())
                                {
                                    rd.Close();
                                    com.CommandText = "INSERT INTO " + HibTableName + " (parcode , parval ) VALUES (" + conf.parPre + "p1 , " + conf.parPre + "p2)";

                                    p = com.CreateParameter();
                                    p.DbType = System.Data.DbType.String;
                                    p.ParameterName = "p1";
                                    p.Value = "seq_state_" + tableName + "_" + c.colName;
                                    com.Parameters.Add(p);

                                    p = com.CreateParameter();
                                    p.DbType = System.Data.DbType.String;
                                    p.ParameterName = "p2";
                                    p.Value = seq.MinValue - seq.Incriment;
                                    com.Parameters.Add(p);

                                    com.ExecuteNonQuery();
                                }
                                else
                                    rd.Close();
                            }

                            com.CommandText = "select parval from " + HibTableName + " where parcode = " + conf.parPre + "p";
                            com.Parameters.Clear();
                            p = com.CreateParameter();
                            p.DbType = System.Data.DbType.String;
                            p.ParameterName = "p";
                            p.Value = "seq_state_" + tableName + "_" + c.colName;
                            com.Parameters.Add(p);


                            Int64 tmp = 0;

                            using (var rd = com.ExecuteReader())
                            {
                                rd.Read();
                                tmp = Convert.ToInt64(rd[0]) + seq.Incriment;
                                rd.Close();
                            }

                            if (c.type != typeof(Int64)) throw new ErrorSeqColumnType();
                            c.SetValue(o, tmp);


                            if (tmp >= seq.MaxValue)
                                throw new ExSeqLimit();

                            com.CommandText = "update  " + HibTableName + " set parval = " + conf.parPre + "v where parcode = " + conf.parPre + "p";
                            com.Parameters.Clear();
                            p = com.CreateParameter();
                            p.DbType = System.Data.DbType.String;
                            p.ParameterName = "p";
                            p.Value = "seq_state_" + tableName + "_" + c.colName;
                            com.Parameters.Add(p);

                            p = com.CreateParameter();
                            p.DbType = System.Data.DbType.String;
                            p.ParameterName = "v";
                            p.Value = tmp.ToString();
                            com.Parameters.Add(p);

                            com.ExecuteNonQuery();

                        }

                        //  tr.Commit();

                        _connection.Close();
                    }

                }
            }

        }

        public void Save(object o)
        {
            NextSeq(o);

            var type = o.GetType();

            var mem = loadedTypes[type];

            string tableName = mem.TableName;

            string inst = "insert into " + tableName + " (";
            string inst2 = " VALUES ( ";
            List<object> pps = new List<object>();
            int k = 0;

            foreach (var c in mem.fields)
            {

                string ctype = GetSqlTypeByMem(c.Mem);

                var cl = GetTypeClassByMem(c.Mem);

                string name = c.colName;

                inst += name + ",";
                inst2 += conf.parPre + "p" + k++ + ",";

                switch (cl)
                {
                    case HidTypeInfo.HibTypeClass.Def:
                        pps.Add(c.GetValue(o));
                        break;
                    case HidTypeInfo.HibTypeClass.External:
                        var idb = loadedTypes[c.type].idInfo.GetValue(c.GetValue(o));
                        if (InBase(idb, c.type))
                            pps.Add(idb);
                        else
                            throw new NonLoadedExternalDate();
                        break;
                    case HidTypeInfo.HibTypeClass.Xml:
                        pps.Add(XmlSer.ToXmlString(c.GetValue(o), c.type));
                        break;

                    default:
                        throw new HidException();
                }


            }

            inst = inst.TrimEnd(',');
            inst2 = inst2.TrimEnd(',');

            inst += ") " + inst2 + ")";


            using (var com = m_connection.CreateCommand())
            {
                com.Transaction = m_tranaction;
                com.CommandText = inst;
                for (int i = 0; i < k; i++)
                {
                    var p = com.CreateParameter();
                    p.ParameterName = "p" + i;
                    p.Value = pps[i];

                    com.Parameters.Add(p);
                }
                com.ExecuteNonQuery();
            }

        }

        public void Update(object o)
        {
            //NextSeq(o);

            var type = o.GetType();

            var mem = loadedTypes[type];

            string tableName = mem.TableName;

            string upd = "update " + tableName + " set ";

            List<object> pps = new List<object>();
            int k = 0;

            foreach (var c in mem.fields)
            {

                string ctype = GetSqlTypeByMem(c.Mem);

                var cl = GetTypeClassByMem(c.Mem);

                string name = c.colName;

                upd += name + " = " + conf.parPre + "p" + k++ + ",";

                switch (cl)
                {
                    case HidTypeInfo.HibTypeClass.Def:
                        pps.Add(c.GetValue(o));
                        break;
                    case HidTypeInfo.HibTypeClass.External:
                        var idb = loadedTypes[c.type].idInfo.GetValue(c.GetValue(o));
                        if (InBase(idb, c.type))
                            pps.Add(idb);
                        else
                            throw new NonLoadedExternalDate();
                        break;
                    case HidTypeInfo.HibTypeClass.Xml:
                        pps.Add(XmlSer.ToXmlString(c.GetValue(o), c.type));
                        break;

                    default:
                        throw new HidException();
                }


            }

            upd = upd.TrimEnd(',');
            upd += " where " + mem.idInfo.colName + " = " + conf.parPre + "pid";

            using (var com = m_connection.CreateCommand())
            {
                DbParameter p;

                com.Transaction = m_tranaction;
                com.CommandText = upd;
                for (int i = 0; i < k; i++)
                {
                    p = com.CreateParameter();
                    p.ParameterName = "p" + i;
                    p.Value = pps[i];

                    com.Parameters.Add(p);
                }

                p = com.CreateParameter();
                p.ParameterName = "pid";
                p.Value = mem.idInfo.GetValue(o);
                com.Parameters.Add(p);

                com.ExecuteNonQuery();
            }

        }

        public T Load<T>(object id) where T : new()
        {
            return (T)Load(id, typeof(T));
        }

        public T[] Load<T>() where T : new()
        {
            var type = typeof(T);
            var mem = loadedTypes[type];

            string tableName = mem.TableName;

            List<object> oo = new List<object>();
            using (var com = m_connection.CreateCommand())
            {
                com.Transaction = m_tranaction;
                com.CommandText = "select " + IdByType(type) + " from " + tableName;

                using (var reader = com.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        oo.Add(reader[0]);
                    }
                    reader.Close();
                }
            }

            T[] ret = new T[oo.Count];
            for (int i = 0; i < ret.Length; i++)
                ret[i] = (T)Load(oo[i], type);

            return ret;
        }

        object Load(object id, Type type)
        {
            if (!loadedTypes.ContainsKey(type))
                throw new NonLOadadTypeError();

            object ret = Activator.CreateInstance(type);
            // Type type = typeof(T);

            var mem = loadedTypes[type];

            string tableName = mem.TableName;

            using (var com = m_connection.CreateCommand())
            {
                com.Transaction = m_tranaction;
                com.CommandText = "select * from " + tableName + " where " + IdByType(type) + " = " + conf.parPre + "p";

                com.Parameters.Clear();
                var p = com.CreateParameter();
                // p.DbType = System.Data.DbType.
                p.ParameterName = "p";
                p.Value = id;
                com.Parameters.Add(p);

                using (var rd = com.ExecuteReader())
                {
                    if (!rd.Read())
                        return ret;

                    var r = UtilsClass.ReaderToDic(rd); // да костыль но в некоторых БД по другому ни как.
                    rd.Close();

                    foreach (var c in mem.fields)
                    {
                        string name = c.colName;

                        var cl = GetTypeClassByMem(c.Mem);

                        switch (cl)
                        {
                            case HidTypeInfo.HibTypeClass.Def:
                                c.SetValue(ret, Convert.ChangeType(r[name], c.type));
                                break;
                            case HidTypeInfo.HibTypeClass.External:
                                if (!loadedTypes.ContainsKey(c.type))
                                    throw new NonLOadadTypeError();
                                var i = Convert.ChangeType(r[name], loadedTypes[c.type].idInfo.type);
                                c.SetValue(ret, Load(i, c.type));
                                break;
                            case HidTypeInfo.HibTypeClass.Xml:
                                c.SetValue(ret, XmlSer.FromString(r[name].ToString(), c.type));
                                break;

                            default:
                                throw new HidException();
                        }



                    }
                }
            }


            return ret;
        }

        bool InBase(object id, Type type)
        {
            if (!loadedTypes.ContainsKey(type))
                throw new NonLOadadTypeError();

            var mem = loadedTypes[type];

            string tableName = mem.TableName;

            using (var com = m_connection.CreateCommand())
            {
                com.Transaction = m_tranaction;
                com.CommandText = "select * from " + tableName + " where " + IdByType(type) + " = " + conf.parPre + "p";

                com.Parameters.Clear();
                var p = com.CreateParameter();
                // p.DbType = System.Data.DbType.
                p.ParameterName = "p";
                p.Value = id;
                com.Parameters.Add(p);

                using (var r = com.ExecuteReader())
                {
                    return r.Read();

                }
            }



        }

        public void Dispose()
        {
            m_connection.Dispose();
        }

        public void Commit()
        {
            m_tranaction.Commit();
            m_tranaction = m_connection.BeginTransaction();
        }
        public void Rollback()
        {
            m_tranaction.Rollback();
        }

        string GetSqlTypeByMem(MemberInfo info)
        {
            Type type = GetTypeByMem(info);
            string stringType = type.ToString();

            if (loadedTypes.ContainsKey(type))
            {
                var h = loadedTypes[type];
                return GetSqlTypeByMem(h.idInfo.Mem);
            }

            return conf.GetSqlCon(stringType);
        }

        HidTypeInfo.HibTypeClass GetTypeClassByMem(MemberInfo info)
        {
            Type type = GetTypeByMem(info);
            string stringType = type.ToString();

            if (conf.CompareType(stringType))
                return HidTypeInfo.HibTypeClass.Def;

            if (loadedTypes.ContainsKey(type))
                return HidTypeInfo.HibTypeClass.External;

            if (Attribute.IsDefined(type, typeof(Table))) // проверка на существование атрибута
                throw new NonLOadadTypeError();

            return HidTypeInfo.HibTypeClass.Xml;
        }

        Type GetTypeByMem(MemberInfo info)
        {
            FieldInfo fieldInfo = info as FieldInfo;
            if (fieldInfo != null)
            {
                return fieldInfo.FieldType;
            }
            else
            {
                PropertyInfo propertyInfo = (PropertyInfo)info;
                return propertyInfo.PropertyType;
            }
        }
    }
}
