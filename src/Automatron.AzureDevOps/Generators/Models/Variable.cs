﻿namespace Automatron.AzureDevOps.Generators.Models
{
    public class Variable : IVariable
    {
        public Variable(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }
        public object Value { get; set; }
    }
}