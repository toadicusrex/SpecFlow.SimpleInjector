﻿using System;
using BoDi;
using SimpleInjector;
using TechTalk.SpecFlow.Infrastructure;

namespace SpecFlow.SimpleInjector
{
    public class SimpleInjectorBindingInstanceResolver : IBindingInstanceResolver
    {
        public object ResolveBindingInstance(Type bindingType, IObjectContainer scenarioContainer)
        {
            var componentContext = scenarioContainer.Resolve<Container>();
            return componentContext.GetInstance(bindingType);
        }
    }
}
