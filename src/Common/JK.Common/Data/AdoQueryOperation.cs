﻿using JK.Common.Data.Contracts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Transactions;

namespace JK.Common.Data
{
    [Obsolete("Generic ADO classes have not been tested in .NET Core. Left in for reference purposes.")]
    public abstract class AdoQueryOperation<T> : AdoBaseOperation, IQueryOperation<T>
    {
        private readonly CommandType commandType;

        private readonly string commandText;

        protected AdoQueryOperation(AdoBaseRepository repository, CommandType adoCommandType, string adoCommandText)
            : base(repository)
        {
            this.commandType = adoCommandType;
            this.commandText = adoCommandText;
        }

        public IEnumerable<T> Execute()
        {
            var items = new List<T>();

            using (var scope = new TransactionScope())
            {
                using (DbConnection connection = this.Repository.GetConnection())
                using (DbCommand command = this.SetupCommand(connection, this.commandType, this.commandText))
                {
                    command.Connection.Open();

                    using (IDataReader dataReader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (dataReader.Read())
                        {
                            T item = this.GetObjectFromDataReader(dataReader);
                            items.Add(item);
                        }
                    }

                    command.Connection.Close();
                }

                scope.Complete();
            }

            return items;
        }

        protected abstract T GetObjectFromDataReader(IDataRecord dataRecord);
    }
}