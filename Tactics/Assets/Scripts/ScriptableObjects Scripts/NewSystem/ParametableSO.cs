using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Battle.ScriptableObjects
{
    public abstract class ParametableSO : ScriptableObject
    {
        [field: SerializeField] public string Id { get; protected set; }
        
        public const string ToolTipText = "Parameter name is case sensitive\nSeparate parameters with \\n (don't type \\n just backspace)\nparameter(:value1,value2,...)\nUse'\\\\' for comments\nInvalid values are ignored";
        
        public abstract IEnumerable<string> SpecificParameters { get; }

        protected IReadOnlyDictionary<string, string> GetSpecificParameter(string allParameters)
        {
            var allParametersDict = ConvertAllParametersToDictionary(allParameters);

            var dict = new Dictionary<string, string>();
            foreach (var specificParameter in SpecificParameters)
            {
                if (allParametersDict.TryGetValue(specificParameter, out var parameter))
                    dict.Add(specificParameter, parameter);
            }

            return dict;
        }

        protected T GetParameterValue<T>(string parameter, IReadOnlyDictionary<string, string> specificParameters)
        {
            var parameterNotFound = !specificParameters.TryGetValue(parameter, out var value);

            return (T) InterpretParameter(parameter, value, parameterNotFound);
        }

        protected abstract object InterpretParameter(string parameter, string value, bool wasNotFound);

        private static ReadOnlyDictionary<string, string> ConvertAllParametersToDictionary(string parameters)
        {
            var dict = new Dictionary<string, string>();
            var parametersArray = parameters.Split('\n');

            if (parametersArray.Length <= 0) return new ReadOnlyDictionary<string, string>(dict);

            foreach (var parameter in parametersArray)
            {
                if (parameter.Length > 2) if(parameter[1] == '\\' && parameter[0] == '\\') continue;
                
                var split = parameter.Split(':');

                dict.Add(split[0], split.Length > 1 ? split[1] : string.Empty);
            }

            return new ReadOnlyDictionary<string, string>(dict);
        }
        
        protected string GenerateListOfParametersText<T>(Func<string,dynamic> parameterGetter,Func<T,(bool use,string text)> generateLineFunc,string startText = "",string normalSeparator = ", ",string lastSeparator = " and ") where T : struct
        {
            var list = new List<string>();

            foreach (var parameter in SpecificParameters)
            {
                var comparison = parameterGetter.Invoke(parameter) as T? ?? new T();

                var generated = generateLineFunc.Invoke(comparison);
                
                if (generated.use) list.Add($"{generated.text}");
            }

            if (list.Count == 0) return string.Empty;

            startText += list[0];

            if (list.Count == 1) return startText;

            for (int i = 1; i < list.Count; i++)
            {
                var text = list[i];
                var separator = i != list.Count - 1 ? normalSeparator : lastSeparator;
                startText += $"{separator}{text}";
            }

            return startText;
        }
        
        protected static string ComparisonToText(Comparison comparison)
        {
            return comparison switch
            {
                Comparison.Equal => "==",
                Comparison.NotEqual => "!=",
                Comparison.Greater => ">",
                Comparison.GreaterOrEqual => ">=",
                Comparison.Lesser => "<",
                Comparison.LesserOrEqual => "<=",
                _ => "=="
            };
        }

        protected static bool TextToComparison(string text, out Comparison comparison)
        {
            comparison = Comparison.Equal;
            switch (text)
            {
                case "==":
                    return true;
                case "!=":
                    comparison = Comparison.NotEqual;
                    return true;
                case ">":
                    comparison = Comparison.Greater;
                    return true;
                case ">=":
                    comparison = Comparison.GreaterOrEqual;
                    return true;
                case "<":
                    comparison = Comparison.Lesser;
                    return true;
                case "<=":
                    comparison = Comparison.LesserOrEqual;
                    return true;
                default:
                    return false;
            }
        }
        
        protected static string OperationToText(Operation operation)
        {
            return operation switch
            {
                Operation.Set => "=",
                Operation.Add => "+",
                Operation.Subtract => "-",
                Operation.Multiply => "*",
                Operation.Divide => "/",
                _ => "+"
            };
        }
        
        protected static bool TextToOperation(string text, out Operation operation)
        {
            operation = Operation.Add;
            switch (text)
            {
                case "=":
                    operation = Operation.Set;
                    return true;
                case "+":
                    return true;
                case "-":
                    operation = Operation.Subtract;
                    return true;
                case "*":
                    operation = Operation.Multiply;
                    return true;
                case "/":
                    operation = Operation.Divide;
                    return true;
                default:
                    return false;
            }
        }
    }
}