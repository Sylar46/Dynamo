﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Controls;
using Dynamo.Models;

namespace Dynamo.Manipulation
{
    public class ManipulatorDaemon
    {
        private readonly Dictionary<Type, INodeManipulatorCreator> registeredManipulators;

        private readonly Dictionary<Guid, IDisposable> activeManipulators =
            new Dictionary<Guid, IDisposable>();

        private ManipulatorDaemon(Dictionary<Type, INodeManipulatorCreator> manips)
        {
            registeredManipulators = manips;
        }

        public static ManipulatorDaemon Create(IManipulatorDaemonInitializer initializer)
        {
            return new ManipulatorDaemon(initializer.GetManipulators());
        }

        public void CreateManipulator(NodeModel model, DynamoView dynamoView)
        {
            INodeManipulatorCreator creator;
            if (registeredManipulators.TryGetValue(model.GetType(), out creator))
            {
                var manipulator = creator.Create(model, new DynamoContext { View = dynamoView });
                if (manipulator != null)
                    activeManipulators[model.GUID] = manipulator;
            }
        }

        public void KillManipulator(NodeModel model)
        {
            IDisposable disposable;
            if (activeManipulators.TryGetValue(model.GUID, out disposable))
                disposable.Dispose();
        }

        internal void KillAll()
        {
            activeManipulators.Values.ToList().ForEach(x => x.Dispose());
            activeManipulators.Clear();
        }
    }
}