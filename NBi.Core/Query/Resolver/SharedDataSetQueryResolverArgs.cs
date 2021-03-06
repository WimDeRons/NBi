﻿using NBi.Core.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBi.Core.Query.Resolver
{
    public class SharedDataSetQueryResolverArgs : QueryResolverArgs
    {
        private readonly string source;
        private readonly string path;
        private readonly string name;

        public string Source { get => source; }
        public string Path { get => path; }
        public string Name { get => name; }

        public SharedDataSetQueryResolverArgs(string source, string path, string name, 
            string connectionString, IEnumerable<IQueryParameter> parameters,
            IEnumerable<IQueryTemplateVariable> variables, int timeout)
            : base(connectionString, parameters, variables, timeout)
        {
            this.source = source;
            this.path = path;
            this.name = name;
        }
    }
}
