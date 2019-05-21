using System.Diagnostics.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Katren.DiReg
{
    /// <summary>
    /// Жизненный цикл регистрируемого сервиса
    /// </summary>
    public enum DiLifetime
    {
        /// <summary>
        ///  Синглтон
        /// </summary>
        Singleton,

        /// <summary>
        ///  Синглтон в рамках скоупа. Например, на каждый обрабатываемый входящий запрос в AspNet
        /// </summary>
        Scoped,

        /// <summary>
        ///  На каждый запрос - свой инстанс
        /// </summary>
        Transient
    }

    /// <summary>
    /// Атрибут для пометки на автоматическую регистрацию класса в Dependency Injection
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DiRegAttribute : Attribute
    {
        /// <summary>
        /// Интерфейс для регистрации в DI
        /// </summary>
        public Type InterfaceType { get; }

        /// <summary>
        /// Тип жизненного цикла для красса в DI
        /// </summary>
        public DiLifetime LifeTime { get; }

        /// <summary>
        /// Конструктор для регистрации в DI класса по интерфейсу
        /// </summary>
        /// <param name="interfaceType">Интерфейс для регистрации в DI</param>
        /// <param name="lifeTime">Жизненный цикл для регистрации в DI</param>
        public DiRegAttribute(Type interfaceType, DiLifetime lifeTime)
        {
            InterfaceType = interfaceType;
            LifeTime = lifeTime;
        }

        /// <summary>
        /// Конструктор для регистрации в DI класса без интерфейса
        /// </summary>
        /// <param name="lifeTime">Жизненный цикл для регистрации в DI</param>
        public DiRegAttribute(DiLifetime lifeTime)
        {
            InterfaceType = null;
            LifeTime = lifeTime;
        }
    }

    /// <summary>
    /// Утилитарные методы для работы с AutoRegAttribute
    /// </summary>
    public static class AutoRegUtils
    {
        /// <summary>
        /// Метод для автоматической регистрации в DI всех классов, помеченных атрибутом AutoReg
        /// </summary>
        public static void AddDiRegClasses(this IServiceCollection services)
        {
            Contract.Requires(services != null);

            IEnumerable<Type> typeList = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes());
            foreach (Type serviceType in typeList)
            {
                foreach (object attrib in serviceType.GetCustomAttributes(true))
                {
                    if (!(attrib is DiRegAttribute attr))
                        continue;

                    Type interfaceType = attr.InterfaceType ?? serviceType;
                    switch (attr.LifeTime)
                    {
                        case DiLifetime.Singleton:
                            services.AddSingleton(interfaceType, serviceType);
                            break;

                        case DiLifetime.Scoped:
                            services.AddScoped(interfaceType, serviceType);
                            break;

                        case DiLifetime.Transient:
                            services.AddTransient(interfaceType, serviceType);
                            break;
                    }
                }
            }
        }
    }
}