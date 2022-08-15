#if NET6_0
using System;
using System.Reflection;

namespace Automatron.Reflection
{
    public abstract class SymbolVisitor
    {
        public virtual void Visit(MemberInfo memberInfo)
        {
            switch (memberInfo)
            {
                case Type type:
                    VisitType(type);
                    break;
                case MethodInfo methodInfo:
                    VisitMethod(methodInfo);
                    break;
                case PropertyInfo propertyInfo:
                    VisitProperty(propertyInfo);
                    break;
            }
        }

        public virtual void VisitType(Type type)
        {
  
        }

        public virtual void VisitMethod(MethodInfo methodInfo)
        {

        }

        public virtual void VisitProperty(PropertyInfo propertyInfo)
        {

        }

        public virtual void VisitAttribute(Attribute attribute)
        {

        }

        public virtual void VisitAttributeData(CustomAttributeData attributeData)
        {

        }
    }
}
#endif