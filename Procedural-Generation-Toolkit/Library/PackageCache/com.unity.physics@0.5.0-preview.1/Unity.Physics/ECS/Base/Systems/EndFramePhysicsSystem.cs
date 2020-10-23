﻿using Unity.Entities;
using Unity.Jobs;

namespace Unity.Physics.Systems
{
    // A system which combines the dependencies of all other physics jobs created during this frame into a single handle,
    // so that any system which depends on all physics work to be finished can just depend on this single handle.
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(BuildPhysicsWorld)), UpdateAfter(typeof(StepPhysicsWorld)), UpdateAfter(typeof(ExportPhysicsWorld))]
    public class EndFramePhysicsSystem : SystemBase, IPhysicsSystem
    {
        private JobHandle m_InputDependency;
        private JobHandle m_OutputDependency;

        protected override void OnCreate()
        {
            m_OutputDependency = Dependency;
        }

        protected override void OnDestroy()
        {
            m_OutputDependency.Complete();
        }

        protected override void OnUpdate()
        {
            // Combine implicit input dependency with the user one
            Dependency = JobHandle.CombineDependencies(Dependency, m_InputDependency);

            m_OutputDependency = Dependency;

            // Invalidate input dependency since it's been used now
            m_InputDependency = default;
        }

        public void AddInputDependency(JobHandle inputDep)
        {
            m_InputDependency = JobHandle.CombineDependencies(m_InputDependency, inputDep);
        }

        public JobHandle GetOutputDependency()
        {
            return m_OutputDependency;
        }
    }
}
