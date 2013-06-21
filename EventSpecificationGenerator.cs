using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace IntelliTrace.Config
{
    using DataQuery = CollectionPlanTracePointProviderDiagnosticEventSpecificationBindingDataQuery;
    using EventSpecification = CollectionPlanTracePointProviderDiagnosticEventSpecification;
    using EventSpecificationBinding = CollectionPlanTracePointProviderDiagnosticEventSpecificationBinding;

    public class EventSpecificationGenerator
    {
        private readonly Type eventsType;
        private readonly string filterMethodName;

        public EventSpecificationGenerator(Type eventsType, string methodName = null)
        {
            this.eventsType = eventsType;
            filterMethodName = methodName ?? string.Empty;
        }

        public static EventSpecificationGenerator Create<TEvents>()
        {
            return new EventSpecificationGenerator(typeof(TEvents));
        }

        public EventSpecification GetEventSpecification(Type dataQuery = null)
        {
            return GetEventSpecifications(dataQuery).Single();
        }

        public IList<EventSpecification> GetEventSpecifications(Type dataQuery = null)
        {
            var specifications = new List<EventSpecification>();
            var methodsByName = from method in eventsType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) 
                                                where !method.IsSpecialName && method.DeclaringType != typeof(object) && (filterMethodName.Length == 0 ? true : method.Name == filterMethodName)
                                                group method by method.Name into g
                                                select new { Name = g.Key, Methods = g };
            foreach(var methodGroup in methodsByName)
            {
                var assemblyName = GetAssemblyName();
                var specification = new EventSpecification { CategoryId =  assemblyName };
                specifications.Add(specification);
                var methodName = methodGroup.Name;
                specification.SettingsName._locID = PrefixClassMethod("settingsName", methodName);
                specification.SettingsDescription.Value = specification.SettingsName.Value = methodName + " (" + ClassName + ")";
                specification.SettingsDescription._locID = PrefixClassMethod("settingsDescription", methodName);
                int index = 1;
                bool hasOverrides = methodGroup.Methods.Count() > 1;
                foreach(var method in methodGroup.Methods)
                {
                    var myMethodName = hasOverrides ? (methodName + "." + index) : methodName;
                    var binding = new EventSpecificationBinding
                    {
                        ModuleSpecificationId = assemblyName,
                        TypeName = FullClassName,
                        MethodName = methodName,
                        MethodId = GetMethodId(method),
                    };
                    if(dataQuery == null)
                    {
                        binding.ProgrammableDataQuery = null;
                        binding.DataQueries.AddRange(GetDataQueries(method, myMethodName));
                        binding.ShortDescription._locID = PrefixClassMethod("shortDescription", myMethodName);
                        binding.LongDescription._locID = PrefixClassMethod("longDescription", myMethodName);
                        binding.LongDescription.Value = binding.ShortDescription.Value = methodName;
                    }
                    else
                    {
                        binding.DataQueries = null;
                        binding.ShortDescription = null;
                        binding.LongDescription = null;
                        binding.ProgrammableDataQuery.ModuleName = dataQuery.Assembly.ManifestModule.Name;
                        binding.ProgrammableDataQuery.TypeName = dataQuery.FullName;
                    }
                    specification.Bindings.Add(binding);
                    index++;
                }
            }
            return specifications;
        }

        private IEnumerable<DataQuery> GetDataQueries(MethodInfo method, string myMethodName)
        {
            return method.GetParameters().Select((parameter, index) => new DataQuery
            {
                name = parameter.Name,
                _locAttrData = "name",
                query = "",
                index = (sbyte)(index + 1),
                type = parameter.ParameterType.Name,
                _locID = PrefixClassMethod("dataquery", myMethodName + "." + parameter.Name),
                maxSize = (ushort)(parameter.ParameterType == typeof(string) ? 256 : 0)
            });
        }

        public string GetMethodId(MethodInfo method)
        {
            var builder = new StringBuilder(FullClassName).Append('.').Append(method.Name).Append('(');
            foreach(var parameter in method.GetParameters())
            {
                builder.Append(parameter.ParameterType.FullName).Append(',');
            }
            builder[builder.Length - 1] = ')';
            builder.Append(':');
            builder.Append(method.ReturnType.FullName);
            return builder.ToString();
        }

        public string PrefixClassMethod(string prefix, string methodName)
        {
            return prefix + "." + ClassName + "." + methodName;
        }

        public string ClassName
        {
            get
            {
                return eventsType.Name;
            }
        }

        public string FullClassName
        {
            get
            {
                return eventsType.FullName;
            }
        }

        private string GetAssemblyName()
        {
            return eventsType.Assembly.GetName().Name;
        }
    }

    public partial class CollectionPlanTracePointProviderDiagnosticEventSpecification
    {
        const string TargetNamespace = "urn:schemas-microsoft-com:visualstudio:tracelog";

        public XElement ToXElement()
        {
            var plan = new CollectionPlanTracePointProvider();
            plan.DiagnosticEventSpecifications.Add(this);
            var name = XName.Get("DiagnosticEventSpecification", TargetNamespace);
            var planXml = plan.ToXElement();
            var eventSpecification = planXml.Descendants(name).Single();
            // remove the xmlns attribute
            foreach(var xElement in eventSpecification.DescendantsAndSelf())
            {
                xElement.Name = xElement.Name.LocalName;
            }
            return eventSpecification;
        }
    }
}