﻿using BlazorMobile.Common.Interop;
using BlazorMobile.Common.Serialization;
using BlazorMobile.Components;
using BlazorMobile.Services;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BlazorMobile.Interop
{
    public static class ContextBridge
    {
        private static object GetDefault(Type type)
        {
            if (type == typeof(void))
            {
                return null;
            }

            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        public static void ClearRequestValues(MethodProxy methodResult)
        {
            if (methodResult == null)
                return;

            methodResult.GenericTypes = null;
            methodResult.InterfaceType = null;
            methodResult.Parameters = null;
        }

        private static object GetResultFromTask(Type returnType, Task taskResult)
        {
            if (returnType == null || returnType == typeof(void) || returnType == typeof(Task))
            {
                return null;
            }

            try
            {
                if (taskResult.IsCompleted == false)
                {
                    taskResult.GetAwaiter().GetResult();
                }

                var result = taskResult.GetType().GetProperty("Result").GetValue(taskResult);

                return result;
            }
            catch (Exception)
            {
                return GetDefault(returnType);
            }
        }

        public static string GetJSONReturnValue(MethodProxy methodResult)
        {
            ClearRequestValues(methodResult);
            return BridgeSerializer.Serialize(methodResult);
        }

        public static MethodProxy GetMethodProxyFromJSON(string json)
        {
            return BridgeSerializer.Deserialize<MethodProxy>(json);
        }

        public static MethodProxy Receive(MethodProxy methodProxy)
        {
            object defaultValue = default(object);

            try
            {
                Type iface = methodProxy.InterfaceType.ResolvedType();
                object concreteService = DependencyServiceExtension.Get(iface);

                MethodInfo baseMethod = MethodProxyHelper.GetClassMethodInfo(concreteService.GetType(), iface, methodProxy.MethodIndex);

                //In case of failure, getting Default Return Type
                defaultValue = GetDefault(baseMethod.ReturnType);

                if (methodProxy.GenericTypes != null && methodProxy.GenericTypes.Length > 0)
                {
                    Type[] genericTypes = methodProxy.GenericTypes.Select(p => p.ResolvedType()).ToArray();

                    methodProxy.ReturnValue = baseMethod.MakeGenericMethod(genericTypes).Invoke(concreteService, methodProxy.Parameters);
                    methodProxy.TaskSuccess = true;
                }
                else
                {
                    methodProxy.ReturnValue = baseMethod.Invoke(concreteService, methodProxy.Parameters);
                    methodProxy.TaskSuccess = true;
                }

                if (methodProxy.AsyncTask)
                {
                    methodProxy.ReturnValue = GetResultFromTask(methodProxy.ReturnType.ResolvedType(), (Task)methodProxy.ReturnValue);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: [Native] - {nameof(ContextBridge)}.{nameof(Receive)}: {ex.Message}");

                methodProxy.ReturnValue = defaultValue;
                methodProxy.TaskSuccess = false;
            }

            return methodProxy;
        }
    }
}
