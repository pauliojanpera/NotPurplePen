using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using PurplePen.Graphics2D;
using PurplePen.MapModel;

namespace Map_Svg.Tests
{
    // Checks that the calls the SVG target does not implement raise an error naming the call, which is how the
    // rendering comparison tells a pending case from a failed one.
    [TestFixture]
    public class UnhonoredCallTests
    {
        // Make a test case of each IGraphicsTarget method and property accessor that the target does not fully
        // implement, named after the member and the types of its parameters.
        private static IEnumerable<TestCaseData> UnimplementedMembers()
        {
            foreach (MethodInfo method in typeof(IGraphicsTarget).GetMethods()) {
                if (Svg_GraphicsTarget.FullyImplements(GetCallName(method)))
                    continue;
                string parameterTypes = string.Join(", ", method.GetParameters().Select(parameter => parameter.ParameterType.Name));
                yield return new TestCaseData(method).SetName(method.Name + "(" + parameterTypes + ")");
            }
        }

        [TestCaseSource(nameof(UnimplementedMembers))]
        public void CallRaisesAnErrorNamingIt(MethodInfo method)
        {
            object[] arguments = method.GetParameters()
                .Select(parameter => parameter.ParameterType.IsValueType ? Activator.CreateInstance(parameter.ParameterType) : null)
                .ToArray();

            Svg_GraphicsTarget target = new Svg_GraphicsTarget(DefaultColorConverter.Instance, true);
            TargetInvocationException invocation = Assert.Throws<TargetInvocationException>(() => method.Invoke(target, arguments));
            Svg_UnsupportedCallException unsupportedCall = invocation.InnerException as Svg_UnsupportedCallException;
            Assert.IsNotNull(unsupportedCall, string.Format("{0} raised {1}", method.Name, invocation.InnerException));
            Assert.AreEqual(GetCallName(method), unsupportedCall.CallName);
        }

        // Get the name that a member's calls go by: a property's own name for its accessors.
        private static string GetCallName(MethodInfo method)
        {
            if (method.IsSpecialName && (method.Name.StartsWith("get_", StringComparison.Ordinal) || method.Name.StartsWith("set_", StringComparison.Ordinal)))
                return method.Name.Substring(4);
            return method.Name;
        }
    }
}
