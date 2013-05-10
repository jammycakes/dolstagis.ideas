﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using Ninject;
using Ninject.Modules;

namespace Dolstagis.Core.Data
{
    public class NHibernateNinjectModule : NinjectModule
    {
        public string ConnectionString { get; private set; }

        private Func<IPersistenceConfigurer> configurationProvider;
        private bool keepConnectionAlive;

        public NHibernateNinjectModule(string connectionString, bool keepConnectionAlive = false)
        {
            this.keepConnectionAlive = keepConnectionAlive;
            string[] keys = new string[] {
                connectionString,
                connectionString + "." + Environment.MachineName
            };

            var cs = keys.Select(x => ConfigurationManager.ConnectionStrings[x])
                .Where(x => x != null)
                .FirstOrDefault(x => x.ConnectionString != null);

            IDictionary<string, Func<IPersistenceConfigurer>> providers =
                new Dictionary<string, Func<IPersistenceConfigurer>>() {
                    { "System.Data.Sqlite", this.ConfigureSqlite },
                    { "System.Data.SqlClient", this.ConfigureMsSql }
                };

            if (cs == null) {
                this.ConnectionString = connectionString;
                this.configurationProvider = this.ConfigureMsSql;
            }
            else {
                this.ConnectionString = cs.ConnectionString;
                if (providers.ContainsKey(cs.ProviderName)) {
                    this.configurationProvider = providers[cs.ProviderName];
                }
                else {
                    this.configurationProvider = this.ConfigureMsSql;
                }
            }
        }


        private IPersistenceConfigurer ConfigureMsSql()
        {
            return MsSqlConfiguration.MsSql2008
                .ConnectionString(this.ConnectionString)
                .FormatSql().DoNot.ShowSql();
        }

        private IPersistenceConfigurer ConfigureSqlite()
        {
            var db = SQLiteConfiguration.Standard
                .ConnectionString(this.ConnectionString)
                .FormatSql()
                .ShowSql();
            if (this.keepConnectionAlive)
                db = db.Raw("connection.release_mode", "on_close");
            return db;
        }

        private NHibernate.Cfg.Configuration BuildConfiguration()
        {
            return Fluently.Configure().Database(this.configurationProvider())
                .Mappings(x => x.FluentMappings
                    .AddFromAssembly(this.GetType().Assembly)
                    .AddFromAssembly(System.Reflection.Assembly.Load("Dolstagis.Contrib"))
                )
                .BuildConfiguration();
        }


        public override void Load()
        {
            Bind<NHibernate.Cfg.Configuration>().ToMethod(x => BuildConfiguration()).InSingletonScope();
            Bind<ISessionFactory>().ToMethod
                (x => x.Kernel.Get<NHibernate.Cfg.Configuration>().BuildSessionFactory())
                .InSingletonScope();
        }
    }
}